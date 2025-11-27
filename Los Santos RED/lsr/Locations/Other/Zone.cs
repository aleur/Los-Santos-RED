using ExtensionsMethods;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable()]
public class Zone
{

    public Zone()
    {

    }
    public Zone(string _GameName, string _TextName, string _CountyID,string _StateID,bool isSpecificLocation, eLocationEconomy economy, eLocationType type)
    {
        InternalGameName = _GameName;
        DisplayName = _TextName;
        CountyID = _CountyID;
        StateID = _StateID;
        IsSpecificLocation = isSpecificLocation;
        Economy = economy;
        Type = type;     
    }
    public Zone(string _GameName, string _TextName, string _CountyID, Vector2[] boundaries, string _StateID, bool isSpecificLocation, eLocationEconomy economy, eLocationType type)
    {
        InternalGameName = _GameName;
        Boundaries = boundaries;
        DisplayName = _TextName;
        CountyID = _CountyID;
        StateID = _StateID;
        IsSpecificLocation = isSpecificLocation;
        Economy = economy;
        Type = type;
    }

    public override string ToString() => DisplayName;
    public string InternalGameName { get; set; }
    public string DisplayName { get; set; }
    public string CountyID { get; set; }
    public string StateID { get; set; }
    public bool IsRestrictedDuringWanted { get; set; } = false;
    public bool IsSpecificLocation { get; set; } = false;
    public string BoroughName { get; set; }
    public eLocationEconomy Economy { get; set; } = eLocationEconomy.Middle;
    public eLocationType Type { get; set; } = eLocationType.Rural;
    public Vector2[] Boundaries { get; set; }



    [XmlIgnore]
    public Agency AssignedLEAgency { get; set; }
    [XmlIgnore]
    public Agency AssignedSecondLEAgeny { get; set; }
    [XmlIgnore]
    public Gang AssignedGang { get; set; }



    [XmlIgnore]
    public Agency AssignedEMSAgency { get; set; }
    [XmlIgnore]
    public Agency AssignedFireAgency { get; set; }


    [XmlIgnore]
    public GameCounty GameCounty { get; set; }
    [XmlIgnore]
    public GameState GameState { get; set; }

    [XmlIgnore]
    public List<Gang> Gangs { get; set; }
    [XmlIgnore]
    public List<Agency> Agencies { get; set; }

    [XmlIgnore]
    public ShopMenuGroupContainer DealerMenus { get; set; }
    [XmlIgnore]
    public ShopMenuGroupContainer CustomerMenus { get; set; }

    public string DealerMenuContainerID { get; set; }
    public string CustomerMenuContainerID { get; set; }
    [XmlIgnore]
    public uint LastUpdateTime { get; set; }
    [XmlIgnore]
    public long TimeUntilNextCrime { get; set; }
    [XmlIgnore]
    public CommitCrime Crime { get; set; }
    // Citizen Crime Events : Defaults to Global Settings if not set
    [XmlIgnore]
    public float CrimeFrequency { get; set; } = -1.0f;
    [XmlIgnore]
    public uint? MinCrimeTime { get; set; }
    [XmlIgnore]
    public uint? MaxCrimeTime { get; set; }

    public bool IsLowPop => Type == eLocationType.Rural || Type == eLocationType.Wilderness;
    public void StoreData(IGangTerritories gangTerritories, IJurisdictions jurisdictions, IShopMenus shopMenus)
    {
        Gangs = new List<Gang>();
        List<Gang> GangStuff = gangTerritories.GetGangs(InternalGameName, 0);
        if (GangStuff != null)
        {
            Gangs.AddRange(GangStuff);
        }
        Agencies = new List<Agency>();
        List<Agency> LEAgency = jurisdictions.GetAgencies(InternalGameName, 0, ResponseType.LawEnforcement);
        if (LEAgency != null)
        {
            Agencies.AddRange(LEAgency);
        }
        List<Agency> EMSAgencies = jurisdictions.GetAgencies(InternalGameName, 0, ResponseType.EMS);
        if (EMSAgencies != null)
        {
            Agencies.AddRange(EMSAgencies);
        }
        List<Agency> FireAgencies = jurisdictions.GetAgencies(InternalGameName, 0, ResponseType.Fire);
        if (FireAgencies != null)
        {
            Agencies.AddRange(FireAgencies);
        }
        AssignedLEAgency = jurisdictions.GetMainAgency(InternalGameName, ResponseType.LawEnforcement);
        Gang mainGang = gangTerritories.GetMainGang(InternalGameName);
        if (mainGang != null)
        {
            AssignedGang = mainGang;
        }
        else
        {
            AssignedGang = null;
        }
        Agency secondaryAgency = jurisdictions.GetNthAgency(InternalGameName, ResponseType.LawEnforcement, 2);
        if (secondaryAgency != null)
        {
            AssignedSecondLEAgeny = secondaryAgency;
        }
        else
        {
            AssignedSecondLEAgeny = null;
        }

        AssignedEMSAgency = jurisdictions.GetMainAgency(InternalGameName, ResponseType.EMS);
        AssignedFireAgency = jurisdictions.GetMainAgency(InternalGameName, ResponseType.Fire);


        DealerMenus = shopMenus.GetSpecificGroupContainer(DealerMenuContainerID);
        CustomerMenus = shopMenus.GetSpecificGroupContainer(CustomerMenuContainerID);
    }
    public ShopMenu GetIllicitMenu(ISettingsProvideable Settings, IShopMenus ShopMenus)
    {
        float dealerPercentage = Settings.SettingsManager.CivilianSettings.DrugDealerPercentageMiddleZones;
        float customerPercentage = Settings.SettingsManager.CivilianSettings.DrugCustomerPercentageMiddleZones;
        string DealerMenuContainerID = StaticStrings.MiddleAreaDrugDealerMenuGroupID;
        string CustomerMenuContainerID = StaticStrings.MiddleAreaDrugCustomerMenuGroupID;

        if (Economy == eLocationEconomy.Rich)
        {
            dealerPercentage = Settings.SettingsManager.CivilianSettings.DrugDealerPercentageRichZones;
            customerPercentage = Settings.SettingsManager.CivilianSettings.DrugCustomerPercentageRichZones;
            DealerMenuContainerID = StaticStrings.RichAreaDrugDealerMenuGroupID;
            CustomerMenuContainerID = StaticStrings.RichAreaDrugCustomerMenuGroupID;
        }
        else if (Economy == eLocationEconomy.Poor)
        {
            dealerPercentage = Settings.SettingsManager.CivilianSettings.DrugDealerPercentagePoorZones;
            customerPercentage = Settings.SettingsManager.CivilianSettings.DrugCustomerPercentagePoorZones;
            DealerMenuContainerID = StaticStrings.PoorAreaDrugDealerMenuGroupID;
            CustomerMenuContainerID = StaticStrings.PoorAreaDrugCustomerMenuGroupID;
        }
        if (RandomItems.RandomPercent(dealerPercentage))
        {
            if(DealerMenus != null)
            {
                string lookingForID = DealerMenus.GetRandomWeightedShopMenuGroupID();
                //EntryPoint.WriteToConsole($"ZONE {DisplayName} GETTING ZONE SPECIFIC DEALER MENU ID: {lookingForID}");
                return ShopMenus.GetWeightedRandomMenuFromGroup(lookingForID);
            }
            else
            {
               // EntryPoint.WriteToConsole($"ZONE {DisplayName} GETTING GENERIC DEALER MENU DealerMenuContainerID: {DealerMenuContainerID}");
                return ShopMenus.GetWeightedRandomMenuFromContainer(DealerMenuContainerID);//  GetRandomMenuIDByEconomy(true);
            }
        }
        else if (RandomItems.RandomPercent(customerPercentage))
        {
            if (CustomerMenus != null)
            {
                string lookingForID = CustomerMenus.GetRandomWeightedShopMenuGroupID();
               // EntryPoint.WriteToConsole($"ZONE {DisplayName} GETTING ZONE SPECIFIC CUSTOMER MENU ID: {lookingForID}");
                return ShopMenus.GetWeightedRandomMenuFromGroup(lookingForID);
            }
            else
            {
                //EntryPoint.WriteToConsole($"ZONE {DisplayName} GETTING GENERIC CUSTOMER MENU CustomerMenuContainerID: {CustomerMenuContainerID}");
                return ShopMenus.GetWeightedRandomMenuFromContainer(CustomerMenuContainerID);
            }
        }
        return null;
    }
    public void DefaultCrimeSettings(ISettingsProvideable settings)
    {
        if (CrimeFrequency < 0)
        {
            if (Economy == eLocationEconomy.Poor)
            {
                CrimeFrequency = settings.SettingsManager.CivilianSettings.CrimeFrequencyPoorZones;
            }
            else if (Economy == eLocationEconomy.Middle)
            {
                CrimeFrequency = settings.SettingsManager.CivilianSettings.CrimeFrequencyMiddleZones;
            }
            else if (Economy == eLocationEconomy.Rich)
            {
                CrimeFrequency = settings.SettingsManager.CivilianSettings.CrimeFrequencyRichZones;
            }
        }

        if (!MaxCrimeTime.HasValue)
        {
            MaxCrimeTime = settings.SettingsManager.CivilianSettings.MaximumTimeBetweenCrimesZones;
            /*
            if (Economy == eLocationEconomy.Poor)
            {
                MaxCrimeTime = settings.SettingsManager.CivilianSettings.MaximumTimeBetweenCrimesPoorZones;
            }
            else if (Economy == eLocationEconomy.Middle)
            {
                MaxCrimeTime = settings.SettingsManager.CivilianSettings.MaximumTimeBetweenCrimesMiddleZones;
            }
            else if (Economy == eLocationEconomy.Rich)
            {
                MaxCrimeTime = settings.SettingsManager.CivilianSettings.MaximumTimeBetweenCrimesRichZones;
            }
            */
        }
        if (!MinCrimeTime.HasValue)
        {
            MinCrimeTime = settings.SettingsManager.CivilianSettings.MinimumTimeBetweenCrimesZones;
            /*
            if (Economy == eLocationEconomy.Poor)
            {
                MinCrimeTime = settings.SettingsManager.CivilianSettings.MinimumTimeBetweenCrimesPoorZones;
            }
            else if (Economy == eLocationEconomy.Middle)
            {
                MinCrimeTime = settings.SettingsManager.CivilianSettings.MinimumTimeBetweenCrimesMiddleZones;
            }
            else if (Economy == eLocationEconomy.Rich)
            {
                MinCrimeTime = settings.SettingsManager.CivilianSettings.MinimumTimeBetweenCrimesRichZones;
            }
            */
        }
    }
    public string GetFullLocationName(IDisplayable Player, ISettingsProvideable settings, string CurrentDefaultTextColor)
    {


        string toDisplay = $"{CurrentDefaultTextColor}" + FullZoneName(settings);
        if (settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowPrimaryAgency && AssignedLEAgency != null)
        {
            toDisplay += $"{CurrentDefaultTextColor} / " + AssignedLEAgency.ColorInitials;
        }
        if (settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowPrimaryGang && AssignedGang != null)
        {
            toDisplay += $"{CurrentDefaultTextColor} - " + AssignedGang.ColorInitials;
            GangReputation gr = Player.RelationshipManager.GangRelationships.GetReputation(AssignedGang);
            if(gr != null) 
            {
                toDisplay += gr.ToZoneString();
            }
        }
        else if (settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowSecondaryAgency && AssignedSecondLEAgeny != null)
        {
            toDisplay += $"{CurrentDefaultTextColor} - " + AssignedSecondLEAgeny.ColorInitials;
        }
        return toDisplay;
    }
    public string FullZoneName(ISettingsProvideable settings)
    {
        string initialDisplay = DisplayName;
        bool shownborough = false;
        if (!string.IsNullOrEmpty(BoroughName) && settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowBorough)
        {
            initialDisplay += ", " + BoroughName;// + "~s~";
            shownborough = true;
        }
        if (GameCounty != null && settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowCounty)
        {
            initialDisplay += ", " + GameCounty.ColorName(shownborough || settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowCountyShort) + "~s~";
        }
        if (GameState != null && settings.SettingsManager.LSRHUDSettings.ZoneDisplayShowState)
        {
            initialDisplay += ", " + GameState.ColorName + "~s~"; ;
        }
        return initialDisplay;
    }
    public void UpdateCrime(ITargetable targetable, bool enableBlips, RelationshipGroup rg)
    {
        long deltaTime = Game.GameTime - LastUpdateTime;
        LastUpdateTime = Game.GameTime;

        TimeUntilNextCrime -= deltaTime;
        if (targetable.CurrentLocation.CurrentZone == this)
        {
            EntryPoint.WriteToConsole($"{TimeUntilNextCrime} left till {Crime.SelectedCrime} in {DisplayName}");
        }

        if (TimeUntilNextCrime <= 60000 && !Crime.CriminalExists)
        {
            Crime.SetZoneCriminal(this, enableBlips, rg);
        }

        if (TimeUntilNextCrime <= 0f)
        {
            //Game.DisplayNotification("CHAR_BLANK_ENTRY", "CHAR_BLANK_ENTRY", "~o~Crime!", "Los Santos ~r~RED", $"{Crime.SelectedCrime} committed in {DisplayName}");
            if (Crime.CloseToPlayer) // If player in zone, simulate crime.
            {
                Game.DisplayNotification("CHAR_GANGAPP", "CHAR_GANGAPP", "~o~CrimeWatch", "Los Santos ~r~RED", $"{Crime.SelectedCrime} committed in {DisplayName}, Distance: {Crime.DistanceToPlayer}");
                Crime.Start();
                EntryPoint.WriteToConsole($"{Crime.SelectedCrime} committed in {DisplayName}, Distance: {Crime.DistanceToPlayer}");
            }

            //EntryPoint.WriteToConsole($"{Crime.SelectedCrime} committed in {DisplayName}");
            Crime = null;
        }
    }
}