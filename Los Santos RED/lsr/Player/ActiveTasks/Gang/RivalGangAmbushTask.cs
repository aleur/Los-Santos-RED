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
        private string AmbushTargetType;
        private TurfStatus TurfStatus;
        private IGangTerritories GangTerritories;
        private IZones Zones;
        private IAgencies Agencies;
        private GangDen HiringGangDen;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private bool AllTargetsKilled = false;
        private int MembersKilled = 0;
        private BlankLocation TargetedScenario;
        private List<SpawnTask> TargetSpawns = new List<SpawnTask>();
        private int KillRequirement { get; set; } = 1;
        private bool HasConditions => HiringGangDen != null && TargetedScenario != null;
        public bool JoinGangOnComplete { get; set; } = false;
        public bool KillAllTargets { get; set; } = false;

        public RivalGangAmbushTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, IGangTerritories gangTerritories, IZones zones, IAgencies agencies, TurfStatus turfStatus, string targetType) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            AmbushTargetType = targetType;
            GangTerritories = gangTerritories;
            Zones = zones;
            Agencies = agencies;
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
                ResetScenario(true);
        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringGang?.ContactName))
            {
                TargetedScenario = TurfStatus.GetTargetScenario(AmbushTargetType);
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
                    ResetScenario(false);
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

                bool missionRequirements = KillAllTargets ? AllTargetsKilled : MembersKilled >= KillRequirement;

                if (missionRequirements)
                {
                    ResetScenario(true);
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
        private void ResetScenario(bool disable)
        {
            TargetSpawns.ForEach(x => x.CreatedPeople.ForEach(m => m?.DeleteBlip()));
            TargetedScenario.IsAmbushTarget = false;
            TargetedScenario.DeactivateBlip();

            if (disable)
                TargetedScenario.IsEnabled = false; //Cant do hit again until x time maybe
        }
        private void UpdateTargetSpawns()
        {
            TargetSpawns.ForEach(x => x.CreatedPeople.ForEach(m => m?.DeleteBlip()));
            TargetSpawns.Clear();

            if (TargetedScenario.PossibleGroupSpawns?.Any() == true)
            {
                TargetSpawns.AddRange(TargetedScenario.PossibleGroupSpawns
                        .Where(x => x.IsAmbushTarget && x.AttemptedSpawn)
                        .SelectMany(x => x.PossiblePedSpawns)
                        .Where(x => x.LocationSpawnTask != null)
                        .Select(x => x.LocationSpawnTask)
                );
                TargetSpawns.AddRange(TargetedScenario.PossibleGroupSpawns
                        .Where(x => x.IsAmbushTarget && x.AttemptedSpawn && x.PossibleVehicleSpawns?.Any(vs => !vs.IsEmpty) == true)
                        .SelectMany(x => x.PossibleVehicleSpawns)
                        .Where(x => x.LocationSpawnTask != null)
                        .Select(x => x.LocationSpawnTask)
                );
            }
            TargetSpawns.ForEach(x => x.CreatedPeople.ForEach(m => m.AddBlip()));
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
            PlayerTasks.AddTask(HiringContact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete,DebugName);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>();
            switch (AmbushTargetType)
            {
                case "Gang":
                    Gang TargetGang = Gangs.GetGang(TargetedScenario.AssignedAssociationID); // temporary for now
                    Replies = KillAllTargets ? new List<string>() {
                        $"They think they can push us around? Not today. Sent you a location. Take out all of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ bitches and I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                        $"Those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks think they can fuck with me? Sent you a location. Put all of them in the fucking ground. Not a single one lives. ~g~${PaymentAmount}~s~",
                        $"We’re sending a clear message to {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~. Sending you a location now. Take them all out. ~g~${PaymentAmount}~s~",
                        $"{TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ decided to make some moves against us. Retaliate at this location I just sent you. Wipe them all out. I'll have ~g~${PaymentAmount}~s~ waiting for you.",
                        $"Check your map, I just sent you the location of some fuckers. Kill all of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks. ~g~${PaymentAmount}~s~",
                        } :
                        new List<string>() {
                        $"They think they can push us around? Not today. Sent you a location. Go take out at least {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ bitches and I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                        $"Those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks think they can fuck with me? Sent you a location. Go give at least {KillRequirement} of those pricks a dirt nap. ~g~${PaymentAmount}~s~",
                        $"We’re sending a clear message to {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~. Sending you a location now. Go deal with at least {KillRequirement} of their men. ~g~${PaymentAmount}~s~",
                        $"{TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ decided to make some moves against us. Retaliate at this location I just sent you. Let them know we don't approve by sending {KillRequirement} of those assholes to the other side. I'll have ~g~${PaymentAmount}~s~ waiting for you.",
                        $"Check your map, I just sent you the location of some fuckers. Kill at least {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks. ~g~${PaymentAmount}~s~",
                        }
                    ; ;
                    break;
                case "Police":
                    Agency TargetAgency = Agencies.GetAgency(TargetedScenario.AssignedAssociationID); // temporary for now
                    Replies = KillAllTargets ? new List<string>() {
                        $"Those fucking pigs. They think they can push us around? Not today. Sent you a location. Take out all of those {TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ bitches and I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                        $"Those {TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ pigs think they can fuck with me? Sent you a location. Put all of them in the fucking ground. Not a single one lives. ~g~${PaymentAmount}~s~",
                        $"Found some {TargetAgency.ColorPrefix}{TargetAgency.ShortName} poking their noses where they don’t belong. Here’s their location. Deal with them fast. ~g~${PaymentAmount}~s~ waiting for you.",
                        $"Looks like {TargetAgency.ColorPrefix}{TargetAgency.ShortName} are snooping around. I’ve marked their position. Handle it quickly. ~g~${PaymentAmount}~s~ for the job.",
                        } :
                        new List<string>() {
                        $"Those fucking pigs. They think they can push us around? Not today. Sent you a location. Take out at least {KillRequirement} of these {TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ bitches and I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                        $"Those {TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ pigs think they can fuck with me? Sent you a location. I need at least {KillRequirement} dead. ~g~${PaymentAmount}~s~",
                        $"Found some {TargetAgency.ColorPrefix}{TargetAgency.ShortName} poking their noses where they don’t belong. Here’s their location. Need at least {KillRequirement} of them gone. ~g~${PaymentAmount}~s~ waiting for you.",
                        $"Looks like {TargetAgency.ColorPrefix}{TargetAgency.ShortName} are snooping around. Need at least {KillRequirement} dead. I’ve marked their position. Handle it quickly. ~g~${PaymentAmount}~s~ for the job.",
                        }
                    ; ;
                    break;
                case "Civilian":
                    Replies = KillAllTargets ? new List<string>() {
                        $"We're sending a message. Not a single one lives. ~g~${PaymentAmount}~s~.",
                        } :
                        new List<string>() {
                        $"Need {KillRequirement} dead here. No questions asked. ~g~${PaymentAmount}~s~",
                        }
                    ; ;
                    break;
                default:
                    Replies = KillAllTargets ? new List<string>() {
                        $"I want all of them dead. I'll make sure you’re paid ~g~${PaymentAmount}~s~.",
                        } :
                        new List<string>() {
                        $"Sent you a location. Give at least {KillRequirement} of them a dirt nap. You'll get ~g~${PaymentAmount}~s~",
                        }
                    ; ;
                    break;
            }
            /*
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
            ; ;*/
            Player.CellPhone.AddPhoneResponse(HiringContact.Name, HiringContact.IconName, Replies.PickRandom());
        }
    }
}
