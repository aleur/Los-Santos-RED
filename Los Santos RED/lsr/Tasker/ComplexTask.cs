﻿using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class ComplexTask
{
    protected IComplexTaskable Ped;
    protected ITargetable Player;
    private uint RunInterval;
    protected ComplexTask(ITargetable player, IComplexTaskable ped, uint runInterval)
    {
        Player = player;
        Ped = ped;
        RunInterval = runInterval;
    }
    public AIDynamic CurrentDynamic
    {
        get
        {
            if (Player.IsInVehicle)
            {
                if (Ped.IsInVehicle)
                {
                    return AIDynamic.Cop_InVehicle_Player_InVehicle;
                }
                else
                {
                    return AIDynamic.Cop_OnFoot_Player_InVehicle;
                }
            }
            else
            {
                if (Ped.IsInVehicle)
                {
                    return AIDynamic.Cop_InVehicle_Player_OnFoot;
                }
                else
                {
                    return AIDynamic.Cop_OnFoot_Player_OnFoot;
                }
            }
        }
    }
    public uint GameTimeLastRan { get; set; }
    public string Name { get; set; }
    public string SubTaskName { get; set; }
    public bool ShouldUpdate => GameTimeLastRan == 0 || Game.GameTime - GameTimeLastRan >= RunInterval;
    public abstract void Start();
    public abstract void Stop();
    public abstract void Update();
}
