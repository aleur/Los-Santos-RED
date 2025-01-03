﻿using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IPlacesOfInterest
    {
        PossibleLocations PossibleLocations { get; set;  }
        List<GameLocation> InteractableLocations();
        List<GameLocation> AllLocations();
        List<ILocationRespawnable> BustedRespawnLocations();
        List<ILocationImpoundable> VehicleImpoundLocations();
        List<ILocationRespawnable> HospitalRespawnLocations();
        List<ILocationSetupable> LocationsToSetup();
        DeadDrop GetUsableDeadDrop(bool IsOnMPMap);
        GangDen GetMainDen(string iD, bool isMPMapLoaded);
        List<ILocationAreaRestrictable> RestrictedAreaLocations();
        List<ILEDispatchableLocation> LEDispatchLocations();
        void Setup();
    }
}
