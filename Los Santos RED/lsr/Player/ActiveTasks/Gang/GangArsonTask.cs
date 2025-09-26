using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using System;
using System.ArrayExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Player.ActiveTasks
{
    public class GangArsonTask : GangTask, IPlayerTask
    {
        private IGangTerritories GangTerritories;
        private IZones Zones;
        private GangDen HiringGangDen;
        private GameLocation TorchLocation;
        private Gang EnemyGang;
        private Zone SelectedZone;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private PhoneContact PhoneContact;
        private bool hasExploded = false;
        private bool WillTorchEnemyTurf;
        private bool HasConditions => TorchLocation != null && HiringGangDen != null;


        public GangArsonTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, IGangTerritories gangTerritories, IZones zones) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            GangTerritories = gangTerritories;
            Zones = zones;
        }
        public override void Setup()
        {
            base.Setup();
            /*
            RepOnCompletion = 500;
            RepOnFail = -2000;
            DaysToComplete = 7;
            DebtOnFail = 0;*/
            DebugName = "Arson";
            hasExploded = false;
        }
        public override void Dispose()
        {
            if (TorchLocation != null) { TorchLocation.IsPlayerInterestedInLocation = false; }
        }
        public override void Start()
        {
            WillTorchEnemyTurf = RandomItems.RandomPercent(Settings.SettingsManager.TaskSettings.GangArsonEnemyTurfPercentage);

            if (PlayerTasks.CanStartNewTask(HiringContact?.Name))
            {
                GetTorchLocation();
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
                    GangTasks.SendGenericTooSoonMessage(PhoneContact);
                }
            }
        }
        protected override void Loop()
        {
            while (true)
            {
                CurrentTask = PlayerTasks.GetTask(HiringGang.ContactName);
                if (CurrentTask == null || !CurrentTask.IsActive)
                {
                    break;
                }
                bool isExplosion = NativeFunction.Natives.IS_EXPLOSION_IN_SPHERE<bool>(-1, TorchLocation.EntrancePosition.X, TorchLocation.EntrancePosition.Y, TorchLocation.EntrancePosition.Z, 50f) &&
                    Player.WeaponEquipment?.CurrentWeapon?.Hash == 615608432;
                if(isExplosion)
                {
                    hasExploded = true;
                }
                if (hasExploded)
                {
                    CurrentTask.OnReadyForPayment(false);
                    break;
                }
                GameFiber.Sleep(250);
            }
        }
        protected override void FinishTask()
        {
            if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
            {
                TorchLocation.IsPlayerInterestedInLocation = false;

                if (HiringGangDen.IsAvailableForPlayer) SendMoneyPickupMessage(HiringGang.DenName, HiringGangDen);
                else SetReadyToPickupDeadDrop();
            }
            else if (CurrentTask != null && !CurrentTask.IsActive)
            {
                Dispose();
            }
            else
            {
                Dispose();
            }

        }
        private void GetTorchLocation()
        {
            if (GangTerritories.GetGangTerritory(HiringGang.ID) == null)
            {
                return;
            }
            List<GangTerritory> hiringGangTerritories = GangTerritories.GetGangTerritory(HiringGang.ID);
            if (hiringGangTerritories == null || !hiringGangTerritories.Any())
            {
                return;
            }
            List<GameLocation> PossibleSpots = PlacesOfInterest.PossibleLocations.RacketeeringTaskLocations().Where(x => x.IsCorrectMap(World.IsMPMapLoaded) && x.IsSameState(Player.CurrentLocation?.CurrentZone?.GameState)).ToList();
            List<GameLocation> AvailableSpots = new List<GameLocation>();
            List<GangTerritory> availableTerritories = new List<GangTerritory>();
            EnemyGang = null;
            if (WillTorchEnemyTurf)
            {
                //availableTerritories = hiringGangTerritories.Where(zj => zj.Priority != 0).ToList();
                if (HiringGang.EnemyGangs != null && HiringGang.EnemyGangs.Any())
                {
                    EnemyGang = Gangs.GetGang(HiringGang.EnemyGangs.PickRandom());
                }
                if (EnemyGang == null)
                {
                    EnemyGang = Gangs.GetAllGangs().Where(x => x.ID.ToLower() != HiringGang.ID.ToLower()).PickRandom();
                }
                if (EnemyGang != null)
                {
                    availableTerritories = GangTerritories.GetGangTerritory(EnemyGang.ID).ToList();
                }
            }
            else
            {
                availableTerritories = hiringGangTerritories.ToList();
            }
            if (!availableTerritories.Any() && WillTorchEnemyTurf)//fallback to friendly turf 
            {
                availableTerritories = hiringGangTerritories.ToList();
                WillTorchEnemyTurf = false;
            }
            if (!availableTerritories.Any())
            {
                return;
            }
            GangTerritory selectedTerritory = availableTerritories.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (WillTorchEnemyTurf && EnemyGang == null)
            {
                EnemyGang = Zones.GetZone(availableTerritories.FirstOrDefault().ZoneInternalGameName).AssignedGang;
            }
            if (Zones.GetZone(selectedTerritory.ZoneInternalGameName) != null)
            {
                SelectedZone = Zones.GetZone(selectedTerritory.ZoneInternalGameName);
                foreach (GameLocation possibleSpot in PossibleSpots)
                {
                    Zone spotZone = Zones.GetZone(possibleSpot.EntrancePosition);
                    bool isNear = PlacesOfInterest.PossibleLocations.PoliceStations.Any(policeStation => possibleSpot.EntrancePosition.DistanceTo2D(policeStation.EntrancePosition) < 100f);
                    if (spotZone.InternalGameName == SelectedZone.InternalGameName && !isNear)// && !possibleSpot.HasVendor)
                    {
                        AvailableSpots.Add(possibleSpot);
                    }
                }
            }
            TorchLocation = AvailableSpots.PickRandom();
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
        }
        protected override void GetPayment()
        {
            PaymentAmount = RandomItems.GetRandomNumberInt(HiringGang.ArsonPaymentMin, HiringGang.ArsonPaymentMax).Round(500);
            if (PaymentAmount <= 0)
            {
                PaymentAmount = 100;
            }
            if (WillTorchEnemyTurf)
            {
                PaymentAmount *= 5;
            }
        }
        protected override void AddTask()
        {
            PlayerTasks.AddTask(HiringContact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
            TorchLocation.IsPlayerInterestedInLocation = true;
            CurrentTask = PlayerTasks.GetTask(HiringGang.ContactName);
            CurrentTask.FailOnStandardRespawn = true;
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies;
            if (WillTorchEnemyTurf)
            {
                Replies = new List<string>() {
                $"Set fire to {TorchLocation.Name}. Make them feel the heat of our dominance. ~g~${PaymentAmount}~s~ for your trouble.",
                $"Burn down {TorchLocation.Name} and show {EnemyGang.ColorPrefix}{EnemyGang.ShortName}~s~ they’re out of their league. Bigger payout this time: ~g~${PaymentAmount}~s~.",
                $"Torch {TorchLocation.Name} and let them know we run this city. Take ~g~${PaymentAmount}~s~ for your efforts.",
                $"Light up {TorchLocation.Name} and remind them that this is our territory. You’ll earn ~g~${PaymentAmount}~s~ for the job.",
                $"Turn {TorchLocation.Name} into ashes and show {EnemyGang.ColorPrefix}{EnemyGang.ShortName}~s~ who really calls the shots. Your reward: ~g~${PaymentAmount}~s~.",
                $"Set {TorchLocation.Name} ablaze and send a message that we’re in charge. Your cut: ~g~${PaymentAmount}~s~."
                };
            }
            else
            {
                Replies = new List<string>() {
                $"Burn down {TorchLocation.Name} and remind them they can’t ignore us. You’ll get ~g~${PaymentAmount}~s~ for your work, 10% cut like usual.",
                $"Set fire to {TorchLocation.Name} to show them we’re not messing around. Your reward: ~g~${PaymentAmount}~s~.",
                $"Torch {TorchLocation.Name} and let them feel the consequences of defying us. ~g~${PaymentAmount}~s~ for you.",
                $"Light {TorchLocation.Name} on fire and make them regret their unpaid debts. You’ll get ~g~${PaymentAmount}~s~ for your trouble.",
                $"Burn {TorchLocation.Name} to the ground and send a message that we’re not here to negotiate. Your cut: ~g~${PaymentAmount}~s~.",
                $"Set {TorchLocation.Name} on fire to remind them they can’t skip out on us. You’ll pocket ~g~${PaymentAmount}~s~ for the job."
                };
            }


            Player.CellPhone.AddPhoneResponse(HiringContact.Name, HiringContact.IconName, Replies.PickRandom());
        }
    }
}
