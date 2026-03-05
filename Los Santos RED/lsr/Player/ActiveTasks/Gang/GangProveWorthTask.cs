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
    public class GangProveWorthTask : GangTask, IPlayerTask
    {
        private GangDen HiringGangDen;
        private Gang TargetGang;
        private GangDen TargetGangDen;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private int KilledMembersAtStart;
        private bool hasExploded = false;
        public int KillRequirement { get; set; } = 1;
        private bool HasTargetGangAndHiringDen => TargetGang != null && HiringGangDen != null && TargetGangDen != null;

        public bool JoinGangOnComplete { get; set; } = false;
        public GangProveWorthTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang) : base(player, time, gangs, placesOfInterest, activeDrops, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {

        }
        public override void Setup()
        {
            hasExploded = false; 
            RepOnCompletion = 2000;
            DebtOnFail = 0;
            RepOnFail = -50000;
            DaysToComplete = 7;
            DebugName = "Terrorize Rival Gang";
        }
        public override void Dispose()
        {

        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringContact?.Name))
            {
                GetTargetGang();
                GetHiringDen();
                if (HasTargetGangAndHiringDen)
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
            uint GameTimeLastCheckedStatus = 0;
            while (true)
            {
                bool isExplosion = NativeFunction.Natives.IS_EXPLOSION_IN_SPHERE<bool>(-1, TargetGangDen.EntrancePosition.X, TargetGangDen.EntrancePosition.Y, TargetGangDen.EntrancePosition.Z, 50f);
                if(isExplosion)
                {
                    hasExploded = true;
                }
                if (Game.GameTime - GameTimeLastCheckedStatus >= 1000)
                {
                    CurrentTask = PlayerTasks.GetTask(HiringGang.ContactName);
                    if (CurrentTask == null || !CurrentTask.IsActive)
                    {
                        break;
                    }
                    if (Player.RelationshipManager.GangRelationships.GetReputation(TargetGang)?.MembersKilled >= KilledMembersAtStart + KillRequirement && hasExploded)
                    {
                        CurrentTask.OnReadyForPayment(false);
                        break;
                    }
                    GameTimeLastCheckedStatus = Game.GameTime;
                }
                GameFiber.Yield();
            }
        }
        protected override void FinishTask()
        {
            if (CurrentTask != null && CurrentTask.IsActive && CurrentTask.IsReadyForPayment)
            {
                PlayerTasks.CompleteTask(HiringGang.Contact, true);
            }//nothing to dispose

        }
        private void GetTargetGang()
        {
            TargetGang = null;
            if (HiringGang.EnemyGangs != null && HiringGang.EnemyGangs.Any())
            {
                TargetGang = Gangs.GetGang(HiringGang.EnemyGangs.PickRandom());
            }
            if (TargetGang == null)
            {
                TargetGang = Gangs.GetAllGangs().Where(x => x.ID != HiringGang.ID).PickRandom();
            }
            if(TargetGang != null)
            {
                TargetGangDen = PlacesOfInterest.PossibleLocations.GangDens.FirstOrDefault(x => x.AssociatedGang?.ID == TargetGang?.ID && x.IsPrimaryGangDen && x.IsCorrectMap(World.IsMPMapLoaded) && x.IsSameState(Player.CurrentLocation?.CurrentZone?.GameState));
            }
        }
        private void GetHiringDen()
        {
            HiringGangDen = PlacesOfInterest.GetMainDen(HiringGang.ID, World.IsMPMapLoaded, Player.CurrentLocation);
        }
        protected override void GetPayment()
        {
            PaymentAmount = 0;
        }
        protected override void AddTask()
        {
            GameTimeToWaitBeforeComplications = RandomItems.GetRandomNumberInt(3000, 10000);
            HasAddedComplications = false;
            WillAddComplications = false;
            GangReputation gr = Player.RelationshipManager.GangRelationships.GetReputation(TargetGang);
            KilledMembersAtStart = gr.MembersKilled;
            PlayerTasks.AddTask(HiringGang.Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName, JoinGangOnComplete);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                $"Need to send a message to {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~. Get to the {TargetGang.DenName} on {TargetGangDen.FullStreetAddress} and send them an explosive surprise. Make sure its right outside the {TargetGang.DenName}. Be sure to also get rid of {KillRequirement} members for good measure.",
                    };
            Player.CellPhone.AddPhoneResponse(HiringGang.Contact.Name, HiringGang.Contact.IconName, Replies.PickRandom());
        }
    }
}
