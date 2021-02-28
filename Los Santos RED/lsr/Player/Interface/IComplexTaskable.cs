﻿using LSR.Vehicles;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IComplexTaskable
    {
        bool IsInVehicle { get; }
        bool IsInHelicopter { get; }
        bool IsInBoat { get; }
        float DistanceToPlayer { get; }
        bool IsDriver { get; }
        bool IsStill { get; }
        Ped Pedestrian { get; }
        int LastSeatIndex { get; }
        List<Crime> CrimesWitnessed { get; }
        VehicleExt VehicleLastSeenPlayerIn { get; }
        WeaponInformation WeaponLastSeenPlayerWith { get; }
        bool EverSeenPlayer { get; }
        float ClosestDistanceToPlayer { get; }
        Vector3 PositionLastSeenCrime { get; }
    }
}