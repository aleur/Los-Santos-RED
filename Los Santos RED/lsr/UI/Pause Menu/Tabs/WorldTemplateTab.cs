using LosSantosRED.lsr.Data;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DispatchScannerFiles;


public class WorldTemplateTab
{
    private ISaveable Player; // Player only here for Button Prompts
    private TabView TabView;
    private List<TabItem> items;

    private IWorldTemplates WorldTemplates;
    private IEntityProvideable World;
    private ITimeControllable Time;
    private ISettingsProvideable Settings;

    public WorldTemplateTab(ISaveable player, ITimeControllable time, ISettingsProvideable settings, IWorldTemplates worldTemplates, TabView tabView, IEntityProvideable world)
    {
        Player = player;
        Time = time;
        Settings = settings;
        WorldTemplates = worldTemplates;
        TabView = tabView;
        World = world;
    }
    public void AddTemplateItems()
    {
        List<UIMenuItem> templateListItems = new List<UIMenuItem>();
        UIMenuItem templateCount = new UIMenuItem($"Number of Templates: {WorldTemplates.WorldTemplateList.Count()}", "") { Enabled = false };
        templateListItems.Add(templateCount);
        if (WorldTemplates.WorldTemplateList != null && WorldTemplates.WorldTemplateList.Any())
        {
            int maxNumber = WorldTemplates.WorldTemplateList.Max(x => x.TemplateNumber);
            for (int i = 1; i <= maxNumber; i++)
            {
                UIMenuItem loadItem;
                WorldTemplate template = WorldTemplates.WorldTemplateList.FirstOrDefault(x => x.TemplateNumber == i);
                if (template != null)
                {
                    loadItem = new UIMenuItem(template.worldName, "") {  };
                    loadItem.Activated += (s, e) =>
                    {
                        SimpleWarning popUpWarning = new SimpleWarning("Load", "Are you sure you want to load this template", "", Player.ButtonPrompts, Settings);
                        popUpWarning.Show();
                        if (popUpWarning.IsAccepted)
                        {
                            Game.FadeScreenOut(1000, true);
                            TabView.Visible = false;
                            Game.IsPaused = false;

                            //EntryPoint.ModController.IsLoadingWorld = true;
                            EntryPoint.ModController.Dispose();

                            WorldTemplates.Load(template);
                        }
                    };
                }
                else
                {
                    loadItem = new UIMenuItem($"{i.ToString("D2")} - Empty Template", "") { Enabled = false };
                }
                templateListItems.Add(loadItem);
            }
        }
        TabInteractiveListItem interactiveListItem2 = new TabInteractiveListItem("LOAD", templateListItems);
        TabView.AddTab(interactiveListItem2);
    }
}