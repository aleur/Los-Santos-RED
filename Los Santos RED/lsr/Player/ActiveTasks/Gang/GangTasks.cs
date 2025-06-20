using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player.ActiveTasks;
using Rage;
using RAGENativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GangTasks : IPlayerTaskGroup
{

    private ITaskAssignable Player;
    private ITimeControllable Time;
    private IGangTerritories GangTerritories;
    private IGangs Gangs; 
    private IZones Zones;
    private PlayerTasks PlayerTasks;
    private IPlacesOfInterest PlacesOfInterest;
    private List<DeadDrop> ActiveDrops = new List<DeadDrop>();
    private ISettingsProvideable Settings;
    private IEntityProvideable World;
    private ICrimes Crimes;
    private IModItems ModItems;
    private IShopMenus ShopMenus;
    private INameProvideable Names;
    private IWeapons Weapons;
    private IPedGroups PedGroups;
    private IAgencies Agencies;

    private List<RivalGangAmbushTask> RivalGangAmbush = new List<RivalGangAmbushTask>();
    private List<RivalGangHitTask> RivalGangHits = new List<RivalGangHitTask>();
    private List<PayoffGangTask> PayoffGangTasks = new List<PayoffGangTask>();
    private List<RivalGangVehicleTheftTask> RivalGangTheftTasks = new List<RivalGangVehicleTheftTask>();
    private List<GangRacketeeringTask> GangRacketeeringTasks = new List<GangRacketeeringTask>();
    private List<GangBriberyTask> GangBriberyTasks = new List<GangBriberyTask>();
    private List<GangPickupTask> GangPickupTasks = new List<GangPickupTask>();
    private List<GangArsonTask> GangArsonTasks = new List<GangArsonTask>();
    private List<GangDeliveryTask> GangDeliveryTasks = new List<GangDeliveryTask>();
    private List<GangWheelmanTask> GangWheelmanTasks = new List<GangWheelmanTask>();
    //private List<GangPizzaDeliveryTask> GangPizzaDeliveryTasks = new List<GangPizzaDeliveryTask>();
    private List<GangProveWorthTask> GangProveWorthTasks = new List<GangProveWorthTask>();
    private List<GangGetCarOutOfImpoundTask> GangGetCarOutOfImpoundTasks = new List<GangGetCarOutOfImpoundTask>();


    private List<GangTask> AllGenericGangTasks = new List<GangTask>();

    public GangTasks(ITaskAssignable player, ITimeControllable time, IGangs gangs, PlayerTasks playerTasks, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IModItems modItems, IShopMenus shopMenus, IWeapons weapons, INameProvideable names, IPedGroups pedGroups, IAgencies agencies, IGangTerritories gangTerritories, IZones zones)
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
        ShopMenus = shopMenus;
        Names = names;
        Weapons = weapons;
        PedGroups = pedGroups;
        Agencies = agencies;
        Agencies = agencies;
        GangTerritories = gangTerritories;
        Zones = zones;
    }
    public void Setup()
    {

    }
    public void Dispose()
    {
        RivalGangAmbush.ForEach(x => x.Dispose());
        RivalGangHits.ForEach(x=> x.Dispose());
        PayoffGangTasks.ForEach(x => x.Dispose());
        RivalGangTheftTasks.ForEach(x => x.Dispose());
        GangBriberyTasks.ForEach(x => x.Dispose());
        GangArsonTasks.ForEach(x => x.Dispose());
        GangRacketeeringTasks.ForEach(x => x.Dispose());
        GangPickupTasks.ForEach(x => x.Dispose());
        GangDeliveryTasks.ForEach(x => x.Dispose());
        GangWheelmanTasks.ForEach(x => x.Dispose());
        //GangPizzaDeliveryTasks.ForEach(x => x.Dispose());
        GangProveWorthTasks.ForEach(x => x.Dispose());
        GangGetCarOutOfImpoundTasks.ForEach(x => x.Dispose());


        AllGenericGangTasks.ForEach(x => x.Dispose());

        RivalGangAmbush.Clear();
        RivalGangHits.Clear();
        PayoffGangTasks.Clear();
        RivalGangTheftTasks.Clear();
        GangBriberyTasks.Clear();
        GangRacketeeringTasks.Clear();
        GangArsonTasks.Clear();
        GangPickupTasks.Clear();
        GangDeliveryTasks.Clear();
        GangWheelmanTasks.Clear();
        //GangPizzaDeliveryTasks.Clear();
        GangProveWorthTasks.Clear();
        GangGetCarOutOfImpoundTasks.Clear();

        AllGenericGangTasks.Clear();
    }
    public GangTask RandomTask(Gang gang, GangContact gangContact)
    {
        int RandomKillRequirement = new Random().Next(1, 5);
        List<Func<GangTask>> options = new List<Func<GangTask>>{
            () => GangHit(gang, RandomKillRequirement, gangContact, Gangs.GetGang(gang.EnemyGangs.PickRandom())),
            () => GangAmbush(gang, RandomKillRequirement, gangContact, Gangs.GetGang(gang.EnemyGangs.PickRandom())),
            () => GangArson(gang, gangContact),
            () => GangBodyDisposal(gang, gangContact),
            () => GangBribery(gang, gangContact),
            () => GangRacketeering(gang, gangContact),
            () => GangPickup(gang, gangContact),
            () => ImpoundTheft(gang, gangContact),
        };
        int rng = new Random().Next(options.Count);
        return options[rng]();
    }
    public GangTask GangProveWorth(Gang gang, int killRequirement, GangContact gangContact)
    {
        GangProveWorthTask newTask = new GangProveWorthTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, ActiveDrops);
        newTask.KillRequirement = killRequirement;
        newTask.JoinGangOnComplete = true;
        GangProveWorthTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask CopHit(Gang gang, int killRequirement, GangContact gangContact, Agency targetAgency)
    {
        GangCopHitTask newTask = new GangCopHitTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetAgency, Agencies, killRequirement);
        AllGenericGangTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangHit(Gang gang, int killRequirement, GangContact gangContact, Gang targetGang)
    {
        RivalGangHitTask newTask = new RivalGangHitTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetGang, killRequirement);
        AllGenericGangTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangAmbush(Gang gang, int killRequirement, GangContact gangContact, Gang targetGang)
    {
        RivalGangAmbushTask newTask = new RivalGangAmbushTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetGang, killRequirement, GangTerritories, Zones);
        RivalGangAmbush.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask PayoffGang(Gang gang, GangContact gangContact)
    {
        PayoffGangTask newTask = new PayoffGangTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, ActiveDrops);
        PayoffGangTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangVehicleTheft(Gang gang, GangContact gangContact, Gang targetGang, string vehicleModelName, string vehicleDisplayName)
    {
        RivalGangVehicleTheftTask newTask = new RivalGangVehicleTheftTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetGang, vehicleModelName, vehicleDisplayName);
        RivalGangTheftTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangBribery(Gang gang, GangContact gangContact)
    {
        GangBriberyTask newTask = new GangBriberyTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones);
        GangBriberyTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangArson(Gang gang, GangContact gangContact)
    {
        GangArsonTask newTask = new GangArsonTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones);
        GangArsonTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangRacketeering(Gang gang, GangContact gangContact)
    {
        GangRacketeeringTask newTask = new GangRacketeeringTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones);
        GangRacketeeringTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangPickup(Gang gang, GangContact gangContact)
    {
        GangPickupTask newTask = new GangPickupTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, ActiveDrops);
        GangPickupTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangDelivery(Gang gang, GangContact gangContact, string modItemName)
    {
        GangDeliveryTask newTask = new GangDeliveryTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, modItemName);
        GangDeliveryTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangWheelman(Gang gang, GangContact gangContact, int robbersToSpawn, string locationType, bool requireAllMembersToFinish)
    {
        GangWheelmanTask newTask = new GangWheelmanTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, robbersToSpawn, locationType, requireAllMembersToFinish);
        GangWheelmanTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }

    public GangTask ImpoundTheft(Gang gang, GangContact gangContact)
    {
        GangGetCarOutOfImpoundTask newTask = new GangGetCarOutOfImpoundTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang);
        GangGetCarOutOfImpoundTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }

    public GangTask GangBodyDisposal(Gang gang, GangContact gangContact)
    {
        GangBodyDisposalTask newTask = new GangBodyDisposalTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems,PlayerTasks,this, gangContact, gang);
        AllGenericGangTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }/*
    public void StartGangPizza(Gang gang, GangContact gangContact)
    {
        GangPizzaDeliveryTask newDelivery = new GangPizzaDeliveryTask(Player, Time, Gangs, PlayerTasks, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, ModItems, ShopMenus, gangContact, this);
        GangPizzaDeliveryTasks.Add(newDelivery);
        newDelivery.Setup();
        newDelivery.Start(gang);
    }*/

    public void StartDrugMeetTask(Gang gang, GangContact gangContact, ModItem modItem,int quantity, Gang meetingGang)
    {
        GangDrugMeetTask dugMeetTask = new GangDrugMeetTask(Player, Time, Gangs, PlacesOfInterest, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, 
            gangContact, gang, modItem, quantity, meetingGang);
        AllGenericGangTasks.Add(dugMeetTask);
        dugMeetTask.Setup();
        dugMeetTask.Start();
    }



    public string GetGeneircTaskAbortMessage()
    {
        List<string> Replies = new List<string>() {
                    "Nothing yet, I'll let you know",
                    "I've got nothing for you yet",
                    "Give me a few days",
                    "Not a lot to be done right now",
                    "We will let you know when you can do something for us",
                    "Check back later.",
                    };
        return Replies.PickRandom();
    }
    public string GetGenericFailMessage()
    {
        List<string> Replies = new List<string>() {
                        $"You fucked that up pretty bad.",
                        $"Do you enjoy pissing me off? The whole job is ruined.",
                        $"You completely fucked up the job",
                        $"The job is fucked.",
                        $"How did you fuck this up so badly?",
                        $"You just cost me a lot with this fuckup.",
                        };
        return Replies.PickRandom();
    }
    public void SendGenericTooSoonMessage(PhoneContact contact)
    {
        Player.CellPhone.AddPhoneResponse(contact.Name, GetGeneircTaskAbortMessage());
    }

    public void SendGenericFailMessage(PhoneContact contact)
    {
        Player.CellPhone.AddScheduledText(contact, GetGenericFailMessage(), 1, false);
    }

    public void SendGenericPickupMoneyMessage(PhoneContact contact,string placetypeName, GameLocation gameLocation, int MoneyToRecieve)
    {
        List<string> Replies = new List<string>() {
                                $"Seems like that thing we discussed is done? Come by the {placetypeName} on {gameLocation.FullStreetAddress} to collect the ${MoneyToRecieve}",
                                $"Word got around that you are done with that thing for us, Come back to the {placetypeName} on {gameLocation.FullStreetAddress} for your payment of ${MoneyToRecieve}",
                                $"Get back to the {placetypeName} on {gameLocation.FullStreetAddress} for your payment of ${MoneyToRecieve}",
                                $"{gameLocation.FullStreetAddress} for ${MoneyToRecieve}",
                                $"Heard you were done, see you at the {placetypeName} on {gameLocation.FullStreetAddress}. We owe you ${MoneyToRecieve}",
                                };
        Player.CellPhone.AddScheduledText(contact, Replies.PickRandom(), 1, false);
    }

    public void SendHitSquadMessage(PhoneContact contact)
    {
        List<string> Replies = new List<string>() {
                                $"I got some guys out there looking for you. Where you at?",
                                $"You hiding from us? Not for long.",
                                $"See you VERY soon.",
                                $"We will be seeing each other shortly.",
                                $"Going to get real very soon.",
                                };
        Player.CellPhone.AddScheduledText(contact, Replies.PickRandom(), 0, true);
    }

    public void OnInteractionMenuCreated(GameLocation gameLocation, MenuPool menuPool, UIMenu interactionMenu)
    {
        EntryPoint.WriteToConsole("Gang Tasks OnTransactionMenuCreated");
        GangRacketeeringTasks.Where(x=> x.PlayerTask != null && x.PlayerTask.IsActive).ToList().ForEach(x => x.OnInteractionMenuCreated(gameLocation, menuPool, interactionMenu));
        GangBriberyTasks.Where(x => x.PlayerTask != null && x.PlayerTask.IsActive).ToList().ForEach(x => x.OnInteractionMenuCreated(gameLocation, menuPool, interactionMenu));
    }
}

