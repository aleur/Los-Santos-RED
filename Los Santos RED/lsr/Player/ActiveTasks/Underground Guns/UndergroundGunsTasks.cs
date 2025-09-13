using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player.ActiveTasks;
using LSR.Vehicles;
using Rage;
using RAGENativeUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class UndergroundGunsTasks : IPlayerTaskGroup
{

    private ITaskAssignable Player;
    private ITimeReportable Time;
    private IGangs Gangs;
    private PlayerTasks PlayerTasks;
    private IPlacesOfInterest PlacesOfInterest;
    private List<DeadDrop> ActiveDrops = new List<DeadDrop>();
    private ISettingsProvideable Settings;
    private IEntityProvideable World;
    private ICrimes Crimes;
    private IModItems ModItems;
    private IWeapons Weapons;

    private List<IPlayerTask> AllTasks = new List<IPlayerTask>();

    public UndergroundGunsTasks(ITaskAssignable player, ITimeReportable time, IGangs gangs, PlayerTasks playerTasks, IPlacesOfInterest placesOfInterest, List<DeadDrop> activeDrops, ISettingsProvideable settings, IEntityProvideable world, ICrimes crimes, IModItems modItems, IWeapons weapons)
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
    }
    public void Setup()
    {

    }
    public void Dispose()
    {
        AllTasks.ForEach(x => x.Dispose());
        AllTasks.Clear();
    }
    public void StartGunTransport(GunDealerContact contact)
    {
        GunTransportTask gunTransportTask = new GunTransportTask(Player, Time, Gangs, PlayerTasks, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, contact);
        AllTasks.Add(gunTransportTask);
        gunTransportTask.Setup();
        gunTransportTask.Start();
    }

    public void StartGunDropoff(GunDealerContact contact, int numDropoffs)
    {
        GunDropoffTask gunDropoffTask = new GunDropoffTask(Player, Time, Gangs, PlayerTasks, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, ModItems, Weapons, contact, numDropoffs);
        AllTasks.Add(gunDropoffTask);
        gunDropoffTask.Setup();
        gunDropoffTask.Start();
    }

    public void OnInteractionMenuCreated(GameLocation gameLocation, MenuPool menuPool, UIMenu interactionMenu)
    {

    }
    internal void StartPayoffTask(GunDealerContact contact)
    {
        PayoffContactTask payoffContactTask = new PayoffContactTask(Player, Time, Gangs, PlayerTasks, PlacesOfInterest, ActiveDrops, Settings, World, Crimes, contact);
        AllTasks.Add(payoffContactTask);
        payoffContactTask.Setup();
        payoffContactTask.Start();
    }
}
