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
    public class RivalGangHitTask : GangTask, IPlayerTask
    {
        private GangDen HiringGangDen;
        private Gang TargetGang;
        private int GameTimeToWaitBeforeComplications;
        private bool HasAddedComplications;
        private bool WillAddComplications;
        private int KilledMembersAtStart;
        public int KillRequirement { get; set; } = 1;
        private bool HasTargetGangAndHiringDen => TargetGang != null && HiringGangDen != null;

        public bool JoinGangOnComplete { get; set; } = false;

        public RivalGangHitTask(ITaskAssignable player, ITimeControllable time, IGangs gangs, IPlacesOfInterest placesOfInterest, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IWeapons weapons, INameProvideable names, IPedGroups pedGroups,
            IShopMenus shopMenus, IModItems modItems, PlayerTasks playerTasks, GangTasks gangTasks, PhoneContact hiringContact, Gang hiringGang, Gang targetGang, int killRequirement) : base(player, time, gangs, placesOfInterest, settings, world, crimes, weapons, names, pedGroups, shopMenus, modItems, playerTasks, gangTasks, hiringContact, hiringGang)
        {
            TargetGang = targetGang;
            KillRequirement = killRequirement;
        }
        public override void Setup()
        {
            RepOnCompletion = 2000;
            DebtOnFail = 0;
            RepOnFail = -500;
            DaysToComplete = 7;
            DebugName = "Rival Gang Hit";
        }
        public override void Dispose()
        {

        }
        public override void Start()
        {
            if (PlayerTasks.CanStartNewTask(HiringGang?.ContactName))
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
            while (true)
            {
                CurrentTask = PlayerTasks.GetTask(HiringContact?.Name);
                if (CurrentTask == null || !CurrentTask.IsActive)
                {
                    break;
                }
                if (Player.RelationshipManager.GangRelationships.GetReputation(TargetGang)?.MembersKilled >= KilledMembersAtStart + KillRequirement)
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
                //GameFiber.Sleep(RandomItems.GetRandomNumberInt(5000, 15000));
                SendMoneyPickupMessage();
            }
        }
        private void GetTargetGang()
        {
           // TargetGang = null;
            //if (HiringGang.EnemyGangs != null && HiringGang.EnemyGangs.Any())
            //{
            //    TargetGang = Gangs.GetGang(HiringGang.EnemyGangs.PickRandom());
            //}
            if (TargetGang == null)
            {
                TargetGang = Gangs.GetAllGangs().Where(x => x.ID != HiringGang.ID).PickRandom();
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
            PlayerTasks.AddTask(HiringGang.Contact, PaymentAmount, RepOnCompletion, DebtOnFail, RepOnFail, DaysToComplete, DebugName);
        }
        protected override void SendInitialInstructionsMessage()
        {
            List<string> Replies = new List<string>() {
                $"Those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ think they can fuck with me? Go give {KillRequirement} of those pricks a dirt nap. Once you are done come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. ${PaymentAmount} to you",
                $"{TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ decided to make some moves against us. Let them know we don't approve by sending {KillRequirement} of those assholes to the other side. When you are finished, get back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. I'll have ${PaymentAmount} waiting for you.",
                $"Go find {KillRequirement} of those {TargetGang.ColorPrefix}{TargetGang.ShortName}~s~ pricks. Make sure they won't ever talk to anyone again. Come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} for your payment of ${PaymentAmount}",
                    };
            Player.CellPhone.AddPhoneResponse(HiringGang.Contact.Name, HiringGang.Contact.IconName, Replies.PickRandom());
        }
        private void SendMoneyPickupMessage()
        {
            List<string> Replies = new List<string>() {
                                $"Seems like that thing we discussed is done? Come by the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} to collect the ${PaymentAmount}",
                                $"Word got around that you are done with that thing for us, Come back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} for your payment of ${PaymentAmount}",
                                $"Get back to the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress} for your payment of ${PaymentAmount}",
                                $"{HiringGangDen.FullStreetAddress} for ${PaymentAmount}",
                                $"Heard you were done, see you at the {HiringGang.DenName} on {HiringGangDen.FullStreetAddress}. We owe you ${PaymentAmount}",
                                };
            Player.CellPhone.AddScheduledText(HiringContact, Replies.PickRandom(), 1, false);
        }
    }
}
