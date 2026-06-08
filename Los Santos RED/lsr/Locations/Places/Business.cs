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

public class Business
{
    private UIMenu ParentMenu;
    private UIMenu ManageBusinessMenu;
    private UIMenu AccessAmenitiesMenu;
    private UIMenu WorkOptionsMenu;
    private ILocationInteractable Player;
    private ITimeControllable Time;
    private ISettingsProvideable Settings;
    private DateTime ClosingTime;
    private UIMenuNumericScrollerItem<int> WorkMenuScrollerItem;
    private UIMenuItem WorkMenuItem;
    public BusinessMenu BusinessMenu { get; set; }
    public GameLocation Location { get; private set; }
    public MenuPool MenuPool { get; private set; }
    public bool HasBannerImage { get; set; } = false;
    public Texture BannerImage { get; set; }
    public bool RemoveBanner { get; set; } = false;
    public bool KeepInteractionGoing { get; set; } = false;

    public Business(MenuPool menuPool, UIMenu parentMenu, BusinessMenu menu, ILocationInteractable player, ITimeControllable time, ISettingsProvideable settings, GameLocation location)
    {
        MenuPool = menuPool;
        ParentMenu = parentMenu;
        BusinessMenu = menu;
        Location = location;
        Player = player;
        Time = time;
        Settings = settings;
        ClosingTime = (Location.CloseTime == 24 && Location.OpenTime == 0) ? new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, 23, 59, 00) :
            new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, Location.CloseTime, 0, 0);
    }
    public Business() : base()
    {

    }
    public void CreateBusinessMenu(IModItems modItems, IEntityProvideable world, IWeapons weapons)
    {
        if (Location == null)
        {
            HasBannerImage = false;
            RemoveBanner = true;
            BannerImage = null;
        }
        else
        {
            HasBannerImage = Location.HasBannerImage;
            RemoveBanner = Location.RemoveBanner;
            BannerImage = Location.BannerImage;
        }
        if(BusinessMenu == null)
        {
            return;
        }
        SetupMenus();
        //player.OnBusinessMenuCreated(Location, MenuPool, ParentMenu);
    }
    private void SetupMenus()
    {
        SetupManageBusinessMenu();
        SetupAccessAmenitiesMenu();
        SetupWorkOptionsMenu();
    }

    private void SetupManageBusinessMenu()
    {
        if (ParentMenu != null)
        {
            ManageBusinessMenu = MenuPool.AddSubMenu(ParentMenu, "Manage Property");
            if (HasBannerImage && BannerImage != null)
            {
                ManageBusinessMenu.SetBannerType(BannerImage);
            }
            else if (RemoveBanner)
            {
                ManageBusinessMenu.RemoveBanner();
            }
        }
        if (!Location.IsOwned && Location.PurchasePrice > 0)
        {
            UIMenu OfferSubMenu = MenuPool.AddSubMenu(ManageBusinessMenu, "Make an Offer");
            if (HasBannerImage && BannerImage != null)
            {
                OfferSubMenu.SetBannerType(BannerImage);
            }
            else if (RemoveBanner)
            {
                OfferSubMenu.RemoveBanner();
            }
            UIMenuItem PurchaseBusinessMenuItem = new UIMenuItem("Purchase", "Select to purchase this business.") { RightLabel =  Location.PurchasePrice.ToString("C0") };
            PurchaseBusinessMenuItem.Activated += (sender, e) =>
            {
                if (Location.Purchase())
                {
                    MenuPool.CloseAllMenus();
                    SetupMenus();
                }
            };
            OfferSubMenu.AddItem(PurchaseBusinessMenuItem);
        }
        else if (Location.IsOwned)
        {
            UIMenuItem SellBusinessItem = new UIMenuItem("Sell Business", "Sell the current business.") { RightLabel = Location.SalesPrice.ToString("C0") };
            SellBusinessItem.Activated += (sender, e) =>
            {
                Location.Sell();
            };
            ManageBusinessMenu.AddItem(SellBusinessItem);
        }
    }
    private void SetupAccessAmenitiesMenu()
    {
        if (ParentMenu != null)
        {
            AccessAmenitiesMenu = MenuPool.AddSubMenu(ParentMenu, "Access Amenities");
            if (HasBannerImage && BannerImage != null)
            {
                AccessAmenitiesMenu.SetBannerType(BannerImage);
            }
            else if (RemoveBanner)
            {
                AccessAmenitiesMenu.RemoveBanner();
            }
            UIMenuNumericScrollerItem<int> RestMenuItem = new UIMenuNumericScrollerItem<int>("Rest", "Rest at your business to recover health. Select up to 12 hours.", 1, 12, 1) { Formatter = v => v.ToString() + " hours" };
            RestMenuItem.Activated += (sender, selectedItem) =>
            {
                Rest(RestMenuItem.Value);
            };
            AccessAmenitiesMenu.AddItem(RestMenuItem);
        }
    }
    private void SetupWorkOptionsMenu()
    {
        if (ParentMenu != null)
        {
            WorkOptionsMenu = MenuPool.AddSubMenu(ParentMenu, "Work Options");
            if (HasBannerImage && BannerImage != null)
            {
                WorkOptionsMenu.SetBannerType(BannerImage);
            }
            else if (RemoveBanner)
            {
                WorkOptionsMenu.RemoveBanner();
            }
            WorkMenuScrollerItem = new UIMenuNumericScrollerItem<int>("Work Hourly", "Hourly shift for minimum wage. 15$ per hour", 1, ClosingTime.Hour - Time.CurrentHour, 1) { Formatter = v => v.ToString() + " hours" };
            WorkMenuItem = new UIMenuItem("Work Part-Time", "Work til' closing time. 15$ per hour");
            WorkMenuScrollerItem.Activated += (sender, selectedItem) =>
            {
                ClosingTime = (Location.CloseTime == 24 && Location.OpenTime == 0) ? new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, 23, 59, 00) :
                    new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, Location.CloseTime, 0, 0);
                if (!Location.IsLocationClosed()) Work(WorkMenuScrollerItem.Value);
            };
            WorkMenuItem.Activated += (sender, selectedItem) =>
            {
                ClosingTime = (Location.CloseTime == 24 && Location.OpenTime == 0) ? new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, 23, 59, 00) :
                    new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, Location.CloseTime, 0, 0);
                if (!Location.IsLocationClosed()) Work();
            };
            WorkOptionsMenu.AddItem(WorkMenuScrollerItem);
            WorkOptionsMenu.AddItem(WorkMenuItem);
        }
    }
    public void ProcessBusinessMenu()
    {
        while ((MenuPool.IsAnyMenuOpen() || KeepInteractionGoing) && Location.UIMenuCategory == "BusinessMenu")
        {
            MenuPool.ProcessMenus();
            Update();
            GameFiber.Yield();
        }

        if (Location.UIMenuCategory == "ShopMenu")
        {
            MenuPool.Where(x => x.ParentMenu != null && x.ParentMenu != ParentMenu).ToList().ForEach(x => // To deal with those not attached to ParentMenu. Otherwise, causes multiple layers of UI.
            {
                x.Clear();
                x.Close();
                MenuPool.Remove(x); // Instead of x.ParentMenu.Clear(); x.ParentMenu.Close(); which also works. Removes duplicates of UIMenu as well.
            });
            DisposeBusinessMenu();
            ParentMenu.Clear();
            Location.SwitchMenus();
        }
    }
    public void Update()
    {
        if (MenuPool.IsAnyMenuOpen())
        {
            UpdateWorkMenu();
        }
        else
        {
            //placeholdere
        }
    }
    private void UpdateWorkMenu()
    {
        if (!WorkOptionsMenu?.Visible == true) return;

        bool isEnabled = WorkMenuScrollerItem.Enabled && WorkMenuItem.Enabled;

        if (!Location.IsOpen(Time.CurrentHour) && isEnabled)
        {
            WorkMenuScrollerItem.Enabled = false;
            WorkMenuItem.Enabled = false;
        }
        else if (Location.IsOpen(Time.CurrentHour) && !isEnabled)
        {
            WorkMenuScrollerItem.Enabled = true;
            WorkMenuItem.Enabled = true;
        }

        if (WorkMenuScrollerItem.Enabled)
        {
            int newMax = ClosingTime.Hour - Time.CurrentHour;
            if (WorkMenuScrollerItem.Maximum != newMax && newMax > 0)
            {
                WorkMenuScrollerItem.Maximum = newMax;
            }
            if (WorkMenuScrollerItem.Minimum < 1)
            {
                WorkMenuScrollerItem.Minimum = 1;
            }
        }
    }
    public void DisposeBusinessMenu()
    {
        ManageBusinessMenu.Close();
        AccessAmenitiesMenu.Close();
        WorkOptionsMenu.Close();
    }

    private void Work(int hours = 0)
    {
        //Time.FastForward(Time.CurrentDateTime.AddHours(Hours));//  new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, 11, 0, 0));
        if (hours > 0) Time.FastForward(hours);
        else Time.FastForward(ClosingTime);
        WorkOptionsMenu.Visible = false;
        KeepInteractionGoing = true;
        Player.IsWorking = true;
        Player.ButtonPrompts.AddPrompt("PropertyWork", "Cancel Work", "PropertyWork", Settings.SettingsManager.KeySettings.InteractCancel, 99);

        GameFiber FastForwardWatcher = GameFiber.StartNew(delegate
        {
            try
            {
                DateTime startedTime = Time.CurrentDateTime;
                while (Time.IsFastForwarding)
                {
                    if (Player.ButtonPrompts.IsPressed("PropertyWork") || !Location.IsOpen(Time.CurrentHour))
                    {
                        Time.StopFastForwarding();
                    }
                    GameFiber.Yield();
                }
                Player.ButtonPrompts.RemovePrompts("PropertyWork");
                WorkOptionsMenu.Visible = true;
                EntryPoint.WriteToConsole($"PART TIME COMPLETE: {Time.CurrentHour - startedTime.Hour} Hours Worked, Total Pay: {15 * (Time.CurrentHour - startedTime.Hour)}");
                Player.BankAccounts.GiveMoney(15 * (Time.CurrentHour - startedTime.Hour), false);
                KeepInteractionGoing = false;
                Player.IsWorking = false;
            }
            catch (Exception ex)
            {
                EntryPoint.WriteToConsole(ex.Message + " " + ex.StackTrace, 0);
                EntryPoint.ModController.CrashUnload();
            }
        }, "FastForwardWatcher");
        //EntryPoint.WriteToConsole($"PLAYER EVENT: START WORK ACTIVITY AT {Location.Name}");
    }
    private void Rest(int Hours)
    {
        Time.FastForward(Time.CurrentDateTime.AddHours(Hours));//  new DateTime(Time.CurrentYear, Time.CurrentMonth, Time.CurrentDay, 11, 0, 0));
        AccessAmenitiesMenu.Visible = false;
        KeepInteractionGoing = true;
        Player.IsResting = true;
        Player.IsSleeping = true;
        Player.ButtonPrompts.AddPrompt("PropertyRest", "Cancel Rest", "PropertyRest", Settings.SettingsManager.KeySettings.InteractCancel, 99);
        DateTime TimeLastAddedItems = Time.CurrentDateTime;

        GameFiber FastForwardWatcher = GameFiber.StartNew(delegate
        {
            try
            {
                while (Time.IsFastForwarding)
                {
                    if (DateTime.Compare(Time.CurrentDateTime, TimeLastAddedItems) >= 0)
                    {
                        if (!Settings.SettingsManager.NeedsSettings.ApplyNeeds)
                        {
                            Player.HealthManager.ChangeHealth(1);
                        }
                        TimeLastAddedItems = TimeLastAddedItems.AddMinutes(30);
                    }
                    if (Player.ButtonPrompts.IsPressed("PropertyRest"))
                    {
                        Time.StopFastForwarding();
                    }
                    GameFiber.Yield();
                }
                Player.ButtonPrompts.RemovePrompts("PropertyRest");
                Player.IsResting = false;
                Player.IsSleeping = false;
                AccessAmenitiesMenu.Visible = true;
                KeepInteractionGoing = false;
            }
            catch (Exception ex)
            {
                EntryPoint.WriteToConsole(ex.Message + " " + ex.StackTrace, 0);
                EntryPoint.ModController.CrashUnload();
            }
        }, "FastForwardWatcher");
        //EntryPoint.WriteToConsole($"PLAYER EVENT: START REST ACTIVITY AT {Location.Name}");
    }
}
