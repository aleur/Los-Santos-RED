using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CivilianEvents
{
    private ITargetable Targetable;
    private IEntityProvideable PedProvider;
    private ISettingsProvideable Settings;

    private uint GameTimeLastGeneratedCrime;
    private uint GameTimeLastGeneratedTrafficCrime;
    private uint MinimumTimeBetweenCrime;
    private uint MaximumTimeBetweenCrime;
    private bool IsTimeToCreateCrime => Game.GameTime - GameTimeLastGeneratedCrime >= Settings.SettingsManager.CivilianSettings.MinimumTimeBetweenRandomCrimes;
    private bool IsTimeToCreateTrafficCrime => Game.GameTime - GameTimeLastGeneratedTrafficCrime >= Settings.SettingsManager.CivilianSettings.MinimumTimeBetweenRandomTrafficCrimes;

    private IZones Zones;
    private IJurisdictions Jurisdictions;
    private ICrimes Crimes;
    private IWeapons Weapons;
    private ITimeControllable Time;
    private IInteriors Interiors;
    private IShopMenus ShopMenus;
    private IGangTerritories GangTerritories;
    private IGangs Gangs;
    private IStreets Streets;
    private IPlacesOfInterest PlacesOfInterest;
    private IAgencies Agencies;
    private IOrganizations Associations;
    private IContacts Contacts;
    private IModItems ModItems;
    public RelationshipGroup CriminalsRG { get; set; }
    public List<PedExt> CriminalsList { get; set; } = new List<PedExt>(); 
    public CivilianEvents(IEntityProvideable world, IZones zones, IJurisdictions jurisdictions, ISettingsProvideable settings, IPlacesOfInterest placesOfInterest, IWeapons weapons, ICrimes crimes, ITimeControllable time, IShopMenus shopMenus,
        IInteriors interiors, IGangs gangs, IGangTerritories gangTerritories, IStreets streets, IAgencies agencies, INameProvideable names, IPedGroups pedGroups, ILocationTypes locationTypes, IPlateTypes plateTypes, 
        IOrganizations associations, IContacts contacts, IModItems modItems, IIssuableWeapons issuableWeapons, IHeads heads, IDispatchablePeople dispatchablePeople)
    {
        PedProvider = world;
        PlacesOfInterest = placesOfInterest;
        Zones = zones;
        Jurisdictions = jurisdictions;
        Settings = settings;
        Weapons = weapons;
        Crimes = crimes;
        Time = time;
        Interiors = interiors;
        ShopMenus = shopMenus;
        Gangs = gangs;
        GangTerritories = gangTerritories;
        Streets = streets;
        Agencies = agencies;
        Associations = associations;
        Contacts = contacts;
        ModItems = modItems;

        GameTimeLastGeneratedCrime = Game.GameTime;
    }
    public void Setup(IContactInteractable contactInteractable, ITargetable targetable)
    {
        CriminalsRG = new RelationshipGroup("CRIMINALS");
        RelationshipGroup.Cop.SetRelationshipWith(CriminalsRG, Relationship.Hate);
        CriminalsRG.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
        Targetable = targetable;

        foreach (Zone zone in PedProvider.Zones.ZoneList)
        {
            zone.LastUpdateTime = Game.GameTime;
            zone.DefaultCrimeSettings(Settings);
        }
    }
    public void Update() // Allow player to enable and disable random zone events. If disabled stick to random crimes
    {
        if (Settings.SettingsManager.CivilianSettings.EnableZoneBasedCrime)
        {
            UpdateZones();
        }
        else if (Settings.SettingsManager.CivilianSettings.AllowRandomCrimes)
        {
            SimpleRandomCrimeUpdate();
        }
    }
    public void Dispose()
    {
        foreach (PedExt c in CriminalsList)
        {
            if (c != null && c.Pedestrian.Exists())
            {
                c.DeleteBlip();
                c.Pedestrian.IsPersistent = false;
                c.Pedestrian.Delete();
            }
        }
    }
    public void CreateCrime(bool isTrafficOnly)
    {
        PedExt Criminal = PedProvider.Pedestrians.GangMemberList.Where(x => x.Pedestrian.Exists() && x.Pedestrian.IsAlive && x.DistanceToPlayer <= 200f && x.CanBeTasked && x.CanBeAmbientTasked && ((isTrafficOnly && x.IsDriver && x.IsInVehicle) || (!isTrafficOnly && !x.IsInVehicle))).FirstOrDefault();//85f//150f
        if (Criminal == null)
        {
            Criminal = PedProvider.Pedestrians.CivilianList.Where(x => x.Pedestrian.Exists() && x.Pedestrian.IsAlive && x.DistanceToPlayer <= 200f && x.CanBeTasked && x.CanBeAmbientTasked && ((isTrafficOnly && x.IsDriver && x.IsInVehicle) || (!isTrafficOnly && !x.IsInVehicle))).FirstOrDefault();//85f//150f
        }
        if (Criminal != null && Criminal.Pedestrian.Exists())
        {
            if (Settings.SettingsManager.CivilianSettings.ShowRandomCriminalBlips && Criminal.Pedestrian.Exists())
            {
                Criminal.BlipName = "Criminal";
                //Criminal.BlipSprite = BlipSprite.CriminalWanted;
                Criminal.AddBlip();

                CriminalsList.Add(Criminal);
                //Blip myBlip = Criminal.Pedestrian.AttachBlip();
                //EntryPoint.WriteToConsole($"CRIME PED BLIP BLIP CREATED");
                //myBlip.Color = Color.Red;
                //myBlip.Sprite = BlipSprite.CriminalWanted;
                //myBlip.Scale = 1.0f;
                //NativeFunction.Natives.BEGIN_TEXT_COMMAND_SET_BLIP_NAME("STRING");
                //NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME("Criminal");
                //NativeFunction.Natives.END_TEXT_COMMAND_SET_BLIP_NAME(myBlip);
                //PedProvider.AddBlip(myBlip);
            }
            Criminal.CanBeAmbientTasked = false;
            Criminal.WasSetCriminal = true;
            Criminal.WillCallPolice = false;
            Criminal.WillCallPoliceIntense = false;

            if (!Criminal.IsGangMember)
            {
                Criminal.Pedestrian.IsPersistent = true;
                Criminal.Pedestrian.RelationshipGroup = CriminalsRG;
            }
            Criminal.CurrentTask = new CommitCrime(Criminal, Targetable, Weapons, PedProvider, Zones) { IsTrafficOnly = isTrafficOnly };
            Criminal.CurrentTask.Start();

            if (isTrafficOnly)
            {
                GameTimeLastGeneratedTrafficCrime = Game.GameTime;
            }
            else
            {
                GameTimeLastGeneratedCrime = Game.GameTime;
            }
        }
    }
    public void SimpleRandomCrimeUpdate()
    {
        if (Settings.SettingsManager.CivilianSettings.AllowRandomCrimes)
        {
            if (IsTimeToCreateCrime)
            {
                CreateCrime(false);
                GameFiber.Yield();
            }
            else if (IsTimeToCreateTrafficCrime)
            {
                CreateCrime(true);
                GameFiber.Yield();
            }
        }
    }
    public void UpdateZones()
    {
        int updated = 0;
        foreach (Zone zone in PedProvider.Zones.ZoneList)
        {
            if (zone.CrimeFrequency == 0) continue; // No crime if frequency is zero.
            if (zone.Crime == null)
            {
                CommitCrime crime = new CommitCrime(null, Targetable, Weapons, PedProvider, Zones);

                crime.SetZoneCriminal(zone, Settings.SettingsManager.CivilianSettings.ShowRandomCriminalBlips, CriminalsRG);
                zone.Crime = crime;
                zone.TimeUntilNextCrime = GetNextCrimeTime(zone);

                EntryPoint.WriteToConsole($"{zone.DisplayName} setup with Crime: {crime.SelectedCrime}, Countdown: {zone.TimeUntilNextCrime}, ");
            }
            else
            {
                zone.UpdateCrime(Targetable, Settings.SettingsManager.CivilianSettings.ShowRandomCriminalBlips, CriminalsRG);
            }
            updated++;
            if (updated >= 5)//15)//5
            {
                GameFiber.Yield();
                updated = 0;
            }
            if (!EntryPoint.ModController.IsRunning)
            {
                break;
            }
        }
    }

    private uint GetNextCrimeTime(Zone zone)
    {
        float freq = MathHelper.Clamp(zone.CrimeFrequency, 0.0f, 1.0f);
        double r = RandomItems.MyRand.NextDouble() * freq; // Doesn't go above freq FUCK WHY AM I DOING MATH FOR THIS SHIT

        double? time = zone.MinCrimeTime + (1 - freq) * (zone.MaxCrimeTime - zone.MinCrimeTime) * r;
        uint result = (uint)Math.Round((double)time);
        return (uint)MathHelper.Min(MathHelper.Max(result, zone.MinCrimeTime), zone.MaxCrimeTime);
    }
}
