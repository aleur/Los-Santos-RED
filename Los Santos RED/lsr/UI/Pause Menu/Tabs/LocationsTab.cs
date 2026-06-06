using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Locations;
using Rage;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


public class LocationsTab : ITabbableMenu
{
    private IPlacesOfInterest PlacesOfInterest;
    private IGangRelateable Player;
    private ITimeReportable Time;
    private ISettingsProvideable Settings;
    private IEntityProvideable World;

    private TabSubmenuItem dynamicLocationsTabSubmenuITem;
    private string AddressFilterString = "";
    private string NameFilterString = "";
    private string TypeFilterString = "";
    private string ZoneFilterString = "";
    private string DistanceFilterString = "";
    private TabTextItem ttx;
    private TabTextItem tta;
    private TabItem SearchCriteria;
    private TabView TabView;
    private TabTextItem removeGPSTTI;

    public LocationsTab(IGangRelateable player, IPlacesOfInterest placesOfInterest, ITimeReportable time, ISettingsProvideable settings, TabView tabView, IEntityProvideable world)
    {
        Player = player;
        PlacesOfInterest = placesOfInterest;
        Time = time;
        Settings = settings;
        TabView = tabView;
        World = world;
    }

    public void AddItems()
    {
        List<TabItem> items = new List<TabItem>();

        SearchCriteria = new TabItem("Search Criteria");

        List<MissionInformation> missionInfoList = new List<MissionInformation>();

        MissionInformation clearCriteria = new MissionInformation("Clear Criteria", "Clear search criteria");
        MissionInformation addressFilter = new MissionInformation("Address", "Filter by address");
        MissionInformation nameFilter = new MissionInformation("Name", "Filter by location name");
        MissionInformation typeFilter = new MissionInformation("Type", "Filter by location type");
        MissionInformation zoneFilter = new MissionInformation("Zone", "Filter by zone");
        MissionInformation distanceFilter = new MissionInformation("Distance", "Filter by max distance");

        missionInfoList.Add(clearCriteria); 
        missionInfoList.Add(addressFilter);
        missionInfoList.Add(nameFilter);
        missionInfoList.Add(typeFilter);
        missionInfoList.Add(zoneFilter);
        missionInfoList.Add(distanceFilter);

        TabMissionSelectItem basicLocationTypeList = new TabMissionSelectItem("Search Criteria", missionInfoList);
        basicLocationTypeList.OnItemSelect += (selectedItem) =>
        {
            if (selectedItem != null)
            {
                if (selectedItem.Name.Contains("Clear Criteria"))
                {
                    NameFilterString = NativeHelper.GetKeyboardInput("");
                    AddressFilterString = NativeHelper.GetKeyboardInput("");
                    ZoneFilterString = NativeHelper.GetKeyboardInput("");
                    DistanceFilterString = NativeHelper.GetKeyboardInput("");
                    nameFilter.Description = $"Filter by location name";
                    addressFilter.Description = $"Filter by address";
                    zoneFilter.Description = $"Filter by zone";
                    distanceFilter.Description = $"Filter by max distance";
                }
                else if (selectedItem.Name.Contains("Address"))
                {
                    AddressFilterString = NativeHelper.GetKeyboardInput("").Trim();
                    addressFilter.Description = "Filter by address" + (string.IsNullOrEmpty(AddressFilterString) ? "" : $"\n~g~{AddressFilterString}");
                }
                else if (selectedItem.Name.Contains("Name"))
                {
                    NameFilterString = NativeHelper.GetKeyboardInput("").Trim();
                    nameFilter.Description = $"Filter by location name" + (string.IsNullOrEmpty(NameFilterString) ? "" : $"\n~g~{NameFilterString}");
                }
                else if (selectedItem.Name.Contains("Type"))
                {
                    TypeFilterString = NativeHelper.GetKeyboardInput("").Trim();
                    typeFilter.Description = $"Filter by location type" + (string.IsNullOrEmpty(TypeFilterString) ? "" : $"\n~g~{TypeFilterString}");
                }
                else if (selectedItem.Name.Contains("Zone"))
                {
                    ZoneFilterString = NativeHelper.GetKeyboardInput("").Trim();
                    zoneFilter.Description = $"Filter by zone" + (string.IsNullOrEmpty(ZoneFilterString) ? "" : $"\n~g~{ZoneFilterString}");
                }
                else if (selectedItem.Name.Contains("Distance"))
                {
                    DistanceFilterString = NativeHelper.GetKeyboardInput("").Trim();
                    if (!DistanceFilterString.All(char.IsDigit)) DistanceFilterString = "";
                    distanceFilter.Description = $"Filter by max distance" + (string.IsNullOrEmpty(DistanceFilterString) ? "" : $"\n~g~{DistanceFilterString}");
                }

                for (int i = dynamicLocationsTabSubmenuITem.Items.Count; i-- > 0;)
                {
                    if (dynamicLocationsTabSubmenuITem.Items[i].Title != "Search Criteria" /*&& dynamicLocationsTabSubmenuITem.Items[i].Title != removeGPSTTI.Title*/)
                    {
                        dynamicLocationsTabSubmenuITem.Items.RemoveAt(i);
                    }
                }
                dynamicLocationsTabSubmenuITem.Items.AddRange(GetDirectoryLocations());
                dynamicLocationsTabSubmenuITem.RefreshIndex();
            }
        };/*
        removeGPSTTI = new TabTextItem("Remove GPS", "Remove GPS", "Remove the GPS Blip");
        removeGPSTTI.Activated += (s, e) =>
        {
            Player.GPSManager.RemoveGPSRoute(true);
        };*/
        items.Add(basicLocationTypeList);
        //items.Add(removeGPSTTI);
        items.AddRange(GetDirectoryLocations());
        dynamicLocationsTabSubmenuITem = new TabSubmenuItem("Locations", items);
        TabView.AddTab(dynamicLocationsTabSubmenuITem);
    }
    private List<TabItem> GetDirectoryLocations()
    {
        List<TabItem> items = new List<TabItem>();
        if (string.IsNullOrEmpty(AddressFilterString))
        {
            AddressFilterString = "";
        }
        if (string.IsNullOrEmpty(NameFilterString))
        {
            NameFilterString = "";
        }
        List<GameLocation> DirectoryLocations = PlacesOfInterest.AllLocations().Where(x => (x.ShowsOnDirectory || Settings.SettingsManager.WorldSettings.ShowAllLocationsOnDirectory) && 
                                                                                            x.IsEnabled && x.IsSameState(Player.CurrentLocation?.CurrentZone?.GameState) && x.IsCorrectMap(World.IsMPMapLoaded) && 
                                                                                            (string.IsNullOrEmpty(AddressFilterString) || AddressFilterString == "" || x.FullStreetAddress.ToLower().Contains(AddressFilterString.ToLower())) &&
                                                                                            (string.IsNullOrEmpty(NameFilterString) || NameFilterString == "" || x.Name.ToLower().Contains(NameFilterString.ToLower())) &&
                                                                                            (string.IsNullOrEmpty(TypeFilterString) || TypeFilterString == "" || x.TypeName.ToLower().Contains(TypeFilterString.ToLower())) &&
                                                                                            (string.IsNullOrEmpty(ZoneFilterString) || ZoneFilterString == "" || World.Zones.GetZone(x.EntrancePosition).DisplayName.ToLower().Contains(ZoneFilterString.ToLower())) &&
                                                                                            (string.IsNullOrEmpty(DistanceFilterString) || DistanceFilterString == "" || Math.Round(Player.Character.DistanceTo2D(x.EntrancePosition) * 0.000621371, 2) < Convert.ToInt32(DistanceFilterString))).ToList();
        foreach (string typeName in DirectoryLocations.OrderBy(x => x.TypeName).Select(x => x.TypeName).Distinct())
        {
            List<MissionInformation> missionInfoList = new List<MissionInformation>();
            foreach (GameLocation bl in DirectoryLocations.Where(x => x.TypeName == typeName).OrderBy(x=> x.SortOrder).ThenBy(x => Player.Character.DistanceTo2D(x.EntrancePosition)))
            {
                MissionInformation locationInfo = new MissionInformation(bl.Name, "", bl.DirectoryInfo(Time.CurrentHour, Player.Character.DistanceTo2D(bl.EntrancePosition)));
                if (bl.HasBannerImage)
                {
                    locationInfo.Logo = new MissionLogo(Game.CreateTextureFromFile($"Plugins\\LosSantosRED\\images\\{bl.BannerImagePath}"));
                }
                missionInfoList.Add(locationInfo);
            }
            TabMissionSelectItem basicLocationTypeList = new TabMissionSelectItem(typeName, missionInfoList);
            basicLocationTypeList.OnItemSelect += (selectedItem) =>
            {
                if (selectedItem != null)
                {
                    string streetAddress = selectedItem.ValueList.FirstOrDefault(x => x.Item1 == "Address:")?.Item2;
                    GameLocation toGPS = DirectoryLocations.FirstOrDefault(x => x.Name == selectedItem.Name && x.StreetAddress == streetAddress);
                    if (toGPS != null)
                    {
                        Player.GPSManager.AddGPSRoute(toGPS.Name, toGPS.EntrancePosition, true);
                    }
                }
            };
            items.Add(basicLocationTypeList);
        }
        return items;
    }
}

