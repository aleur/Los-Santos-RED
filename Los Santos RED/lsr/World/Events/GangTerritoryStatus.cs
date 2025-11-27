using ExtensionsMethods;
using LosSantosRED.lsr;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

public class GangTerritoryStatus
{
    private readonly IEntityProvideable World;
    private readonly IZones Zones;
    private readonly IPlacesOfInterest PlacesOfInterest;
    private IGangTerritories GangTerritories;
    private GangTerritory GangTerritory;
    private Gang Gang;
    // Location Checks
    public bool HasStores { get; set; }
    public bool HasEnemyGangScenarios { get; set; }
    public bool IsMainTurf { get; set; }
    // Turf Needs
    public bool NeedsReinforcements { get; set; }
    public bool NeedsSupplies { get; set; }
    public GangTerritoryStatus(IEntityProvideable world, IZones zones, IPlacesOfInterest placesOfInterest, IGangTerritories gangTerritories, GangTerritory gangTerritory, Gang gang)
    {
        World = world;
        Zones = zones;
        PlacesOfInterest = placesOfInterest;
        GangTerritories = gangTerritories;
        GangTerritory = gangTerritory;
        Gang = gang;

        HasStores = World.Places.ActiveLocations.Any(x => Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName) && PlacesOfInterest.PossibleLocations.RacketeeringTaskLocations().Contains(x));
        HasEnemyGangScenarios = SelectEnemyScenario() != null;
        IsMainTurf = GangTerritory.Priority == 0;
    }
    public BlankLocation SelectEnemyScenario()
    {
        return World.Places.ActiveLocations.OfType<BlankLocation>().Where(x => Zones.GetZone(x.EntrancePosition) == Zones.GetZone(GangTerritory.ZoneInternalGameName) && x.PossiblePedSpawns.OfType<GangConditionalLocation>().Where(y => Gang.EnemyGangs.Contains(y.AssociationID)).Any()).PickRandom();
    }
}
