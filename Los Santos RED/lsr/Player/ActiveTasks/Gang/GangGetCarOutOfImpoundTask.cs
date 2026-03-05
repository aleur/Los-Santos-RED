using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Player.ActiveTasks
{
    public class GangGetCarOutOfImpoundTask : GangTask, IPlayerTask
    {
        private bool SpawnedVehicle = false;
        private GangDen HiringGangDen;
        private PoliceStation ImpoundLocation;
        private VehicleExt ImpoundedVehicle;
        private bool HasEnteredVehicle;
        private int startingHealth;
        private float startingEngineHealth;
        private uint GameTimeShownHealthWarning;

        private bool HasTaskData => HiringGangDen != null && ImpoundLocation != null && SpawnedVehicle;
        public GangGetCarOutOfImpoundTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {

        }
        public override void Setup()
        {
            base.Setup();
            DebugName = "Lockup Theft";
        }
        public override void Dispose()
        {
            if (ImpoundLocation != null)
            {
                ImpoundLocation.IsPlayerInterestedInLocation = false;
            }
            if (ImpoundedVehicle == null || !ImpoundedVehicle.Vehicle.Exists())
            {
                return;
            }
            ImpoundedVehicle.WasSpawnedEmpty = false;
            ImpoundedVehicle.IsManualCleanup = false;
            ImpoundedVehicle.Vehicle.IsPersistent = false;
        }
        public override void Start()
        {
            if(!PlayerTasks.CanStartNewTask(HiringContact?.Name))
            {
                GangTasks.SendGenericTooSoonMessage(HiringContact);
                return;
            }
            GetTaskLocations();
            if (!HasTaskData)
            {
                Game.DisplayHelp($"Error Setting Up Task for {HiringContact?.Name}.");
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
            }, "PayoffFiber");       
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
                if(ImpoundedVehicle == null || !ImpoundedVehicle.Vehicle.Exists())
                {
                    EntryPoint.WriteToConsole("Impound Theft Task: Vehicle Does Not Exist.");
                    break;
                }
                if (ImpoundedVehicle.Vehicle.Speed <= 0.5f && ImpoundedVehicle.Vehicle.DistanceTo2D(HiringGangDen.EntrancePosition) <= 70f && HiringGangDen.DistanceToPlayer <= 70f)//Player.RelationshipManager.GangRelationships.GetReputation(TargetGang)?.MembersKilled >= KilledMembersAtStart + KillRequirement && hasExploded)
                {
                    if(CheckVehicleHealth())
                    {
                        CurrentTask.OnReadyForPayment(true);
                        break;
                    }
                    else if (Game.GameTime - GameTimeShownHealthWarning >= 30000)
                    {
                        Game.DisplayHelp("The vehicle is ~r~damaged~s~!~n~Get it fixed and return!");
                        GameTimeShownHealthWarning = Game.GameTime;
                    }
                }
                if(!HasEnteredVehicle && ImpoundedVehicle != null && ImpoundedVehicle.Vehicle.Exists() && Player.CurrentVehicle != null && Player.CurrentVehicle.Handle == ImpoundedVehicle.Handle)
                { 
                    OnEnteredImpoundedVehicle();
                }
                GameFiber.Sleep(1000);
            }
        }
        private bool CheckVehicleHealth()
        {
            if(ImpoundedVehicle == null || !ImpoundedVehicle.Vehicle.Exists())
            {
                return false;
            }
            if(ImpoundedVehicle.Vehicle.Health <= startingHealth - 200)
            {
                return false;
            }
            if (ImpoundedVehicle.Vehicle.EngineHealth <= startingEngineHealth - 200)
            {
                return false;
            }
            return true;
        }
        private void OnEnteredImpoundedVehicle()
        {
            HasEnteredVehicle = true;
            SendReturnToBaseMessage();
        }
        protected override void FinishTask()
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
        protected override void SetReadyToPickupMoney()
        {
            OnCompletedOrFailed();
        }
        protected override void SetFailed()
        {
            OnCompletedOrFailed();
            GangTasks.SendGenericFailMessage(HiringContact);
            PlayerTasks.FailTask(HiringContact);
        }
        private void OnCompletedOrFailed()
        {
            if (ImpoundLocation != null)
            {
                ImpoundLocation.IsPlayerInterestedInLocation = false;
            }
            if (ImpoundedVehicle == null || !ImpoundedVehicle.Vehicle.Exists())
            {
                return;
            }
            ImpoundedVehicle.WasSpawnedEmpty = false;
            ImpoundedVehicle.IsManualCleanup = false;
            ImpoundedVehicle.Vehicle.LockStatus = (VehicleLockStatus)10;
            //ImpoundedVehicle.Vehicle.IsPersistent = false;
        }
        private void GetTaskLocations()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
            ImpoundLocation = PlacesOfInterest.PossibleLocations.PoliceStations.Where(x => x.HasImpoundLot && x.IsCorrectMap(World.IsMPMapLoaded) && x.IsSameState(Player.CurrentLocation?.CurrentZone?.GameState)).PickRandom();
            if(SpawnCar())
            {
                SpawnedVehicle = true;
            }
        }
        protected override void GetPayment()
        {
            PaymentAmount = RandomItems.GetRandomNumberInt(HiringGang.ImpoundTheftPaymentMin, HiringGang.ImpoundTheftPaymentMax).Round(500);
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 500;
            }
        }
        protected override void AddTask()
        {
            if (ImpoundLocation != null)
            {
                ImpoundLocation.IsPlayerInterestedInLocation = true;
            }
            PlayerTasks.AddTask(HiringGang.Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                $"Need you to get my {ImpoundedVehicle.FullName(false)} Plate # {ImpoundedVehicle.CarPlate.PlateNumber} out of the impound lot at " +
                $"~p~{ImpoundLocation.Name}~s~ and bring it back to {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} " +
                $"for your payment of ${PaymentAmount}. " +
                $"Better be in great shape.",
                    };
            Player.CellPhone.AddPhoneResponse(HiringContact?.Name, HiringContact?.IconName, Replies.PickRandom());
        }
        private void SendReturnToBaseMessage()
        {
            List<string> Replies = new List<string>() {
                $"Heard that thing was moving along. Bring it back to {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} and make sure it is pristine!"
                    };
            Player.CellPhone.AddScheduledText(HiringContact, Replies.PickRandom(),1, false);
        }
        private bool SpawnCar()
        {
            DispatchableVehicle dispatchableVehicle = HiringGang.GetRandomVehicle(0, false, false, true, "", Settings);
            EntryPoint.WriteToConsole($"dispatchableVehicle: {dispatchableVehicle}");
            if(dispatchableVehicle == null || ImpoundLocation == null)
            {
                return false;
            }
            SpawnPlace existingSpawn = ImpoundLocation.VehicleImpoundLot.ParkingSpots.PickRandom();
            if (existingSpawn == null)
            {
                return false;
            }
            SpawnLocation toSpawn = new SpawnLocation(existingSpawn.Position);
            toSpawn.Heading = existingSpawn.Heading;
            GangSpawnTask gmSpawn = new GangSpawnTask(HiringGang, toSpawn, dispatchableVehicle, null, false, Settings, Weapons, Names, false, Crimes, PedGroups, ShopMenus, World, ModItems, false, false, false);
            gmSpawn.AllowAnySpawn = true;
            gmSpawn.AllowBuddySpawn = false;
            gmSpawn.AddEmptyVehicleBlip = true;
            gmSpawn.AttemptSpawn();
            gmSpawn.CreatedVehicles.ForEach(x => x.AddVehicleToList(World));//World.Vehicles.AddEntity(x, ResponseType.None));
            ImpoundedVehicle = gmSpawn.CreatedVehicles.FirstOrDefault();
            if(ImpoundedVehicle == null || !ImpoundedVehicle.Vehicle.Exists())
            {
                return false;
            }
            ImpoundedVehicle.Vehicle.IsPersistent = true;
            ImpoundedVehicle.SetRandomPlate();
            ImpoundedVehicle.WasModSpawned = true;
            ImpoundedVehicle.WasSpawnedEmpty = true;
            ImpoundedVehicle.IsManualCleanup = true;
            ImpoundedVehicle.IsAlwaysOpenForPlayer = true;
            startingHealth = ImpoundedVehicle.Vehicle.Health;
            startingEngineHealth = ImpoundedVehicle.Vehicle.EngineHealth;
            if(!ImpoundLocation.VehicleImpoundLot.ImpoundVehicle(ImpoundedVehicle, Time, false, Weapons))
            {
                return false;
            }
            return ImpoundedVehicle != null && ImpoundedVehicle.Vehicle.Exists();
        }


    }
}
