using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Properties
{
    private IPropertyOwnable Player;
    private IPlacesOfInterest PlacesOfInterest;
    private ITimeReportable Time;
    private IEntityProvideable World;
    public Properties(IPropertyOwnable player, IPlacesOfInterest placesOfInterest, ITimeReportable time, IEntityProvideable world)
    {
        Player = player;
        PlacesOfInterest = placesOfInterest;
        Time = time;
        World = world;
    }
    public List<GameLocation> PropertyList { get; private set; } = new List<GameLocation>();
    public List<Residence> Residences { get; private set; } = new List<Residence>();
    public List<GameLocation> Businesses { get; private set; } = new List<GameLocation>();
    //public List<GameLocation> PayoutProperties { get; private set; } = new List<GameLocation>();
    //public List<GameLocation> CraftingLocations { get; private set; } = new List<GameLocation>();
    public void Setup()
    {

    }
    public void Update()
    {
        //int businessesPayingOut = 0;
        foreach (GameLocation property in PropertyList)
        {
            property.HandleOwnedLocation(Player, Time);
        }
    }
    public void Dispose()
    {
        Reset();
    }
    public void Reset()
    {
        foreach(GameLocation property in PropertyList)
        {
            property.Reset();
        }
        PropertyList.Clear();
        Residences.Clear();
        Businesses.Clear();
    }
    public void AddOwnedLocation(GameLocation toAdd)
    {
        if (!PropertyList.Any(x => x.Name == toAdd.Name && x.EntrancePosition == toAdd.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            PropertyList.Add(toAdd);
        }
        if (!Businesses.Any(x => x.Name == toAdd.Name && x.EntrancePosition == toAdd.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            Businesses.Add(toAdd);
        }
    }
    public void AddOwnedLocation(Residence toAdd)
    {
        if (!PropertyList.Any(x => x.Name == toAdd.Name && x.EntrancePosition == toAdd.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            PropertyList.Add(toAdd);
        }
        if (!Residences.Any(x => x.Name == toAdd.Name && x.EntrancePosition == toAdd.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            Residences.Add(toAdd);
        }
    }
    public void RemoveOwnedLocation(GameLocation toRemove)
    {
        if (PropertyList.Any(x => x.Name == toRemove.Name && x.EntrancePosition == toRemove.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            PropertyList.Remove(toRemove);
        }
        if (Businesses.Any(x => x.Name == toRemove.Name && x.EntrancePosition == toRemove.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            Businesses.Remove(toRemove);
        }
    }
    public void RemoveOwnedLocation(Residence toRemove)
    {
        if (PropertyList.Any(x => x.Name == toRemove.Name && x.EntrancePosition == toRemove.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            PropertyList.Remove(toRemove);
        }
        if (Residences.Any(x => x.Name == toRemove.Name && x.EntrancePosition == toRemove.EntrancePosition && x.IsCorrectMap(World.IsMPMapLoaded)))
        {
            Residences.Remove(toRemove);
        }
    }
}

