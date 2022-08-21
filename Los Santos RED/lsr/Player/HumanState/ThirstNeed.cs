﻿using LosSantosRED.lsr.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ThirstNeed : HumanNeed
{
    private IHumanStateable Player;
    private float MinChangeValue = -0.004f;
    private DateTime TimeLastUpdatedValue;
    private ITimeReportable Time;
    private float RealTimeScalar;
    private ISettingsProvideable Settings;
    private bool ShouldSlowDrain => Player.IsResting || Player.IsSleeping || Player.IsSitting || Player.IsLayingDown;
    private bool ShouldChange => Player.IsAlive && !RecentlyChanged;
    public ThirstNeed(string name, float minValue, float maxValue, IHumanStateable humanStateable, ITimeReportable time, ISettingsProvideable settings) : base(name, minValue, maxValue, humanStateable, time, settings.SettingsManager.NeedsSettings.ThirstDisplayDigits)
    {
        Player = humanStateable;
        Time = time;
        TimeLastUpdatedValue = Time.CurrentDateTime;
        Settings = settings;
    }
    public override void OnMaximum()
    {

    }
    public override void OnMinimum()
    {

    }
    public override void Update()
    {
        if (NeedsValueUpdate && Settings.SettingsManager.NeedsSettings.ApplyThirst)
        {
            UpdateRealTimeScalar();
            if (ShouldChange)
            {
                Drain();
            }
        }
    }
    private void Drain()
    {
        float ChangeAmount = MinChangeValue * RealTimeScalar;
        if (ShouldSlowDrain)
        {
            ChangeAmount *= 0.25f;
        }
        if (!Player.IsInVehicle)
        {
            ChangeAmount *= FootSpeedMultiplier();
        }
        Change(ChangeAmount, false);
    }
    private float FootSpeedMultiplier()
    {
        float Multiplier = 1.0f;
        if (Player.FootSpeed >= 1.0f)
        {
            Multiplier = Player.FootSpeed / 5.0f;
        }
        if (Multiplier <= 1.0f)
        {
            Multiplier = 1.0f;
        }
        return Multiplier;
    }
    private void UpdateRealTimeScalar()
    {
        RealTimeScalar = 1.0f;
        TimeSpan TimeDifference = Time.CurrentDateTime - TimeLastUpdatedValue;
        RealTimeScalar = (float)TimeDifference.TotalSeconds;
        TimeLastUpdatedValue = Time.CurrentDateTime;
    }
}
