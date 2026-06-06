using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class PropertyMenu
{
    public PropertyMenu()
    {

    }
    public PropertyMenu(string iD, string name)
    {
        ID = iD;
        Name = name;
    }
    public PropertyMenu(string iD, string name, int purchasePrice, int salesPrice)
    {
        ID = iD;
        Name = name;
        PurchasePrice = purchasePrice;
        SalesPrice = salesPrice;
    }
    public string ID { get; set; }
    public string Name { get; set; }
    //
    public int PurchasePrice { get; set; }
    public int SalesPrice { get; set; }
    public int MaxSalesPrice { get; set; }
    public bool CashPurchaseOnly { get; set; }
    public int PayoutFrequency { get; set; }
    public int PayoutMin { get; set; }
    public int PayoutMax { get; set; }
    public int GrowthPercentage { get; set; }
    public int RacketeeringAmountMin { get; set; }
    public int RacketeeringAmountMax { get; set; }
    public int MinPriceRefreshHours { get; set; }
    public int MaxPriceRefreshHours { get; set; }
    public int MinRestockHours { get; set; }
    public int MaxRestockHours { get; set; }
    public int RegisterCashMin { get; set; }
    public int RegisterCashMax { get; set; }
    public void OverrideData(GameLocation loc)
    {
        if (loc.PurchasePrice == null) loc.PurchasePrice = PurchasePrice;
        if (loc.SalesPrice == null) loc.SalesPrice = SalesPrice;
        if (loc.CashPurchaseOnly == null) loc.CashPurchaseOnly = CashPurchaseOnly;
        if (loc.PayoutFrequency == null) loc.PayoutFrequency = PayoutFrequency;
        if (loc.PayoutMin == null) loc.PayoutMin = PayoutMin;
        if (loc.PayoutMax == null) loc.PayoutMax = PayoutMax;
        if (loc.GrowthPercentage == null) loc.GrowthPercentage = GrowthPercentage;
        if (loc.RacketeeringAmountMin == null) loc.RacketeeringAmountMin = RacketeeringAmountMin;
        if (loc.RacketeeringAmountMax == null) loc.RacketeeringAmountMax = RacketeeringAmountMax;
        if (loc.MinPriceRefreshHours == null) loc.MinPriceRefreshHours = MinPriceRefreshHours;
        if (loc.MaxPriceRefreshHours == null) loc.MaxPriceRefreshHours = MaxPriceRefreshHours;
        if (loc.MinRestockHours == null) loc.MinRestockHours = MinRestockHours;
        if (loc.MaxRestockHours == null) loc.MaxRestockHours = MaxRestockHours;
        if (loc.RegisterCashMin == null) loc.RegisterCashMin = RegisterCashMin;
        if (loc.RegisterCashMax == null) loc.RegisterCashMax = RegisterCashMax;
    }
}

