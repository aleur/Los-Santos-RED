using LosSantosRED.lsr.Interface;
using Rage;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ResidencesTab : ITabbableMenu
{
    private IGangRelateable Player;
    private TabView TabView;
    private TabSubmenuItem TabItem;
    public List<TabItem> items;
    public ResidencesTab(IGangRelateable player, TabView tabView)
    {
        Player = player;
        TabView = tabView;
    }
    public void AddItems()
    {
        List<TabItem> items = new List<TabItem>();
        bool addedItems = false;
        foreach (GameLocation residence in Player.Properties.Residences)
        {

            string ListEntryText = $"{residence.Name} - ~p~{residence.ZoneName}~s~";
            string DescriptionHeaderText = $"{residence.Name}";
            string LocationText = residence.FullStreetAddress;

            string DescriptionText = $"~n~Location: {LocationText}";

            DescriptionText += $"~n~Select to set ~r~GPS~s~";

            TabItem tItem = new TabTextItem(ListEntryText, DescriptionHeaderText, DescriptionText);
            tItem.Activated += (s, e) =>
            {
                Player.GPSManager.AddGPSRoute(residence.Name, residence.EntrancePosition, true);
            };
            items.Add(tItem);
            addedItems = true;
        }
        if (addedItems)
        {
            TabView.AddTab(new TabSubmenuItem("Residences", items));
        }
        else
        {
            TabView.AddTab(new TabItem("Residences"));
        }
    }
}
