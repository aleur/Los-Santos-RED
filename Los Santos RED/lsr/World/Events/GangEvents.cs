using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player.ActiveTasks;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GangEvents
{
    private IContactInteractable Player;
    private IZones Zones;
    private IJurisdictions Jurisdictions;
    private ISettingsProvideable Settings;
    private ICrimes Crimes;
    private IWeapons Weapons;
    private ITimeControllable Time;
    private IInteriors Interiors;
    private IShopMenus ShopMenus;
    private IGangTerritories GangTerritories;
    private IGangs Gangs;
    private IStreets Streets;
    private IPlacesOfInterest PlacesOfInterest;
    private IEntityProvideable World;
    private IAgencies Agencies;
    private IOrganizations Associations;
    private IContacts Contacts;
    private IModItems ModItems;
    public GangEvents(IEntityProvideable world, IZones zones, IJurisdictions jurisdictions, ISettingsProvideable settings, IPlacesOfInterest placesOfInterest, IWeapons weapons, ICrimes crimes, ITimeControllable time, IShopMenus shopMenus,
        IInteriors interiors, IGangs gangs, IGangTerritories gangTerritories, IStreets streets, IAgencies agencies, INameProvideable names, IPedGroups pedGroups, ILocationTypes locationTypes, IPlateTypes plateTypes, 
        IOrganizations associations, IContacts contacts, IModItems modItems, IIssuableWeapons issuableWeapons, IHeads heads, IDispatchablePeople dispatchablePeople)
    {
        World = world;
        PlacesOfInterest = placesOfInterest;
        Zones = zones;
        Jurisdictions = jurisdictions;
        Settings = settings;
        Weapons = weapons;
        Crimes = crimes;
        Time = time;
        Interiors = interiors;
        ShopMenus = shopMenus;
        Gangs = gangs;
        GangTerritories = gangTerritories;
        Streets = streets;
        Agencies = agencies;
        Associations = associations;
        Contacts = contacts;
        ModItems = modItems;
    }
    public void Setup(IContactInteractable contactInteractable)
    {
        Player = contactInteractable;
        foreach (GangTerritory gt in World.GangTerritories.GangTerritoriesList)
        {
            gt.LastUpdateTime = Game.GameTime;
            gt.DefaultTaskSettings(Settings);
        }
    }
    public void Dispose()
    {

    }
    public void Update()
    {
        UpdateTerritories();
    }

    public void UpdateTerritories()
    {
        int updated = 0;
        foreach (GangTerritory gt in World.GangTerritories.GangTerritoriesList)
        {
            Gang gang = Gangs.GetGang(gt.GangID);
            if (gt.TaskFrequency == 0 || gang == null) continue; // No gang tasks if frequency is zero.

            if (gt.TurfStatus == null)
            {
                gt.TurfStatus = new TurfStatus(World, Zones, PlacesOfInterest, GangTerritories, Gangs, gt, gang);
            }
            else
            {
                gt.TurfStatus.Update();
            }

            /*
            if (gt.GangTask == null)
            {
                GangTask gangTask = Player.PlayerTasks.GangTasks.RandomZoneTask(gang, gang.Contact, gt.TurfStatus);
                if (gangTask == null) continue;

                gt.GangTask = gangTask;
                gt.GangTask.TargetZone = Zones.GetZone(gt.ZoneInternalGameName);
                gt.TimeUntilNextTask = GetNextTaskTime(gt);

                EntryPoint.WriteToConsole($"{gt.ZoneInternalGameName}:{gt.GangID} setup with GangTask: {gt.GangTask.DebugName}, Countdown: {gt.TimeUntilNextTask}, ");
            }
            else
            {
                gt.UpdateTask(Player, gang);
            }*/
            updated++;
            if (updated >= 5)//15)//5
            {
                GameFiber.Yield();
                updated = 0;
            }
            if (!EntryPoint.ModController.IsRunning)
            {
                break;
            }
        }
    }

    private uint GetNextTaskTime(GangTerritory territory)
    {
        float freq = MathHelper.Clamp(territory.TaskFrequency, 0.0f, 1.0f);
        double r = RandomItems.MyRand.NextDouble() * freq; // Doesn't go above freq FUCK WHY AM I DOING MATH FOR THIS SHIT

        double? time = territory.MinTaskTime + (1 - freq) * (territory.MaxTaskTime - territory.MinTaskTime) * r;
        uint result = (uint)Math.Round((double)time);
        return (uint)MathHelper.Min(MathHelper.Max(result, territory.MinTaskTime), territory.MaxTaskTime);
    }
}
