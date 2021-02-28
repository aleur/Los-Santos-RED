﻿using LosSantosRED.lsr;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class Radio
{
    private VehicleExt VehicleToMonitor;
    private string CurrentRadioStationName;

    public Radio(VehicleExt vehicleToMonitor)
    {
        VehicleToMonitor = vehicleToMonitor;
    }
    public bool CanChangeStation
    {
        get
        {
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false) && Game.LocalPlayer.Character.CurrentVehicle.IsEngineOn)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public void Update(string DesiredStation)
    {
        if (DesiredStation != "NONE")
        {
            unsafe
            {
                IntPtr ptr = NativeFunction.CallByName<IntPtr>("GET_PLAYER_RADIO_STATION_NAME");
                CurrentRadioStationName = Marshal.PtrToStringAnsi(ptr);
            }
            if (CurrentRadioStationName != DesiredStation)
            {
                SetRadioStation(DesiredStation);
            }
        }
    }
    public void SetNextTrack()
    {
        if (CanChangeStation)
        {
            NativeFunction.CallByName<bool>("SKIP_RADIO_FORWARD");
        }

    }
    private void SetRadioStation(string StationName)
    {
        if (VehicleToMonitor != null && VehicleToMonitor.Vehicle.IsEngineOn && VehicleToMonitor.Vehicle.Exists())
        {
            NativeFunction.CallByName<bool>("SET_VEH_RADIO_STATION", VehicleToMonitor.Vehicle, StationName);
        }
    }
}