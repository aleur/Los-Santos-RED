using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Player.ActiveTasks
{
    public class RivalGangAmbushTask : GangTask, IPlayerTask
    {
        private IGangTerritories GangTerritories;
        private IZones Zones;
        private GangDen HiringGangDen;
        private Gang TargetGang;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private bool PlayerHasKilled => MembersKilled > 0;
        private int MembersKilled = 0;
        private BlankLocation TargetedScenario;
        private List<GangSpawnTask> TargetSpawns = new List<GangSpawnTask>();
        private int KillRequirement { get; set; } = 1;
        private bool HasConditions => HiringGangDen != null && TargetedScenario != null;
        public bool JoinGangOnComplete { get; set; } = false;

        public RivalGangAmbushTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, Gang targetGang, int killRequirement, IGangTerritories gangTerritories, IZones zones) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            TargetGang = targetGang;
            //KillRequirement = killRequirement;
            GangTerritories = gangTerritories;
            Zones = zones;
        }
        public override void Setup()
        {
            base.Setup();/*
            RepOnCompletion = 2000;
            DebtOnFail = 0;
            RepOnFail = -500;
            DaysToComplete = 7;*/
            DebugName = "Rival Gang Ambush";
        }
        public override void Dispose()
        {

        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringGang?.ContactName))
            {
                if (TargetZone != null && TargetZone.AssignedGang != null && TargetZone.AssignedGang != HiringGang)
                {
                    TargetGang = TargetZone.AssignedGang;
                }
                else
                {
                    GetTargetGang();
                    GetTargetZone();
                }
                GetTargetScenario();
                GetHiringDen();
                if (HasConditions)
                {
                    GetPayment();
                    UpdateTargetSpawns();
                    SendInitialInstructionsMessage();
                    AddTask();
                    GameFiber PayoffFiber = GameFiber.StartNew(delegate
                    {
                        try
                        {
                            Loop();
                            FinishTask();
                        }
                        catch (Exception ex)
                        {
                            EntryPoint.WriteToConsole(ex.Message + " " + ex.StackTrace, 0);
                            EntryPoint.ModController.CrashUnload();
                        }
                    }, "PayoffFiber");
                }
                else
                {
                    GangTasks.SendGenericTooSoonMessage(HiringContact);
                }
            }
        }
        protected override void Loop()
        {
            while (true)
            {
                CurrentTask = PlayerTasks.GetTask(HiringContact?.Name);
                if (CurrentTask == null || !CurrentTask.IsActive)
                {
                    break;
                }
                if (TargetedScenario.DistanceToPlayer <= TargetedScenario.ActivateDistance && TargetedScenario.IsNearby && !PlayerHasKilled) // Check if player near -- if not dont account for those recently spawned
                {
                    UpdateTargetSpawns();
                }
                if (TargetSpawns == null || !TargetSpawns.Any())
                {
                    GameFiber.Sleep(1000);
                    continue;
                }

                MembersKilled = TargetSpawns.Sum(x => x.CreatedPeople?.Count(y => y.IsDead) ?? 0); // Regardless if player was their killer

                if (MembersKilled >= KillRequirement)
                {
                    CurrentTask.OnReadyForPayment(true);
                    break;
                }

                GameFiber.Sleep(1000);
            }
        }
        protected override void FinishTask()
        {
            if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
            {
                if (HiringGangDen.IsAvailableForPlayer) SendMoneyPickupMessage(HiringGang.DenName, HiringGangDen);
                else SetReadyToPickupDeadDrop(); 
            }
        }
        private void GetTargetGang()
        {
            if (TargetGang == null)
            {
                TargetGang = Gangs.GetAllGangs().Where(x => x.ID != HiringGang.ID).PickRandom();
            }
        }
        private void GetTargetScenario()
        {
            List<BlankLocation> targetableScenarios = new List<BlankLocation>();

            // Get blank locations where GangConditionalLocation exists
            foreach (BlankLocation bl in World.Places.ActiveLocations.OfType<BlankLocation>().ToList().Where(x => x.IsEnabled && x.CanBeAmbushableTarget && Zones.GetZone(x.EntrancePosition) == TargetZone).ToList())
            {
                bool UsingGroupSpawns = bl.PossibleGroupSpawns != null && bl.PossibleGroupSpawns.Any(),
                     UsingPedSpawns = bl.PossiblePedSpawns != null && bl.PossiblePedSpawns.Any();

                if (UsingGroupSpawns) // Detailed ambushes (using descriptions)
                {
                    List<ConditionalGroup> cgGroup = bl.PossibleGroupSpawns.Where(x => x.CanBeAmbushableTarget && x.PossiblePedSpawns.OfType<GangConditionalLocation>().ToList().Any()).ToList();
                    
                    if (cgGroup.Any()) targetableScenarios.Add(bl);
                }
                else if (UsingPedSpawns && bl.PossiblePedSpawns.OfType<GangConditionalLocation>().Any()) // Basic ambushes (x amount of members of x gang to kill)
                {
                    targetableScenarios.Add(bl);
                }
            }
            TargetedScenario = targetableScenarios.Count > 0 ? targetableScenarios[MathHelper.GetRandomInteger(0,targetableScenarios.Count)] : null;
        }
        private void UpdateTargetSpawns()
        {
            bool UsingGroupSpawns = TargetedScenario.PossibleGroupSpawns != null && TargetedScenario.PossibleGroupSpawns.Any(),
                 UsingPedSpawns = TargetedScenario.PossiblePedSpawns != null && TargetedScenario.PossiblePedSpawns.Any();

            TargetSpawns.Clear();

            if (UsingGroupSpawns) // Detailed ambushes (using descriptions)
            {
                TargetSpawns.AddRange(TargetedScenario.PossibleGroupSpawns
                        .Where(x => x.CanBeAmbushableTarget)
                        .SelectMany(x => x.PossiblePedSpawns.OfType<GangConditionalLocation>())
                        .Where(x => x.GangSpawnTask != null)
                        .Select(x => x.GangSpawnTask)
                );
            }
            else if (UsingPedSpawns) // Basic ambushes (x amount of members of x gang to kill)
            {
                TargetSpawns.AddRange(TargetedScenario.PossiblePedSpawns
                    .OfType<GangConditionalLocation>()
                    .Where(x => x.GangSpawnTask != null)
                    .Select(x => x.GangSpawnTask)
                );
            }
        }
        private void GetTargetZone()
        {
            if (TargetGang == null || TargetZone != null)
            {
                return;
            }
            List<GangTerritory> totalTerritories = GangTerritories.GetGangTerritory(TargetGang.ID)?.Where(x => x.Priority == 0).ToList();
            if(totalTerritories == null || !totalTerritories.Any())
            {
                return;
            }
            GangTerritory selectedTerritory = totalTerritories.PickRandom();
            if (selectedTerritory != null)
            {
                TargetZone = Zones.GetZone(selectedTerritory.ZoneInternalGameName);
            }
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
        }
        protected override void GetPayment()
        {
            PaymentAmount = RandomItems.GetRandomNumberInt(HiringGang.HitPaymentMin, HiringGang.HitPaymentMax).Round(500);
            PaymentAmount *= KillRequirement;
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 500;
            }
        }
        protected override void AddTask()
        {
            TargetedScenario.IsAmbushTarget = true;
            GameTimeToWaitBeforeComplications = RandomItems.GetRandomNumberInt(3000, 10000);
            HasAddedComplications = false;
            WillAddComplications = false;// RandomItems.RandomPercent(Settings.SettingsManager.TaskSettings.RivalGangHitComplicationsPercentage);
            GangReputation gr = Player.RelationshipManager.GangRelationships.GetReputation(TargetGang);
            PlayerTasks.AddTask(HiringContact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete,DebugName);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = HiringGangDen.IsAvailableForPlayer ? new List<string>() {
                $"They think they can push us around? Not today. Get to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and take out {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ bitches. Return to {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} when you're done, and I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                $"Those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ think they can fuck with me? Go to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and give {KillRequirement} of those pricks a dirt nap. Once you are done come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. ~g~${PaymentAmount}~s~ to you",
                $"We’re sending a clear message to {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~. Go to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and deal with {KillRequirement} of their men. After the job, swing by {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} for your ~g~${PaymentAmount}~s~.",
                $"{TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ decided to make some moves against us. Go over to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and let them know we don't approve by sending {KillRequirement} of those assholes to the other side. I'll have ~g~${PaymentAmount}~s~ waiting for you.",
                $"Go to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and find {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks. Make sure they won't ever talk to anyone again. Come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} for your payment of ~g~${PaymentAmount}~s~",
                } :
                new List<string>() {
                $"They think they can push us around? Not today. Get to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and take out {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ bitches. I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                $"Those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ think they can fuck with me? Go to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and give {KillRequirement} of those pricks a dirt nap. ~g~${PaymentAmount}~s~ to you",
                $"We’re sending a clear message to {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~. Go to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and deal with {KillRequirement} of their men. ~g~${PaymentAmount}~s~.",
                $"{TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ decided to make some moves against us. Go over to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and let them know we don't approve by sending {KillRequirement} of those assholes to the other side. I'll have ~g~${PaymentAmount}~s~ waiting for you.",
                $"Go to {TargetGang.ColorPrefix}{TargetZone.DisplayName}~s~ and find {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks. Make sure they won't ever talk to anyone again. ~g~${PaymentAmount}~s~",
                }
            ; ;
            Player.CellPhone.AddPhoneResponse(HiringContact.Name, HiringContact.IconName, Replies.PickRandom());
        }
    }
}
