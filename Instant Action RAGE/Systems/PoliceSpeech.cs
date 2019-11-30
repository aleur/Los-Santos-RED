﻿using ExtensionsMethods;
using NAudio.Wave;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class PoliceSpeech
{
    private static Random rnd;
    private static List<string> DeadlyChaseSpeech = new List<string> { "CHALLENGE_THREATEN", "COMBAT_TAUNT", "FIGHT", "GENERIC_INSULT", "GENERIC_WAR_CRY", "GET_HIM", "REQUEST_BACKUP", "REQUEST_NOOSE", "SHOOTOUT_OPEN_FIRE" };
    private static List<string> UnarmedChaseSpeech = new List<string> { "FOOT_CHASE", "FOOT_CHASE_AGGRESIVE", "FOOT_CHASE_LOSING", "FOOT_CHASE_RESPONSE", "GET_HIM", "SUSPECT_SPOTTED" };
    private static List<string> CautiousChaseSpeech = new List<string> { "DRAW_GUN", "GET_HIM", "COP_ARRIVAL_ANNOUNCE", "MOVE_IN", "MOVE_IN_PERSONAL" };
    private static List<string> ArrestedWaitSpeech = new List<string> { "DRAW_GUN", "GET_HIM", "COP_ARRIVAL_ANNOUNCE", "MOVE_IN", "MOVE_IN_PERSONAL", "SURROUNDED" };
    private static List<string> PlayerDeadSpeech = new List<string> { "SUSPECT_KILLED", "WON_DISPUTE" };

    public static bool IsRunning { get; set; } = true;
    static PoliceSpeech()
    {
        rnd = new Random();
    }
    public static void Initialize()
    {
        MainLoop();
    }
    public static void Dispose()
    {
        IsRunning = false;
    }
    private static void MainLoop()
    {
        GameFiber.StartNew(delegate
        {
            try
            {
                while (IsRunning)
                {
                    CheckSpeech();
                    GameFiber.Sleep(500);
                }
            }
            catch (Exception e)
            {
                InstantAction.Dispose();
                Debugging.WriteToLog("Error", e.Message + " : " + e.StackTrace);
            }
        });
    }
    private static void CheckSpeech()
    {
        try
        {
            foreach (GTACop Cop in PoliceScanning.CopPeds.Where(x => x.CanSpeak && x.DistanceToPlayer <= 45f && x.CopPed.Exists() && !x.CopPed.IsDead))
            {
                if (Cop.isTasked)
                {
                    if (InstantAction.IsBusted && Cop.DistanceToPlayer <= 20f)
                    {
                        Cop.CopPed.PlayAmbientSpeech("ARREST_PLAYER");
                       LocalWriteToLog("CheckSpeech", "ARREST_PLAYER");
                    }
                    else if (Police.CurrentPoliceState == Police.PoliceState.UnarmedChase)
                    {
                        string Speech = UnarmedChaseSpeech.PickRandom();
                        Cop.CopPed.PlayAmbientSpeech(Speech);
                        LocalWriteToLog("CheckSpeech", Speech);
                    }
                    else if (Police.CurrentPoliceState == Police.PoliceState.CautiousChase)
                    {
                        string Speech = CautiousChaseSpeech.PickRandom();
                        Cop.CopPed.PlayAmbientSpeech(Speech);
                        LocalWriteToLog("CheckSpeech", Speech);
                    }
                    else if (Police.CurrentPoliceState == Police.PoliceState.ArrestedWait)
                    {
                        string Speech = ArrestedWaitSpeech.PickRandom();
                        Cop.CopPed.PlayAmbientSpeech(Speech);
                        LocalWriteToLog("CheckSpeech", Speech);
                    }
                    else if (Police.CurrentPoliceState == Police.PoliceState.DeadlyChase)
                    {
                        string Speech = DeadlyChaseSpeech.PickRandom();
                        Cop.CopPed.PlayAmbientSpeech(Speech);
                        LocalWriteToLog("CheckSpeech", Speech);
                    }
                    else //Normal State
                    {
                        if(Cop.DistanceToPlayer <= 4f)
                        {
                            Cop.CopPed.PlayAmbientSpeech("CRIMINAL_WARNING");
                            LocalWriteToLog("CheckSpeech", "CRIMINAL_WARNING");
                        }
                    }
                }
                else
                {
                    if(InstantAction.IsDead && Cop.DistanceToPlayer <= 20f)
                    {
                        string Speech = PlayerDeadSpeech.PickRandom();
                        Cop.CopPed.PlayAmbientSpeech(Speech);
                        LocalWriteToLog("CheckSpeech", Speech);
                    }
                }
                Cop.GameTimeLastSpoke = Game.GameTime - (uint)rnd.Next(500,1000);
            }          
        }
        catch (Exception e)
        {
            Game.Console.Print(e.Message);
        }
    }
    private static void LocalWriteToLog(string ProcedureString, string TextToLog)
    {
        if (Settings.PoliceSpeechLogging)
            Debugging.WriteToLog(ProcedureString, TextToLog);
    }

}
