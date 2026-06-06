using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Locations;
using Rage;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Windows.Media;
using System.Xml.Linq;
using static DispatchScannerFiles;


public class BusinessesTab : ITabbableMenu
{
    private IGangRelateable Player;
    private TabSubmenuItem TabItem;
    private TabView TabView;
    public BusinessesTab(IGangRelateable player, TabView tabView)
    {
        Player = player;
        TabView = tabView;
    }
    public void AddItems()
    {
        List<TabItem> items = new List<TabItem>();
        bool addedItems = false;
        foreach (GameLocation business in Player.Properties.Businesses)
        {
            string ListEntryText = $"{business.Name} - ~p~{business.ZoneName}~s~";
            string DescriptionHeaderText = $"{business.Name}";
            string LocationText = business.FullStreetAddress;

            string DescriptionText = $"~n~Location: {LocationText}";

            DescriptionText += $"~n~Select to set ~r~GPS~s~";

            TabItem tItem = new TabTextItem(ListEntryText, DescriptionHeaderText, DescriptionText);
            tItem.Activated += (s, e) =>
            {
                Player.GPSManager.AddGPSRoute(business.Name, business.EntrancePosition, true);
            };
            items.Add(tItem);
            addedItems = true;
        }
        if (addedItems)
        {
            TabView.AddTab(new TabSubmenuItem("Businesses", items));
        }
        else
        {
            TabView.AddTab(new TabItem("Businesses"));
        }
    }
}
