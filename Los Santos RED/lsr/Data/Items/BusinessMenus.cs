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
            foreach (PropertyMenu PropertyMenu in additivePossibleItems.PropertyMenuList)
            {
                PossibleBusinessMenus.PropertyMenuList.RemoveAll(x => x.ID == PropertyMenu.ID);
                PossibleBusinessMenus.PropertyMenuList.Add(PropertyMenu);
            }
        }
    }
    private List<BusinessMenu> AllMenus()
    {
        List<BusinessMenu> AllBusinessMenus = new List<BusinessMenu>();
        AllBusinessMenus.AddRange(PossibleBusinessMenus.BusinessMenuList);
        return AllBusinessMenus;
    }
    private void DefaultConfig()
    {
        SetupBusinessMenus();
        SetupPropertyMenus();
        Serialization.SerializeParam(PossibleBusinessMenus, ConfigFileName);
    }
    private void SetupBusinessMenus()
    {
        PossibleBusinessMenus.BusinessMenuList.AddRange(
            new List<BusinessMenu>()
            {
                new BusinessMenu("ConvenienceStoreMenu", "ConvenienceStoreProperty"),
                new BusinessMenu("GenericBusinessMenu", "GenericBusinessProperty"),
                new BusinessMenu("SmallAirstripMenu", "SmallAirstripProperty"),
                new BusinessMenu("DealershipMenu", "DealershipProperty"),
            }
        );
    }
    private void SetupPropertyMenus()
    {
        PossibleBusinessMenus.PropertyMenuList.AddRange(
            new List<PropertyMenu>()
            {
                new PropertyMenu("ConvenienceStoreProperty", 50000, 10000) { MaxSalesPrice = 0, CashPurchaseOnly = false, PayoutFrequency = 7, PayoutMin = 1000, PayoutMax = 5000, GrowthPercentage = 20, 
                                                                                           RacketeeringAmountMin = 500, MinPriceRefreshHours = 0, MaxPriceRefreshHours = 0, MinRestockHours = 0, MaxRestockHours = 0, 
                                                                                           RacketeeringAmountMax = 1000, RegisterCashMax = 1550, RegisterCashMin = 250 },
                new PropertyMenu("GenericBusinessProperty", 100000, 50000) { MaxSalesPrice = 0, CashPurchaseOnly = false, PayoutFrequency = 7, PayoutMin = 1000, PayoutMax = 5000, GrowthPercentage = 20,
                                                                                           RacketeeringAmountMin = 500, MinPriceRefreshHours = 0, MaxPriceRefreshHours = 0, MinRestockHours = 0, MaxRestockHours = 0,
                                                                                           RacketeeringAmountMax = 1000, RegisterCashMax = 1550, RegisterCashMin = 250 },
                new PropertyMenu("SmallAirstripProperty", 200000, 100000) { MaxSalesPrice = 0, CashPurchaseOnly = false, PayoutFrequency = 7, PayoutMin = 10000, PayoutMax = 100000, GrowthPercentage = 50,
                                                                                           RacketeeringAmountMin = 500, MinPriceRefreshHours = 0, MaxPriceRefreshHours = 0, MinRestockHours = 0, MaxRestockHours = 0,
                                                                                           RacketeeringAmountMax = 1000, RegisterCashMax = 1550, RegisterCashMin = 250 },
                new PropertyMenu("DealershipProperty", 500000, 200000) { MaxSalesPrice = 0, CashPurchaseOnly = false, PayoutFrequency = 7, PayoutMin = 10000, PayoutMax = 100000, GrowthPercentage = 50,
                                                                                           RacketeeringAmountMin = 5000, MinPriceRefreshHours = 0, MaxPriceRefreshHours = 0, MinRestockHours = 0, MaxRestockHours = 0,
                                                                                           RacketeeringAmountMax = 10000, RegisterCashMax = 3050, RegisterCashMin = 1000 }
            }
        );
    }
    public BusinessMenu GetSpecificBusinessMenu(string menuID)
    {
        return PossibleBusinessMenus.BusinessMenuList.Where(x => x.ID == menuID).FirstOrDefault();
    }
    public PropertyMenu GetSpecificPropertyMenu(string menuID)
    {
        return PossibleBusinessMenus.PropertyMenuList.Where(x => x.ID == menuID).FirstOrDefault();
    }
    public void Setup()
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

