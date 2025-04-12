﻿using LosSantosRED.lsr.Interface;
using Rage;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TimerBarController
{
    private uint GameTimeLastUpdated;
    private TimerBarPool TimerBarPool;
    private IDisplayable Player;

    private BarTimerBar StaminaBar;
    private BarTimerBar Intoxication;
    private BarTimerBar SearchMode;
    private int itemsDisplaying;
    private ISettingsProvideable Settings;
    //private TextTimerBar BlankBar;
    private TextTimerBar RaceTimer;

    private bool IsTimeToUpdate => Game.GameTime - GameTimeLastUpdated >= 250;
    public int ItemsDisplaying { get; private set; }
    public TimerBarController(IDisplayable player, TimerBarPool timerBarPool, ISettingsProvideable settings)
    {
        Player = player;
        TimerBarPool = timerBarPool;
        Settings = settings;
    }
    public void Setup()
    {
        return;
        StaminaBar = new BarTimerBar("Stamina");
        StaminaBar.BackgroundColor = Color.FromArgb(100, 142, 50, 50);
        StaminaBar.ForegroundColor = Color.FromArgb(255, 181, 48, 48);//Red

        Intoxication = new BarTimerBar("Intoxication Level");
        Intoxication.BackgroundColor = Color.FromArgb(100, 72, 133, 164);
        Intoxication.ForegroundColor = Color.FromArgb(255, 72, 133, 164);//Blue

        SearchMode = new BarTimerBar("Police Searching");
        SearchMode.BackgroundColor = Color.FromArgb(100, 202, 169, 66);
        SearchMode.ForegroundColor = Color.FromArgb(255, 202, 169, 66);//Yellow


      //  BlankBar = new TextTimerBar("TEST", "HELLO");

        RaceTimer = new TextTimerBar("Time","00:00.000");
        Player.RacingManager.SetRaceTimer(RaceTimer);

        //if (TimerBarPool != null)
        //{
        //    TimerBarPool.Add(StaminaBar, Intoxication, SearchMode);
        //}
    }
    public void Update()
    {
        //itemsDisplaying = 0;
        //UpdateStamina();
        //UpdateIntoxication();
        //UpdateSearchMode();
        //UpdateRaceTimer();
        //ItemsDisplaying = itemsDisplaying;
        //TimerBarPool.OrderBy(x => x.Label);
        //GameTimeLastUpdated = Game.GameTime;




    }
    public void Dispose()
    {

    }
    private void UpdateRaceTimer()
    {
        if(!Player.RacingManager.IsRacing)
        {
            SafeRemove(RaceTimer);
        }
        else
        {
            itemsDisplaying++;
            SafeAdd(RaceTimer);
        }
    }
    private void UpdateStamina()
    {
        StaminaBar.Percentage = Player.Sprinting.StaminaPercentage;
        if (Player.IsAliveAndFree && Player.Sprinting.StaminaPercentage < 1.0f && Settings.SettingsManager.LSRHUDSettings.ShowStaminaDisplay && Player.Sprinting.CanSprint && Player.Sprinting.CanRegainStamina)
        {
            itemsDisplaying++;
            //ItemsDisplaying++;
           // SafeAdd(BlankBar);
            SafeAdd(StaminaBar);
            
        }
        else
        {
           // SafeRemove(BlankBar);
            SafeRemove(StaminaBar);
        }
    }
    private void UpdateIntoxication()
    {
        Intoxication.Percentage = Player.Intoxication.CurrentIntensityPercent;
        if (Player.IsAliveAndFree && Player.Intoxication.CurrentIntensityPercent > 0.0f && Settings.SettingsManager.LSRHUDSettings.ShowIntoxicationDisplay)
        {
            itemsDisplaying++;
            SafeAdd(Intoxication);
        }
        else
        {
            SafeRemove(Intoxication);
        }
    }
    private void UpdateSearchMode()
    {
        SearchMode.Percentage = Player.SearchMode.SearchModePercentage;
        if (Player.IsAliveAndFree && Player.IsInSearchMode && Settings.SettingsManager.LSRHUDSettings.ShowSearchModeDisplay)
        {
            itemsDisplaying++;
            SafeAdd(SearchMode);         
        }
        else
        {
            SafeRemove(SearchMode);
        }
    }
    private void SafeRemove(TimerBarBase toRemove)
    {
        if (TimerBarPool.Contains(toRemove))
        {
            TimerBarPool.Remove(toRemove);
        }
    }
    private void SafeAdd(TimerBarBase toAdd)
    {
        if (!TimerBarPool.Contains(toAdd))
        {
            TimerBarPool.Add(toAdd);
        }

    }
}

