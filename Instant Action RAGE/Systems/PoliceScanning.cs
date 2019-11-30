﻿using ExtensionsMethods;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


public static class PoliceScanning
{
    public static List<GTACop> CopPeds { get; private set; } = new List<GTACop>();
    public static List<GTACop> K9Peds { get; private set; } = new List<GTACop>();
    public static List<Ped> Civilians { get; private set; } = new List<Ped>();
    public static bool IsRunning { get; set; } = true;
    public static string AgenciesChasingPlayer
    {
        get
        {
            return string.Join(" ", CopPeds.Where(x => x.SeenPlayerSince(60000)).Select(x => x.AssignedAgency.ColorPrefix + x.AssignedAgency.Initials).Distinct().ToArray());
        }
    }
    public static void Initialize()
    {

    }
    public static void Dispose()
    {
        IsRunning = false;
        foreach(GTACop Cop in CopPeds)
        {
            if(Cop.CopPed.Exists())
            {
                if (Cop.CopPed.IsInAnyVehicle(false))
                    Cop.CopPed.CurrentVehicle.Delete();
                Cop.CopPed.Delete();
            }
        }
        CopPeds.Clear();
    }
    public static void ScanForPolice()
    {
        Ped[] Pedestrians = Array.ConvertAll(World.GetEntities(Game.LocalPlayer.Character.Position, 450f, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed).Where(x => x is Ped).ToArray(), (x => (Ped)x));//250
        foreach (Ped Pedestrian in Pedestrians.Where(s => s.Exists() && !s.IsDead && s.IsVisible))
        {
            if(Pedestrian.isPoliceArmy())
            {
                if (!CopPeds.Any(x => x.CopPed == Pedestrian))
                {
                    bool canSee = false;
                    if (Pedestrian.PlayerIsInFront() && Pedestrian.IsInRangeOf(Game.LocalPlayer.Character.Position, 55f) && NativeFunction.CallByName<bool>("HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT", Pedestrian, Game.LocalPlayer.Character))
                        canSee = true;

                    GTACop myCop = new GTACop(Pedestrian, canSee, canSee ? Game.GameTime : 0, canSee ? Game.LocalPlayer.Character.Position : new Vector3(0f, 0f, 0f), Pedestrian.Health,Agencies.GetAgencyFromPed(Pedestrian));
                    Pedestrian.IsPersistent = false;
                    if (Settings.OverridePoliceAccuracy)
                        Pedestrian.Accuracy = Settings.PoliceGeneralAccuracy;
                    Pedestrian.Inventory.Weapons.Clear();
                    Police.IssueCopPistol(myCop);
                    NativeFunction.CallByName<bool>("SET_PED_COMBAT_ATTRIBUTES", Pedestrian, 7, false);//No commandeering//https://gtaforums.com/topic/833391-researchguide-combat-behaviour-flags/
                    if (Tasking.SpotterCop != null && Tasking.SpotterCop.Handle == Pedestrian.Handle)
                        continue;

                    CopPeds.Add(myCop);

                    if (Settings.IssuePoliceHeavyWeapons && Police.CurrentPoliceState == Police.PoliceState.DeadlyChase)
                        Police.IssueCopHeavyWeapon(myCop);
                }
            }
            else
            {
                //if (!Civilians.Any(x => x == Pedestrian))
                //{
                //    Civilians.Add(Pedestrian);
                //}
            }
        }
        CopPeds.RemoveAll(x => !x.CopPed.Exists() || x.CopPed.IsDead);
        K9Peds.RemoveAll(x => !x.CopPed.Exists() || x.CopPed.IsDead);
        Civilians.RemoveAll(x => !x.Exists() || x.IsDead);
    }
}