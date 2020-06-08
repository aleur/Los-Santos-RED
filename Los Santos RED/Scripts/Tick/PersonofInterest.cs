﻿using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PersonOfInterest
{
    public static bool PlayerIsPersonOfInterest { get; set; }
    public static bool IsRunning { get; set; } = true;
    public static List<CriminalHistory> CriminalHistory { get; set; }
    public static int MaxWantedLevel
    {
        get
        {
            CriminalHistory LastWanted = GetLastWantedStats();
            if (LastWanted != null)
                return LastWanted.MaxWantedLevel;
            else
                return 0;
        }
    }
    public static float SearchRadius
    {
        get
        {
            if (MaxWantedLevel > 0)
                return MaxWantedLevel * General.MySettings.Police.LastWantedCenterSize;
            else
                return General.MySettings.Police.LastWantedCenterSize;
        }
    }

    
    public static void Initialize()
    {
        IsRunning = true;
        CriminalHistory = new List<CriminalHistory>();
        PlayerIsPersonOfInterest = false;
    }
    public static void Dispose()
    {
        IsRunning = false;
    }    
    public static void Tick()
    {
        if (IsRunning && !PlayerState.IsDead && !PlayerState.IsBusted)
        {
            CheckCurrentVehicle();
            CheckSight();

            if(PlayerState.IsWanted)
            {
                if (!PlayerIsPersonOfInterest && Police.AnyCanSeePlayer)
                {
                    PlayerIsPersonOfInterest = true;
                }
            }
            else
            {
                if (PlayerIsPersonOfInterest && WantedLevelScript.HasBeenNotWantedFor >= 120000)
                {
                    Reset();
                }
            }
        }
    }
    public static void StoreCriminalHistory(CriminalHistory rapSheet)
    {
        CriminalHistory.Add(rapSheet);
        Debugging.WriteToLog("StoreCriminalHistory", "Stored this Rap Sheet");
    }
    public static void Reset()
    {
        PlayerIsPersonOfInterest = false;
        CriminalHistory.Clear();
        Debugging.WriteToLog("ResetPersonOfInterest", "All Previous wanted items are cleared");
    }
    private static void CheckCurrentVehicle()
    {
        if ((PlayerState.IsNotWanted || PlayerState.WantedLevel == 1) && Police.AnyCanRecognizePlayer && PlayerState.IsInVehicle && Game.LocalPlayer.Character.IsInAnyVehicle(false))//first check is cheaper, but second is required to verify
        {
            VehicleExt VehicleToCheck = PlayerState.CurrentVehicle;

            if (VehicleToCheck == null)
                return;

            if (VehicleToCheck.WasReportedStolen && VehicleToCheck.IsStolen && VehicleToCheck.MatchesOriginalDescription)
            {
                ApplyWantedStatsForPlate(VehicleToCheck.CarPlate.PlateNumber);
            }
            else if (VehicleToCheck.CarPlate.IsWanted && !VehicleToCheck.IsStolen && VehicleToCheck.ColorMatchesDescription)
            {
                ApplyWantedStatsForPlate(VehicleToCheck.CarPlate.PlateNumber);
            }
        }
    }
    private static void CheckSight()
    {
        if (PlayerIsPersonOfInterest && Police.AnyCanSeePlayer)
        {
            if (PlayerState.IsWanted)
            {
                ApplyLastWantedStats();
            }
            else
            {
                if (WantedLevelScript.NearLastWanted(SearchRadius) && WantedLevelScript.HasBeenNotWantedFor >= 5000)
                {
                    ApplyLastWantedStats();
                }
            }
        }
    }
    private static void ApplyWantedStatsForPlate(string PlateNumber)
    {
        CriminalHistory StatsForPlate = GetWantedLevelStatsForPlate(PlateNumber);
        if (StatsForPlate != null)
            ApplyWantedStats(StatsForPlate);
    }
    private static void ApplyLastWantedStats()
    {
        CriminalHistory CriminalHistory = GetLastWantedStats();
        if (CriminalHistory != null)
            ApplyWantedStats(CriminalHistory);
    }
    private static void ApplyWantedStats(CriminalHistory CriminalHistory)
    {
        if (CriminalHistory == null)
            return;

        if (PlayerState.WantedLevel < CriminalHistory.MaxWantedLevel)
            WantedLevelScript.SetWantedLevel(CriminalHistory.MaxWantedLevel, "Applying old Wanted stats",true);


        PersonOfInterest.CriminalHistory.Remove(CriminalHistory);
        WantedLevelScript.CurrentCrimes = CriminalHistory;

        DispatchAudio.ClearDispatchQueue();
        Debugging.WriteToLog("WantedLevelStats Replace", WantedLevelScript.CurrentCrimes.DebugPrintCrimes());
    }
    private static CriminalHistory GetLastWantedStats()
    {
        if (CriminalHistory == null || !CriminalHistory.Where(x => x.PlayerSeenDuringWanted).Any())
            return null;

        return CriminalHistory.Where(x => x.PlayerSeenDuringWanted).OrderByDescending(x => x.GameTimeWantedEnded).OrderByDescending(x => x.GameTimeWantedStarted).FirstOrDefault();
    }
    private static CriminalHistory GetWantedLevelStatsForPlate(string PlateNumber)
    {
        if (CriminalHistory == null || !CriminalHistory.Where(x => x.PlayerSeenDuringWanted).Any())
            return null;

        return CriminalHistory.Where(x => x.PlayerSeenDuringWanted && x.WantedPlates.Any(y => y.PlateNumber == PlateNumber)).OrderByDescending(x => x.GameTimeWantedEnded).OrderByDescending(x => x.GameTimeWantedStarted).FirstOrDefault();
    }
}