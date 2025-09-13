using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Animation;

namespace LosSantosRED.lsr.Player.ActiveTasks
{
    public class GunDropoffTask : IPlayerTask
    {
        private WeaponItem WeaponToDeliver;
        private List<WeaponItem> DeliveryWeapons = new List<WeaponItem>();
        private DeadDrop DeadDrop;
        private List<DeadDrop> DeadDrops = new List<DeadDrop>();
        private IModItems ModItems;
        private ITaskAssignable Player;
        private ITimeReportable Time;
        private IGangs Gangs;
        private PlayerTasks PlayerTasks;
        private IPlacesOfInterest PlacesOfInterest;
        private List<DeadDrop> ActiveDrops = new List<DeadDrop>();
        private ISettingsProvideable Settings;
        private IEntityProvideable World;
        private ICrimes Crimes;
        private IWeapons Weapons;
        private PlayerTask CurrentTask;
        private bool hasGottenInCar;
        private uint GameTimeGotInCar;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private bool hasSpawnedCar;
        private bool returnVehicleMessage;
        private Vehicle SpawnedVehicle = null;
        private VehicleExt SpawnedVehicleExt;
        private Rage.Object GunProp;
        private int NumDropoffs = 1;


        private GunStore PickUpStore;
        private bool HasDeadDropAndStore => DeadDrops != null && DeadDrops.Any() && PickUpStore != null;


        private GunDealerContact Contact;
        public int PaymentAmount { get; set; }
        public int RepOnCompletion { get; set; }
        public int DebtOnFail { get; set; }
        public int RepOnFail { get; set; }
        public int DaysToComplete { get; set; }
        public string DebugName { get; set; }

        private bool DropoffsCompleted => DeadDrops.All(x => x.InteractionComplete);
        private bool IsSpawnedVehicleDestroyed => !SpawnedVehicle.Exists() || SpawnedVehicle.Health <= 300 || SpawnedVehicle.EngineHealth <= 300;
        private bool IsSpawnedVehicleParkedAtDestination => SpawnedVehicle.Exists() && NativeHelper.IsNearby(EntryPoint.FocusCellX, EntryPoint.FocusCellY, PickUpStore.CellX, PickUpStore.CellY, 2) && !SpawnedVehicle.HasOccupants && SpawnedVehicle.DistanceTo2D(PickUpStore.EntrancePosition) <= 50f;
        private bool IsPlayerDrivingSpawnedVehicle => SpawnedVehicle.Exists() && SpawnedVehicle.Driver?.Handle == Player.Character.Handle;
        private bool IsPlayerFarAwayFromSpawnedVehicle => SpawnedVehicle.Exists() && SpawnedVehicle.DistanceTo2D(Player.Character) >= 850f;
        private bool IsPlayerNearbyPickupStore => NativeHelper.IsNearby(EntryPoint.FocusCellX, EntryPoint.FocusCellY, PickUpStore.CellX, PickUpStore.CellY, 5);
        public GunDropoffTask(ITaskAssignable player, ITimeReportable time, IGangs gangs, PlayerTasks playerTasks, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IModItems modItems, IWeapons weapons, GunDealerContact contact, int numDropoffs)
        {
            Player = player;
            Time = time;
            Gangs = gangs;
            PlayerTasks = playerTasks;
            PlacesOfInterest = placesOfInterest;
            ActiveDrops = activeDrops;
            Settings = settings;
            World = world;
            Crimes = crimes;
            ModItems = modItems;
            Weapons = weapons;
            Contact = contact;

            NumDropoffs = numDropoffs;
        }
        public void Setup()
        {
            RepOnCompletion = 2000;
            DebtOnFail = 0;
            RepOnFail = -500;
            DaysToComplete = 7;
            DebugName = "Gun Dropoff";
        }
        public void Dispose()
        {
            if (SpawnedVehicle.Exists())
            {
                SpawnedVehicleExt?.RemoveBlip();
                SpawnedVehicle.IsPersistent = false;
                SpawnedVehicle.Delete();
            }
            if (GunProp.Exists())
            {
                GunProp.Delete();
            }
            if (PickUpStore != null)
            {
                PickUpStore.IsPlayerInterestedInLocation = false;
            }

            foreach (DeadDrop dd in DeadDrops)
            {
                dd?.Reset();
                dd?.Deactivate(true);
            }
        }
        public void Start()
        {
            if (PlayerTasks.CanStartNewTask(Contact.Name))
            {
                GetDeadDrops();
                GetShop();
                if (HasDeadDropAndStore)
                {
                    GetPayment();
                    SendInitialInstructionsMessage();
                    //EntryPoint.WriteToConsoleTestLong($"Starting Underground Guns Dropoff Guns Task");
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
                    }, "GunDropoffFiber");
                }
                else
                {
                    SendTaskAbortMessage();
                }
            }
        }
        private void SetCompleted()
        {
            if (PickUpStore != null)
            {
                PickUpStore.IsPlayerInterestedInLocation = false;
            }

            foreach (DeadDrop dd in DeadDrops)
            {
                dd?.Reset();
                dd?.Deactivate(true);
            }
            PlayerTasks.CompleteTask(Contact, true);
            SendCompletedMessage();
        }
        private void SetFailed()
        {
            if (SpawnedVehicle.Exists())
            {
                SpawnedVehicleExt?.RemoveBlip();
                SpawnedVehicle.IsPersistent = false;
            }
            if (GunProp.Exists())
            {
                GunProp.Delete();
            }
            if (PickUpStore != null)
            {
                PickUpStore.IsPlayerInterestedInLocation = false;
            }
            foreach (DeadDrop dd in DeadDrops)
            {
                dd?.Reset();
                dd?.Deactivate(true);
            }
            PlayerTasks.CancelTask(Contact);
        }
        private void FinishTask()
        {
            if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
            {
                SetCompleted();
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
        private void Loop()
        {
            while (true)
            {
                if (CurrentTask == null || !CurrentTask.IsActive)
                {
                    //EntryPoint.WriteToConsoleTestLong($"Task Inactive for {StaticStrings.UndergroundGunsContactName}");
                    break;
                }
                else if (hasSpawnedCar && IsSpawnedVehicleDestroyed)
                {
                    //EntryPoint.WriteToConsoleTestLong($"Task Inactive for {StaticStrings.UndergroundGunsContactName}, the spawned vehicle was destroyed");
                    Game.DisplayHelp($"{Contact.Name} Vehicle Destroyed");
                    break;
                }
                else if (hasSpawnedCar && hasGottenInCar && IsPlayerFarAwayFromSpawnedVehicle)
                {
                    //EntryPoint.WriteToConsoleTestLong($"Task Inactive for {StaticStrings.UndergroundGunsContactName}, you ran away from the car");
                    break;
                }
                if (!hasSpawnedCar && IsPlayerNearbyPickupStore)
                {
                    hasSpawnedCar = SpawnVehicle(PickUpStore);
                }
                if (hasSpawnedCar && !hasGottenInCar && IsPlayerDrivingSpawnedVehicle)
                {
                    OnGotInCar();
                }
                else if (hasSpawnedCar && hasGottenInCar && Player.IsNotWanted && !DropoffsCompleted && DeadDrop.InteractionComplete && !DeadDrop.CheckIsNearby(EntryPoint.FocusCellX, EntryPoint.FocusCellY, 1))
                {
                    GetDelivery();
                }
                else if (!returnVehicleMessage && hasSpawnedCar && hasGottenInCar && Player.IsNotWanted && DropoffsCompleted && !DeadDrop.CheckIsNearby(EntryPoint.FocusCellX, EntryPoint.FocusCellY, 1))
                {
                    SendVanDropOffMessage();
                    returnVehicleMessage = true;
                }
                else if (returnVehicleMessage && hasSpawnedCar && hasGottenInCar && Player.IsNotWanted && DropoffsCompleted && IsSpawnedVehicleParkedAtDestination)
                {
                    OnCompletedAllDeliveries();
                    break;
                }
                if (WillAddComplications && hasSpawnedCar && hasGottenInCar && !HasAddedComplications && Game.GameTime - GameTimeGotInCar >= GameTimeToWaitBeforeComplications)
                {
                    AddComplications();
                }


                GameFiber.Sleep(1000);
            }
        }
        private void OnCompletedAllDeliveries()
        {
            if (SpawnedVehicle.Exists())
            {
                SpawnedVehicle.IsPersistent = false;
            }
            if(GunProp.Exists())
            {
                GunProp.IsPersistent = false;
            }
            CurrentTask.OnReadyForPayment(true);
            Player.LastFriendlyVehicle = null;
           // SpawnedVehicle.SetLock((VehicleLockStatus)10);
            //
            SpawnedVehicle.LockStatus = (VehicleLockStatus)10;
            //EntryPoint.WriteToConsoleTestLong($"You ARRIVED! so it is now ready for payment!, doors are locked!");

            Game.DisplayHelp($"You have arrived, leave the vehicle");

            //Game.DisplayHelp($"You have arrived, leave the vehicle");
        }
        private void OnGotInCar()
        {
            hasGottenInCar = true;
            GameTimeGotInCar = Game.GameTime;
            GetDelivery();
        }
        private void AddComplications()
        {
            GangReputation gr = Player.RelationshipManager.GangRelationships.GangReputations.Where(x => x.GangRelationship == GangRespect.Hostile).PickRandom();
            if (gr != null && gr.Gang != null && SpawnedVehicleExt != null)
            {
                SendGangSabotageMessage(gr.Gang);
                GameFiber.Sleep(RandomItems.GetRandomNumberInt(5000, 8000));
                SpawnedVehicleExt.CarPlate.IsWanted = true;
                SpawnedVehicleExt.OriginalLicensePlate.IsWanted = true;
                Player.AddCrime(Crimes.CrimeList?.FirstOrDefault(x => x.ID == "GrandTheftAuto"), false, Player.Character.Position, SpawnedVehicleExt, null, true, true, true, false);
                //EntryPoint.WriteToConsoleTestLong("GUNS CONTACT, COMPLICATIONS ADDED!");
            }
            HasAddedComplications = true;
        }
        private void AddTask()
        {
            PlayerTasks.AddTask(Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
            CurrentTask = PlayerTasks.GetTask(Contact.Name);
            hasGottenInCar = false;
            hasSpawnedCar = false;
            GameTimeGotInCar = 0;
            GameTimeToWaitBeforeComplications = RandomItems.GetRandomNumberInt(3000, 10000);
            HasAddedComplications = false;
            WillAddComplications = RandomItems.RandomPercent(Settings.SettingsManager.TaskSettings.UndergroundGunsGunDropoffComplicationsPercentage);
        }
        private void GetPayment()
        {
            PaymentAmount = RandomItems.GetRandomNumberInt(Settings.SettingsManager.TaskSettings.UndergroundGunsGunDropoffPaymentMin, Settings.SettingsManager.TaskSettings.UndergroundGunsGunDropoffPaymentMax).Round(500);
            PaymentAmount *= NumDropoffs;
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 500;
            }
        }
        private void GetShop()
        {
            PickUpStore = PlacesOfInterest.PossibleLocations.GunStores.Where(x => x.ContactName == Contact.Name && 
                                                                                  x.ParkingSpaces.Any() && 
                                                                                  x.IsCorrectMap(World.IsMPMapLoaded) && 
                                                                                  x.IsSameState(Player.CurrentLocation?.CurrentZone?.GameState)).PickRandom();
            if (PickUpStore != null)
            {
                PickUpStore.IsPlayerInterestedInLocation = true;
            }
        }
        private void GetDeadDrops()
        {
            for (int i = 0; i < NumDropoffs; i++)
            {
                DeadDrops.Add(PlacesOfInterest.GetUsableDeadDrop(World.IsMPMapLoaded, Player.CurrentLocation));
            }
        }
        private void GetDelivery()
        {
            DeadDrop = DeadDrops.Where(x => !x.InteractionComplete).PickRandom();

            WeaponToDeliver = DeliveryWeapons.PickRandom();
            DeadDrop.SetupDrop(WeaponToDeliver, true);
            DeadDrop.WeaponInformation = Weapons.GetWeapon(WeaponToDeliver.ModelItem?.ModelName);

            DeliveryWeapons.Remove(WeaponToDeliver);
            ActiveDrops.Add(DeadDrop);

            SendDropoffInstructionsMessage();
        }
        private bool SpawnVehicle(GunStore PickUpStore)
        {
            SpawnLocation SpawnLocation = new SpawnLocation(PickUpStore.EntrancePosition);
            SpawnPlace ParkingSpot = PickUpStore.ParkingSpaces.PickRandom();// TR NOTE REMOVED ENTITY CHECK
            //foreach (SpawnPlace sp in PickUpStore.ParkingSpaces)
            //{
            //    if (!Rage.World.GetEntities(sp.Position, 10f, GetEntitiesFlags.ConsiderAllVehicles).Any())
            //    {
            //        ParkingSpot = sp;
            //        break;
            //    }
            //}
            if (ParkingSpot == null)
            {
                return false;
            }
            SpawnLocation.StreetPosition = ParkingSpot.Position;
            SpawnLocation.Heading = ParkingSpot.Heading;
            if (SpawnLocation.StreetPosition != Vector3.Zero)
            {
                World.Vehicles.CleanupAmbient();
                SpawnedVehicle = new Vehicle("burrito3", SpawnLocation.StreetPosition, SpawnLocation.Heading);
                GameFiber.Yield();
                if (SpawnedVehicle.Exists())
                {
                    SpawnedVehicle.PrimaryColor = Color.Navy;
                    SpawnedVehicle.IsPersistent = true;
                    Player.LastFriendlyVehicle = null;
                    Player.LastFriendlyVehicle = SpawnedVehicle;
                    try
                    {
                        GunProp = new Rage.Object("gr_prop_gr_gunsmithsupl_03a", PickUpStore.EntrancePosition);
                    }
                    catch (Exception ex)
                    {
                        //EntryPoint.WriteToConsoleTestLong($"Error Spawning Model {ex.Message} {ex.StackTrace}");
                    }
                    if (GunProp.Exists())
                    {
                        GunProp.AttachTo(SpawnedVehicle, SpawnedVehicle.GetBoneIndex("chassis_dummy"), new Vector3(0f, -1f, -0.3f), new Rotator(0f, 0f, 0f));
                    }
                    VehicleExt SpawnedVehicleExt = new VehicleExt(SpawnedVehicle, Settings);
                    World.Vehicles.AddCivilian(SpawnedVehicleExt);
                    SpawnedVehicleExt.CanHavePlateRandomlyUpdated = false;
                    SpawnedVehicleExt.SetRandomPlate();
                    SpawnedVehicleExt.AddBlip();
                    SpawnedVehicleExt.SetRandomColor();
                    SpawnedVehicleExt.CanHaveRandomWeapons = false;

                    for (int i = 0; i < NumDropoffs; i++)
                    {
                        WeaponItem gun = ModItems.GetRandomWeapon(true);
                        DeliveryWeapons.Add(gun);
                        SpawnedVehicleExt.WeaponStorage.AddWeapon(Weapons,gun,1);
                    }

                    SendVehicleSpawnedMessage();
                    return true;
                }
            }
            return false;
        }
        private void SendVanDropOffMessage()
        {
            List<string> Replies = new List<string>() {
                                $"Take my van back to the shop to get your payment of ${PaymentAmount}.",
                                $"Now bring me my van, don't get lost",
                                $"Remember that is MY VAN you're in. Don't do anything stupid. Your payment of ${PaymentAmount} is waiting for you",
                                $"Drop the van off at the shop",
                                $"Take the van where it needs to go, then you'll get your ${PaymentAmount}",
                                $"Bring the van back to us. Don't take long.",  };
            Player.CellPhone.AddScheduledText(Contact, Replies.PickRandom(), 0, true);
        }
        private void SendDropoffInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                $"Drop off the {WeaponToDeliver.DisplayName} at {DeadDrop.FullStreetAddress}, its {DeadDrop.Description}.",
                $"Client is getting impatient. Get the {WeaponToDeliver.DisplayName} to {DeadDrop.Description}, address is {DeadDrop.FullStreetAddress}.",
                $"Make a drop off of the {WeaponToDeliver.DisplayName} to {DeadDrop.FullStreetAddress}, don't loiter.",
                    };
            Player.CellPhone.AddScheduledText(Contact, Replies.PickRandom(), 0, true);
        }
        private void SendVehicleSpawnedMessage()
        {
            List<string> PickupMessage = new List<string>() {
                        $"The Dark Blue ~p~Burrito Van~s~ is parked out front, plate number is {SpawnedVehicle.LicensePlate}. The \"tools\" are already in the trunk. Don't lose them.",
                        $"Got some chrome ready for you in the back of the Dark Blue ~p~Burrito Van~s~ out front. Got some impatient clients, so don't mess this up.",
                        $"The Dark Blue ~p~Burrito Van~s~ is out front, loaded up with the gear. Keys are in the visor, but the trunk’s where the action is.",
                        $"The guns are loaded in the Dark Blue ~p~Burrito Van~s~, plate number is {SpawnedVehicle.LicensePlate}. Keys should be in it.",
                        $"Take the Dark Blue ~p~Burrito Van~s~. It is already loaded and ready to go.",
                        };
            Player.CellPhone.AddScheduledText(Contact, PickupMessage.PickRandom(), 0, true);
        }
        private void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                $"Need you to pickup some guns from our shop on {PickUpStore.FullStreetAddress} and do some dropoffs. The weapons are loaded in the ~p~Burrito Van~s~. ${PaymentAmount} when you are done",
                $"Go get the van from {PickUpStore.FullStreetAddress}, got some people that are itchin for some guns. ${PaymentAmount} on completion",
                $"There is a van in front of {PickUpStore.FullStreetAddress}, go get it and drop off some \"tools\". Some sensitive stuff in the back, don't draw attention. Payment is ${PaymentAmount}",
                    };
            Player.CellPhone.AddPhoneResponse(Contact.Name, Replies.PickRandom());
        }
        private void SendCompletedMessage()
        {
            List<string> Replies = new List<string>() {
                        $"Seems like that thing we discussed is done? Sending you ${PaymentAmount}",
                        $"Word got around that you are done with that thing for us, sending your payment of ${PaymentAmount}",
                        $"Sending your payment of ${PaymentAmount}",
                        $"Sending ${PaymentAmount}",
                        $"Heard you were done. We owe you ${PaymentAmount}",
                        };
            Player.CellPhone.AddScheduledText(Contact, Replies.PickRandom(), 1, false);
        }
        private void SendTaskAbortMessage()
        {
            List<string> Replies = new List<string>() {
                    "Nothing yet, I'll let you know",
                    "I've got nothing for you yet",
                    "Give me a few days",
                    "Not a lot to be done right now",
                    "We will let you know when you can do something for us",
                    "Check back later.",
                    };
            Player.CellPhone.AddPhoneResponse(Contact.Name, Replies.PickRandom());
        }
        private void SendGangSabotageMessage(Gang gang)
        {
            List<string> Replies2 = new List<string>() {
                                        $"Seems the police might be looking for that van. Wonder who could have informed them.",
                                        $"You've got some balls, driving a hot van full of guns. I let the LSPD know, and they think so too.",

                                        $"Seems like a van was just called in as stolen, must be a coincidence.",
                                        $"LSPD gonna fuck you and your van full of guns up prick.",
                                        $"Cops already know about the van, good luck dickhead.",
                                        $"Trying to move some hot guns I see? LSPD will be thrilled to know.",

                                        $"Did you think you could keep this from us? Would be unfortunate to get caught with all those guns.",
                                        $"Enjoy your time in Bolingbroke SHU",
                                            };
            Player.CellPhone.AddScheduledText(gang.Contact, Replies2.PickRandom(), 0, true);
        }
    }
}
