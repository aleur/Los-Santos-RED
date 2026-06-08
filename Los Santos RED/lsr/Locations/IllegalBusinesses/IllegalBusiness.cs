using LosSantosRED.lsr.Interface;
using Mod;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

[Serializable]
public class IllegalBusiness
{
    #region Property Management
    public int PurchasePrice { get; set; }
    public int DemolishPrice { get; set; }
    public bool CashPurchaseOnly { get; set; } // Keep this for more user customization.
    #endregion

    #region DirtyMoney
    public int PayoutFrequency { get; set; }
    public int PayoutMin { get; set; }
    public int PayoutMax { get; set; }
    #endregion

    #region ModItems
    public List<string> CraftingFlags { get; set; }
    public List<string> PossibleModItemPayouts { get; set; }
    public int ModItemPayoutAmountMin { get; set; }
    public int ModItemPayoutAmountMax { get; set; }
    public int ModItemProductionEfficiency { get; set; }
    public int ModItemPayoutFrequency { get; set; }
    #endregion

    public IllegalBusiness() : base()
    {

    }
}
