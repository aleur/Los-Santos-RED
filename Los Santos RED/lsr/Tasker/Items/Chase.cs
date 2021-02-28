﻿using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LosSantosRED.lsr.Helper;


public class Chase : ComplexTask
{
    private bool NeedsUpdates;
    private SubTask CurrentSubTask;
    private Task CurrentTask = Task.Nothing;
    private Vehicle CopsVehicle;
    private bool IsGoingFast;
    private bool IsStuck;
    private Vector3 LastPosition;
    private bool IsFirstRun;
    private uint GameTimeGotStuck;
    private uint GameTimeVehicleStoppedMoving;
    private uint GameTimeChaseStarted;
    private float ChaseDistance = 5f;
    private int VehicleMissionFlag;
    private bool IsChasingRecklessly;

    private enum Task
    {
        VehicleChase,
        VehicleChasePed,
        ExitVehicle,
        EnterVehicle,
        CarJack,
        FootChase,
        Nothing,
    }
    private enum eVehicleMissionType
    {
        Cruise = 1,
        Ram = 2,
        Block = 3,
        GoTo = 4,
        Stop = 5,
        Attack = 6,
        Follow = 7,
        Flee = 8,
        Circle = 9,
        Escort = 12,
        FollowRecording = 15,
        PoliceBehaviour = 16,
        Land = 19,
        Land2 = 20,
        Crash = 21,
        PullOver = 22,
        HeliProtect = 23
    };
    private enum SubTask
    {
        Shoot,
        Aim,
        Goto,
    }
    private bool ShouldChaseRecklessly => Player.PoliceResponse.IsDeadlyChase;
    private bool ShouldChaseVehicleInVehicle => Ped.IsDriver && Ped.Pedestrian.CurrentVehicle.Exists() && !ShouldExitPoliceVehicle && Player.CurrentVehicle != null;
    private bool ShouldChasePedInVehicle => Ped.DistanceToPlayer >= 25f;
    private bool ShouldGetBackInCar => CopsVehicle.Exists() && Ped.Pedestrian.DistanceTo2D(CopsVehicle) <= 30f && CopsVehicle.IsDriveable && CopsVehicle.FreeSeatsCount > 0;
    private bool ShouldCarJackPlayer => Player.CurrentVehicle != null && Player.CurrentVehicle.Vehicle.Exists() && !Player.IsMovingFast;// && !Cop.Pedestrian.IsGettingIntoVehicle;
    private bool ShouldExitPoliceVehicle => Ped.DistanceToPlayer < 25f && Ped.Pedestrian.CurrentVehicle.Exists() && VehicleIsStopped && !Player.IsMovingFast && !ChaseRecentlyStarted && !Ped.IsInHelicopter && !Ped.IsInBoat;
    private bool ChaseRecentlyStarted => GameTimeChaseStarted != 0 &&  Game.GameTime - GameTimeChaseStarted <= 3000;
    private bool VehicleIsStopped => GameTimeVehicleStoppedMoving != 0 && Game.GameTime - GameTimeVehicleStoppedMoving >= 500;//20000
    private bool ShouldShoot => !Player.IsBusted && !Player.IsAttemptingToSurrender;
    private Task GetCurrentTaskDynamic()
    {
        if (CurrentDynamic == AIDynamic.Cop_InVehicle_Player_InVehicle)
        {
            if (ShouldExitPoliceVehicle)
            {
                return Task.ExitVehicle;
            }
            else if (ShouldChaseVehicleInVehicle)
            {
                return Task.VehicleChase;
            }
            else
            {
                return Task.Nothing;
            }
        }
        else if (CurrentDynamic == AIDynamic.Cop_InVehicle_Player_OnFoot)
        {
            if (Ped.IsDriver)
            {
                if (ShouldChasePedInVehicle)
                {
                    return Task.VehicleChasePed;
                }
                else if (ShouldExitPoliceVehicle)
                {
                    return Task.ExitVehicle;
                }
                else
                {
                    return Task.Nothing;
                }
            }
            else
            {
                if (ShouldExitPoliceVehicle)
                {
                    return Task.ExitVehicle;
                }
                else
                {
                    return Task.Nothing;
                }
            }
        }
        else if (CurrentDynamic == AIDynamic.Cop_OnFoot_Player_InVehicle)
        {
            if (ShouldCarJackPlayer)
            {
                return Task.CarJack;
            }
            else if (ShouldGetBackInCar)
            {
                return Task.EnterVehicle;
            }
            else
            {
                return Task.Nothing;
            }
        }
        else if (CurrentDynamic == AIDynamic.Cop_OnFoot_Player_OnFoot)
        {
            return Task.FootChase;
        }
        else
        {
            return Task.Nothing;
        }
    }
    public Chase(IComplexTaskable cop, ITargetable player) : base(player, cop, 500)//was 500
    {
        Name = "Chase";
        SubTaskName = "";
    }
    public Chase(IComplexTaskable cop, ITargetable player, float chaseDistance, int vehicleMissionFlag) : base(player, cop, 500)//was 500
    {
        Name = "Chase";
        SubTaskName = "";
        ChaseDistance = chaseDistance;
        VehicleMissionFlag = vehicleMissionFlag;
    }
    public override void Start()
    {
        EntryPoint.WriteToConsole($"TASKER: Chase Start: {Ped.Pedestrian.Handle} ChaseDistance: {ChaseDistance} VehicleMissionFlag: {VehicleMissionFlag}",5);
        GameTimeChaseStarted = Game.GameTime;
        NativeFunction.Natives.SET_PED_PATH_CAN_USE_CLIMBOVERS(Ped.Pedestrian, true);
        NativeFunction.Natives.SET_PED_PATH_CAN_USE_LADDERS(Ped.Pedestrian, true);
        NativeFunction.Natives.SET_PED_PATH_CAN_DROP_FROM_HEIGHT(Ped.Pedestrian, true);
        Update();
    }
    public override void Update()
    {

        if (Ped.Pedestrian.Exists() && ShouldUpdate)
        {
            if (Ped.Pedestrian.IsInAnyPoliceVehicle)
            {
                CopsVehicle = Ped.Pedestrian.CurrentVehicle;
            }
            Task UpdatedTask = GetCurrentTaskDynamic();
            if (CurrentTask != UpdatedTask)
            {
                IsFirstRun = true;
                CurrentTask = UpdatedTask;
                //EntryPoint.WriteToConsole($"TASKER: Chase SubTask Changed: {Ped.Pedestrian.Handle} to {CurrentTask} {CurrentDynamic}");
                ExecuteCurrentSubTask();
            }
            else if (NeedsUpdates)
            {
                ExecuteCurrentSubTask();
            }
            if (Ped.IsInVehicle)//CurrentTask == Task.VehicleChase || CurrentTask == Task.VehicleChasePed || Cu)
            {
                NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Ped.Pedestrian, (int)eChaseBehaviorFlag.NoContact, true);
                SetSiren();
                if (Ped.IsInVehicle && Ped.Pedestrian.CurrentVehicle.Exists())
                {
                    if (Ped.Pedestrian.CurrentVehicle.Speed == 0f)
                    {
                        if (GameTimeVehicleStoppedMoving == 0)
                        {
                            GameTimeVehicleStoppedMoving = Game.GameTime;
                        }

                    }
                    else
                    {
                        GameTimeVehicleStoppedMoving = 0;
                    }
                }
            }
            GameTimeLastRan = Game.GameTime;
        }
        //EntryPoint.WriteToConsole($"TASKER: Chase UpdateEnd: {Ped.Pedestrian.Handle} InVeh: {Ped.IsInVehicle} InVeh2: {Ped.Pedestrian.IsInAnyVehicle(false)}");
    }
    private void ExecuteCurrentSubTask()
    {
        if (CurrentTask == Task.CarJack)
        {
            SubTaskName = "CarJack";
            GoToPlayersCar();
        }
        else if (CurrentTask == Task.EnterVehicle)
        {
            SubTaskName = "EnterVehicle";
            EnterVehicle();
        }
        else if (CurrentTask == Task.ExitVehicle)
        {
            SubTaskName = "ExitVehicle";
            ExitVehicle();
        }
        else if (CurrentTask == Task.FootChase)
        {
            SubTaskName = "FootChase";
            FootChase();
        }
        else if (CurrentTask == Task.VehicleChase)
        {
            SubTaskName = "VehicleChase";
            VehicleChase();
        }
        else if (CurrentTask == Task.VehicleChasePed)
        {
            SubTaskName = "VehicleChasePed";
            VehicleChasePed();
        }
        else if (CurrentTask == Task.Nothing)
        {
            SubTaskName = "Nothing";
            VehicleChasePed();
        }
        GameTimeLastRan = Game.GameTime;
    }
    private void GoToPlayersCar()
    {
        NeedsUpdates = false;
        unsafe
        {
            int lol = 0;
            NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
            NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", 0, Player.CurrentVehicle.Vehicle, -1, 7f, 500f, 1073741824, 1); //Original and works ok
           // NativeFunction.CallByName<bool>("TASK_OPEN_VEHICLE_DOOR", 0, Player.CurrentVehicle.Vehicle, -1, -1, 7f); //doesnt really work
            NativeFunction.CallByName<bool>("TASK_ENTER_VEHICLE", 0, Player.CurrentVehicle.Vehicle, -1, Player.Character.SeatIndex, 5.0f, 9);//caused them to get confused about getting back in thier car
            NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
            NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
            NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
            NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
        }
    }
    private void EnterVehicle()
    {
        NeedsUpdates = false;
        if (CopsVehicle.Exists())
        {
            NativeFunction.CallByName<bool>("TASK_ENTER_VEHICLE", Ped.Pedestrian, CopsVehicle, -1, Ped.LastSeatIndex, 2.0f, 9);
        }
        //EntryPoint.WriteToConsole(string.Format("Started Enter Old Car: {0}", Ped.Pedestrian.Handle));

    }
    private void ExitVehicle()
    {
        NeedsUpdates = false;
        if (Ped.Pedestrian.CurrentVehicle.Exists())
        {
            //NativeFunction.CallByName<uint>("TASK_VEHICLE_TEMP_ACTION", Cop.Pedestrian, Cop.Pedestrian.CurrentVehicle, 27, 2000);
            //NativeFunction.CallByName<bool>("TASK_LEAVE_VEHICLE", Cop.Pedestrian, Cop.Pedestrian.CurrentVehicle, 256);
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<uint>("TASK_VEHICLE_TEMP_ACTION", 0, Ped.Pedestrian.CurrentVehicle, 27, 1000);
                NativeFunction.CallByName<bool>("TASK_LEAVE_VEHICLE", 0, Ped.Pedestrian.CurrentVehicle, 256);
                NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", 0, Player.Character, -1, 7f, 500f, 1073741824, 1); //Original and works ok
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, false);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
            //EntryPoint.WriteToConsole(string.Format("Started Exit Car: {0}", Ped.Pedestrian.Handle));
        }
    }
    private void FootChase()
    {
        NeedsUpdates = true;
        if (Player.WantedLevel >= 2)
        {
            NativeFunction.CallByName<uint>("SET_PED_MOVE_RATE_OVERRIDE", Ped.Pedestrian, (float)(RandomItems.MyRand.NextDouble() * (1.175 - 1.1) + 1.1));
        }
        Ped.Pedestrian.BlockPermanentEvents = true;
        Ped.Pedestrian.KeepTasks = true;
        if (CurrentSubTask != SubTask.Shoot && ShouldShoot && Ped.DistanceToPlayer <= 7f)
        {
            CurrentSubTask = SubTask.Shoot;
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY_WHILE_AIMING_AT_ENTITY", 0, Player.Character, Player.Character, 200f, true, 4.0f, 200f, false, false, (uint)FiringPattern.DelayFireByOneSecond);
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
            //EntryPoint.WriteToConsole($"FootChase Shoot: {Ped.Pedestrian.Handle}");
        }
        else if (CurrentSubTask != SubTask.Aim && !ShouldShoot && Ped.DistanceToPlayer <= 7f)
        {
            CurrentSubTask = SubTask.Aim;
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<bool>("TASK_GOTO_ENTITY_AIMING", 0, Player.Character, 4f, 20f);
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
            //EntryPoint.WriteToConsole($"FootChase Aim: {Ped.Pedestrian.Handle}");
        }
        else if (CurrentSubTask != SubTask.Goto && Ped.DistanceToPlayer >= 15f)
        {
            CurrentSubTask = SubTask.Goto;
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<bool>("TASK_GO_TO_ENTITY", 0, Player.Character, -1, 7f, 500f, 1073741824, 1); //Original and works ok
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
            //EntryPoint.WriteToConsole($"FootChase GoTo: {Ped.Pedestrian.Handle}");
        }
    }
    private void VehicleChase()
    {
        NeedsUpdates = true;
        if(IsFirstRun)
        {
            NativeFunction.Natives.SET_DRIVER_ABILITY(Ped.Pedestrian, 100f);

            //doesnt seem to do anything with these tasks
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(Ped.Pedestrian, ChaseDistance);//5f// RandomItems.GetRandomNumber(3f,10f));
            //NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Ped.Pedestrian, (int)eChaseBehaviorFlag.NoContact, false);
            //NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Ped.Pedestrian, (int)eChaseBehaviorFlag.LowContact, false);
            //NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Ped.Pedestrian, (int)eChaseBehaviorFlag.MediumContact, false);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Ped.Pedestrian, (int)eChaseBehaviorFlag.PIT, true);
            //  NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Ped.Pedestrian, (int)eChaseBehaviorFlag.FullContact, true);

            //NativeFunction.Natives.TASK_VEHICLE_CHASE(Ped.Pedestrian, Player.Character);
            Ped.Pedestrian.BlockPermanentEvents = true;
            Ped.Pedestrian.KeepTasks = true;
            int DesiredStyle = 0;//(int)eDrivingStyles.AvoidVehicles | (int)eDrivingStyles.AvoidEmptyVehicles | (int)eDrivingStyles.AvoidPeds | (int)eDrivingStyles.AvoidObject | (int)eDrivingStyles.AllowWrongWay | (int)eDrivingStyles.ShortestPath;
                                 //unsafe
                                 //{
                                 //    int lol = 0;
                                 //    NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);

            //    // NativeFunction.CallByName<bool>("TASK_VEHICLE_CHASE", 0, Player.CurrentVehicle.Vehicle);


            //    NativeFunction.CallByName<bool>("TASK_VEHICLE_MISSION", 0, Ped.Pedestrian.CurrentVehicle, Player.CurrentVehicle.Vehicle, VehicleMissionFlag, 50f, DesiredStyle, ChaseDistance, ChaseDistance, true);//8f
            //    //pullover, they drive , not the best seem to stop
            //    //follow, follow you like chase, but will hit each other
            //    //police behavior = do nothing
            //    //GoTo, they drive m not the best
            //    //escort, they get right on the bumper, drive pretty well
            //    //ram, does as expected
            //    //block, latches onto bumper and tries to speed off
            //    //attack same as block, but seems to only go once?
            //    //crash, freaks out the car
            //    //Circle, nothing
            //    //cruise, wander
            //    //stop, nothing
            //    //followrecording, nothing

            //    //10 gets on the back left side
            //    //11 follows on right side
            //    //13 kinda rams and drives in front
            //    //14 will ram a bit, drives kinda slow
            //    //17 turns right and drives straight
            //    //18 same but in reverse
            //    NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
            //    NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
            //    NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
            //    NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            //}
            //vanilla uses different missions for ramming, but it seems to use CHASE for the main chase thing
            //the other stuff doesnt seem to work!

            if (Ped.IsInHelicopter)
            {
                NativeFunction.Natives.TASK_HELI_CHASE(Ped.Pedestrian, Player.Character, -50f, 50f, 60f);
            }
            else if (Ped.IsInBoat)
            {
                NativeFunction.Natives.TASK_VEHICLE_CHASE(Ped.Pedestrian, Player.Character);
            }
            else
            {
                if (ShouldChaseRecklessly)
                {
                    IsChasingRecklessly = true;
                    unsafe
                    {
                        int lol = 0;
                        NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                        NativeFunction.CallByName<bool>("TASK_VEHICLE_MISSION", 0, Ped.Pedestrian.CurrentVehicle, Player.CurrentVehicle.Vehicle, (int)eVehicleMissionType.Ram, 50f, DesiredStyle, ChaseDistance, 0f, true);//8f
                        NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                        NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                        NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                        NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
                    }
                }
                else
                {
                    IsChasingRecklessly = false;
                    NativeFunction.Natives.TASK_VEHICLE_CHASE(Ped.Pedestrian, Player.Character);
                }
            }
            EntryPoint.WriteToConsole($"VehicleChase Vehicle Target: {Ped.Pedestrian.Handle} IsChasingRecklessly: {IsChasingRecklessly}",5);
            IsFirstRun = false;
        }
        else
        {
            NativeFunction.Natives.SET_DRIVER_ABILITY(Ped.Pedestrian, 100f);
            if (Ped.IsInHelicopter)
            {
                NativeFunction.Natives.TASK_HELI_CHASE(Ped.Pedestrian, Player.Character, -50f, 50f, 60f);
            }
            else if (Ped.IsInBoat)
            {
                NativeFunction.Natives.TASK_VEHICLE_CHASE(Ped.Pedestrian, Player.Character);
            }
            else
            {
                if (IsChasingRecklessly != ShouldChaseRecklessly)
                {
                    if (ShouldChaseRecklessly)
                    {
                        IsChasingRecklessly = true;
                        unsafe
                        {
                            int lol = 0;
                            NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                            NativeFunction.CallByName<bool>("TASK_VEHICLE_MISSION", 0, Ped.Pedestrian.CurrentVehicle, Player.CurrentVehicle.Vehicle, (int)eVehicleMissionType.Ram, 50f, 0, 0f, 0f, true);//8f
                            NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                            NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                            NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                            NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
                        }
                    }
                    else
                    {
                        IsChasingRecklessly = false;
                        NativeFunction.Natives.TASK_VEHICLE_CHASE(Ped.Pedestrian, Player.Character);
                    }
                }
            }
            Vector3 CurrentPosition = Ped.Pedestrian.Position;
            IsStuck = LastPosition.DistanceTo2D(CurrentPosition) <= 1.0f;
            if (IsStuck)
            {
                if (GameTimeGotStuck == 0)
                {
                    GameTimeGotStuck = Game.GameTime;
                }
            }
            else
            {
                GameTimeGotStuck = 0;
            }
            if (IsStuck && Game.GameTime - GameTimeGotStuck >= 3000)
            {
                EntryPoint.WriteToConsole($"VehicleChase Vehicle Target I AM STUCK!!: {Ped.Pedestrian.Handle}",5);
            }
            LastPosition = CurrentPosition;
        }
    }
    private void VehicleChaseOLD()
    {
        NeedsUpdates = true;
        if (IsFirstRun)
        {
            NativeFunction.Natives.SET_DRIVER_ABILITY(Ped.Pedestrian, 100f);
            NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(Ped.Pedestrian, ChaseDistance);//5f// RandomItems.GetRandomNumber(3f,10f));
            Ped.Pedestrian.BlockPermanentEvents = true;
            Ped.Pedestrian.KeepTasks = true;
            int DesiredStyle = (int)eDrivingStyles.AvoidVehicles | (int)eDrivingStyles.AvoidEmptyVehicles | (int)eDrivingStyles.AvoidPeds | (int)eDrivingStyles.AvoidObject | (int)eDrivingStyles.AllowWrongWay | (int)eDrivingStyles.ShortestPath;
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<bool>("TASK_VEHICLE_MISSION", 0, Ped.Pedestrian.CurrentVehicle, Player.CurrentVehicle.Vehicle, (int)eVehicleMissionType.Follow, 50f, DesiredStyle, ChaseDistance, 0f, true);//8f
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
            //EntryPoint.WriteToConsole($"VehicleChase Vehicle Target: {Ped.Pedestrian.Handle}");
            IsFirstRun = false;
        }
        else
        {
            NativeFunction.Natives.SET_DRIVER_ABILITY(Ped.Pedestrian, 100f);
            Vector3 CurrentPosition = Ped.Pedestrian.Position;
            IsStuck = LastPosition.DistanceTo2D(CurrentPosition) <= 1.0f;
            if (IsStuck)
            {
                if (GameTimeGotStuck == 0)
                {
                    GameTimeGotStuck = Game.GameTime;
                }
            }
            else
            {
                GameTimeGotStuck = 0;
            }
            if (IsStuck && Game.GameTime - GameTimeGotStuck >= 3000)
            {
                EntryPoint.WriteToConsole($"VehicleChase Vehicle Target I AM STUCK!!: {Ped.Pedestrian.Handle}",5);
            }
            LastPosition = CurrentPosition;
        }
    }
    private void VehicleChasePed()
    {
        if(Ped.Pedestrian.CurrentVehicle.Exists())
        {
            NeedsUpdates = false;
        }
        else
        {
            NeedsUpdates = true;
            return;

        }

        Ped.Pedestrian.BlockPermanentEvents = true;
        Ped.Pedestrian.KeepTasks = true;
        float Speed = 30f;
        if(Ped.DistanceToPlayer <= 10f)
        {
            Speed = 10f;
        }
        if(Ped.IsInHelicopter)
        {
            NativeFunction.Natives.TASK_HELI_CHASE(Ped.Pedestrian, Player.Character, -50f, 50f, 60f);
        }
        else if (Ped.IsInBoat)
        {
            NativeFunction.Natives.TASK_VEHICLE_CHASE(Ped.Pedestrian, Player.Character);
        }
        else
        {
            unsafe
            {
                int lol = 0;
                NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
                NativeFunction.CallByName<bool>("TASK_VEHICLE_MISSION_PED_TARGET", 0, Ped.Pedestrian.CurrentVehicle, Player.Character, 7, Speed, 4 | 8 | 16 | 32 | 512 | 262144, 8f, 0f, true);
                NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
                NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
                NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", Ped.Pedestrian, lol);
                NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            }
        }
        //NativeFunction.CallByName<bool>("TASK_VEHICLE_MISSION_PED_TARGET", Cop.Pedestrian, Cop.Pedestrian.CurrentVehicle, Player.Character, 7, 30f, 4 | 8 | 16 | 32 | 512 | 262144, 0f, 0f, true);
        EntryPoint.WriteToConsole($"VehicleChase Ped Target: {Ped.Pedestrian.Handle}",5);
    }
    private void SetSiren()
    {
        if (Ped.Pedestrian.CurrentVehicle.Exists() && Ped.Pedestrian.CurrentVehicle.HasSiren && !Ped.Pedestrian.CurrentVehicle.IsSirenOn)
        {
            Ped.Pedestrian.CurrentVehicle.IsSirenOn = true;
            Ped.Pedestrian.CurrentVehicle.IsSirenSilent = false;
        }
    }
    public override void Stop()
    {

    }
}
