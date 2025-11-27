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
    public string ZoneName { get; private set; } 
    // Location Checks
    public bool HasRacketStores { get; set; }
    public bool HasRobberyStores { get; set; }
    public bool IsMainTurf { get; set; }
    public List<Gang> EnemyGangsWithScenarios { get; set; } = new List<Gang>();
    public List<Gang> AmbientEnemyGangs { get; set; } = new List<Gang>();
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

        ZoneName = Zones.GetZone(gangTerritory.ZoneInternalGameName).DisplayName;

        HasRacketStores = PlacesOfInterest.PossibleLocations.RacketeeringTaskLocations().Any(x => x.IsCorrectMap(World.IsMPMapLoaded) && Zones.GetZone(x.EntrancePosition) == Zones.GetZone(gangTerritory.ZoneInternalGameName));
        HasRobberyStores = PlacesOfInterest.PossibleLocations.RobberyTaskLocations().Any(x => x.IsCorrectMap(World.IsMPMapLoaded) && Zones.GetZone(x.EntrancePosition) == Zones.GetZone(gangTerritory.ZoneInternalGameName));

        CheckAmbientEnemyGangs();
        GetEnemyGangsInTurf();

        IsMainTurf = GangTerritory.Priority == 0;
    }
    private void GetEnemyGangsInTurf()
    {
        List<BlankLocation> blankLocations = World.Places.ActiveLocations.OfType<BlankLocation>().Where(x => Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName) && x.IsEnabled && x.CanBeAmbushableTarget).ToList();
        //EntryPoint.WriteToConsole($"{Gang.FullName}{GangTerritory.ZoneInternalGameName} BL? {blankLocations.Any()}");
        foreach (BlankLocation bl in blankLocations)
        {
            List<GangConditionalLocation> enemyPedSpawns = bl.PossiblePedSpawns?.OfType<GangConditionalLocation>().Where(y => Gang.EnemyGangs.Contains(y.AssociationID)).ToList();
            List<GangConditionalLocation> enemyGroupSpawns = bl.PossibleGroupSpawns?.Where(g => g.CanBeAmbushableTarget).SelectMany(g => g.PossiblePedSpawns.OfType<GangConditionalLocation>().Where(y => Gang.EnemyGangs.Contains(y.AssociationID))).ToList();

            List<GangConditionalLocation> selectedSpawns = new List<GangConditionalLocation>();

            if (enemyGroupSpawns?.Any() == true) selectedSpawns = enemyGroupSpawns;
            else if (enemyPedSpawns?.Any() == true) selectedSpawns = enemyPedSpawns;

            foreach (GangConditionalLocation gcl in selectedSpawns)
            {
                Gang gang = Gangs.GetGang(gcl.AssociationID);
                if (gang != null && !EnemyGangsWithScenarios.Contains(gang)) 
                    EnemyGangsWithScenarios.Add(gang);
            }
        }
    }
    private bool CheckEnemyScenarios()
    {
        return World.Places.ActiveLocations.OfType<BlankLocation>().Any(x => Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName) && x.IsEnabled && x.CanBeAmbushableTarget &&
            (x.PossiblePedSpawns?.OfType<GangConditionalLocation>().Where(y => Gang.EnemyGangs.Contains(y.AssociationID)).Any() == true || 
            x.PossibleGroupSpawns?.Any(y => y.CanBeAmbushableTarget && y.PossiblePedSpawns.OfType<GangConditionalLocation>().ToList().Any()) == true));
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
