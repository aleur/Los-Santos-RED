using ExtensionsMethods;

using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;

using System;

using System.Collections.Generic;

using System.IO;
using System.Linq;



public class BusinessMenus : IBusinessMenus
{
    private readonly string ConfigFileName = "Plugins\\LosSantosRED\\BusinessMenus.xml";
    public BusinessMenuTypes PossibleBusinessMenus { get; private set; }
    public BusinessMenus()
    {
        PossibleBusinessMenus = new BusinessMenuTypes();
    }
    public void ReadConfig(string configName)
    {
        string fileName = string.IsNullOrEmpty(configName) ? "BusinessMenus_*.xml" : $"BusinessMenus_{configName}.xml";

        DirectoryInfo LSRDirectory = new DirectoryInfo("Plugins\\LosSantosRED");
        FileInfo ConfigFile = LSRDirectory.GetFiles(fileName).Where(x => !x.Name.Contains("+")).OrderByDescending(x => x.Name).FirstOrDefault();
        if (ConfigFile != null && !configName.Equals("Default"))
        {
            EntryPoint.WriteToConsole($"Loaded Business Menus config  {ConfigFile.FullName}", 0);
            PossibleBusinessMenus = Serialization.DeserializeParam<BusinessMenuTypes>(ConfigFile.FullName);
        }
        else if (File.Exists(ConfigFileName))
        {
            EntryPoint.WriteToConsole($"Loaded Business Menus config  {ConfigFileName}", 0);
            PossibleBusinessMenus = Serialization.DeserializeParam<BusinessMenuTypes>(ConfigFileName);
        }
        else
        {
            EntryPoint.WriteToConsole($"No Business Menus config found, creating default", 0);
            DefaultConfig();
        }
        foreach (FileInfo fileInfo in LSRDirectory.GetFiles("BusinessMenus+_*.xml").OrderByDescending(x => x.Name))
        {
            EntryPoint.WriteToConsole($"Loaded ADDITIVE BUSINESS MENUS config  {fileInfo.FullName}", 0);
            BusinessMenuTypes additivePossibleItems = Serialization.DeserializeParam<BusinessMenuTypes>(fileInfo.FullName);
            foreach (BusinessMenu BusinessMenu in additivePossibleItems.BusinessMenuList)
            {
                PossibleBusinessMenus.BusinessMenuList.RemoveAll(x => x.ID == BusinessMenu.ID);
                PossibleBusinessMenus.BusinessMenuList.Add(BusinessMenu);
            }
        }
    }
    public BusinessMenu GetSpecificMenu(string menuID)
    {
        return PossibleBusinessMenus.BusinessMenuList.Where(x => x.ID == menuID).FirstOrDefault();// BusinessMenuList.Where(x => x.ID == menuID).FirstOrDefault()?.Copy();
    }
    private List<BusinessMenu> AllMenus()
    {
        List<BusinessMenu> AllBusinessMenus = new List<BusinessMenu>();
        AllBusinessMenus.AddRange(PossibleBusinessMenus.BusinessMenuList);
        return AllBusinessMenus;
    }
    private void DefaultConfig()
    {
        // make hella methods
        Serialization.SerializeParam(PossibleBusinessMenus, ConfigFileName);
    }
    public void Setup(IModItems modItems)
    {
        /*
        foreach (BusinessMenu sm in AllMenus())
        {
            int totalItems = sm.Items.Count;
            for (int i = totalItems - 1; i >= 0; i--)
            {
                MenuItem mi = sm.Items[i];
                if (mi != null)
                {
                    mi.ModItem = modItems.Get(mi.ModItemName);
                }
                if (mi.ModItem == null)
                {
                    EntryPoint.WriteToConsole($"Shop Menus ERROR Corresponding Item NOT FOUND {mi.ModItemName} in MENU {sm.Name} REMOVING FROM MENU", 0);
                    sm.Items.RemoveAt(i);
                }
                mi.UpdatePrices();
                mi.UpdateStock();
            }
            if (totalItems == 0)
            {
                EntryPoint.WriteToConsole($"Shop Menus ERROR No Menu Items in MENU {sm.Name}", 0);
            }
        }*/
    }


}

