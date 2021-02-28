﻿using LosSantosRED.lsr;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mod;

public class DispatchableVehicle
{
    public string ModelName { get; set; }
    public int AmbientSpawnChance { get; set; } = 0;
    public int WantedSpawnChance { get; set; } = 0;
    public int MinOccupants { get; set; } = 1;
    public int MaxOccupants { get; set; } = 2;
    public int MinWantedLevelSpawn { get; set; } = 0;
    public int MaxWantedLevelSpawn { get; set; } = 5;
    public List<string> RequiredPassengerModels { get; set; } = new List<string>();//only ped models can spawn in this, if emptyt any ambient spawn can
    public List<int> Liveries { get; set; } = new List<int>();
    public bool IsCar
    {
        get
        {
            return NativeFunction.Natives.IS_THIS_MODEL_A_CAR<bool>(Game.GetHashKey(ModelName));
        }
    }
    public bool IsMotorcycle
    {
        get
        {
            return NativeFunction.Natives.IS_THIS_MODEL_A_BIKE<bool>(Game.GetHashKey(ModelName));
        }
    }
    public bool IsHelicopter
    {
        get
        {
            return NativeFunction.Natives.IS_THIS_MODEL_A_HELI<bool>(Game.GetHashKey(ModelName));
        }
    }
    public bool IsBoat
    {
        get
        {
            return NativeFunction.Natives.IS_THIS_MODEL_A_BOAT<bool>(Game.GetHashKey(ModelName));
        }
    }
    public bool CanSpawnWanted
    {
        get
        {
            if (WantedSpawnChance > 0)
            {
                return true;
            }
            else
                return false;
        }
    }
    public bool CanSpawnAmbient
    {
        get
        {
            if (AmbientSpawnChance > 0)
            {
                return true;
            }
            else
                return false;
        }
    }
    public bool CanCurrentlySpawn(int WantedLevel)
    {
        if (WantedLevel > 0)
        {
            if (WantedLevel >= MinWantedLevelSpawn && WantedLevel <= MaxWantedLevelSpawn)
            {
                return CanSpawnWanted;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return CanSpawnAmbient;
        }
    }
    public int CurrentSpawnChance(int WantedLevel)
    {
        if (!CanCurrentlySpawn(WantedLevel))
        {
            return 0;
        }
        if (WantedLevel > 0)
        {
            if (WantedLevel >= MinWantedLevelSpawn && WantedLevel <= MaxWantedLevelSpawn)
            {
                return WantedSpawnChance;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return AmbientSpawnChance;
        }
    }
    public DispatchableVehicle()
    {

    }
    public DispatchableVehicle(string modelName, int ambientSpawnChance, int wantedSpawnChance)
    {
        ModelName = modelName;
        AmbientSpawnChance = ambientSpawnChance;
        WantedSpawnChance = wantedSpawnChance;
    }
}