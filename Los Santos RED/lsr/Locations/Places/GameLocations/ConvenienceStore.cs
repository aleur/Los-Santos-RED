using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public class ConvenienceStore : GameLocation, IGasPumpable
{
    public ConvenienceStore() : base()
    {

    }
    public override string TypeName { get; set; } = "Convenience Store";
    public override int MapIcon { get; set; } = (int)BlipSprite.CriminalHoldups;
    public override string ButtonPromptText { get; set; }
    public int PricePerGallon { get; set; } = 3;
    public override int? PurchasePrice { get; set; }
    public override int? SalesPrice { get; set; }
    public override int? PayoutMin { get; set; }
    public override int? PayoutMax { get; set; }
    public ConvenienceStore(Vector3 _EntrancePosition, float _EntranceHeading, string _Name, string _Description, string menuID) : base(_EntrancePosition, _EntranceHeading, _Name, _Description)
    {
        MenuID = menuID;
    }
    public override bool CanCurrentlyInteract(ILocationInteractable player)
    {
        ButtonPromptText = $"Shop At {Name}";
        return true;
    }
    public override void AddLocation(PossibleLocations possibleLocations)
    {
        possibleLocations.ConvenienceStores.Add(this);
        base.AddLocation(possibleLocations);
    }
    protected override void OnPurchased()
    {
        Player.BankAccounts.GiveMoney(-1 * PurchasePrice ?? 0, true);
        IsOwned = true;
        Player.Properties.AddOwnedLocation(this);
        PlaySuccessSound();
        DisplayMessage("~g~Purchased", $"Thank you for purchasing {Name}");
        DatePayoutPaid = Time.CurrentDateTime;
        DatePayoutDue = DatePayoutPaid.AddDays(PayoutFrequency ?? 0);
        CurrentSalesPrice = SalesPrice ?? 0;
    }
}

