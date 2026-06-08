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
    public int MaxSalesPrice { get; set; } = -1;
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
        if (loc.PurchasePrice < 0) loc.PurchasePrice = PurchasePrice;
        if (loc.SalesPrice < 0) loc.SalesPrice = SalesPrice;
        if (loc.IsCashPurchaseOnly == null) loc.CashPurchaseOnly = CashPurchaseOnly;
        if (loc.PayoutFrequency < 0) loc.PayoutFrequency = PayoutFrequency;
        if (loc.PayoutMin < 0) loc.PayoutMin = PayoutMin;
        if (loc.PayoutMax < 0) loc.PayoutMax = PayoutMax;
        if (loc.GrowthPercentage < 0) loc.GrowthPercentage = GrowthPercentage;
        if (loc.RacketeeringAmountMin < 0) loc.RacketeeringAmountMin = RacketeeringAmountMin;
        if (loc.RacketeeringAmountMax < 0) loc.RacketeeringAmountMax = RacketeeringAmountMax;
        if (loc.MinPriceRefreshHours < 0) loc.MinPriceRefreshHours = MinPriceRefreshHours;
        if (loc.MaxPriceRefreshHours < 0) loc.MaxPriceRefreshHours = MaxPriceRefreshHours;
        if (loc.MinRestockHours < 0) loc.MinRestockHours = MinRestockHours;
        if (loc.MaxRestockHours < 0) loc.MaxRestockHours = MaxRestockHours;
        if (loc.RegisterCashMin < 0) loc.RegisterCashMin = RegisterCashMin;
        if (loc.RegisterCashMax < 0) loc.RegisterCashMax = RegisterCashMax;
    }
}

