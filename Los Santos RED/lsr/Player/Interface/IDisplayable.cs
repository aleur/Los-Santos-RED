﻿using LosSantosRED.lsr.Locations;
using LosSantosRED.lsr.Player;
using LSR.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IDisplayable
    {
       // Street CurrentCrossStreet { get; }
        VehicleExt CurrentSeenVehicle { get; }
       // Street CurrentStreet { get; }
        VehicleExt CurrentVehicle { get; }
        //Zone CurrentZone { get; }



        LocationData CurrentLocation { get; set; }
        string DebugLine2 { get; }
        string DebugLine7 { get; }
        string DebugLine4 { get; }
        string DebugLine5 { get; }
        string DebugLine6 { get; }
        string DebugLine3 { get; }
        string DebugLine1 { get; }
        bool IsAliveAndFree { get; }
        bool IsDead { get; }
        bool IsBusted { get; }
        bool IsSpeeding { get; }
        bool IsViolatingAnyTrafficLaws { get; }
        bool IsConversing { get; }
        bool IsPerformingActivity { get; }
        bool CanConverseWithLookedAtPed { get; }
        List<ButtonPrompt> ButtonPrompts { get; }
        string CurrentSpeedDisplay { get; }
        string DebugLine8 { get; }
        string DebugLine9 { get; }
        string DebugLine10 { get; }
    }
}