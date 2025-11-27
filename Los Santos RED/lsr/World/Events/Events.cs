using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Events
{
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
    public Events(IEntityProvideable world, IZones zones, IJurisdictions jurisdictions, ISettingsProvideable settings, IPlacesOfInterest placesOfInterest, IWeapons weapons, ICrimes crimes, ITimeControllable time, IShopMenus shopMenus,
        IInteriors interiors, IGangs gangs, IGangTerritories gangTerritories, IStreets streets, IAgencies agencies, INameProvideable names, IPedGroups pedGroups, ILocationTypes locationTypes, IPlateTypes plateTypes, 
        IOrganizations associations, IContacts contacts, IModItems modItems, IIssuableWeapons issuableWeapons, IHeads heads, IDispatchablePeople dispatchablePeople, IClothesNames clothesNames)
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

        CivilianEvents = new CivilianEvents(world, zones, jurisdictions, settings, placesOfInterest, weapons, crimes, time, shopMenus, interiors, gangs, gangTerritories, streets, agencies, names, pedGroups, locationTypes, plateTypes, associations, contacts, modItems, issuableWeapons, heads, dispatchablePeople);
        GangEvents = new GangEvents(world, zones, jurisdictions, settings, placesOfInterest, weapons, crimes, time, shopMenus, interiors, gangs, gangTerritories, streets, agencies, names, pedGroups, locationTypes, plateTypes, associations, contacts, modItems, issuableWeapons, heads, dispatchablePeople);
    }
    public GangEvents GangEvents { get; private set; }
    public CivilianEvents CivilianEvents { get; private set; }
    public void Setup(IContactInteractable contactInteractable, ITargetable targetable)
    {
        GangEvents.Setup(contactInteractable);
        CivilianEvents.Setup(contactInteractable, targetable);
    }
    public void Dispose()
    {
        CivilianEvents.Dispose();
        GangEvents.Dispose();
    }
    public void UpdateEvents()
    {
        if (!EntryPoint.ModController.IsRunning)
        {
            return;
        }

        CivilianEvents.Update();
        GangEvents.Update();
    }
    public void Reset()
    {

    }


}
