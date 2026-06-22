using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Mod;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public class Hospital : GameLocation, ILocationRespawnable, ILicensePlatePreviewable
{
    private ShopMenu AgencyMenu;
    private UIMenu TreatmentOptionsMenu;
    private List<MedicalTreatment> MedicalTreatments;
    public Hospital(Vector3 _EntrancePosition, float _EntranceHeading, string _Name, string _Description) : base(_EntrancePosition, _EntranceHeading, _Name, _Description)
    {

    }
    public Hospital() : base()
    {

    }
    public string LicensePlatePreviewText { get; set; } = "UNIT1";
    public override string TypeName { get; set; } = "Hospital";
    public override int MapIcon { get; set; } = (int)BlipSprite.Hospital;
    public Vector3 RespawnLocation { get; set; }
    public float RespawnHeading { get; set; }
    public string TreatmentOptionsID { get; set; } = "DefaultMedicalTreatments";
    [XmlIgnore]
    public override string MenuPromptName { get; set; } = "Hospital";
    public override void StoreData(IShopMenus shopMenus, IAgencies agencies, IGangs gangs, IZones zones, IJurisdictions jurisdictions, IGangTerritories gangTerritories, INameProvideable Names, ICrimes Crimes, IPedGroups PedGroups, IEntityProvideable world,
        IStreets streets, ILocationTypes locationTypes, ISettingsProvideable settings, IPlateTypes plateTypes, IOrganizations associations, IContacts contacts, IInteriors interiors,
        ILocationInteractable player, IModItems modItems, IWeapons weapons, ITimeControllable time, IPlacesOfInterest placesOfInterest, IIssuableWeapons issuableWeapons, IHeads heads, IDispatchablePeople dispatchablePeople, ModDataFileManager modDataFileManager)
    {
        base.StoreData(shopMenus, agencies, gangs, zones, jurisdictions, gangTerritories, Names, Crimes, PedGroups, world, streets, locationTypes, settings, plateTypes, associations, contacts, interiors, player, modItems, weapons, time, placesOfInterest, issuableWeapons, heads, dispatchablePeople, modDataFileManager);
        if (AssignedAgency == null)
        {
            Zone assignedZone = zones.GetZone(EntrancePosition);
            if (assignedZone != null)
            {
                AssignedAgency = assignedZone.AssignedEMSAgency;
            }
            if(AssignedAgency == null && assignedZone != null)
            {
                EntryPoint.WriteToConsole("HOSPITAL FALLBACK TO COUNTY AGENCY");
                AssignedAgency = jurisdictions.GetRespondingAgency(null, assignedZone.CountyID, ResponseType.EMS);
            }
        }
        if(!string.IsNullOrEmpty(TreatmentOptionsID))
        {
            MedicalTreatments = shopMenus.GetMedicalTreatments(TreatmentOptionsID);
            UIMenuCategory = "ShopMenu";
        }
        if (!string.IsNullOrEmpty(BusinessID))
        {
            BusinessMenu = modDataFileManager.BusinessMenus.GetSpecificBusinessMenu(BusinessID);
            if (BusinessMenu != null)
                BusinessMenu.SetupBusiness(this, modDataFileManager.BusinessMenus.GetSpecificPropertyMenu(BusinessMenu.PropertyMenuID));
        }
    }
    public override void AddDistanceOffset(Vector3 offsetToAdd)
    {
        if (RespawnLocation != Vector3.Zero)
        {
            RespawnLocation += offsetToAdd;
        }
        base.AddDistanceOffset(offsetToAdd);
    }
    public override bool CanCurrentlyInteract(ILocationInteractable player)
    {
        ButtonPromptText = $"Enter {Name}";
        return true;
    }
    public override void OnInteract()//ILocationInteractable player, IModItems modItems, IEntityProvideable world, ISettingsProvideable settings, IWeapons weapons, ITimeControllable time, IPlacesOfInterest placesOfInterest)
    {
        //Player = player;
        //ModItems = modItems;
        //World = world;
        //Settings = settings;
        //Weapons = weapons;
        //Time = time;
        if (IsLocationClosed())
        {
            return;
        }
        if (!CanInteract)
        {
            return;
        }
        if (AssignedAgency == null)
        {
            Game.DisplayHelp("No Agency Assigned");
            return;
        }
        if (Interior != null && Interior.IsTeleportEntry)
        {
            DoEntranceCamera(true);
            Interior.Teleport(Player, this, StoreCamera);
        }
        else
        {
            StandardInteract(null, false);
        }
    }
    public override void StandardInteract(LocationCamera locationCamera, bool isInside)
    {
        Player.ActivityManager.IsInteractingWithLocation = true;
        Player.CurrentInteractedLocation = this;
        CanInteract = false;
        Player.IsTransacting = true;
        GameFiber.StartNew(delegate
        {
            try
            {
                SetupLocationCamera(locationCamera, isInside, true);
                CreateInteractionMenu();
                if (MedicalTreatments != null && MedicalTreatments.Any())
                {
                    // placeholder
                }
                if (BusinessMenu != null)
                {
                    Business = new Business(MenuPool, InteractionMenu, BusinessMenu, Player, Time, Settings, this);
                }
                HasMenuSwitch = MedicalTreatments != null && MedicalTreatments.Any() && Business != null;
                if (MedicalTreatments != null && MedicalTreatments.Any())
                {
                    InteractAsOther();
                    UIMenuCategory = "ShopMenu";
                    InteractionMenu.Visible = true;
                    ProcessHospitalMenu();
                }
                else if (BusinessMenu != null)
                {
                    Business.CreateBusinessMenu(ModItems, World, Weapons);
                    UIMenuCategory = "BusinessMenu";
                    InteractionMenu.Visible = true;
                    Business.ProcessBusinessMenu();
                    Business.DisposeBusinessMenu();
                }
                DisposeInteractionMenu();
                DisposeCamera(isInside);
                DisposeInterior();
                ResetInteractBools();
            }
            catch (Exception ex)
            {
                EntryPoint.WriteToConsole("Location Interaction" + ex.Message + " " + ex.StackTrace, 0);
                EntryPoint.ModController.CrashUnload();
            }
        }, "HospitalInteract");
    }
    public override void SwitchMenus()
    {
        if (UIMenuCategory == "ShopMenu")
        {
            InteractAsOther();
            InteractionMenu.Visible = true;
            ProcessHospitalMenu();
        }
        else if (UIMenuCategory == "BusinessMenu")
        {
            Business.CreateBusinessMenu(ModItems, World, Weapons);
            InteractionMenu.Visible = true;
            Business.ProcessBusinessMenu();
            Business.DisposeBusinessMenu();
        }
    }
    private void SetupMenu()
    {
        /* removed for now
        if (Player.IsEMT)
        {
            InteractAsEMT(ModItems, World, Settings, Weapons, Time);
        }
        else
        {
            InteractAsOther();
        }*/
    }
    private void ProcessHospitalMenu()
    {
        while (MenuPool.IsAnyMenuOpen() && UIMenuCategory == "ShopMenu")
        {
            MenuPool.ProcessMenus();
            GameFiber.Yield();
        }
        if (UIMenuCategory == "BusinessMenu")
        {
            MenuPool.Where(x => x != InteractionMenu).ToList().ForEach(x => // To deal with the multiple UIMenus created. Otherwise, causes multiple layers of UI.
            {
                MenuPool.Remove(x);
            });
            InteractionMenu.Clear();
            SwitchMenus();
        }
    }
    private void InteractAsEMT(IModItems modItems, IEntityProvideable world, ISettingsProvideable settings, IWeapons weapons, ITimeControllable time)
    {
        AgencyMenu = AssignedAgency.GenerateMenu(ModItems);
        Transaction = new Transaction(MenuPool, InteractionMenu, AgencyMenu, this);
        Transaction.LicensePlatePreviewable = this;
        if ((VehicleDeliveryLocations == null || !VehicleDeliveryLocations.Any()) && PossibleVehicleSpawns?.Any() == true)
        {
            List<SpawnPlace> places = new List<SpawnPlace>();
            foreach (ConditionalLocation place in PossibleVehicleSpawns)
            {
                places.Add(new SpawnPlace(place.Location, place.Heading));
            }
            Transaction.VehicleDeliveryLocations = places;
        }
        else
        {
            Transaction.VehicleDeliveryLocations = VehicleDeliveryLocations;
        }
        Transaction.VehiclePreviewPosition = VehiclePreviewLocation;
        Transaction.IsFreeItems = true;
        Transaction.IsFreeWeapons = true;
        Transaction.IsFreeVehicles = true;
        Transaction.IsPurchasing = false;
        Transaction.RotateVehiclePreview = false;
        Transaction.CreateTransactionMenu(Player, modItems, world, settings, weapons, time);
        UIMenuCategory = "ShopMenu";
        InteractionMenu.Visible = true;
        Transaction.ProcessTransactionMenu();
        Transaction.DisposeTransactionMenu();
    }
    private void InteractAsOther()
    {
        TreatmentOptionsMenu = MenuPool.AddSubMenu(InteractionMenu, "Treatment Options");
        TreatmentOptionsMenu.SubtitleText = "Pick a Treatment";
        InteractionMenu.MenuItems[InteractionMenu.MenuItems.Count() - 1].Description = "Pick one of our state of the art treatment options!";
        if (MedicalTreatments != null && MedicalTreatments.Any())
        {
            foreach (MedicalTreatment medicalTreatment in MedicalTreatments)
            {
                medicalTreatment.AddToMenu(this, TreatmentOptionsMenu,Player);
            }
        }
        UIMenuItem PayHospitalBills = new UIMenuItem("Pay Hospital Bills", "Pay your outstanding hospital bills.") { RightLabel = $"${Player.Respawning.HospitalBillPastDue}" };
        PayHospitalBills.Activated += (sender, selectedItem) =>
        {
            if (Player.BankAccounts.GetMoney(true) <= Player.Respawning.HospitalBillPastDue)
            {
                new GTANotification(Name, "~r~Insufficient Funds", "We are sorry, we are unable to complete this transaction.").Display();
                NativeHelper.PlayErrorSound();
                return;
            }
            Player.BankAccounts.GiveMoney(-1 * Player.Respawning.HospitalBillPastDue, true);
            new GTANotification(Name, "~g~Accepted", $"Your hospital bills have been paid.").Display();
            Player.Respawning.PayPastDueHospitalBills();
            PayHospitalBills.Enabled = Player.Respawning.HospitalBillPastDue > 0;
        };
        PayHospitalBills.Enabled = Player.Respawning.HospitalBillPastDue > 0;
        InteractionMenu.AddItem(PayHospitalBills);
    }
    public void DisplayInsufficientFundsMessage()
    {
        PlayErrorSound();
        DisplayMessage("~r~Insufficient Funds", "We are sorry, we are unable to complete this transation.");
    }
    public void DisplayPurchaseMessage()
    {
        PlaySuccessSound();
        DisplayMessage("~g~Purchase", $"Thank you for your purchase.");
    }
    public override void AddLocation(PossibleLocations possibleLocations)
    {
        possibleLocations.Hospitals.Add(this);
        base.AddLocation(possibleLocations);
    }
}

