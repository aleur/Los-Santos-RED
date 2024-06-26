﻿using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Mod;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GunshotAlert : PedAlert
{
    public GunshotAlert(PedExt pedExt, ISettingsProvideable settings) : base(pedExt,settings, ePedAlertType.GunShot)
    {
        Priority = 0;
        IsPositionAlert = true;
        TimeOutTime = Settings.SettingsManager.WorldSettings.GunshotAlertTimeout;
    }
    public override void Update(IPoliceRespondable policeRespondable, IEntityProvideable world)
    {
        if(!PedExt.PedAlertTypes.HasFlag(ePedAlertType.GunShot))
        {
            return;
        }
        if (policeRespondable.Violations.WeaponViolations.RecentlyShot && PedExt.WithinWeaponsAudioRange)
        {
            //EntryPoint.WriteToConsole($"I AM PED {PedExt.Handle} AND I JUST HEARD GUNFIRE FROM THE PLAYER");
            AddAlert(policeRespondable.Position);
        }
        else if (policeRespondable.Violations.WeaponViolations.RecentlyShotSuppressed && PedExt.DistanceToPlayer <= 10f)
        {
            //EntryPoint.WriteToConsole($"I AM PED {PedExt.Handle} AND I JUST HEARD GUNFIRE FROM THE PLAYER");
            AddAlert(policeRespondable.Position);
        }
        Cop cop = world.Pedestrians.AllPoliceList.FirstOrDefault(x => NativeHelper.IsNearby(PedExt.CellX, PedExt.CellY, x.CellX, x.CellY, 3) && x.IsShooting && x.Pedestrian.Exists());
        if (cop != null && cop.Pedestrian.Exists())
        {
            //EntryPoint.WriteToConsole($"I AM PED {PedExt.Handle} AND I JUST HEARD GUNFIRE FROM ANOTHER COP");
            AddAlert(cop.Pedestrian.Position);
        }  
        base.Update(policeRespondable, world);

        PedExt.OnHeardGunfire(policeRespondable);


    }
}

