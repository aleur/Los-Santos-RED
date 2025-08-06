﻿using ExtensionsMethods;
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
    public class GangCopHitTask : GangTask, IPlayerTask
    {
        private IAgencies Agencies;
        private GangDen HiringGangDen;
        private Agency TargetAgency;
        private int KilledMembersAtStart;
        public int KillRequirement { get; set; } = 1;
        private bool HasTargetAgencyAndDen => TargetAgency != null && HiringGangDen != null;

        public GangCopHitTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world,
    ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups, IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, 
    Gang hiringGang, Agency targetAgency, IAgencies agencies, int killRequirement) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            TargetAgency = targetAgency;
            Agencies = agencies;
            KillRequirement = killRequirement;
        }

        public override void Setup()
        {
            RepOnCompletion = 2000;
            DebtOnFail = 0;
            RepOnFail = -500;
            DaysToComplete = 7;
            DebugName = "Cop Hit";
        }
        public override void Dispose()
        {

        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringGang?.ContactName))
            {
                GetTargetAgency();
                GetHiringDen();
                if (HasTargetAgencyAndDen)
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
                if (CurrentTask == null || !CurrentTask.IsActive)
                {
                    break;
                }
                int killedCops = Player.Violations.DamageViolations.CountKilledCopsByAgency(TargetAgency.ID);
                if (killedCops >= KilledMembersAtStart + KillRequirement)
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
        private void GetTargetAgency()
        {
            if (TargetAgency == null)
            {
                TargetAgency = Agencies.GetAgenciesByResponse(ResponseType.LawEnforcement).PickRandom();// Gangs.GetAllGangs().Where(x => x.ID != HiringGang.ID).PickRandom();
            }
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
        }
        protected override void GetPayment()
        {
            PaymentAmount = RandomItems.GetRandomNumberInt(HiringGang.CopHitPaymentMin, HiringGang.CopHitPaymentMax).Round(500);
            PaymentAmount *= KillRequirement;
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 500;
            }
        }
        protected override void AddTask()
        {
            KilledMembersAtStart = Player.Violations.DamageViolations.CountKilledCopsByAgency(TargetAgency.ID);
            EntryPoint.WriteToConsole($"Starting Gang Cop Hit, KilledMembersAtStart {KilledMembersAtStart}");
            PlayerTasks.AddTask(HiringGang.Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                $"The pigs at {TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ have fucked with us for the last time. Be sure to waste {KillRequirement} of those pricks. Once you are done come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. ${PaymentAmount} to you",
                $"{TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ is starting to become an issue. Get rid of {KillRequirement} of those assholes. When you are finished, get back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. I'll have ${PaymentAmount} waiting for you.",
                $"The pigs got their noses in our business, need you to waste {KillRequirement} of those {TargetAgency.ColorPrefix}{TargetAgency.ShortName}~s~ pricks. Come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} for your payment of ${PaymentAmount}",
                    };
            Player.CellPhone.AddPhoneResponse(HiringGang.Contact.Name, HiringGang.Contact.IconName, Replies.PickRandom());
        }
    }
}
