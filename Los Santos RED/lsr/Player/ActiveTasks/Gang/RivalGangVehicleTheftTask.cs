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
    public class RivalGangVehicleTheftTask : GangTask, IPlayerTask
    {
        private GangDen HiringGangDen;
        private Gang TargetGang;
        private uint VehicleToStealHash;
        private string VehicleModelName;
        private string VehicleDisplayName;
        private bool HasTargetGangVehicleAndHiringDen => TargetGang != null && HiringGangDen != null && VehicleToStealHash != 0;
        private bool IsInStolenGangCar => Player.CurrentVehicle != null && Player.CurrentVehicle.Vehicle.Exists() && Player.CurrentVehicle.Vehicle.Model.Hash == VehicleToStealHash && Player.CurrentVehicle.WasModSpawned && Player.CurrentVehicle.AssociatedGang != null && Player.CurrentVehicle.AssociatedGang.ID == TargetGang.ID;
        public RivalGangVehicleTheftTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, Gang targetGang, string vehicleModelName, string vehicleDisplayName) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            TargetGang = targetGang;
            VehicleModelName = vehicleModelName;
            VehicleDisplayName = vehicleDisplayName;
        }
        public override void Setup()
        {
            base.Setup();/*
            RepOnCompletion = 1000;
            DebtOnFail = 0;
            RepOnFail = -500;
            DaysToComplete = 5;*/
            DebugName = "Auto Theft for Gang";
        }
        public override void Dispose()
        {

        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringContact?.Name))
            {
                GetStolenCarHash();
                GetHiringDen();
                if (HasTargetGangVehicleAndHiringDen)
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
                if (IsInStolenGangCar)
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
        private void GetStolenCarHash()
        {
            VehicleToStealHash = 0;
            VehicleToStealHash = Game.GetHashKey(VehicleModelName);
            EntryPoint.WriteToConsole($"VehicleSteal {VehicleDisplayName} {VehicleModelName} {VehicleToStealHash} {TargetGang?.ID}");
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
        }
        protected override void GetPayment()
        {
            PaymentAmount = RandomItems.GetRandomNumberInt(HiringGang.TheftPaymentMin, HiringGang.TheftPaymentMax).Round(500);
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 500;
            }
        }
        protected override void AddTask()
        {
            PlayerTasks.AddTask(HiringContact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                    $"Go steal a ~p~{VehicleDisplayName}~s~ from those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ assholes. Once you are done come back to {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. ${PaymentAmount} on completion",
                    $"Go get me a ~p~{VehicleDisplayName}~s~ with {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ gang colors. Bring it back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. Payment ${PaymentAmount}",
                    };
            Player.CellPhone.AddPhoneResponse(HiringContact.Name, HiringContact.IconName, Replies.PickRandom());
        }
    }
}
