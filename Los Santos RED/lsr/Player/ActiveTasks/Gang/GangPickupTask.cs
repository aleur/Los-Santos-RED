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
    public class GangPickupTask : GangTask, IPlayerTask
    {
        private DeadDrop DeadDrop;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private int MoneyToPickup;
        private GangDen HiringGangDen;

        private bool HasDeadDropAndDen => DeadDrop != null && HiringGangDen != null;

        public GangPickupTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {

        }
        public override void Setup()
        {
            RepOnCompletion = 500;
            RepOnFail = -1000;
            DaysToComplete = 2;
            DebugName = "Pickup for Gang";
        }
        public override void Dispose()
        {
            if (HiringGangDen != null) HiringGangDen.ExpectedMoney = 0;
            DeadDrop?.Reset();
            DeadDrop?.Deactivate(true);
        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringContact?.Name))
            {
                GetDeadDrop();
                GetHiringDen();
                if (HasDeadDropAndDen)
                {
                    GetRequiredPayment();
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
                    //EntryPoint.WriteToConsoleTestLong($"Task Inactive for {HiringGang.ContactName}");
                    break;
                }
                if (DeadDrop.InteractionComplete)
                {
                    DeadDrop.CheckIsNearby(EntryPoint.FocusCellX, EntryPoint.FocusCellY, 5);
                }
                if (DeadDrop.InteractionComplete && !DeadDrop.IsNearby)
                {
                    CurrentTask.OnReadyForPayment(false);
                    break;
                }
                GameFiber.Sleep(1000);
            }
        }
        protected override void FinishTask()
        {
            if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
            {
                DeadDrop?.Reset();
                DeadDrop?.Deactivate(true);
                SendMoneyDropOffMessage();
            }
            else if (CurrentTask != null && !CurrentTask.IsActive)
            {
                Dispose();
            }
            else
            {
                Dispose();//the failing messages are handled above if they cancel
            }
        }
        private void GetDeadDrop()
        {
            DeadDrop = PlacesOfInterest.GetUsableDeadDrop(World.IsMPMapLoaded, Player.CurrentLocation);
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
        }
        private void GetRequiredPayment()
        {
            int Payment = RandomItems.GetRandomNumberInt(HiringGang.PickupPaymentMin, HiringGang.PickupPaymentMax).Round(100);
            MoneyToPickup = Payment * 10;
            float TenPercent = (float)MoneyToPickup / 10;
            Payment = (int)TenPercent;
            if (Payment <= 0)
            {
                Payment = 500;
            }
            PaymentAmount = Payment.Round(10);
        }
        protected override void AddTask()
        {
            DebtOnFail = -1 * MoneyToPickup;
            PlayerTasks.AddTask(HiringGang.Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
            DeadDrop.SetupDrop(MoneyToPickup, false);
            ActiveDrops.Add(DeadDrop);
            HiringGangDen.ExpectedMoney = MoneyToPickup;
            GameTimeToWaitBeforeComplications = RandomItems.GetRandomNumberInt(3000, 10000);
            HasAddedComplications = false;
            WillAddComplications = false;// RandomItems.RandomPercent(Settings.SettingsManager.TaskSettings.RivalGangHitComplicationsPercentage);
        }
        private void SendMoneyDropOffMessage()
        {
            List<string> Replies = new List<string>() {
                                "Take the money to the designated place.",
                                "Now bring me the money, don't get lost",
                                "Remeber that is MY MONEY you are just holding it. Drop it off where we agreed.",
                                "Drop the money off at the designated place",
                                "Take the money where it needs to go",
                                "Bring the stuff back to us. Don't take long.",  };
            Player.CellPhone.AddScheduledText(HiringContact, Replies.PickRandom(), 0, true);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                    $"Pickup ${MoneyToPickup} from {DeadDrop.FullStreetAddress}, its {DeadDrop.Description}. Bring it to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. You get 10% on completion",
                    $"Go get ${MoneyToPickup} from {DeadDrop.Description}, address is {DeadDrop.FullStreetAddress}. Bring it to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. 10% to you when you drop it off",
                    $"Make a pickup of ${MoneyToPickup} from {DeadDrop.Description} on {DeadDrop.FullStreetAddress}. Take it to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. You'll get 10% when I get my money.",
                    };
            Player.CellPhone.AddPhoneResponse(HiringContact?.Name, HiringContact?.IconName, Replies.PickRandom());
        }
    }
}
