using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using NAudio.Wave;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Player.ActiveTasks
{
    public class GangDeliveryTask : GangTask, IPlayerTask
    {
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private GangDen HiringGangDen;
        private ModItem ItemToDeliver;
        private int NumberOfItemsToDeliver;
        private string ModItemNameToDeliver;
        private List<MenuItem> HiddenItems;
        private bool HasDen => HiringGangDen != null;

        public GangDeliveryTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, string modItemNameToDeliver) : base(player, time, gangs, placesOfInterest, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            ModItemNameToDeliver = modItemNameToDeliver;
        }
        public override void Setup()
        {
            RepOnCompletion = 500;
            DebtOnFail = 0;
            RepOnFail = -1000;
            DaysToComplete = 4;
            DebugName = "Delivery for Gang";
        }
        public override void Dispose()
        {
            if (HiringGangDen != null)
            {
                HiringGangDen.ExpectedItem = null;
                HiringGangDen.ExpectedItemAmount = 0;
            }
        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringGang?.ContactName))
            {
                GetHiringDen();
                if (HasDen)
                {
                    GetRequiredPayment();
                    if (ItemToDeliver != null)
                    {
                        SendInitialInstructionsMessage();
                        AddTask();
                        WatchTaskLoop();
                    }
                    else
                    {
                        Game.DisplayHelp("Could not find item");
                    }
                }
                else
                {
                    GangTasks.SendGenericTooSoonMessage(HiringContact);
                }
            }
        }
        private void WatchTaskLoop()
        {
            GameFiber PayoffFiber = GameFiber.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        if (CurrentTask == null || !CurrentTask.IsActive)
                        {
                            HiringGangDen.Menu.Items.AddRange(HiddenItems);
                            break;
                        }
                        GameFiber.Sleep(1000);
                    }
                    Dispose();
                }
                catch (Exception ex)
                {
                    EntryPoint.WriteToConsole(ex.Message + " " + ex.StackTrace, 0);
                    EntryPoint.ModController.CrashUnload();
                }
            }, "PayoffFiber");
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID,World.IsMPMapLoaded, Player.CurrentLocation);
        }
        private void GetRequiredPayment()
        {
            int DeliveryPayment = RandomItems.GetRandomNumberInt(HiringGang.DeliveryPaymentMin, HiringGang.DeliveryPaymentMax).Round(100);
            ItemToDeliver = null;
            /*if(HiringGangDen.Menu.Items.Any(x => x.Purchaseable && x.ModItemName == ModItemNameToDeliver))
            {
                return;
            }*/
            if (ModItemNameToDeliver != "")
            {
                ItemToDeliver = ModItems.Get(ModItemNameToDeliver);
            }
            if(ItemToDeliver != null)
            {
                Tuple<int, int> Prices = ShopMenus.GetPrices(ItemToDeliver.Name);
                float MoreThaMax = (float)Prices.Item2 * 1.5f;
                NumberOfItemsToDeliver = (int)(DeliveryPayment / MoreThaMax);
                PaymentAmount = DeliveryPayment;
                //EntryPoint.WriteToConsoleTestLong($"GANG DELIVERY Item: {ItemToDeliver.Name} Number: {NumberOfItemsToDeliver} Lowest: {Prices.Item1}  Highest {Prices.Item2} Payment: {PaymentAmount}");
            }
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 500;
            }
            PaymentAmount = PaymentAmount.Round(10);
        }
        protected override void AddTask()
        {
            PlayerTasks.AddTask(HiringGang.Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
            HiringGangDen.ExpectedItem = ItemToDeliver;
            HiringGangDen.ExpectedItemAmount = NumberOfItemsToDeliver;

            HiddenItems = HiringGangDen.Menu.Items.Where(x => x.Purchaseable && x.ModItemName == ModItemNameToDeliver).ToList();
            HiringGangDen.Menu.Items.RemoveAll(x => x.Purchaseable && x.ModItemName == ModItemNameToDeliver);

            CurrentTask = PlayerTasks.GetTask(HiringGang.ContactName);
            CurrentTask.OnReadyForPayment(false);


            GameTimeToWaitBeforeComplications = RandomItems.GetRandomNumberInt(3000, 10000);
            HasAddedComplications = false;
            WillAddComplications = false;// RandomItems.RandomPercent(Settings.SettingsManager.TaskSettings.RivalGangHitComplicationsPercentage);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                    $"I want you to find us {NumberOfItemsToDeliver} {ItemToDeliver.MeasurementName}(s) of {ItemToDeliver.Name}. We need it quick. Bring it to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. ${PaymentAmount}",
                    $"Go get {NumberOfItemsToDeliver} {ItemToDeliver.MeasurementName}(s) of {ItemToDeliver.Name} from somewhere, I don't wanna know. Don't take too long. Bring it to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. ${PaymentAmount} to you when you drop it off",
                    $"We need you to find {NumberOfItemsToDeliver} {ItemToDeliver.MeasurementName}(s) of {ItemToDeliver.Name}. We can't wait all week. Take it to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. You'll get ${PaymentAmount} when I get my item.",
                    };
            Player.CellPhone.AddPhoneResponse(HiringContact?.Name, HiringContact?.IconName, Replies.PickRandom());
        }
    }
}
