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
        private TurfStatus TurfStatus;
        private IGangTerritories GangTerritories;
        private IZones Zones;
        private GangDen HiringGangDen;
        private Gang TargetGang;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private bool AllTargetsKilled = false;
        private int MembersKilled = 0;
        private BlankLocation TargetedScenario;
        private List<GangSpawnTask> TargetSpawns = new List<GangSpawnTask>();
        private int KillRequirement { get; set; } = 1;
        private bool HasConditions => HiringGangDen != null && TargetedScenario != null;
        public bool JoinGangOnComplete { get; set; } = false;

        public RivalGangAmbushTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, Gang targetGang, IGangTerritories gangTerritories, IZones zones, TurfStatus turfStatus) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            TargetGang = targetGang;
            //KillRequirement = killRequirement;
            GangTerritories = gangTerritories;
            Zones = zones;
            TurfStatus = turfStatus;
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
            if (TargetedScenario != null)
                ResetScenario();
        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringGang?.ContactName))
            {
                /* No more random.
                if (TargetZone == null || TargetGang == null)
                {
                    GetTargetGang();
                    GetTargetZone();
                }*/
                EntryPoint.WriteToConsole($"Starting Ambush on {TargetGang.ID} in {TargetZone.InternalGameName}, tasked by {HiringGang.ID}");
                TargetedScenario = TurfStatus.GetTargetScenario(typeof(GangConditionalLocation));
                GetHiringDen();
                if (HasConditions)
                {
                    SetupScenario();
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
                    ResetScenario();
                    break;
                }
                if (TargetedScenario.DistanceToPlayer <= TargetedScenario.ActivateDistance && TargetedScenario.IsNearby && MembersKilled == 0) // Check if player near -- if not dont account for those recently spawned
                {
                    UpdateTargetSpawns();
                }
                if (TargetSpawns == null || !TargetSpawns.Any())
                {
                    GameFiber.Sleep(1000);
                    continue;
                }

                MembersKilled = TargetSpawns.Sum(x => x.CreatedPeople?.Count(y => y.IsDead) ?? 0); // Regardless if player was their killer
                AllTargetsKilled = !TargetSpawns.Any(x => x.CreatedPeople?.Any(y => !y.IsDead) ?? false);

                if (MembersKilled > 0)
                {
                    ResetScenario();
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
        private void SetupScenario()
        {
            EntryPoint.WriteToConsole($"{TargetedScenario.Name} SELECTED TWIN");
            TurfStatus.SetupScenarioLocations(TargetedScenario);

            TargetedScenario.MapIcon = (int)BlipSprite.Destination;
            TargetedScenario.MapIconColorString = "Blue";
            TargetedScenario.MapIconScale = 1.0f;
            TargetedScenario.MapIconRadius = 55f;
            TargetedScenario.MapOpenIconAlpha = 0.35f;
            TargetedScenario.ActivateBlip(Time, World);
        }
        private void ResetScenario()
        {
            TargetSpawns.ForEach(x => x.CreatedPeople.ForEach(m => m?.DeleteBlip()));
            TargetedScenario.IsAmbushTarget = false;
            TargetedScenario.IsEnabled = false; //Cant do hit again until x time maybe
            TargetedScenario.DeactivateBlip();
        }
        private void UpdateTargetSpawns()
        {
            TargetSpawns.ForEach(x => x.CreatedPeople.ForEach(m => m?.DeleteBlip()));
            TargetSpawns.Clear();

            if (TargetedScenario.PossibleGroupSpawns != null && TargetedScenario.PossibleGroupSpawns.Any()) // Detailed ambushes (using descriptions)
            {
                TargetSpawns.AddRange(TargetedScenario.PossibleGroupSpawns
                        .Where(x => x.IsAmbushTarget && x.AttemptedSpawn)
                        .SelectMany(x => x.PossiblePedSpawns.OfType<GangConditionalLocation>())
                        .Where(x => x.GangSpawnTask != null)
                        .Select(x => x.GangSpawnTask)
                );
            }

            TargetSpawns.ForEach(x => x.CreatedPeople.ForEach(m => m.AddBlip()));
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
