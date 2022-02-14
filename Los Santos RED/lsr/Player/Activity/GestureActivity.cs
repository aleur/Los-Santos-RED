﻿using ExtensionsMethods;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player.Activity;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;

namespace LosSantosRED.lsr.Player
{
    public class GestureActivity : DynamicActivity
    {
        private bool IsCancelled;
        private IActionable Player;      
        private uint GameTimeStartedGesturing;
        private GestureData GestureData;
        private int AnimationFlag = 50;
        private float AnimationBlendOutTime = -1.0f;

        public GestureActivity(IActionable player, GestureData gestureData) : base()
        {
            Player = player;
            GestureData = gestureData;
        }

        public override ModItem ModItem { get; set; }
        public override string DebugString => "";
        public override void Cancel()
        {
            IsCancelled = true;
            Player.IsPerformingActivity = false;
        }
        public override void Pause()
        {

        }
        public override void Continue()
        {

        }
        public override void Start()
        {
            EntryPoint.WriteToConsole($"Gesture Start: {GestureData.Name}", 5);
            GameFiber GestureWatcher = GameFiber.StartNew(delegate
            {
                Setup();
                Enter();
            }, "GestureActivity");
        }
        private void Enter()
        {
            Player.SetUnarmed();
            Player.IsPerformingActivity = true;       
            if(GestureData.IsInsulting)
            {
                Player.IsMakingInsultingGesture = true;
            }
            if (GestureData.AnimationEnter != "")
            {
                EntryPoint.WriteToConsole($"Gesture Enter: {GestureData.AnimationEnter}", 5);
                GameTimeStartedGesturing = Game.GameTime;
                NativeFunction.CallByName<uint>("TASK_PLAY_ANIM", Player.Character, GestureData.AnimationDictionary, GestureData.AnimationEnter, 4.0f, AnimationBlendOutTime, -1, AnimationFlag, 0, false, false, false);//-1
                while (Player.CanPerformActivities && !IsCancelled && Game.GameTime - GameTimeStartedGesturing <= 5000)
                {
                    Player.SetUnarmed();
                    float AnimationTime = NativeFunction.CallByName<float>("GET_ENTITY_ANIM_CURRENT_TIME", Player.Character, GestureData.AnimationDictionary, GestureData.AnimationEnter);
                    if (AnimationTime >= 1.0f)
                    {
                        break;
                    }

                   // Rage.Debug.DrawArrowDebug(Player.Character.Position + new Vector3(0f, 0f, 1f), Vector3.Zero, Rotator.Zero, 1f, Color.Yellow);

                    GameFiber.Yield();
                }
                //GameFiber.Sleep(250);
            } 
            Idle();
        }
        private void Idle()
        {
            if (GestureData.AnimationName != "")
            {
                EntryPoint.WriteToConsole($"Gesture Idle: {GestureData.AnimationName}", 5);
                GameTimeStartedGesturing = Game.GameTime;
                NativeFunction.CallByName<uint>("TASK_PLAY_ANIM", Player.Character, GestureData.AnimationDictionary, GestureData.AnimationName, 4.0f, AnimationBlendOutTime, -1, AnimationFlag, 0, false, false, false);//-1
                while (Player.CanPerformActivities && !IsCancelled && Game.GameTime - GameTimeStartedGesturing <= 5000)
                {
                    Player.SetUnarmed();
                    float AnimationTime = NativeFunction.CallByName<float>("GET_ENTITY_ANIM_CURRENT_TIME", Player.Character, GestureData.AnimationDictionary, GestureData.AnimationName);
                    if (AnimationTime >= 1.0f)
                    {
                        break;
                    }

                   // Rage.Debug.DrawArrowDebug(Player.Character.Position + new Vector3(0f, 0f, 1f), Vector3.Zero, Rotator.Zero, 1f, Color.Orange);

                    GameFiber.Yield();
                }
                //GameFiber.Sleep(250);
            }
            Exit();
        }
        private void Exit()
        {
            try
            {
                if (GestureData.AnimationExit != "")
                {
                    EntryPoint.WriteToConsole($"Gesture Exit: {GestureData.AnimationExit}", 5);
                    GameTimeStartedGesturing = Game.GameTime;
                    NativeFunction.CallByName<uint>("TASK_PLAY_ANIM", Player.Character, GestureData.AnimationDictionary, GestureData.AnimationExit, 4.0f, AnimationBlendOutTime, -1, AnimationFlag, 0, false, false, false);//-1
                    while (Player.CanPerformActivities && !IsCancelled && Game.GameTime - GameTimeStartedGesturing <= 5000)
                    {
                        Player.SetUnarmed();
                        float AnimationTime = NativeFunction.CallByName<float>("GET_ENTITY_ANIM_CURRENT_TIME", Player.Character, GestureData.AnimationDictionary, GestureData.AnimationExit);
                        if (AnimationTime >= 1.0f)
                        {
                            break;
                        }

                        //Rage.Debug.DrawArrowDebug(Player.Character.Position + new Vector3(0f, 0f, 1f), Vector3.Zero, Rotator.Zero, 1f, Color.Red);

                        GameFiber.Yield();
                    }
                    //GameFiber.Sleep(250);
                }
                if(GestureData.AnimationEnter != "")
                {
                    NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
                }
                else
                {
                    NativeFunction.Natives.CLEAR_PED_SECONDARY_TASK(Player.Character);
                }
                if (GestureData.IsInsulting)
                {
                    Player.IsMakingInsultingGesture = false;
                }
                //NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
            }
            catch
            {
                Game.DisplayNotification("FAIL");
            }
            Player.IsPerformingActivity = false;
        }
        private void Setup()
        {
            if (GestureData.AnimationDictionary == "")//auto detect
            {
                if (Player.IsMale)
                {
                    if (Player.IsInVehicle)
                    {
                        GestureData.AnimationDictionary = "gestures@m@car@std@casual@ds";
                    }
                    else
                    {
                        if (Player.IsSitting)
                        {
                            GestureData.AnimationDictionary = "gestures@m@sitting@generic@casual";
                        }
                        else
                        {
                            GestureData.AnimationDictionary = "gestures@m@standing@casual";
                        }
                    }
                }
                else
                {
                    if (Player.IsInVehicle)
                    {
                        GestureData.AnimationDictionary = "gestures@m@car@std@casual@ds";
                    }
                    else
                    {
                        if (Player.IsSitting)
                        {
                            GestureData.AnimationDictionary = "gestures@m@sitting@generic@casual";
                        }
                        else
                        {
                            GestureData.AnimationDictionary = "gestures@f@standing@casual";
                        }
                    }
                }
            }

            if(GestureData.AnimationEnter != "")
            {
                AnimationFlag = 2;
                AnimationBlendOutTime = -4.0f;
            }

            EntryPoint.WriteToConsole($"Gesture Setup AnimationDictionary: {GestureData.AnimationDictionary} AnimationEnter: {GestureData.AnimationEnter} AnimationName: {GestureData.AnimationName} AnimationExit: {GestureData.AnimationExit}", 5);
            AnimationDictionary.RequestAnimationDictionay(GestureData.AnimationDictionary);
        }
    }
}




/*
 * 
 * 
 * 
 * anim@mp_player_intselfieblow_kiss enter
anim@mp_player_intselfieblow_kiss exit
anim@mp_player_intselfieblow_kiss idle_a
anim@mp_player_intselfiedock enter
anim@mp_player_intselfiedock exit
anim@mp_player_intselfiedock idle_a
anim@mp_player_intselfiejazz_hands enter
anim@mp_player_intselfiejazz_hands exit
anim@mp_player_intselfiejazz_hands idle_a
anim@mp_player_intselfiethe_bird enter
anim@mp_player_intselfiethe_bird exit
anim@mp_player_intselfiethe_bird idle_a
anim@mp_player_intselfiethumbs_up enter
anim@mp_player_intselfiethumbs_up exit
anim@mp_player_intselfiethumbs_up idle_a
anim@mp_player_intselfiewank enter
anim@mp_player_intselfiewank exit
anim@mp_player_intselfiewank idle_a



mp_player_intfinger mp_player_int_finger
mp_player_introck mp_player_int_rock
mp_player_intsalute mp_player_int_salute
mp_player_intwank mp_player_int_wank

 * */