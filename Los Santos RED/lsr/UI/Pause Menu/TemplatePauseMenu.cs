using LosSantosRED.lsr.Data;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Locations;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class TemplatePauseMenu
{
    private ISaveable Player;
    private TabView tabView;
    private ITimeControllable Time;
    private WorldTemplateTab NewWorldTemplateTab;
    private ISettingsProvideable Settings;
    private IEntityProvideable World;
    private IWorldTemplates WorldTemplates;
    public TemplatePauseMenu(ISaveable player, ITimeControllable time, ISettingsProvideable settings, IWorldTemplates worldTemplates, IEntityProvideable world)
    {
        Player = player;
        Time = time;
        Settings = settings;
        WorldTemplates = worldTemplates;
        World = world;
    }
    public void Setup()
    {
        tabView = new TabView("Los Santos ~r~RED~s~ World Template Manager");
        tabView.Tabs.Clear();
        tabView.ScrollTabs = true;
        tabView.OnMenuClose += (s, e) =>
        {
            Game.IsPaused = false;
        };
        Game.RawFrameRender += (s, e) => tabView.DrawTextures(e.Graphics);
        NewWorldTemplateTab = new WorldTemplateTab(Player, Time, Settings, WorldTemplates,tabView,World);
    }
    public void Toggle()
    {
        if (!TabView.IsAnyPauseMenuVisible)
        {
            if (!tabView.Visible)
            {
                UpdateMenu();
                Game.IsPaused = true;
            }
            tabView.Visible = !tabView.Visible;
        }
    }
    public void Update()
    {
        tabView.Update();
        if (tabView.Visible)
        {
            tabView.Money = Time.CurrentDateTime.ToString("ddd, dd MMM yyyy hh:mm tt");
        }
    }
    private void UpdateMenu()
    {
        //tabView.MoneySubtitle = Player.BankAccounts.TotalMoney.ToString("C0");
        //tabView.Name = Player.PlayerName;
        //tabView.Money = Time.CurrentTime;
        tabView.Tabs.Clear();

        NewWorldTemplateTab.AddTemplateItems();

        tabView.RefreshIndex();
    }
}