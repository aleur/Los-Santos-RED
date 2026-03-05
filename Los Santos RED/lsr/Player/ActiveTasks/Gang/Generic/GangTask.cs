using ExtensionsMethods;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Player.ActiveTasks
{
    public class GangTask : IPlayerTask
    {
        protected ITaskAssignable Player;
        protected List<DeadDrop> ActiveDrops = new List<DeadDrop>();
        protected DeadDrop DeadDropPayment;
        protected ITimeControllable Time;
        protected IGangs Gangs;
        protected IPlacesOfInterest PlacesOfInterest;
        protected ISettingsProvideable Settings;
        protected IEntityProvideable World;
        protected ICrimes Crimes; 
        protected IWeapons Weapons;
        protected INameProvideable Names;
        protected IPedGroups PedGroups;
        protected IShopMenus ShopMenus;
        protected IModItems ModItems;

        protected GangTasks GangTasks;
        protected PlayerTask CurrentTask;
        protected PlayerTasks PlayerTasks;

        protected Gang HiringGang;
        protected PhoneContact HiringContact;
        public Zone TargetZone { get; set; }
        public int PaymentAmount { get; set; }
        public int RepOnCompletion { get; set; }
        public int DebtOnFail { get; set; }
        public int RepOnFail { get; set; }
        public int DaysToComplete { get; set; }
        public string DebugName { get; set; }

        public GangTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world,ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups, 
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang)
        {
            Player = player;

            Time = time;
            Gangs = gangs;  
            PlacesOfInterest = placesOfInterest;
            ActiveDrops = activeDrops;
            Settings = settings;
            World = world;
            Crimes = crimes;
            Weapons = weapons;
            Names = names;
            PedGroups = pedGroups;
            ShopMenus = shopMenus;
            ModItems = modItems;


            PlayerTasks = playerTasks;
            GangTasks = gangTasks;
            HiringContact = hiringContact;
            HiringGang = hiringGang;
        }
        public virtual void Setup()
        {
            RepOnCompletion = Settings.SettingsManager.TaskSettings.GangRepOnCompletingTask;
            RepOnFail = Settings.SettingsManager.TaskSettings.GangRepOnFailingTask;
            DebtOnFail = 0;
            DaysToComplete = Settings.SettingsManager.TaskSettings.GangDaysToCompleteTask;
        }
        public virtual void Dispose()
        {

        }
        public virtual void Start()
        {
            if(!CanStartNewTask())
            {
                GangTasks.SendGenericTooSoonMessage(HiringContact);
                return;
            }
            if (!GetTaskData())
            {
                Game.DisplayHelp($"Error Setting Up Task for {HiringContact.Name}.");
                return;
            }
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
            }, "GangTaskLoop");
        }


        protected virtual bool CanStartNewTask()
        {
            if (!PlayerTasks.CanStartNewTask(HiringContact?.Name))
            {
                return false;
            }
            return true;
        }
        protected virtual bool GetTaskData()
        {
            //Get Dens, Locations, Spawn Vehicles, Etc.
            return true;
        }

        protected virtual void GetPayment()
        {
            //Set the money to receive and stuffo

        }
        protected virtual void SendInitialInstructionsMessage()
        {

        }
        protected virtual void AddTask()
        {
            PlayerTasks.AddTask(HiringContact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName, false);
        }
        protected virtual void Loop()
        {

        }

        protected virtual void FinishTask()
        {
            if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
            {
                SetReadyToPickupMoney();
            }
            else if (CurrentTask != null && CurrentTask.IsActive)
            {
                SetFailed();
            }
            else
            {
                Dispose();
            }
        }
        protected virtual void SetReadyToPickupMoney()
        {
            OnTaskCompletedOrFailed();
        }
        protected void SetReadyToPickupDeadDrop()
        {
            DeadDropPayment = PlacesOfInterest.GetUsableDeadDrop(World.IsMPMapLoaded, Player.CurrentLocation);
            if (DeadDropPayment != null)
            {
                EntryPoint.WriteToConsole($"Setup DeadDrop with ${PaymentAmount}");
                DeadDropPayment.SetupDrop(0, false); // Set to zero because PlayerTask pays out automatically
                ActiveDrops.Add(DeadDropPayment);
                SendDeadDropStartMessage();
                while (true)
                {
                    if (CurrentTask == null || !CurrentTask.IsActive)
                    {
                        break;
                    }
                    if (DeadDropPayment.InteractionComplete)
                    {
                        Game.DisplayHelp($"{HiringContact.Name} Money Picked Up");
                        break;
                    }
                    GameFiber.Sleep(1000);
                }
                if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
                {
                    PlayerTasks.CompleteTask(HiringContact, true);
                }
                DeadDropPayment?.Reset();
                DeadDropPayment?.Deactivate(true);
            }
            else
            {
                PlayerTasks.CompleteTask(HiringContact, true);
                SendQuickPaymentMessage();
            }
        }
        protected virtual void SetFailed()
        {
            OnTaskCompletedOrFailed();
            GangTasks.SendGenericFailMessage(HiringContact);
            PlayerTasks.FailTask(HiringContact);
        }
        protected virtual void OnTaskCompletedOrFailed()
        {

        }
        protected virtual void SendMoneyPickupMessage(string placetypeName, GameLocation gameLocation)
        {
            List<string> Replies = new List<string>() {
                                $"Seems like that thing we discussed is done? Come by the {placetypeName} on {gameLocation.FullStreetAddress} to collect the ${PaymentAmount}",
                                $"Word got around that you are done with that thing for us, Come back to the {placetypeName} on {gameLocation.FullStreetAddress} for your payment of ${PaymentAmount}",
                                $"Get back to the {placetypeName} on {gameLocation.FullStreetAddress} for your payment of ${PaymentAmount}",
                                $"{gameLocation.FullStreetAddress} for ${PaymentAmount}",
                                $"Heard you were done, see you at the {placetypeName} on {gameLocation.FullStreetAddress}. We owe you ${PaymentAmount}",
                                };
            Player.CellPhone.AddScheduledText(HiringContact, Replies.PickRandom(), 1, false);
        }
        private void SendQuickPaymentMessage()
        {
            List<string> Replies = new List<string>() {
                            $"Seems like that thing we discussed is done? Sending you ${PaymentAmount}",
                            $"Word got around that you are done with that thing for us, sending your payment of ${PaymentAmount}",
                            $"Sending your payment of ${PaymentAmount}",
                            $"Sending ${PaymentAmount}",
                            $"Heard you were done. We owe you ${PaymentAmount}",
                            };
            Player.CellPhone.AddScheduledText(HiringContact, Replies.PickRandom(), 1, false);
        }
        private void SendDeadDropStartMessage()
        {
            List<string> Replies = new List<string>() {
                            $"Pickup your payment of ${PaymentAmount} from {DeadDropPayment.FullStreetAddress}, its {DeadDropPayment.Description}.",
                            $"Go get your payment of ${PaymentAmount} from {DeadDropPayment.Description}, address is {DeadDropPayment.FullStreetAddress}.",
                            };

            Player.CellPhone.AddScheduledText(HiringContact, Replies.PickRandom(), 1, false);
        }
    }
}

