using LosSantosRED.lsr.Data;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class WorldTemplates : IWorldTemplates
{
    private IAgencies Agencies;
    private ICellphones Cellphones;
    private IContacts Contacts;
    private IJurisdictions CountyJurisdictions;
    private ICrimes Crimes;
    private IDances Dances;
    private IDispatchablePeople DispatchablePeople;
    private IDispatchableVehicles DispatchableVehicles;
    private IGangTerritories GangTerritories;
    private IGangs Gangs;
    private IGestures Gestures;
    private IHeads Heads;
    private IInteriors Interiors;
    private IIssuableWeapons IssuableWeapons;
    private IIntoxicants Itoxicants;
    private ILocationTypes LocationTypes;
    private IPlacesOfInterest PlacesOfInterest;
    private IModItems ModItems;
    private INameProvideable Names;
    private IOrganizations Organizations;
    private IPedGroups PedGroups;
    private IPropItems PhysicalItems;
    private IPlateTypes PlateTypes;
    private IGameSaves GameSaves;
    private ISavedOutfits SavedOutfits;
    private ISettingsProvideable Settings;
    private IShopMenus ShopMenus;
    private ISpawnBlocks SpawnBlocks;
    private ISpeeches Speeches;
    private IStreets Streets;
    private IWantedLevels WantedLevels;
    private IWeapons Weapons;
    private IJurisdictions ZoneJurisdictions;
    private IZones Zones;

    public WorldTemplates(IAgencies agencies, ICellphones cellphones, IContacts contacts, IJurisdictions countyJurisdictions, ICrimes crimes,
                          IDances dances, IDispatchablePeople dispatchablePeople, IDispatchableVehicles dispatchableVehicles, IGangTerritories gangTerritories, IGangs gangs, IGestures gestures,
                          IHeads heads, IInteriors interiors, IIssuableWeapons issuableWeapons, IIntoxicants intoxicants, ILocationTypes locationTypes, IPlacesOfInterest placesOfInterest,
                          IModItems modItems, INameProvideable names, IOrganizations organizations, IPedGroups pedGroups, IPropItems physicalItems, IPlateTypes plateTypes, IGameSaves gameSaves,
                          ISavedOutfits savedOutfits, ISettingsProvideable settings, IShopMenus shopMenus, ISpawnBlocks spawnBlocks, ISpeeches speeches, IStreets streets, IWantedLevels wantedLevels,
                          IWeapons weapons, IJurisdictions zoneJurisdictions, IZones zones)
    {
        Agencies = agencies;
        Cellphones = cellphones;
        Contacts = contacts;
        CountyJurisdictions = countyJurisdictions;
        Crimes = crimes;
        Dances = dances;
        DispatchablePeople = dispatchablePeople;
        DispatchableVehicles = dispatchableVehicles;
        GangTerritories = gangTerritories;
        Gangs = gangs;
        Gestures = gestures;
        Heads = heads;
        Interiors = interiors;
        IssuableWeapons = issuableWeapons;
        Itoxicants = intoxicants;
        LocationTypes = locationTypes;
        PlacesOfInterest = placesOfInterest;
        ModItems = modItems;
        Names = names;
        Organizations = organizations;
        PedGroups = pedGroups;
        PhysicalItems = physicalItems;
        PlateTypes = plateTypes;
        GameSaves = gameSaves;
        SavedOutfits = savedOutfits;
        Settings = settings;
        ShopMenus = shopMenus;
        SpawnBlocks = spawnBlocks;
        Speeches = speeches;
        Streets = streets;
        WantedLevels = wantedLevels;
        Weapons = weapons;
        ZoneJurisdictions = zoneJurisdictions;
        Zones = zones;
    }
    public List<WorldTemplate> WorldTemplateList { get; private set; } = new List<WorldTemplate>();
    public void Setup()
    {
        DirectoryInfo LSRDirectory = new DirectoryInfo("Plugins\\LosSantosRED");

        List<FileInfo> allFiles = LSRDirectory.GetFiles("*.xml").ToList();

        Dictionary<string, List<FileInfo>> groupedConfigs = new Dictionary<string, List<FileInfo>>();

        foreach (FileInfo file in allFiles)
        {
            if (file.Name.StartsWith("SavedVariation", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
            int lastUnderscoreIndex = fileNameWithoutExtension.LastIndexOf('_');
            if (lastUnderscoreIndex != -1)
            {
                string configSuffix = fileNameWithoutExtension.Substring(lastUnderscoreIndex + 1);

                if (!groupedConfigs.ContainsKey(configSuffix))
                {
                    groupedConfigs[configSuffix] = new List<FileInfo>();
                }
                groupedConfigs[configSuffix].Add(file);
            }
            else
            {
                if (!groupedConfigs.ContainsKey("Default"))
                {
                    groupedConfigs["Default"] = new List<FileInfo>();
                }
                groupedConfigs["Default"].Add(file);
            }
        }

        List<string> groupKeys = groupedConfigs.Keys.ToList();

        for (int i = 0; i < groupKeys.Count; i++)
        {
            string groupKey = groupKeys[i];
            List<FileInfo> groupFiles = groupedConfigs[groupKey];

            EntryPoint.WriteToConsole($"Config Group: {groupKey}", 0);

            WorldTemplate template = new WorldTemplate(Agencies, Cellphones, Contacts, CountyJurisdictions, Crimes, Dances, DispatchablePeople, DispatchableVehicles, GangTerritories, Gangs, Gestures, Heads, Interiors, IssuableWeapons, Itoxicants, LocationTypes, PlacesOfInterest, ModItems, Names, Organizations, PedGroups, PhysicalItems, PlateTypes, GameSaves, SavedOutfits, Settings, ShopMenus, SpawnBlocks, Speeches, Streets, WantedLevels, Weapons, ZoneJurisdictions, Zones);
            template.TemplateNumber = i + 1;
            template.worldName = groupKey;
            template.files = groupFiles;

            WorldTemplateList.Add(template);
        }
    }

    public void Load(WorldTemplate template)
    {
        template.Load();
    }
}
