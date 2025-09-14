using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


public class DebugConfigSubMenu : DebugSubMenu
{
    private UI UI;
    private IGameConfigs GameConfigs;
    private UIMenuItem SaveConfigsToFile;
    private UIMenuItem AddConfigToFile;
    private GameConfig NewConfig = new GameConfig("New Config", "Default");

    public DebugConfigSubMenu(UIMenu debug, MenuPool menuPool, IActionable player, UI ui, IGameConfigs gameConfigs) : base(debug, menuPool, player)
    {
        UI = ui;
        GameConfigs = gameConfigs;
    }

    public override void AddItems()
    {
        SubMenu = MenuPool.AddSubMenu(Debug, "Config Manager");
        Debug.MenuItems[Debug.MenuItems.Count() - 1].Description = "Change, Save, and Load Configs.";
        SubMenu.SetBannerType(EntryPoint.LSRedColor);
        CreateMenu();
    }
    public override void Update()
    {
        CreateMenu();
    }
    private void CreateMenu()
    {
        SubMenu.Clear();

        SubMenu.Clear();
        UIMenuItem ShowGameConfigsMenu = new UIMenuItem("Configs", "Shows a list of configurations.");
        ShowGameConfigsMenu.RightBadge = UIMenuItem.BadgeStyle.Mask;
        ShowGameConfigsMenu.Activated += (s, e) =>
        {
            UI.ConfigPauseMenu.Toggle();
            SubMenu.Visible = false;
        };
        SubMenu.AddItem(ShowGameConfigsMenu);

        UIMenu ShowConfigCuratorMenu = MenuPool.AddSubMenu(SubMenu, "Config Curator");
        ShowConfigCuratorMenu.SetBannerType(EntryPoint.LSRedColor);

        CreateCreateConfigsMenu(ShowConfigCuratorMenu);
        CreateEditConfigsMenu(ShowConfigCuratorMenu);
    }


    private void CreateEditConfigsMenu(UIMenu menu)
    {
        UIMenu editConfigMenu = MenuPool.AddSubMenu(menu, "Edit Configs");
        editConfigMenu.SetBannerType(EntryPoint.LSRedColor);
        SaveConfigsToFile = new UIMenuItem("Save Changes", "Saves the changes to the config");
        editConfigMenu.AddItem(SaveConfigsToFile);

        editConfigMenu.OnItemSelect += OnItemSelect;
        editConfigMenu.OnListChange += OnListChange;

        if (GameConfigs.SuffixConfigList != null && GameConfigs.SuffixConfigList.Any())
        {
            foreach (GameConfig config in GameConfigs.CustomConfigList)
                if (config != null) CreateItemSubMenu(editConfigMenu, $"{config.ConfigName}", config);
        }
    }
    private void CreateCreateConfigsMenu(UIMenu menu)
    {
        UIMenu createConfigMenu = MenuPool.AddSubMenu(menu, "Create Config");
        createConfigMenu.SetBannerType(EntryPoint.LSRedColor);
        AddConfigToFile = new UIMenuItem("Save Config", "Adds config to CustomConfigs.xml");
        createConfigMenu.AddItem(AddConfigToFile);

        createConfigMenu.OnItemSelect += OnItemSelect;
        createConfigMenu.OnListChange += OnListChange;

        NewConfig = new GameConfig("New Config", "Default");
        CreateItemSubMenu(createConfigMenu, "Create New Config", NewConfig);
    }


    private void UpdateSettings(UIMenu sender, UIMenuItem selectedItem, int index, PropertyInfo[] MyProperties, GameConfig ToSet)
    {
        string MySettingName = selectedItem.Text.Split(':')[0];
        PropertyInfo MySetting = MyProperties.FirstOrDefault(x => x.Name == MySettingName);
        if (MySetting == null || MySetting.PropertyType == typeof(bool))
            return;
        string Value = NativeHelper.GetKeyboardInput("");

        if (MySetting.PropertyType == typeof(float))
        {
            if (float.TryParse(Value, out float myFloat))
            {
                MySetting.SetValue(ToSet, myFloat);
                selectedItem.Text = $"{MySettingName}: {Value}";
            }
        }
        else if (MySetting.PropertyType == typeof(int))
        {
            if (int.TryParse(Value, out int myInt))
            {
                MySetting.SetValue(ToSet, myInt);
                selectedItem.Text = $"{MySettingName}: {Value}";
            }
        }
        else if (MySetting.PropertyType == typeof(uint))
        {
            if (uint.TryParse(Value, out uint myInt))
            {
                MySetting.SetValue(ToSet, myInt);
                selectedItem.Text = $"{MySettingName}: {Value}";
            }
        }
        else if (MySetting.PropertyType == typeof(string))
        {
            MySetting.SetValue(ToSet, Value);
            selectedItem.Text = $"{MySettingName}: {Value}";
        }
        else if (MySetting.PropertyType == typeof(Keys))
        {
            if (Keys.TryParse(Value, out Keys myInt))
            {
                MySetting.SetValue(ToSet, myInt);
                selectedItem.Text = $"{MySettingName}: {Value}";
            }
        }
        MenuPool.ProcessMenus();
    }
    private void CreateItemSubMenu(UIMenu menu, string name, GameConfig config)
    {
        UIMenu genericMenu = MenuPool.AddSubMenu(menu, name);
        genericMenu.SetBannerType(EntryPoint.LSRedColor);
        genericMenu.Width = 0.5f;

        PropertyInfo[] properties = config.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            string Description = property.Name;
            Description = Description.Substring(0, Math.Min(800, Description.Length));
            if (property.PropertyType == typeof(bool))
            {
                UIMenuCheckboxItem MySetting = new UIMenuCheckboxItem(property.Name, (bool)property.GetValue(config), Description);
                MySetting.CheckboxEvent += (sender, Checked) =>
                {
                    property.SetValue(config, Checked);
                };
                genericMenu.AddItem(MySetting);
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(string) || property.PropertyType == typeof(float) || property.PropertyType == typeof(uint) || property.PropertyType == typeof(Keys))
            {
                UIMenuItem MySetting = new UIMenuItem($"{property.Name}: {property.GetValue(config)}", Description);
                MySetting.Activated += (sender, selectedItem) =>
                {
                    UpdateSettings(sender, selectedItem, 0, config.GetType().GetProperties(), config);
                };
                genericMenu.AddItem(MySetting);
            }
        }
    }
    private void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
    {
        EntryPoint.WriteToConsole($"{selectedItem == SaveConfigsToFile}, {selectedItem == AddConfigToFile}");
        if (selectedItem == SaveConfigsToFile)
        {
            GameConfigs.SerializeAllSettings();
            Update(); // Needed to update UIMenu to update CustomConfigList. Can't place below conditions or else SaveConfigsToFile obj not recognized for some reason.
        }
        else if (selectedItem == AddConfigToFile)
        {
            GameConfigs.CustomConfigList.Add(NewConfig);
            GameConfigs.SerializeAllSettings();
            Update();
        }
        sender.Visible = false;
    }
    private void OnListChange(UIMenu sender, UIMenuListItem list, int index)
    {
    }
}

