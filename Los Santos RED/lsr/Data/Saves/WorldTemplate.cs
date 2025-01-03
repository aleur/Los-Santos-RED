using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player;
using LSR.Vehicles;
using Mod;
using Rage;
using Rage.Native;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace LosSantosRED.lsr.Data
{
    public class WorldTemplate
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
        public WorldTemplate(IAgencies agencies, ICellphones cellphones, IContacts contacts, IJurisdictions countyJurisdictions, ICrimes crimes,
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

        private readonly string AgenciesDefault = "Plugins\\LosSantosRED\\Agencies.xml";
        private readonly string CellphonesDefault = "Plugins\\LosSantosRED\\Cellphones.xml";
        private readonly string ContactsDefault = "Plugins\\LosSantosRED\\Contacts.xml";
        private readonly string CountyJurisdictionsDefault = "Plugins\\LosSantosRED\\CountyJurisdictions.xml";
        private readonly string CrimesDefault = "Plugins\\LosSantosRED\\Crimes.xml";
        private readonly string DancesDefault = "Plugins\\LosSantosRED\\Dances.xml";
        private readonly string DispatchablePeopleDefault = "Plugins\\LosSantosRED\\DispatchablePeople.xml";
        private readonly string DispatchableVehiclesDefault = "Plugins\\LosSantosRED\\DispatchableVehicles.xml";
        private readonly string GangTerritoriesDefault = "Plugins\\LosSantosRED\\GangTerritories.xml";
        private readonly string GangsDefault = "Plugins\\LosSantosRED\\Gangs.xml";
        private readonly string GesturesDefault = "Plugins\\LosSantosRED\\Gestures.xml";
        private readonly string HeadsDefault = "Plugins\\LosSantosRED\\Heads.xml";
        private readonly string InteriorsDefault = "Plugins\\LosSantosRED\\Interiors.xml";
        private readonly string IssuableWeaponsDefault = "Plugins\\LosSantosRED\\IssuableWeapons.xml";
        private readonly string ItoxicantsDefault = "Plugins\\LosSantosRED\\Itoxicants.xml";
        private readonly string LocationTypesDefault = "Plugins\\LosSantosRED\\LocationTypes.xml";
        private readonly string LocationsDefault = "Plugins\\LosSantosRED\\Locations.xml";
        private readonly string ModItemsDefault = "Plugins\\LosSantosRED\\ModItems.xml";
        private readonly string NamesDefault = "Plugins\\LosSantosRED\\Names.xml";
        private readonly string OrganizationsDefault = "Plugins\\LosSantosRED\\Organizations.xml";
        private readonly string PedGroupsDefault = "Plugins\\LosSantosRED\\PedGroups.xml";
        private readonly string PhysicalItemsDefault = "Plugins\\LosSantosRED\\PhysicalItems.xml";
        private readonly string PlateTypesDefault = "Plugins\\LosSantosRED\\PlateTypes.xml";
        private readonly string SaveGamesDefault = "Plugins\\LosSantosRED\\SaveGames.xml";
        private readonly string SavedOutfitsDefault = "Plugins\\LosSantosRED\\SavedOutfits.xml";
        private readonly string SettingsDefault = "Plugins\\LosSantosRED\\Settings.xml";
        private readonly string ShopMenusDefault = "Plugins\\LosSantosRED\\ShopMenus.xml";
        private readonly string SpawnBlocksDefault = "Plugins\\LosSantosRED\\SpawnBlocks.xml";
        private readonly string SpeechesDefault = "Plugins\\LosSantosRED\\Speeches.xml";
        private readonly string StreetsDefault = "Plugins\\LosSantosRED\\Streets.xml";
        private readonly string WantedLevelsDefault = "Plugins\\LosSantosRED\\WantedLevels.xml";
        private readonly string WeaponsDefault = "Plugins\\LosSantosRED\\Weapons.xml";
        private readonly string ZoneJurisdictionsDefault = "Plugins\\LosSantosRED\\ZoneJurisdictions.xml";
        private readonly string ZonesDefault = "Plugins\\LosSantosRED\\Zones.xml";

        public int TemplateNumber { get; set; }
        public string worldName { get; set; }
        
        public List<FileInfo> files;

        /*

        !!! I can use this for save transfers possibly.

        private DirectoryInfo worldDirectory { get; set; }


        //Save
        public void Save(string worldName, List<FileInfo> files)
        {
            DirectoryInfo baseDirectory = new DirectoryInfo("Plugins\\LosSantosRED\\Worlds");

            worldDirectory = baseDirectory.CreateSubdirectory(worldName);

            foreach (FileInfo file in files)
            {
                string destinationPath = Path.Combine(worldDirectory.FullName, file.Name);
                file.CopyTo(destinationPath, overwrite: true);
            }

            EntryPoint.WriteToConsole($"World '{worldName}' saved with {files.Count} files in {worldDirectory.FullName}", 0);
        }*/

        //Load
        public void Load()
        {
            try
            {
                /*
                var handlers = new Dictionary<string, Action<FileInfo>>
                {
                    { "Settings", HandleSettings }, 
                    { "Weapons", HandleWeapons }, 
                    { "PhysicalItems", HandlePhysicalItems },
                    { "Itoxicants", HandleItoxicants }, 
                    { "Cellphones", HandleCellphones },
                    { "ModItems", HandleModItems }, 
                    { "ShopMenus", HandleShopMenus }, 
                    { "LocationTypes", HandleLocationTypes }, 
                    { "Zones", HandleZones },
                    { "PlateTypes", HandlePlateTypes },
                    { "Streets", HandleStreets },
                    { "Names", HandleNames },
                    { "Heads", HandleHeads },
                    { "DispatchableVehicles", HandleDispatchableVehicles },
                    { "IssuableWeapons", HandleIssuableWeapons },
                    { "DispatchablePeople", HandleDispatchablePeople },
                    { "Contacts", HandleContacts },
                    { "Agencies", HandleAgencies },
                    { "Gangs", HandleGangs },
                    { "Locations", HandleLocations },
                    { "CountyJurisdictions", HandleCountyJurisdictions },
                    { "ZoneJurisdictions", HandleZoneJurisdictions },
                    { "GangTerritories", HandleGangTerritories },
                    { "PedGroups", HandlePedGroups },
                    { "Crimes", HandleCrimes },
                    { "SaveGames", HandleSaveGames },
                    { "Interiors", HandleInteriors },
                    { "Dances", HandleDances },
                    { "Gestures", HandleGestures },
                    { "Speeches", HandleSpeeches },
                    { "WantedLevels", HandleWantedLevels },
                    { "SavedOutfits", HandleSavedOutfits },
                    { "Organizations", HandleOrganizations },
                    { "SpawnBlocks", HandleSpawnBlocks }
                };

                foreach (var handler in handlers)
                {
                    foreach (FileInfo file in files)
                    {
                        if (file.Name.StartsWith(handler.Key))
                        {
                            handler.Value.Invoke(file);
                            GameFiber.Yield();
                            break;
                        }
                    }
                }*/

                EntryPoint.ModController.Setup();
                EntryPoint.ModController.IsLoadingWorld = false;
                GameFiber.Yield();

                Game.DisplayNotification($"~s~Los Santos ~r~RED~s~ {worldName} Config Loaded");
                EntryPoint.WriteToConsole($"Loaded {worldName} template", 0);
                Game.FadeScreenIn(1500, true);
            }
            catch (Exception e)
            {
                Game.FadeScreenIn(0);
                EntryPoint.WriteToConsole("Error Loading World Template: " + e.Message + " " + e.StackTrace, 0);
                Game.DisplayNotification("Error Loading World Template");
            }
        }

        public void HandleSettings(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Settings config: {ConfigFile.FullName}", 0);
                Settings.SettingsManager = Serialization.DeserializeParam<SettingsManager>(ConfigFile.FullName);
            }
            else if (File.Exists(SettingsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Settings config: {SettingsDefault}", 0);
                Settings.SettingsManager = Serialization.DeserializeParam<SettingsManager>(SettingsDefault);
            }
            Settings.SettingsManager.Setup();
        }
        public void HandleWeapons(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Weapons config: {ConfigFile.FullName}", 0);
                Weapons.WeaponsList = Serialization.DeserializeParams<WeaponInformation>(ConfigFile.FullName);
            }
            else if (File.Exists(WeaponsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Weapons config: {WeaponsDefault}", 0);
                Weapons.WeaponsList = Serialization.DeserializeParams<WeaponInformation>(WeaponsDefault);
            }
        }
        public void HandlePhysicalItems(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Physical Items config: {ConfigFile.FullName}", 0);
                PhysicalItems.PhysicalItemsList = Serialization.DeserializeParams<PhysicalItem>(ConfigFile.FullName);
            }
            else if (File.Exists(PhysicalItemsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Physical Items config: {PhysicalItemsDefault}", 0);
                PhysicalItems.PhysicalItemsList = Serialization.DeserializeParams<PhysicalItem>(PhysicalItemsDefault);
            }
            else {  }
        }
        public void HandleItoxicants(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Intoxicants config: {ConfigFile.FullName}", 0);
                Itoxicants.IntoxicantList = Serialization.DeserializeParams<Intoxicant>(ConfigFile.FullName);
            }
            else if (File.Exists(ItoxicantsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Intoxicants config: {ItoxicantsDefault}", 0);
                Itoxicants.IntoxicantList = Serialization.DeserializeParams<Intoxicant>(ItoxicantsDefault);
            }
        }
        public void HandleCellphones(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Cellphones config: {ConfigFile.FullName}", 0);
                Cellphones.CellphoneList = Serialization.DeserializeParams<CellphoneData>(ConfigFile.FullName);
            }
            else if (File.Exists(CellphonesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Cellphones config: {CellphonesDefault}", 0);
                Cellphones.CellphoneList = Serialization.DeserializeParams<CellphoneData>(CellphonesDefault);
            }
        }
        public void HandleModItems(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Mod Items config: {ConfigFile.FullName}", 0);
                ModItems.PossibleItems = Serialization.DeserializeParam<PossibleItems>(ConfigFile.FullName);
            }
            else if (File.Exists(ModItemsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Mod Items config: {ModItemsDefault}", 0);
                ModItems.PossibleItems = Serialization.DeserializeParam<PossibleItems>(ModItemsDefault);
            }
            ModItems.Setup(PhysicalItems, Weapons, Itoxicants, Cellphones);
        }
        public void HandleShopMenus(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Shop Menus config: {ConfigFile.FullName}", 0);
                ShopMenus.PossibleShopMenus = Serialization.DeserializeParam<ShopMenuTypes>(ConfigFile.FullName);
            }
            else if (File.Exists(ShopMenusDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Shop Menus config: {ShopMenusDefault}", 0);
                ShopMenus.PossibleShopMenus = Serialization.DeserializeParam<ShopMenuTypes>(ShopMenusDefault);
            }
            ShopMenus.Setup(ModItems);
        }
        public void HandleLocationTypes(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Location Types config: {ConfigFile.FullName}", 0);
                LocationTypes.LocationTypeNames = Serialization.DeserializeParam<LocationTypeManager>(ConfigFile.FullName);
            }
            else if (File.Exists(LocationTypesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Location Types config: {LocationTypesDefault}", 0);
                LocationTypes.LocationTypeNames = Serialization.DeserializeParam<LocationTypeManager>(LocationTypesDefault);
            }
        }
        public void HandleZones(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Zones config: {ConfigFile.FullName}", 0);
                Zones.ZoneList = Serialization.DeserializeParams<Zone>(ConfigFile.FullName);
            }
            else if (File.Exists(ZonesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Zones config: {ZonesDefault}", 0);
                Zones.ZoneList = Serialization.DeserializeParams<Zone>(ZonesDefault);
            }
            Zones.Setup(LocationTypes);
        }
        public void HandlePlateTypes(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Plate Types config: {ConfigFile.FullName}", 0);
                PlateTypes.PlateTypeManager = Serialization.DeserializeParam<PlateTypeManager>(ConfigFile.FullName);
            }
            else if (File.Exists(PlateTypesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Plate Types config: {PlateTypesDefault}", 0);
                PlateTypes.PlateTypeManager = Serialization.DeserializeParam<PlateTypeManager>(PlateTypesDefault);
            }
        }
        public void HandleStreets(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Streets config: {ConfigFile.FullName}", 0);
                Streets.StreetsList = Serialization.DeserializeParams<Street>(ConfigFile.FullName);
            }
            else if (File.Exists(StreetsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Streets config: {StreetsDefault}", 0);
                Streets.StreetsList = Serialization.DeserializeParams<Street>(StreetsDefault);
            }
        }
        public void HandleNames(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Names config: {ConfigFile.FullName}", 0);
                Names.PossibleNames = Serialization.DeserializeParam<PossibleNames>(ConfigFile.FullName);
            }
            else if (File.Exists(NamesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Names config: {NamesDefault}", 0);
                Names.PossibleNames = Serialization.DeserializeParam<PossibleNames>(NamesDefault);
            }
        }
        public void HandleHeads(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Heads config: {ConfigFile.FullName}", 0);
                Heads.RandomHeadDataLookup = Serialization.DeserializeParams<HeadDataGroup>(ConfigFile.FullName);
            }
            else if (File.Exists(HeadsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Heads config: {HeadsDefault}", 0);
                Heads.RandomHeadDataLookup = Serialization.DeserializeParams<HeadDataGroup>(HeadsDefault);
            }
            else { Heads.DefaultConfig(); }
        }
        public void HandleDispatchableVehicles(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Dispatchable Vehicles config: {ConfigFile.FullName}", 0);
                DispatchableVehicles.VehicleGroupLookup = Serialization.DeserializeParams<DispatchableVehicleGroup>(ConfigFile.FullName);
            }
            else if (File.Exists(DispatchableVehiclesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Dispatchable Vehicles config: {DispatchableVehiclesDefault}", 0);
                DispatchableVehicles.VehicleGroupLookup = Serialization.DeserializeParams<DispatchableVehicleGroup>(DispatchableVehiclesDefault);
            }
            DispatchableVehicles.Setup(PlateTypes);
        }
        public void HandleIssuableWeapons(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Issuable Weapons config: {ConfigFile.FullName}", 0);
                IssuableWeapons.IssuableWeaponsGroupLookup = Serialization.DeserializeParams<IssuableWeaponsGroup>(ConfigFile.FullName);
            }
            else if (File.Exists(IssuableWeaponsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Issuable Weapons config: {IssuableWeaponsDefault}", 0);
                IssuableWeapons.IssuableWeaponsGroupLookup = Serialization.DeserializeParams<IssuableWeaponsGroup>(IssuableWeaponsDefault);
            }
        }
        public void HandleDispatchablePeople(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Dispatchable People config: {ConfigFile.FullName}", 0);
                DispatchablePeople.PeopleGroupLookup = Serialization.DeserializeParams<DispatchablePersonGroup>(ConfigFile.FullName);
            }
            else if (File.Exists(DispatchablePeopleDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Dispatchable People config: {DispatchablePeopleDefault}", 0);
                DispatchablePeople.PeopleGroupLookup = Serialization.DeserializeParams<DispatchablePersonGroup>(DispatchablePeopleDefault);
            }
            DispatchablePeople.Setup(IssuableWeapons);
        }
        public void HandleContacts(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Contacts Config: {ConfigFile.FullName}", 0);
                Contacts.PossibleContacts = Serialization.DeserializeParam<PossibleContacts>(ConfigFile.FullName);
            }
            else if (File.Exists(ContactsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Contacts Config  {ContactsDefault}", 0);
                Contacts.PossibleContacts = Serialization.DeserializeParam<PossibleContacts>(ContactsDefault);
            }
        }
        public void HandleAgencies(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Agencies config: {ConfigFile.FullName}", 0);
                Agencies.AgenciesList = Serialization.DeserializeParams<Agency>(ConfigFile.FullName);
            }
            else if (File.Exists(AgenciesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Agencies config: {AgenciesDefault}", 0);
                Agencies.AgenciesList = Serialization.DeserializeParams<Agency>(AgenciesDefault);
            }
            Agencies.Setup(Heads, DispatchableVehicles, DispatchablePeople, IssuableWeapons);
        }
        public void HandleGangs(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Gangs config: {ConfigFile.FullName}", 0);
                Gangs.GangsList = Serialization.DeserializeParams<Gang>(ConfigFile.FullName);
            }
            else if (File.Exists(GangsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Gangs config: {GangsDefault}", 0);
                Gangs.GangsList = Serialization.DeserializeParams<Gang>(GangsDefault);
            }
            Gangs.Setup(Heads, DispatchableVehicles, DispatchablePeople, IssuableWeapons, Contacts);
        }
        public void HandleLocations(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Locations config: {ConfigFile.FullName}", 0);
                PlacesOfInterest.PossibleLocations = Serialization.DeserializeParam<PossibleLocations>(ConfigFile.FullName);
            }
            else if (File.Exists(LocationsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Locations config: {LocationsDefault}", 0);
                PlacesOfInterest.PossibleLocations = Serialization.DeserializeParam<PossibleLocations>(LocationsDefault);
            }
            PlacesOfInterest.Setup();
        }
        public void HandleCountyJurisdictions(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded County Jurisdictions config: {ConfigFile.FullName}", 0);
                CountyJurisdictions.CountyJurisdictionList = Serialization.DeserializeParams<CountyJurisdiction>(ConfigFile.FullName);
            }
            else if (File.Exists(CountyJurisdictionsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded County Jurisdictions config: {CountyJurisdictionsDefault}", 0);
                CountyJurisdictions.CountyJurisdictionList = Serialization.DeserializeParams<CountyJurisdiction>(CountyJurisdictionsDefault);
            }
            CountyJurisdictions.Setup(Agencies);
        }
        public void HandleZoneJurisdictions(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Zone Jurisdictions config: {ConfigFile.FullName}", 0);
                ZoneJurisdictions.ZoneJurisdictionsList = Serialization.DeserializeParams<ZoneJurisdiction>(ConfigFile.FullName);
            }
            else if (File.Exists(ZoneJurisdictionsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Zone Jurisdictions config: {ZoneJurisdictionsDefault}", 0);
                ZoneJurisdictions.ZoneJurisdictionsList = Serialization.DeserializeParams<ZoneJurisdiction>(ZoneJurisdictionsDefault);
            }
            ZoneJurisdictions.Setup(Agencies);
        }
        public void HandleGangTerritories(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Gang Territories config: {ConfigFile.FullName}", 0);
                GangTerritories.ZoneJurisdictionsList = Serialization.DeserializeParams<ZoneJurisdiction>(ConfigFile.FullName);
            }
            else if (File.Exists(GangTerritoriesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Gang Territories config: {GangTerritoriesDefault}", 0);
                GangTerritories.ZoneJurisdictionsList = Serialization.DeserializeParams<ZoneJurisdiction>(GangTerritoriesDefault);
            }
            GangTerritories.Setup(Gangs);
            Gangs.CheckTerritory(GangTerritories);
        }
        public void HandlePedGroups(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Ped Groups config: {ConfigFile.FullName}", 0);
                PedGroups.PedGroupList = Serialization.DeserializeParams<PedGroup>(ConfigFile.FullName);
            }
            else if (File.Exists(PedGroupsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Ped Groups config: {PedGroupsDefault}", 0);
                PedGroups.PedGroupList = Serialization.DeserializeParams<PedGroup>(PedGroupsDefault);
            }
        }
        public void HandleCrimes(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Crimes config: {ConfigFile.FullName}", 0);
                Crimes.CrimeList = Serialization.DeserializeParams<Crime>(ConfigFile.FullName);
            }
            else if (File.Exists(CrimesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Crimes config: {CrimesDefault}", 0);
                Crimes.CrimeList = Serialization.DeserializeParams<Crime>(CrimesDefault);
            }
        }
        public void HandleSaveGames(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Save Games config: {ConfigFile.FullName}", 0);
                GameSaves.GameSaveList = Serialization.DeserializeParams<GameSave>(ConfigFile.FullName);
            }
            else if (File.Exists(SaveGamesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Save Games config: {SaveGamesDefault}", 0);
                GameSaves.GameSaveList = Serialization.DeserializeParams<GameSave>(SaveGamesDefault);
            }
        }
        public void HandleInteriors(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Interiors config: {ConfigFile.FullName}", 0);
                Interiors.PossibleInteriors = Serialization.DeserializeParam<PossibleInteriors>(ConfigFile.FullName);
            }
            else if (File.Exists(InteriorsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Interiors config: {InteriorsDefault}", 0);
                Interiors.PossibleInteriors = Serialization.DeserializeParam<PossibleInteriors>(InteriorsDefault);
            }
        }

        public void HandleDances(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Dances config: {ConfigFile.FullName}", 0);
                Dances.DanceLookups = Serialization.DeserializeParams<DanceData>(ConfigFile.FullName);
            }
            else if (File.Exists(DancesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Dances config: {DancesDefault}", 0);
                Dances.DanceLookups = Serialization.DeserializeParams<DanceData>(DancesDefault);
            }
        }
        public void HandleGestures(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Gestures config: {ConfigFile.FullName}", 0);
                Gestures.GestureLookups = Serialization.DeserializeParams<GestureData>(ConfigFile.FullName);
            }
            else if (File.Exists(GesturesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Gestures config: {GesturesDefault}", 0);
                Gestures.GestureLookups = Serialization.DeserializeParams<GestureData>(GesturesDefault);
            }
        }
        public void HandleSpeeches(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Speeches config: {ConfigFile.FullName}", 0);
                Speeches.SpeechLookups = Serialization.DeserializeParams<SpeechData>(ConfigFile.FullName);
            }
            else if (File.Exists(SpeechesDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Speeches config: {SpeechesDefault}", 0);
                Speeches.SpeechLookups = Serialization.DeserializeParams<SpeechData>(SpeechesDefault);
            }
        }
        public void HandleWantedLevels(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Wanted Levels config: {ConfigFile.FullName}", 0);
                WantedLevels.WantedLevelList = Serialization.DeserializeParams<WantedLevel>(ConfigFile.FullName);
            }
            else if (File.Exists(WantedLevelsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Wanted Levels config: {WantedLevelsDefault}", 0);
                WantedLevels.WantedLevelList = Serialization.DeserializeParams<WantedLevel>(WantedLevelsDefault);
            }
            WantedLevels.Setup(Heads, DispatchableVehicles, DispatchablePeople, IssuableWeapons);
        }
        public void HandleSavedOutfits(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Saved Outfits config: {ConfigFile.FullName}", 0);
                SavedOutfits.SavedOutfitList = Serialization.DeserializeParams<SavedOutfit>(ConfigFile.FullName);
            }
            else if (File.Exists(SavedOutfitsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Saved Outfits config: {SavedOutfitsDefault}", 0);
                SavedOutfits.SavedOutfitList = Serialization.DeserializeParams<SavedOutfit>(SavedOutfitsDefault);
            }
        }
        public void HandleOrganizations(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Organizations config: {ConfigFile.FullName}", 0);
                Organizations.PossibleOrganizations = Serialization.DeserializeParam<PossibleOrganizations>(ConfigFile.FullName);
            }
            else if (File.Exists(OrganizationsDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Organizations config: {OrganizationsDefault}", 0);
                Organizations.PossibleOrganizations = Serialization.DeserializeParam<PossibleOrganizations>(OrganizationsDefault);
            }
            Organizations.Setup(Heads, DispatchableVehicles, DispatchablePeople, IssuableWeapons, Contacts);
            Contacts.Setup(Organizations);
            // need to setup contacts here as well
        }
        public void HandleSpawnBlocks(FileInfo ConfigFile)
        {
            if (ConfigFile != null)
            {
                EntryPoint.WriteToConsole($"Loaded Spawn Blocks config: {ConfigFile.FullName}", 0);
                SpawnBlocks.PossibleSpawnBlocks = Serialization.DeserializeParam<PossibleSpawnBlocks>(ConfigFile.FullName);
            }
            else if (File.Exists(SpawnBlocksDefault))
            {
                EntryPoint.WriteToConsole($"Loaded Spawn Blocks config: {SpawnBlocksDefault}", 0);
                SpawnBlocks.PossibleSpawnBlocks = Serialization.DeserializeParam<PossibleSpawnBlocks>(SpawnBlocksDefault);
            }
        }
    }
}
