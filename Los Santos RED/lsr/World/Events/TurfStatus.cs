using ExtensionsMethods;
using LosSantosRED.lsr;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;

public class TurfStatus
{
    private readonly IEntityProvideable World;
    private readonly IZones Zones;
    private readonly IPlacesOfInterest PlacesOfInterest;
    private IGangTerritories GangTerritories;
    private GangTerritory GangTerritory;
    private Gang Gang;
    private IGangs Gangs;
    public string GangName { get; private set; }
    public string ZoneName { get; private set; } 
    // Location Checks
    public bool HasRacketStores { get; set; }
    public bool HasRobberyStores { get; set; }
    public bool IsMainTurf { get; set; }
    public List<Gang> EnemyGangsWithScenarios { get; set; } = new List<Gang>(); // ass variable name
    public List<Gang> AmbientEnemyGangs { get; set; } = new List<Gang>();
    // Scenarios
    public List<BlankLocation> EnemyGangScenarios { get; set; } = new List<BlankLocation>();
    public List<BlankLocation> PoliceTargetScenarios { get; set; } = new List<BlankLocation>();
    public List<BlankLocation> OrganizationTargetScenarios { get; set; } = new List<BlankLocation>();
    public List<BlankLocation> CivilianTargetScenarios { get; set; } = new List<BlankLocation>();
    public List<BlankLocation> DignitaryTargetScenarios { get; set; } = new List<BlankLocation>();
    public List<BlankLocation> WitnessTargetScenarios { get; set; } = new List<BlankLocation>();
    // Turf Needs
    public bool NeedsReinforcements { get; set; }
    public bool NeedsSupplies { get; set; }
    public TurfStatus(IEntityProvideable world, IZones zones, IPlacesOfInterest placesOfInterest, IGangTerritories gangTerritories, IGangs gangs, GangTerritory gangTerritory, Gang gang)
    {
        World = world;
        Zones = zones;
        PlacesOfInterest = placesOfInterest;
        GangTerritories = gangTerritories;
        GangTerritory = gangTerritory;
        Gangs = gangs;
        Gang = gang;
    }
    public void Setup()
    {
        ZoneName = Zones.GetZone(GangTerritory.ZoneInternalGameName).DisplayName;
        GangName = Gang.FullName;
        IsMainTurf = GangTerritory.Priority == 0;

        EstablishScenarios();
        HasRacketStores = PlacesOfInterest.PossibleLocations.RacketeeringTaskLocations().Any(x => x.IsCorrectMap(World.IsMPMapLoaded) && Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName));
        HasRobberyStores = PlacesOfInterest.PossibleLocations.RobberyTaskLocations().Any(x => x.IsCorrectMap(World.IsMPMapLoaded) && Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName));
        CheckAmbientEnemyGangs();

        Update();
    }
    public void Update()
    {
        // No need to update Witness locations.
        RefreshEnemyGangScenarios();
    }
    private void EstablishScenarios()
    {
        // Not checking pedspawns or vehiclespawns anymore. If players fuck it up thats on them, but still checking ConditionalGroups for target availability.
        EnemyGangScenarios = PlacesOfInterest.PossibleLocations.BlankLocations.Where(x => IsValidTargetScenario(x) && x.MissionTargetType == "Gang" && Gang.EnemyGangs.Contains(x.AssignedAssociationID)).ToList(); 
        PoliceTargetScenarios = PlacesOfInterest.PossibleLocations.BlankLocations.Where(x => IsValidTargetScenario(x) && x.MissionTargetType == "Police").ToList();
        WitnessTargetScenarios = PlacesOfInterest.PossibleLocations.BlankLocations.Where(x => IsValidTargetScenario(x) && x.MissionTargetType == "Witness").ToList();
        CivilianTargetScenarios = PlacesOfInterest.PossibleLocations.BlankLocations.Where(x => IsValidTargetScenario(x) && x.MissionTargetType == "Civilian").ToList();
    }
    private bool IsValidTargetScenario (BlankLocation bl)
    {   
        // might need to split function between ped & vehicle target
        // no .IsEnabled let others filter that
        // planned: re-enable scenarios after some time
        bool positionCheck = bl.IsCorrectMap(World.IsMPMapLoaded) && Zones.GetZone(bl.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName);
        bool ambushCheck = bl.CanBeAmbushableTarget && bl.PossibleGroupSpawns?.Any(x => x.CanBeAmbushableTarget && x.TargetSelectionChance > 0) == true;
        bool vehicleTargetCheck = bl.CanVehiclesBeTarget && bl.PossibleGroupSpawns?.Any(x => x.CanVehiclesBeTarget && x.TargetSelectionChance > 0) == true;

        return positionCheck && (ambushCheck || vehicleTargetCheck);
    }
    private void RefreshEnemyGangScenarios() // Remove gangs from list if they no longer have enabled location scenarios in zone. bye bye crash
    {
        List<Gang> activeGangs = new List<Gang>();
        foreach (BlankLocation bl in EnemyGangScenarios.Where(x => x.IsEnabled))
        {
            Gang gang = Gangs.GetGang(bl.AssignedAssociationID);
            if (gang != null && !activeGangs.Contains(gang))
            {
                activeGangs.Add(gang);
                //EntryPoint.WriteToConsole($"{Gang.FullName}{GangTerritory.ZoneInternalGameName} - Adding ENEMY {gang.ID}");
            }
        }
        EnemyGangsWithScenarios.RemoveAll(gang => !activeGangs.Contains(gang));
        foreach (Gang gang in activeGangs)
        {
            if (!EnemyGangsWithScenarios.Contains(gang))
            {
                EnemyGangsWithScenarios.Add(gang);
            }
        }
    }
    public BlankLocation GetTargetScenario(string targetTypeString)
    {
        List<BlankLocation> targetableScenarios = new List<BlankLocation>();
        if (!Enum.TryParse<TargetType>(targetTypeString, true, out TargetType targetType))
        {
            return null;
        }

        switch (targetType)
        {
            case TargetType.Gang:
                targetableScenarios = EnemyGangScenarios;
                break;

            case TargetType.Police:
                targetableScenarios = PoliceTargetScenarios;
                break;

            case TargetType.Organization:
                targetableScenarios = OrganizationTargetScenarios;
                break;

            case TargetType.Civilian:
                targetableScenarios = CivilianTargetScenarios;
                break;

            case TargetType.Dignitary:
                targetableScenarios = DignitaryTargetScenarios;
                break;

            case TargetType.Witness:
                targetableScenarios = WitnessTargetScenarios;
                break;

            default
                : return null;
        }

        if (!targetableScenarios.Any())
        {
            return null;
        }

        return targetableScenarios.Where(bl => bl.IsEnabled).PickRandom();
    }
    public void SetupScenarioLocations(BlankLocation TargetedScenario)
    {
        TargetedScenario.IsAmbushTarget = true;

        if (TargetedScenario.PossibleGroupSpawns == null || !TargetedScenario?.PossibleGroupSpawns.Any() == true) return;

        if (TargetedScenario.WillSelectMultipleTargetGroups)
        {
            foreach (ConditionalGroup cg in TargetedScenario.PossibleGroupSpawns.Where(s => s.CanBeAmbushableTarget)) // Still works if a group already spawned, they just wont be set as ambushtarget.
            {
                cg.IsAmbushTarget = RandomItems.RandomPercent(cg.TargetSelectionChance);
            }
            if (!TargetedScenario.PossibleGroupSpawns.Any(x => x.IsAmbushTarget)) // pick random group with chance if all cg are false. Should pick at least once because GetTargetScenario() already filtered out for us
            {
                TargetedScenario.PossibleGroupSpawns.Where(cg => cg.CanBeAmbushableTarget && cg.TargetSelectionChance > 0).ToList().PickRandom().IsAmbushTarget = true;
            }
        }
        else // Select one group to target
        {
            float PercentageTotal = TargetedScenario.PossibleGroupSpawns.Sum(cg => cg.TargetSelectionChance);
            float RandomPick = RandomItems.MyRand.Next(0, (int)PercentageTotal);
            foreach (ConditionalGroup cg in TargetedScenario.PossibleGroupSpawns.Where(s => s.CanBeAmbushableTarget && s.TargetSelectionChance > 0))
            {
                if (RandomPick < cg.TargetSelectionChance)
                {
                    cg.IsAmbushTarget = true;
                    return;
                }
                RandomPick -= cg.TargetSelectionChance;
            }
            if (!TargetedScenario.PossibleGroupSpawns.Any(x => x.IsAmbushTarget)) // pick random group with chance if all cg are false. Should pick at least once because GetTargetScenario() already filtered out for us
            {
                TargetedScenario.PossibleGroupSpawns.Where(cg => cg.CanBeAmbushableTarget && cg.TargetSelectionChance > 0).ToList().PickRandom().IsAmbushTarget = true;
            }
        }
    }
    private void CheckAmbientEnemyGangs()
    {
        foreach (GangTerritory gt in GangTerritories.GangTerritoriesList.Where(x => x.ZoneInternalGameName == GangTerritory.ZoneInternalGameName && x.GangID != GangTerritory.GangID && Gang.EnemyGangs.Contains(x.GangID)))
        {
            AmbientEnemyGangs.Add(Gangs.GetGang(gt.GangID));
        }
        //Gang.EnemyGangs.Where(g => GangTerritories.GangTerritoriesList.Any(x => x.ZoneInternalGameName == GangTerritory.ZoneInternalGameName && GangTerritory.GangID == g)).ToList().ForEach(g => { AmbientEnemyGangs.Add(Gangs.GetGang(g)); EntryPoint.WriteToConsole($"{Gang.ID} Territory: {GangTerritory.ZoneInternalGameName}, ENEMY AMBIENT GANG {g} ADDED"); });
    }
    private int CalculateTerritoryValue()
    {
        // Defaulted to Min values, but soon will work with the updated values
        return PlacesOfInterest.PossibleLocations.RacketeeringTaskLocations().Where(x => Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName) && x.IsEnabled).ToList().Sum(x => x.RacketeeringAmountMin);
    }
    public bool IsRobberyTypeAvailable(string LocationType)
    {
        return PlacesOfInterest.PossibleLocations.RobberyTaskLocations().Any(x => (string.IsNullOrEmpty(LocationType) || LocationType == "Random" || x.TypeName == LocationType) && x.IsCorrectMap(World.IsMPMapLoaded) && Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName));
    }
}
