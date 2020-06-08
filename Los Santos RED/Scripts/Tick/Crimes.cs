﻿using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class Crimes
{
    private static uint GameTimeStartedBrandishing;
    public static bool IsRunning { get; set; } = true;
    public static List<Crime> CrimeList = new List<Crime>();
    public static Crime KillingPolice { get; set; }
    public static Crime FiringWeaponNearPolice { get; set; }
    public static Crime TerroristActivity { get; set; }
    public static Crime BrandishingHeavyWeapon { get; set; }
    public static Crime AimingWeaponAtPolice { get; set; }
    public static Crime HurtingPolice { get; set; }
    public static Crime TrespessingOnGovtProperty { get; set; }
    public static Crime GotInAirVehicleDuringChase { get; set; }
    public static Crime FiringWeapon { get; set; }
    public static Crime KillingCivilians { get; set; }
    public static Crime GrandTheftAuto { get; set; }
    public static Crime DrivingStolenVehicle { get; set; }
    public static Crime Mugging { get; set; }
    public static Crime AttemptingSuicide { get; set; }
    public static Crime BrandishingWeapon { get; set; }
    public static Crime BrandishingCloseCombatWeapon { get; set; }   
    public static Crime HitPedWithCar { get; set; }
    public static Crime HurtingCivilians { get; set; }
    public static Crime HitCarWithCar { get; set; }
    public static Crime ChangingPlates { get; set; }
    public static Crime ResistingArrest { get; set; }
    public static Crime DrivingAgainstTraffic { get; set; }
    public static Crime DrivingOnPavement { get; set; }
    public static Crime NonRoadworthyVehicle { get; set; }
    public static Crime FelonySpeeding { get; set; }
    public static Crime RunningARedLight { get; set; }
    public static List<Crime> CurrentlyViolatingCanBeReportedByCivilians
    {
        get
        {
            return CrimeList.Where(x => x.IsCurrentlyViolating && x.CanBeReportedByCivilians).ToList();
        }
    }
    public static void Initialize()
    {
        IsRunning = true;
        DefaultConfig();
    }
    public static void Dispose()
    {
        IsRunning = false;
    }
    private static void DefaultConfig()
    {

        



         KillingPolice = new Crime("Police Fatality", 3, true, 1) { CanBeReportedByCivilians = false, IsAlwaysFlagged = true };
         TerroristActivity = new Crime("Terrorist Activity", 4, true, 2);
         FiringWeaponNearPolice = new Crime("Shots Fired at Police",  3, true,3) { CanBeReportedByCivilians = false };

         AimingWeaponAtPolice = new Crime("Aiming Weapons At Police",  3, false,4) { CanBeReportedByCivilians = false };
         HurtingPolice = new Crime("Assaulting Police",  3, false,5);
         BrandishingHeavyWeapon = new Crime("Brandishing Heavy Weapon", 3, false, 6) { WillAngerCivilians = true };

         TrespessingOnGovtProperty = new Crime("Trespassing on Government Property",  3, false,7) { CanBeReportedByCivilians = false };
         GotInAirVehicleDuringChase = new Crime("Stealing an Air Vehicle",  3, false, 8);
         
         FiringWeapon = new Crime("Firing Weapon",  2, false,9) { WillAngerCivilians = true };
         KillingCivilians = new Crime("Civilian Fatality", 2, false,10) { WillAngerCivilians = true };
         
         Mugging = new Crime("Mugging", 2, false,11) { WillAngerCivilians = true };
         AttemptingSuicide = new Crime("Attempting Suicide", 2, false,12);






        HurtingCivilians = new Crime("Assaulting Civilians", 2, false, 14) { WillAngerCivilians = true };
        HitPedWithCar = new Crime("Pedestrian Hit and Run", 2, false,15) { WillAngerCivilians = true };
         

         GrandTheftAuto = new Crime("Grand Theft Auto", 2, false, 16) { WillAngerCivilians = true };
         DrivingStolenVehicle = new Crime("Driving a Stolen Vehicle", 2, false, 17) { CanBeReportedByCivilians = false };

        BrandishingWeapon = new Crime("Brandishing Weapon", 2, false, 18) { WillAngerCivilians = true };

        ResistingArrest = new Crime("Resisting Arrest", 2, false, 19) { CanBeReportedByCivilians = false };

         BrandishingCloseCombatWeapon = new Crime("Brandishing Close Combat Weapon", 1, false, 20) { WillAngerCivilians = true };

         HitCarWithCar = new Crime("Hit and Run", 1, false,30) { WillAngerCivilians = true, WillScareCivilians = false };
         ChangingPlates = new Crime("Stealing License Plates", 1, false,31) { WillAngerCivilians = true, WillScareCivilians = false };  
         DrivingAgainstTraffic = new Crime("Driving Against Traffic", 1, false,32) { CanBeReportedByCivilians = false, WillAngerCivilians = false, WillScareCivilians = false };
         DrivingOnPavement = new Crime("Driving On Pavement", 1, false,33) { CanBeReportedByCivilians = false, WillAngerCivilians = false, WillScareCivilians = false };
         NonRoadworthyVehicle = new Crime("Non-Roadworthy Vehicle", 1, false,34) { CanBeReportedByCivilians = false, WillAngerCivilians = false, WillScareCivilians = false };
         FelonySpeeding = new Crime("Speeding", 1, false,35) { CanBeReportedByCivilians = false, WillAngerCivilians = false, WillScareCivilians = false };
         RunningARedLight = new Crime("Running a Red Light", 1, false,36) { CanBeReportedByCivilians = false, WillAngerCivilians = false, WillScareCivilians = false };

        CrimeList = new List<Crime>
        {
            BrandishingCloseCombatWeapon,TerroristActivity,BrandishingHeavyWeapon, FiringWeapon, Mugging, AttemptingSuicide, ResistingArrest, KillingPolice, FiringWeaponNearPolice, AimingWeaponAtPolice, HurtingPolice, TrespessingOnGovtProperty, GotInAirVehicleDuringChase, KillingCivilians, BrandishingWeapon,
            ChangingPlates, GrandTheftAuto, DrivingStolenVehicle, HurtingCivilians, DrivingAgainstTraffic, DrivingOnPavement, HitPedWithCar, HitCarWithCar, NonRoadworthyVehicle, FelonySpeeding, RunningARedLight
        };

    }
    public static void Tick()
    {
        CheckCrimes();
        FlagCrimes();
    }
    private static void CheckCrimes()
    {
        if (PlayerState.IsBusted || PlayerState.IsDead)
            return;

        if (PedWounds.RecentlyKilledCop(5000))
        {
            KillingPolice.IsCurrentlyViolating = true;
        }
        else
        {
            KillingPolice.IsCurrentlyViolating = false;
        }

        if (PedWounds.RecentlyHurtCop(5000))
        {
            HurtingPolice.IsCurrentlyViolating = true;
        }
        else
        {
            HurtingPolice.IsCurrentlyViolating = false;
        }

        if (PedWounds.RecentlyKilledCivilian(5000) || PedWounds.NearCivilianMurderVictim(9f))
        {
            KillingCivilians.IsCurrentlyViolating = true;
        }
        else
        {
            KillingCivilians.IsCurrentlyViolating = false;
        }

        if (PedWounds.RecentlyHurtCivilian(5000))
        {
            HurtingCivilians.IsCurrentlyViolating = true;
        }
        else
        {
            HurtingCivilians.IsCurrentlyViolating = false;
        }

        if (PlayerState.RecentlyShot(5000) || Game.LocalPlayer.Character.IsShooting)
        {
            if (!(Game.LocalPlayer.Character.IsCurrentWeaponSilenced || PlayerState.CurrentWeaponCategory == GTAWeapon.WeaponCategory.Melee))
            {
                FiringWeapon.IsCurrentlyViolating = true;
                if (Police.AnyCanSeePlayer)
                    FiringWeaponNearPolice.IsCurrentlyViolating = true;
            }  
        }
        else
        {
            FiringWeapon.IsCurrentlyViolating = false;
            FiringWeaponNearPolice.IsCurrentlyViolating = false;
        }

        if (Surrender.IsCommitingSuicide)
        {
            AttemptingSuicide.IsCurrentlyViolating = true;
        }
        else
        {
            AttemptingSuicide.IsCurrentlyViolating = false;
        }

        if (CheckBrandishing() && Game.LocalPlayer.Character.Inventory.EquippedWeapon != null && !PlayerState.IsInVehicle)
        {
            BrandishingWeapon.IsCurrentlyViolating = true;
            if(PlayerState.CurrentWeapon != null && PlayerState.CurrentWeapon.WeaponLevel >= 4)
            {
                TerroristActivity.IsCurrentlyViolating = true;
            }
            else
            {
                TerroristActivity.IsCurrentlyViolating = false;
            }
            if (PlayerState.CurrentWeapon != null && PlayerState.CurrentWeapon.WeaponLevel >= 3)
            {
                BrandishingHeavyWeapon.IsCurrentlyViolating = true;
            }
            else
            {
                BrandishingHeavyWeapon.IsCurrentlyViolating = false;
            }
            if (PlayerState.CurrentWeapon != null && PlayerState.CurrentWeapon.Category == GTAWeapon.WeaponCategory.Melee)
            {
                BrandishingCloseCombatWeapon.IsCurrentlyViolating = true;
            }
            else
            {
                BrandishingCloseCombatWeapon.IsCurrentlyViolating = false;
            }
        }
        else
        {
            BrandishingCloseCombatWeapon.IsCurrentlyViolating = false;
            BrandishingWeapon.IsCurrentlyViolating = false;
            TerroristActivity.IsCurrentlyViolating = false;
            BrandishingHeavyWeapon.IsCurrentlyViolating = false;
        }


        if (PlayerState.IsBreakingIntoCar)
        {
            GrandTheftAuto.IsCurrentlyViolating = true;
        }
        else
        {
            GrandTheftAuto.IsCurrentlyViolating = false;
        }

        if (LicensePlateTheft.PlayerChangingPlate)
        {
            ChangingPlates.IsCurrentlyViolating = true;
        }
        else
        {
            ChangingPlates.IsCurrentlyViolating = false;
        }


        if (MuggingScript.IsMugging)
        {
            Mugging.IsCurrentlyViolating = true;
        }
        else
        {
            Mugging.IsCurrentlyViolating = false;
        }

        if (PlayerState.IsWanted && PlayerLocation.PlayerCurrentZone.IsRestrictedDuringWanted)
        {
            TrespessingOnGovtProperty.IsCurrentlyViolating = true;
        }
        else
        {
            TrespessingOnGovtProperty.IsCurrentlyViolating = false;
        }

        if(PlayerState.CurrentVehicle != null && PlayerState.CurrentVehicle.WasReportedStolen)
        {
            DrivingStolenVehicle.IsCurrentlyViolating = true;
        }
        else
        {
            DrivingStolenVehicle.IsCurrentlyViolating = false;
        }


        //if (Game.LocalPlayer.Character.IsAiming && Police.AnyCanSeePlayer)
        //{
        //    AimingWeaponAtPolice.IsCurrentlyViolating = true;
        //}
        //else
        //{
        //    AimingWeaponAtPolice.IsCurrentlyViolating = false;
        //}

        if (PlayerState.IsWanted && Police.AnyCanSeePlayer && Game.LocalPlayer.Character.Speed >= 2.0f && !PlayerState.HandsAreUp && WantedLevelScript.HasBeenWantedFor >= 10000)
        {
            ResistingArrest.IsCurrentlyViolating = true;
        }
        else
        {
            ResistingArrest.IsCurrentlyViolating = false;
        }

        if (PlayerState.IsWanted && PlayerState.IsInVehicle && Game.LocalPlayer.Character.IsInAirVehicle)
        {
            GotInAirVehicleDuringChase.IsCurrentlyViolating = true;
        }
        else
        {
            GotInAirVehicleDuringChase.IsCurrentlyViolating = false;
        }
    }
    private static void FlagCrimes()
    {

        foreach (Crime Violating in CrimeList.Where(x => x.IsCurrentlyViolating))
        {
            if (Police.AnyCanSeePlayer || (Violating.CanReportBySound && Police.AnyCanHearPlayer) || Violating.IsAlwaysFlagged)
            {
                WantedLevelScript.CurrentCrimes.AddCrime(Violating, true,PlayerState.CurrentPosition,PlayerState.CurrentVehicle,PlayerState.CurrentWeapon);
            }
        }
    }
    //private static void MarkVehicleAsStolen(VehicleExt StolenCar)
    //{
    //    GameFiber ReportStolenVehicle = GameFiber.StartNew(delegate
    //    {
    //        GameFiber.Sleep(10000);
    //        StolenCar.WasReportedStolen = true;
    //        if (StolenCar.CarPlate != null && StolenCar.CarPlate.PlateNumber == StolenCar.OriginalLicensePlate.PlateNumber) //if you changed it between when it was reported, dont count it
    //            StolenCar.CarPlate.IsWanted = true;

    //        foreach (LicensePlate Plate in LicensePlateTheft.SpareLicensePlates)
    //        {
    //            if (Plate.PlateNumber == StolenCar.OriginalLicensePlate.PlateNumber)
    //            {
    //                Plate.IsWanted = true;
    //            }
    //        }
    //        //Debugging.WriteToLog("StolenVehicles", String.Format("Vehicle {0} was just reported stolen", StolenCar.VehicleEnt.Handle));

    //    }, "PlayDispatchQueue");
    //    Debugging.GameFibers.Add(ReportStolenVehicle);
    //}
    private static bool CheckBrandishing()
    {
        if (PlayerState.IsConsideredArmed)
        {
            if (GameTimeStartedBrandishing == 0)
                GameTimeStartedBrandishing = Game.GameTime;
        }
        else
        {
            GameTimeStartedBrandishing = 0;
        }

        if (GameTimeStartedBrandishing > 0 && Game.GameTime - GameTimeStartedBrandishing >= 1500)
            return true;
        else
            return false;
    }

}
[Serializable()]
public class Crime
{
    public string Name { get; set; }
    public int ResultingWantedLevel { get; set; } = 0;
    public bool ResultsInLethalForce { get; set; } = false;
    public int Priority { get; set; } = 99;
    public bool CanBeReportedByCivilians { get; set; } = true;
    public bool CanReportBySound { get; set; } = false;
    public bool WillAngerCivilians { get; set; } = false;
    public bool WillScareCivilians { get; set; } = true;
    public bool IsCurrentlyViolating { get; set; } = false;
    public bool IsAlwaysFlagged { get; set; } = false;
    public Crime()
    {

    }
    public Crime(string _Name, int _ResultingWantedLevel, bool _ResultsInLethalForce, int priority)
    {
        ResultsInLethalForce = _ResultsInLethalForce;
        ResultingWantedLevel = _ResultingWantedLevel;
        Name = _Name;
        Priority = priority;
    }
}

public class CrimeEvent
{
    private uint GameTimeLastOccurred;
    private uint InstanceDuration = 20000;
    public CrimeEvent(Crime crimeToReport)
    {
        AssociatedCrime = crimeToReport;
        GameTimeLastOccurred = Game.GameTime;
    }
    public Crime AssociatedCrime { get; set; }
    public int Instances { get; set; } = 1;
    public bool CanAddInstance
    {
        get
        {
            if (Game.GameTime - GameTimeLastOccurred >= InstanceDuration)
                return true;
            else
                return false;
        }
    }
    public bool RecentlyOccurred(uint TimeSince)
    {
        if (Game.GameTime - GameTimeLastOccurred <= TimeSince)
            return true;
        else
            return false;
    }
    public void AddInstance()
    {
        if (CanAddInstance)
        {
            GameTimeLastOccurred = Game.GameTime;
            Instances++;
        }
    }
}

public class CriminalHistory
{
    public int MaxWantedLevel = 0;
    public List<CrimeEvent> CrimesObserved = new List<CrimeEvent>();
    public List<CrimeEvent> CrimesReported = new List<CrimeEvent>();
    public List<LicensePlate> WantedPlates = new List<LicensePlate>();
    public uint GameTimeWantedStarted;
    public uint GameTimeWantedEnded;
    public bool PlayerSeenDuringWanted = false;
    public bool LethalForceAuthorized
    {
        get
        {
            return CrimesObserved.Any(x => x.AssociatedCrime.ResultsInLethalForce);
        }
    }
    public bool CommittedAnyCrimes
    {
        get
        {
            return CrimesObserved.Any();
        }
    }
    public CriminalHistory()
    {

    }
    public string PrintCrimes()
    {
        string CrimeString = "";
        foreach (CrimeEvent MyCrime in CrimesObserved.Where(x => x.Instances > 0).OrderBy(x => x.AssociatedCrime.Priority).Take(3))
        {
            CrimeString += string.Format("~n~{0} ({1})~s~", MyCrime.AssociatedCrime.Name, MyCrime.Instances);
        }
        return CrimeString;
    }
    public string DebugPrintCrimes()
    {
        string CrimeString = "";
        foreach (CrimeEvent MyCrime in CrimesObserved)
        {
            CrimeString += Environment.NewLine + string.Format("{0} ({1})", MyCrime.AssociatedCrime.Name, MyCrime.Instances);
        }
        return CrimeString;
    }
    public int InstancesOfCrime(Crime ToCheck)
    {
        CrimeEvent MyStuff = CrimesObserved.Where(x => x.AssociatedCrime == ToCheck).FirstOrDefault();
        if (MyStuff == null)
            return 0;
        else
            return MyStuff.Instances;
    }
    public void AddCrime(Crime CrimeInstance, bool ByPolice, Vector3 Location, VehicleExt VehicleObserved, GTAWeapon WeaponObserved)
    {
        CrimeEvent PreviousViolation;
        if (ByPolice)
            PreviousViolation = CrimesObserved.FirstOrDefault(x => x.AssociatedCrime.Name == CrimeInstance.Name);
        else
            PreviousViolation = CrimesReported.FirstOrDefault(x => x.AssociatedCrime.Name == CrimeInstance.Name);

        if (PreviousViolation != null)
        {
            PreviousViolation.AddInstance();
        }
        else
        {
            if (ByPolice)
                CrimesObserved.Add(new CrimeEvent(CrimeInstance));
            else
                CrimesReported.Add(new CrimeEvent(CrimeInstance));
        }
        if (ByPolice && PlayerState.WantedLevel != CrimeInstance.ResultingWantedLevel)
        {
            WantedLevelScript.SetWantedLevel(CrimeInstance.ResultingWantedLevel, CrimeInstance.Name, true);
        }
        ScannerScript.AnnounceCrime(CrimeInstance, new DispatchCallIn(!PlayerState.IsInVehicle, ByPolice, Location) { VehicleSeen = VehicleObserved,WeaponSeen = WeaponObserved });
    }

}