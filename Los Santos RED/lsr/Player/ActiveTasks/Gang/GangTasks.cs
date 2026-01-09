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

    private List<RivalGangAmbushTask> RivalGangAmbushes = new List<RivalGangAmbushTask>();
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
        RivalGangAmbushes.ForEach(x => x.Dispose());
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

        RivalGangAmbushes.Clear();
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
            () => GangArson(gang, gangContact),
            () => GangBodyDisposal(gang, gangContact),
            () => GangBribery(gang, gangContact),
            () => GangRacketeering(gang, gangContact),
            () => GangPickup(gang, gangContact),
            () => GangImpoundTheft(gang, gangContact),
        };
        int rng = new Random().Next(options.Count);
        return options[rng]();
    }
    public GangTask RandomZoneTask(Gang gang, GangContact gangContact, TurfStatus gtStatus)
    {
        int RandomKillRequirement = new Random().Next(1, 5);
        List<Func<GangTask>> options = new List<Func<GangTask>>();
        
        if (gtStatus.AmbientEnemyGangs.Any())
        {
            options.Add(() => GangHit(gang, RandomKillRequirement, gangContact, gtStatus.AmbientEnemyGangs.PickRandom()));
        }
        if (gtStatus.EnemyGangsWithScenarios.Any())
        {
            options.Add(() => GangAmbush(gang, gangContact, gtStatus, "Gang", RandomItems.RandomPercent(25)));
        }
        if (gtStatus.HasRacketStores)
        {
            options.Add(() => GangRacketeering(gang, gangContact));
            options.Add(() => GangArson(gang, gangContact));
        }
        if (gtStatus.HasRobberyStores)
        {
            int robberyPedCount = new Random().Next(1, 4);
            options.Add(() => GangWheelman(gang, gangContact, robberyPedCount, "Random", true));
        }
        //EntryPoint.WriteToConsole($"{gtStatus.ZoneName} - {gtStatus.GangName} GangTask: {gtStatus.AmbientEnemyGangs.Any()}, {gtStatus.EnemyGangsWithScenarios.Any()}, {gtStatus.HasRacketStores}, {options.Count}");
        if (options.Count > 0)
        {
            int rng = new Random().Next(options.Count);
            return options[rng]();
        }
        else return null;
    }
    public GangTask RandomUntrustedTask(Gang gang, GangContact gangContact) // tasks if you're not friendly yet i guess
    {
        int RandomKillRequirement = new Random().Next(1, 5);
        Gang enemyGang = Gangs.GetGang(gang.EnemyGangs.PickRandom());
        GangTerritory gangTerritory = World.GangTerritories.GangTerritoriesList.Where(x => x.GangID == enemyGang.ID).PickRandom();

        List<Func<GangTask>> options = new List<Func<GangTask>>{
            () => GangHit(gang, RandomKillRequirement, gangContact, enemyGang),
            () => GangAmbush(gang, gangContact, gangTerritory.TurfStatus, "Gang", RandomItems.RandomPercent(30)),
            () => GangArson(gang, gangContact),
            () => GangImpoundTheft(gang, gangContact)
        };
        int rng = new Random().Next(options.Count);
        return options[rng]();
    }
    public GangTask GangProveWorth(Gang gang, int killRequirement, GangContact gangContact)
    {
        GangProveWorthTask newTask = new GangProveWorthTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang);
        newTask.KillRequirement = killRequirement;
        newTask.JoinGangOnComplete = true;
        GangProveWorthTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangCopHit(Gang gang, int killRequirement, GangContact gangContact, Agency targetAgency)
    {
        GangCopHitTask newTask = new GangCopHitTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetAgency, Agencies, killRequirement);
        AllGenericGangTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangHit(Gang gang, int killRequirement, GangContact gangContact, Gang targetGang)
    {
        RivalGangHitTask newTask = new RivalGangHitTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetGang, killRequirement, GangTerritories, Zones);
        RivalGangHits.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangAmbush(Gang gang, GangContact gangContact, TurfStatus turfStatus, string TargetType, bool killAll)
    {
        RivalGangAmbushTask newTask = new RivalGangAmbushTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones, Agencies, turfStatus, TargetType);
        RivalGangAmbushes.Add(newTask);
        newTask.Setup();
        newTask.KillAllTargets = killAll;
        return newTask;
    }
    public GangTask PayoffGang(Gang gang, GangContact gangContact)
    {
        PayoffGangTask newTask = new PayoffGangTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang);
        PayoffGangTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangVehicleTheft(Gang gang, GangContact gangContact, Gang targetGang, string vehicleModelName, string vehicleDisplayName)
    {
        RivalGangVehicleTheftTask newTask = new RivalGangVehicleTheftTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, targetGang, vehicleModelName, vehicleDisplayName);
        RivalGangTheftTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangBribery(Gang gang, GangContact gangContact)
    {
        GangBriberyTask newTask = new GangBriberyTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones);
        GangBriberyTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangArson(Gang gang, GangContact gangContact)
    {
        GangArsonTask newTask = new GangArsonTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones);
        GangArsonTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangRacketeering(Gang gang, GangContact gangContact)
    {
        GangRacketeeringTask newTask = new GangRacketeeringTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, GangTerritories, Zones);
        GangRacketeeringTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangPickup(Gang gang, GangContact gangContact)
    {
        GangPickupTask newTask = new GangPickupTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang);
        GangPickupTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangDelivery(Gang gang, GangContact gangContact, string modItemName)
    {
        GangDeliveryTask newTask = new GangDeliveryTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, modItemName);
        GangDeliveryTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }
    public GangTask GangWheelman(Gang gang, GangContact gangContact, int robbersToSpawn, string locationType, bool requireAllMembersToFinish)
    {
        GangWheelmanTask newTask = new GangWheelmanTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang, robbersToSpawn, locationType, requireAllMembersToFinish);
        GangWheelmanTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }

    public GangTask GangImpoundTheft(Gang gang, GangContact gangContact)
    {
        GangGetCarOutOfImpoundTask newTask = new GangGetCarOutOfImpoundTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, gangContact, gang);
        GangGetCarOutOfImpoundTasks.Add(newTask);
        newTask.Setup();
        return newTask;
    }

    public GangTask GangBodyDisposal(Gang gang, GangContact gangContact)
    {
        GangBodyDisposalTask newTask = new GangBodyDisposalTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems,PlayerTasks,this, gangContact, gang);
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

    public void StartDrugMeetTask(Gang gang, GangContact gangContact, ModItem modItem,int quantity, Gang meetingGang, bool IsPlayerSellingDrugs, GameLocation dealingLocation)
    {
        GangDrugMeetTask drugMeetTask = new GangDrugMeetTask(Player, Time, Gangs, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, Weapons, Names, PedGroups, ShopMenus, ModItems, PlayerTasks, this, 
            gangContact, gang, modItem, quantity, meetingGang, dealingLocation);
        drugMeetTask.IsPlayerSellingDrugs = IsPlayerSellingDrugs;
        AllGenericGangTasks.Add(drugMeetTask);
        drugMeetTask.Setup();
        drugMeetTask.Start();
    }



    public string GetGenericTaskAbortMessage()
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
        Player.CellPhone.AddPhoneResponse(contact.Name, GetGenericTaskAbortMessage());
    }

    public void SendGenericFailMessage(PhoneContact contact)
    {
        Player.CellPhone.AddScheduledText(contact, GetGenericFailMessage(), 1, false);
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
        EntryPoint.WriteToConsole($"Gang Tasks OnTransactionMenuCreated");
        GangRacketeeringTasks.Where(x=> x.PlayerTask != null && x.PlayerTask.IsActive).ToList().ForEach(x => x.OnInteractionMenuCreated(gameLocation, menuPool, interactionMenu));
        GangBriberyTasks.Where(x => x.PlayerTask != null && x.PlayerTask.IsActive).ToList().ForEach(x => x.OnInteractionMenuCreated(gameLocation, menuPool, interactionMenu));
    }
}

