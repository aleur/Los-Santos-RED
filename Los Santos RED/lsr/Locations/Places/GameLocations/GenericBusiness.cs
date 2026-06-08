using LosSantosRED.lsr.Data.Interface;
using LosSantosRED.lsr.Interface;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public class GenericBusiness : GameLocation, ILocationSetupable, IPayoutDisbursable, ICraftable
{
    private UIMenu OfferSubMenu;
    private string CanPurchaseRightLabel => $"{PurchasePrice:C0}";
    private UIMenuItem PurchaseBusinessMenuItem;
    private UIMenuItem SellBusinessItem;
    private UIMenu outfitsSubMenu;
    private bool KeepInteractionGoing;
    private UIMenuNumericScrollerItem<int> RestMenuItem;

    public GenericBusiness() : base()
    {
    }
    public void Setup()
    {
        if (SimpleInventory == null)
        {
            SimpleInventory = new SimpleInventory(Settings);
        }
        if (WeaponStorage == null)
        {
            WeaponStorage = new WeaponStorage(Settings);
        }
        if (CashStorage == null)
        {
            CashStorage = new CashStorage();
        }
    }
    public override string TypeName { get; set; } = "Business";
    public override int MapIcon { get; set; } = (int)BlipSprite.BusinessForSale;
    public bool IsPayoutInModItems => PossibleModItemPayouts != null && PossibleModItemPayouts.Any();
    public List<string> PossibleModItemPayouts { get; set; }
    public int ModItemPayoutAmount { get; set; } = 1;
    [XmlIgnore]
    public string ModItemToPayout { get; set; }
    [XmlIgnore]
    public SimpleInventory SimpleInventory { get; set; }
    [XmlIgnore]
    public WeaponStorage WeaponStorage { get; set; }
    [XmlIgnore]
    public CashStorage CashStorage { get; set; }
    [XmlIgnore]
    public BusinessInterior BusinessInterior { get; set; }
    public bool CanBuy => !IsOwned && PurchasePrice > 0;
    public string CraftingFlag { get; set; } = null;
    public GenericBusiness(Vector3 _EntrancePosition, float _EntranceHeading, string _Name, string _Description) : base(_EntrancePosition, _EntranceHeading, _Name, _Description)
    {

    }
    public override List<Tuple<string, string>> DirectoryInfo(int currentHour, float distanceTo)
    {
        List<Tuple<string, string>> BaseList = base.DirectoryInfo(currentHour, distanceTo).ToList();
        return BaseList;
    }
    public override void AddLocation(PossibleLocations possibleLocations)
    {
        possibleLocations.Businesses.Add(this);
        base.AddLocation(possibleLocations);
    }/*
    public override void OnInteract()
    {
        if (BusinessInterior != null && BusinessInterior.IsTeleportEntry && IsOwned)
        {
            DoEntranceCamera(false);
            BusinessInterior.SetBusiness(this);
            BusinessInterior.Teleport(Player, this, StoreCamera);
        }
        else
        {
            StandardInteract(null, false);
        }
    }
    public override void StoreData(IShopMenus shopMenus, IAgencies agencies, IGangs gangs, IZones zones, IJurisdictions jurisdictions, IGangTerritories gangTerritories, INameProvideable Names, ICrimes Crimes, IPedGroups PedGroups, IEntityProvideable world,
    IStreets streets, ILocationTypes locationTypes, ISettingsProvideable settings, IPlateTypes plateTypes, IOrganizations associations, IContacts contacts, IInteriors interiors,
        ILocationInteractable player, IModItems modItems, IWeapons weapons, ITimeControllable time, IPlacesOfInterest placesOfInterest, IIssuableWeapons issuableWeapons, IHeads heads, IDispatchablePeople dispatchablePeople, ModDataFileManager modDataFileManager)
    {
        base.StoreData(shopMenus, agencies, gangs, zones, jurisdictions, gangTerritories, Names, Crimes, PedGroups, world, streets, locationTypes, settings, plateTypes, associations, contacts, interiors, player, modItems, weapons, time, placesOfInterest, issuableWeapons, heads, dispatchablePeople, modDataFileManager);
        if (HasInterior)
        {
            BusinessInterior = interiors.PossibleInteriors.BusinessInteriors.Where(x => x.LocalID == InteriorID).FirstOrDefault();
            interior = BusinessInterior;
            if (BusinessInterior != null)
            {
                BusinessInterior.SetBusiness(this);
            }
        }


        if(IsPayoutInModItems && PossibleModItemPayouts != null)
        {
            ModItemToPayout = PossibleModItemPayouts.OrderBy(x => x).FirstOrDefault();
        }
    }
    public override void StandardInteract(LocationCamera locationCamera, bool isInside)
    {
        Player.ActivityManager.IsInteractingWithLocation = true;
        CanInteract = false;
        Player.IsTransacting = true;
        GameFiber.StartNew(delegate
        {
            try
            {
                SetupLocationCamera(locationCamera, isInside, false);
                CreateInteractionMenu();
                InteractionMenu.Visible = true;
                if (!HasBannerImage)
                {
                    InteractionMenu.SetBannerType(EntryPoint.LSRedColor);
                }
                GenerateBusinessMenu(isInside);
                while (IsAnyMenuVisible || Time.IsFastForwarding)
                {
                    MenuPool.ProcessMenus();
                    GameFiber.Yield();
                }
                DisposeInteractionMenu();
                DisposeCamera(isInside);
                DisposeInterior();
                Player.ActivityManager.IsInteractingWithLocation = false;
                CanInteract = true;
                Player.IsTransacting = false;
            }
            catch (Exception ex)
            {
                EntryPoint.WriteToConsole("Location Interaction" + ex.Message + " " + ex.StackTrace, 0);
                EntryPoint.ModController.CrashUnload();
            }
        }, "BusinessInteract");
    }*/
    public virtual int CalculatePayoutAmount(int daysSinceLastPayment)
    {
        int payout = RandomItems.GetRandomNumberInt((int)PayoutMin, (int)PayoutMax);
        int payoutAmount = payout * daysSinceLastPayment / (int) PayoutFrequency;
        return payoutAmount;
    }
    //public void Payout(IPropertyOwnable player, ITimeReportable time)
    //{
    //    try
    //    {
    //        if (player == null)
    //        {
    //            return;
    //        }
    //        int daysSinceLastPayment = (time.CurrentDateTime - DatePayoutPaid).Days;
    //        DatePayoutPaid = time.CurrentDateTime;
    //        DatePayoutDue = DatePayoutPaid.AddDays(PayoutFrequency);

    //        if (IsPayoutInModItems)
    //        {
    //            ModItem itemToAdd = ModItems.Get(ModItemToPayout);
    //            int payoutAmount = (daysSinceLastPayment / PayoutFrequency) * ModItemPayoutAmount;
    //            if(payoutAmount>0)
    //            {
    //                SimpleInventory.Add(itemToAdd, payoutAmount);
    //                UpdateSalesPrice();
    //            }
    //        }
    //        else
    //        {
    //            int payoutAmount = CalculatePayoutAmount(daysSinceLastPayment);
    //            CashStorage.StoredCash = CashStorage.StoredCash + payoutAmount;
    //            UpdateSalesPrice();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        EntryPoint.WriteToConsole($"{ex.Message} {ex.StackTrace}", 0);
    //        Game.DisplayNotification($"ERROR in Payout {ex.Message}");
    //    }
    //}


    public void Payout(IPropertyOwnable player, ITimeReportable time)
    {
        try
        {
            if (player == null)
            {
                return;
            }
            double daysElapsed = (time.CurrentDateTime - DatePayoutPaid).TotalDays;
            if(daysElapsed < PayoutFrequency)
            {
                return;
            }
            int completedCycles = (int)(daysElapsed / PayoutFrequency);/*
            if (IsPayoutInModItems)
            {
                ModItem itemToAdd = ModItems.Get(ModItemToPayout);
                int totalItems = completedCycles * ModItemPayoutAmount;
                if (totalItems > 0)
                {
                    SimpleInventory.Add(itemToAdd, totalItems);
                }
            }
            else
            {
                int payoutAmount = CalculatePayoutAmount(completedCycles);
                CashStorage.StoredCash += payoutAmount;
            }*/
            UpdateSalesPrice();
            DatePayoutPaid = DatePayoutPaid.AddDays(completedCycles * PayoutFrequency);
            DatePayoutDue = DatePayoutPaid.AddDays(PayoutFrequency);       
        }
        catch (Exception ex)
        {
            EntryPoint.WriteToConsole($"{ex.Message} {ex.StackTrace}", 0);
        }
    }
    private void UpdateSalesPrice()
    {
        int salesPriceToAdd = (int)(PurchasePrice * (GrowthPercentage / 100.0));
        CurrentSalesPrice = Math.Min(CurrentSalesPrice + salesPriceToAdd, MaxSalesPrice == 0 ? (int)(PurchasePrice * 1.2f) : (int) MaxSalesPrice);
    }
    public override void Reset()
    {
        IsOwned = false;
        WeaponStorage.Reset();
        SimpleInventory.Reset();
        UpdateStoredData();
        CashStorage.Reset();
        ModItemToPayout = String.Empty;
    }

    public override bool Purchase()
    {
        if (CanBuy && Player.BankAccounts.GetMoney(true) >= PurchasePrice)
        {
            OnPurchased();
            return true;
        }
        PlayErrorSound();
        DisplayMessage("~r~Purchased Failed", "We are sorry, we are unable to complete this purchase. Please make sure you have the funds.");
        return false;
    }
    private void UpdateStoredData()
    {
        //ButtonPromptText = GetButtonPromptText();
        //if (Blip.Exists())
        //{
        //    Blip.Sprite = IsOwned ? BlipSprite.Business : BlipSprite.BusinessForSale;
        //}
    }
    public override void UpdateBlip(ITimeReportable time)
    {
        if (Blip.Exists())
        {
            MapIconColorString = (IsOwned ? "Green" : "White");
            Blip.Sprite = IsOwned ? BlipSprite.Business : BlipSprite.BusinessForSale;
            Blip.Color = Color.FromName(IsOwned ? "Green" : "White");
        }
        if (IsOwned)
        {
            return;
        }
        base.UpdateBlip(time);
    }
    public void RefreshUI()
    {
        UpdateStoredData();
    }
    public override string GetInquireDescription()
    {
        if(IsPayoutInModItems)
        {
            if(PossibleModItemPayouts != null && PossibleModItemPayouts.Any())
            {
                return $"{ModItemPayoutAmount} every {PayoutFrequency} day(s)" + "~n~Producible Items:~n~" + string.Join(", ", PossibleModItemPayouts); 
            }
            return $"{ModItemPayoutAmount} every {PayoutFrequency} day(s)";
        }
        else
        {
            return $"Earn between {PayoutMin:C0} and {PayoutMax:C0} every {PayoutFrequency} day(s)";
        }
    }
    public override void HandleOwnedLocation(IPropertyOwnable player, ITimeReportable time)
    {
        Payout(player, time);
    }
    public override SavedGameLocation GetSaveData()
    {
        SavedBusiness myBiz = new SavedBusiness(Name, IsOwned);
        if (IsOwned)
        {
            myBiz.DateOfLastPayout = DatePayoutPaid;
            myBiz.PayoutDate = DatePayoutDue;
            myBiz.ModItemToPayout = ModItemToPayout;
            myBiz.EntrancePosition = EntrancePosition;
            myBiz.CurrentSalesPrice = CurrentSalesPrice;
            if (WeaponStorage != null)
            {
                myBiz.WeaponInventory = new List<StoredWeapon>();
                foreach (StoredWeapon storedWeapon in WeaponStorage.StoredWeapons)
                {
                    myBiz.WeaponInventory.Add(storedWeapon.Copy());
                }
            }
            if (SimpleInventory != null)
            {
                myBiz.InventoryItems = new List<InventorySave>();
                foreach (InventoryItem ii in SimpleInventory.ItemsList)
                {
                    myBiz.InventoryItems.Add(new InventorySave(ii.ModItem?.Name, ii.RemainingPercent));
                }
            }
            if (CashStorage != null)
            {
                myBiz.StoredCash = CashStorage.StoredCash;
            }
        }
        return myBiz;
    }
    public override TabMissionSelectItem GetUIInformation()
    {
        MissionLogo missionLogo = null;
        if (HasBannerImage)
        {
            missionLogo = new MissionLogo(Game.CreateTextureFromFile($"Plugins\\LosSantosRED\\images\\{BannerImagePath}"));
        }
        List<MissionInformation> propertyInfos = new List<MissionInformation>();
        List<Tuple<string, string>> financialTuples = AddFinancials();
        financialTuples.Add(Tuple.Create<string, string>("Mode of Payment", IsPayoutInModItems ? ModItemToPayout : "Cash"));
        MissionInformation financialInformation = new MissionInformation("Financials", "", financialTuples);
        financialInformation.Logo = missionLogo;
        propertyInfos.Add(financialInformation);
        List<Tuple<string, string>> storageTuples = new List<Tuple<string, string>>();
        foreach (InventoryItem item in SimpleInventory.ItemsList)
        {
            storageTuples.Add(Tuple.Create<string, string>(item.ModItem.Name, item.Amount.ToString()));
        }
        storageTuples.Add(Tuple.Create<string, string>("Cash Storage", $"${CashStorage.StoredCash}"));
        MissionInformation storageInformation = new MissionInformation("Storage", "", storageTuples);
        storageInformation.Logo = missionLogo;
        propertyInfos.Add(storageInformation);
        List<Tuple<string, string>> gpsTuple = AddGPS();
        MissionInformation gpsInformation = new MissionInformation("GPS", "", gpsTuple);
        gpsInformation.Logo = missionLogo;
        propertyInfos.Add(gpsInformation);
        return new TabMissionSelectItem($"{Name} - {ZoneName}", propertyInfos);
    }
}