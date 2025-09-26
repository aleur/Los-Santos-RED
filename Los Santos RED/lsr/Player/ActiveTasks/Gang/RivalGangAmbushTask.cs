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
        private Zone TargetZone;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private int KilledMembersAtStart;
        private int ExternalZoneKills = 0;
        private int InternalZoneKills = 0;
        private int KillRequirement { get; set; } = 1;
        private bool HasConditions => HiringGangDen != null && TargetZone != null;

        public bool JoinGangOnComplete { get; set; } = false;

        public RivalGangAmbushTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, Gang targetGang, int killRequirement, IGangTerritories gangTerritories, IZones zones) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            TargetGang = targetGang;
            KillRequirement = killRequirement;
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
                GetTargetZone();
                GetHiringDen();
                if (HasConditions)
                {
                    GetPayment();
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
                //Game.DisplaySubtitle($"{InternalZoneKills}/{KillRequirement} {ExternalZoneKills} OOZ Kills");
                if (CurrentTask == null || !CurrentTask.IsActive)
                {
                    break;
                }
                if (Zones.GetZone(Player.Character.Position) != TargetZone)
                {
                    ExternalZoneKills = Player.RelationshipManager.GangRelationships.GetReputation(TargetGang).MembersKilled - KilledMembersAtStart - InternalZoneKills;
                }
                else
                {
                    InternalZoneKills = Player.RelationshipManager.GangRelationships.GetReputation(TargetGang).MembersKilled - KilledMembersAtStart - ExternalZoneKills;
                }
                if (InternalZoneKills >= KillRequirement)
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
        private void GetTargetZone()
        {
            if (TargetGang == null)
            {
                return;
            }
            List<GangTerritory> totalTerritories = GangTerritories.GetGangTerritory(TargetGang.ID)?.Where(x=> x.Priority == 0).ToList();
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
            GameTimeToWaitBeforeComplications = RandomItems.GetRandomNumberInt(3000, 10000);
            HasAddedComplications = false;
            WillAddComplications = false;// RandomItems.RandomPercent(Settings.SettingsManager.TaskSettings.RivalGangHitComplicationsPercentage);
            GangReputation gr = Player.RelationshipManager.GangRelationships.GetReputation(TargetGang);
            KilledMembersAtStart = gr.MembersKilled;
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
