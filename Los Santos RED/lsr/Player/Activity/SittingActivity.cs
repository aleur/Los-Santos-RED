﻿using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player.Activity;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LosSantosRED.lsr.Player
{
    public class SittingActivity : DynamicActivity
    {
        private string PlayingAnim;
        private string PlayingDict;
        private SittingData Data;
        private bool IsCancelled;
        private IActionable Player;
        private ISettingsProvideable Settings;
        private bool IsActivelySitting = false;
        private Vector3 StartingPosition;
        private int PlayerScene;
        private List<SeatModel> SeatModels;
        private List<TableModel> TableModels;
        //0xe7ed1a59 doesnt work properly, del pierro beach bench
        //0xd3c6d323 del pierro beach plastic chair
        //0x643d1f90 maze bus bench, sit too far forward
        private Entity ClosestSittableEntity;
        private Entity PossibleCollisionTable;
        private Vector3 StoredPlayerPosition;
        private float StoredPlayerHeading;
        private bool FindSittingProp = false;
        private bool EnterForward = true;


        private Vector3 SeatEntryPosition;
        private float SeatEntryHeading;


        public SittingActivity(IActionable player, ISettingsProvideable settings, bool findSittingProp, bool enterForward) : base()
        {
            Player = player;
            Settings = settings;
            FindSittingProp = findSittingProp;
            EnterForward = enterForward;
        }
        public override ModItem ModItem { get; set; }
        public override string DebugString => "";
        public override void Cancel()
        {
            IsCancelled = true;
            Player.IsSitting = false;
            //Player.IsPerformingActivity = false;
        }
        public override void Pause()
        {

        }
        public override void Continue()
        {
        }
        public override void Start()
        {
            EntryPoint.WriteToConsole("Sitting Activity Started", 5);
            Setup();
            GameFiber ScenarioWatcher = GameFiber.StartNew(delegate
            {
                Enter();
            }, "Sitting");
        }
        private void Enter()
        {
            EntryPoint.WriteToConsole($"Sitting Activity Enter {FindSittingProp}", 5);
            Player.SetUnarmed();
            Player.IsSitting = true;
            if (FindSittingProp)
            {
                if (!GetSittableProp() || !GetSeatCoordinates() || !MoveToSeatCoordinates())
                {
                    Player.IsSitting = false;
                    if (ClosestSittableEntity.Exists())
                    {
                        ClosestSittableEntity.IsPositionFrozen = false;
                    }
                    EntryPoint.WriteToConsole("Sitting Activity No Seat Found", 5);
                    return;
                }
            }
            else
            {
                SeatEntryPosition = Player.Character.Position;
                SeatEntryHeading = Player.Character.Heading;
            }
            GetPossibleBlockingProp();
            SitDown();
            if (IsActivelySitting)
            {
                Idle();
            }
            else
            {
                Exit();
            }
        }
        private void Idle()
        {
            EntryPoint.WriteToConsole("Sitting Activity Idle", 5);
            StartNewBaseScene();
            float AnimationTime;
            while (Player.CanPerformActivities && !IsCancelled)
            {
                AnimationTime = NativeFunction.CallByName<float>("GET_SYNCHRONIZED_SCENE_PHASE", PlayerScene);
                if(AnimationTime >= 1.0f && !Player.IsPerformingActivity)
                {
                    StartNewIdleScene();
                }
                if(Player.IsMoveControlPressed)
                {
                    IsCancelled = true;
                }
                Player.SetUnarmed();

                if (PossibleCollisionTable.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                {
                    NativeFunction.Natives.SET_ENTITY_NO_COLLISION_ENTITY(Player.Character, PossibleCollisionTable, true);
                }


                GameFiber.Yield();
            }
            Exit();
        }
        private void Exit()
        {
            EntryPoint.WriteToConsole("Sitting Activity Exit", 5);
            Player.PauseDynamicActivity();

            if (Settings.SettingsManager.ActivitySettings.TeleportWhenSitting && FindSittingProp)
            {
                Game.FadeScreenOut(500, true);
                Player.Character.Position = StoredPlayerPosition;
                Player.Character.Heading = StoredPlayerHeading;
                EntryPoint.WriteToConsole($"Sitting Teleport, Set To Stored Previous Position {StoredPlayerPosition}", 5);
                NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
                Game.FadeScreenIn(500, true);
                Player.IsSitting = false;
                
            }
            else
            {

                if (IsActivelySitting)
                {
                    PlayingDict = Data.AnimExitDictionary;
                    PlayingAnim = Data.AnimExit;
                    Vector3 Position = Game.LocalPlayer.Character.Position;
                    float Heading = Game.LocalPlayer.Character.Heading;
                    PlayerScene = NativeFunction.CallByName<int>("CREATE_SYNCHRONIZED_SCENE", Position.X, Position.Y, Game.LocalPlayer.Character.Position.Z, 0.0f, 0.0f, Heading, 2);//270f //old
                    NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_LOOPED", PlayerScene, false);
                    NativeFunction.CallByName<bool>("TASK_SYNCHRONIZED_SCENE", Game.LocalPlayer.Character, PlayerScene, PlayingDict, PlayingAnim, 1000.0f, -4.0f, 64, 0, 0x447a0000, 0);//std_perp_ds_a
                    NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_PHASE", PlayerScene, 0.0f);

                    float AnimationTime = 0f;
                    while (AnimationTime < 1.0f)
                    {
                        AnimationTime = NativeFunction.CallByName<float>("GET_SYNCHRONIZED_SCENE_PHASE", PlayerScene);
                        Player.SetUnarmed();
                        GameFiber.Yield();
                    }
                    EntryPoint.WriteToConsole("Sitting Activity Exit 2", 5);
                }
                EntryPoint.WriteToConsole("Sitting Activity Exit 3", 5);
                AnimationDictionary.RequestAnimationDictionay("ped");
                NativeFunction.Natives.TASK_PLAY_ANIM(Player.Character, "ped", "handsup_enter", 2.0f, -2.0f, -1, 2, 0, false, false, false);
                if (ClosestSittableEntity.Exists())
                {
                    ClosestSittableEntity.IsPositionFrozen = false;
                }
                //if (PossibleCollisionTable.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                //{
                //    NativeFunction.Natives.SET_ENTITY_NO_COLLISION_ENTITY(Player.Character, PossibleCollisionTable, false);
                //}

                GameFiber.Yield();
                NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
                //Player.IsPerformingActivity = false;
                Player.IsSitting = false;
                GameFiber.Sleep(5000);
                //if (PossibleCollisionTable.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                //{
                //    EntryPoint.WriteToConsole("Sitting Activity Exit Collision Added", 5);
                //    NativeFunction.Natives.SET_ENTITY_NO_COLLISION_ENTITY(Player.Character, 0, true);
                //}

                if (PossibleCollisionTable.Exists() && ClosestSittableEntity.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                {
                    PossibleCollisionTable.IsPositionFrozen = false;
                }
            }
        }
        private void SitDown()
        {
            PlayingDict = Data.AnimEnterDictionary;
            PlayingAnim = Data.AnimEnter;
            StartingPosition = Game.LocalPlayer.Character.Position;
            float Heading = Game.LocalPlayer.Character.Heading;
            PlayerScene = NativeFunction.CallByName<int>("CREATE_SYNCHRONIZED_SCENE", StartingPosition.X, StartingPosition.Y, Game.LocalPlayer.Character.Position.Z, 0.0f, 0.0f, Heading, 2);//270f //old
            NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_LOOPED", PlayerScene, false);
            NativeFunction.CallByName<bool>("TASK_SYNCHRONIZED_SCENE", Game.LocalPlayer.Character, PlayerScene, Data.AnimEnterDictionary, Data.AnimEnter, 1000.0f, -4.0f, 64, 0, 0x447a0000, 0);//std_perp_ds_a
            NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_PHASE", PlayerScene, 0.0f);
            float AnimationTime = 0f;
            while (Player.CanPerformActivities && !IsCancelled && AnimationTime < 1.0f)
            {
                AnimationTime = NativeFunction.CallByName<float>("GET_SYNCHRONIZED_SCENE_PHASE", PlayerScene);
                if (Player.IsMoveControlPressed)
                {
                    IsCancelled = true;
                }
                Player.SetUnarmed();
                GameFiber.Yield();
            }
            if (NativeFunction.CallByName<float>("GET_SYNCHRONIZED_SCENE_PHASE", PlayerScene) >= 0.95f)
            {
                IsActivelySitting = true;
            }
        }
        private bool GetSittableProp()
        {
            List<Rage.Object> Objects = World.GetAllObjects().ToList();
            float ClosestDistance = 999f;
            foreach (Rage.Object obj in Objects)
            {
                if (obj.Exists())
                {
                    string modelName = obj.Model.Name.ToLower();
                    uint hash = obj.Model.Hash;
                    SeatModel seatModel = SeatModels.FirstOrDefault(x => x.ModelHash == hash);
                    if(seatModel == null)
                    {
                        seatModel = SeatModels.FirstOrDefault(x => x.ModelName?.ToLower() == modelName);
                    }
                    if (seatModel != null || modelName.Contains("chair") || modelName.Contains("sofa") || modelName.Contains("couch") || modelName.Contains("bench") || modelName.Contains("seat") || modelName.Contains("chr"))
                    {
                        float DistanceToObject = obj.DistanceTo(Game.LocalPlayer.Character.Position);
                        if (DistanceToObject <= 2.5f && DistanceToObject >= 0.5f && DistanceToObject <= ClosestDistance)//
                        {
                            ClosestSittableEntity = obj;
                            ClosestDistance = DistanceToObject;
                        }
                    }
                }
            }
            return ClosestSittableEntity.Exists();
        }
        private bool GetSeatCoordinates()
        {
            if (ClosestSittableEntity.Exists())
            {
                EntryPoint.WriteToConsole($"Sitting Closest = {ClosestSittableEntity.Model.Name}", 5);
                ClosestSittableEntity.IsPositionFrozen = true;
                uint hash = ClosestSittableEntity.Model.Hash;
                string modelName = ClosestSittableEntity.Model.Name.ToLower();
                SeatModel seatModel = SeatModels.FirstOrDefault(x => x.ModelHash == hash);
                if (seatModel == null)
                {
                    seatModel = SeatModels.FirstOrDefault(x => x.ModelName?.ToLower() == modelName);
                }
                float offset = -0.5f;
                if (seatModel != null)
                {
                    offset = seatModel.EntryOffsetFront;
                    EntryPoint.WriteToConsole($"Sitting Closest = {ClosestSittableEntity.Model.Name} using custom offset {offset}", 5);
                }
                EntryPoint.WriteToConsole($"Sitting Activity ClosestSittableEntity X {ClosestSittableEntity.Model.Dimensions.X} Y {ClosestSittableEntity.Model.Dimensions.Y} Z {ClosestSittableEntity.Model.Dimensions.Z}", 5);
                Vector3 DesiredPos = ClosestSittableEntity.GetOffsetPositionFront(offset);
                DesiredPos = new Vector3(DesiredPos.X, DesiredPos.Y, Game.LocalPlayer.Character.Position.Z);
                float DesiredHeading = Math.Abs(ClosestSittableEntity.Heading + 180f);
                float ObjectHeading = ClosestSittableEntity.Heading;
                if (ClosestSittableEntity.Heading >= 180f)
                {
                    DesiredHeading = ClosestSittableEntity.Heading - 180f;
                }
                else
                {
                    DesiredHeading = ClosestSittableEntity.Heading + 180f;
                }


                float ModelWidth = ClosestSittableEntity.Model.Dimensions.X;
                float TrimmedModelWidth = ModelWidth - 0.6f;//add two edges
                if (ModelWidth >= 1.2f)
                {
                    int SeatNumber = (int)(ModelWidth / 0.7f);
                    if(SeatNumber > 1)
                    {
                        List<Vector3> PossibleSeatPositions = new List<Vector3>();

                        float FirstPosition = ModelWidth / (float)SeatNumber;
                        

                        for (int i = 0; i < SeatNumber; i++)
                        {
                            float XPosition = (FirstPosition * (i + 1)) - 0.6f;
                            Vector3 SeatPosition = ClosestSittableEntity.GetOffsetPosition(new Vector3((-1 * ModelWidth / 2) + XPosition,offset, 0f));//   ClosestSittableEntity.GetOffsetPositionFront(offset) + ClosestSittableEntity.GetOffsetPositionRight((-1 * ModelWidth / 2) + XPosition);
                            PossibleSeatPositions.Add(SeatPosition);
                            EntryPoint.WriteToConsole($"Sitting Partition {SeatNumber} XPosition {XPosition} Position {SeatPosition}", 5);
                        }

                        float ClosestDistance = 99f;
                        Vector3 ClosestPosition = Vector3.Zero;
                        foreach(Vector3 possiblePos in PossibleSeatPositions)
                        {
                            float distanceto = Player.Character.Position.DistanceTo2D(possiblePos);
                            if(distanceto < ClosestDistance)
                            {
                                ClosestPosition = possiblePos;
                                ClosestDistance = distanceto;
                            }
                        }
                        SeatEntryPosition = ClosestPosition;
                        SeatEntryHeading = DesiredHeading;
                        return true;
                    }
                }




                SeatEntryPosition = DesiredPos;
                SeatEntryHeading = DesiredHeading;
                return true;
            }
            else
            {
                return false;
            }
        }
        private void GetPossibleBlockingProp()
        {
            if (Settings.SettingsManager.ActivitySettings.SetNoTableCollisionWhenSitting)
            {
                List<Rage.Object> Objects = World.GetAllObjects().ToList();
                float ClosestDistance = 999f;
                foreach (Rage.Object obj in Objects)
                {
                    if (obj.Exists() && (!ClosestSittableEntity.Exists() || obj.Handle != ClosestSittableEntity.Handle))// && obj.Model.Name.ToLower().Contains("chair") || obj.Model.Name.ToLower().Contains("bench") || obj.Model.Name.ToLower().Contains("seat") || obj.Model.Name.ToLower().Contains("chr") || SeatModels.Contains(obj.Model.Hash))
                    {
                        uint tableHash = obj.Model.Hash;
                        string tableModelName = obj.Model.Name.ToLower();
                        TableModel tableModel = TableModels.FirstOrDefault(x => x.Hash == tableHash);
                        if (tableModel == null)
                        {
                            tableModel = TableModels.FirstOrDefault(x => x.ModelName?.ToLower() == tableModelName);
                        }
                        float DistanceToObject = obj.DistanceTo2D(SeatEntryPosition);
                        if (tableModel != null && DistanceToObject <= 2f)
                        {
                            PossibleCollisionTable = obj;
                            ClosestDistance = DistanceToObject;
                            break;
                        }
                        if (Player.AttachedProp.Exists() && Player.AttachedProp.Handle != obj.Handle)
                        {
                            if (DistanceToObject <= 2f && DistanceToObject <= ClosestDistance)
                            {
                                PossibleCollisionTable = obj;
                                ClosestDistance = DistanceToObject;
                            }
                        }
                    }
                }
                //if (PossibleCollisionTable.Exists() && ClosestSittableEntity.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                //{
                //    NativeFunction.Natives.SET_ENTITY_NO_COLLISION_ENTITY(Player.Character, PossibleCollisionTable, true);
                //}


                if(PossibleCollisionTable.Exists() && ClosestSittableEntity.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                {
                    PossibleCollisionTable.IsPositionFrozen = true;
                }
            }
        }
        private bool MoveToSeatCoordinates()
        {
            if (Settings.SettingsManager.ActivitySettings.TeleportWhenSitting)
            {
                StoredPlayerPosition = Player.Character.Position;
                StoredPlayerHeading = Player.Character.Heading;
                Game.FadeScreenOut(500, true);

                EntryPoint.WriteToConsole($"Sitting Teleport, Stored Previous Position {StoredPlayerPosition}", 5);
                Player.Character.Position = SeatEntryPosition;
                Player.Character.Heading = SeatEntryHeading;
                Game.FadeScreenIn(500, true);
                return true;
            }
            else
            {
                NativeFunction.Natives.TASK_GO_STRAIGHT_TO_COORD(Game.LocalPlayer.Character, SeatEntryPosition.X, SeatEntryPosition.Y, SeatEntryPosition.Z, 1.0f, -1, SeatEntryHeading, 0.2f);
                uint GameTimeStartedSitting = Game.GameTime;
                float heading = Game.LocalPlayer.Character.Heading;
                bool IsFacingDirection = false;
                bool IsCloseEnough = false;
                while (Game.GameTime - GameTimeStartedSitting <= 5000 && !IsCloseEnough && !IsCancelled)
                {
                    if (PossibleCollisionTable.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                    {
                        NativeFunction.Natives.SET_ENTITY_NO_COLLISION_ENTITY(Player.Character, PossibleCollisionTable, true);
                    }
                    if (Player.IsMoveControlPressed)
                    {
                        IsCancelled = true;
                    }
                    IsCloseEnough = Game.LocalPlayer.Character.DistanceTo2D(SeatEntryPosition) < 0.2f;
                    GameFiber.Yield();
                }
                GameFiber.Sleep(250);
                GameTimeStartedSitting = Game.GameTime;
                while (Game.GameTime - GameTimeStartedSitting <= 5000 && !IsFacingDirection && !IsCancelled)
                {
                    if (PossibleCollisionTable.Exists() && PossibleCollisionTable.Handle != ClosestSittableEntity.Handle)
                    {
                        NativeFunction.Natives.SET_ENTITY_NO_COLLISION_ENTITY(Player.Character, PossibleCollisionTable, true);
                    }
                    heading = Game.LocalPlayer.Character.Heading;
                    if (Math.Abs(Extensions.GetHeadingDifference(heading, SeatEntryHeading)) <= 0.5f)//0.5f)
                    {
                        IsFacingDirection = true;
                        EntryPoint.WriteToConsole($"Sitting FACING TRUE {Game.LocalPlayer.Character.DistanceTo(SeatEntryPosition)} {Extensions.GetHeadingDifference(heading, SeatEntryHeading)} {heading} {SeatEntryHeading}", 5);
                    }
                    GameFiber.Yield();
                }
                GameFiber.Sleep(250);
                if (IsCloseEnough && IsFacingDirection && !IsCancelled)
                {
                    EntryPoint.WriteToConsole($"Sitting IN POSITION {Game.LocalPlayer.Character.DistanceTo(SeatEntryPosition)} {Extensions.GetHeadingDifference(heading, SeatEntryHeading)} {heading} {SeatEntryHeading}", 5);
                    return true;
                }
                else
                {
                    NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
                    EntryPoint.WriteToConsole($"Sitting NOT IN POSITION EXIT {Game.LocalPlayer.Character.DistanceTo(SeatEntryPosition)} {Extensions.GetHeadingDifference(heading, SeatEntryHeading)} {heading} {SeatEntryHeading}", 5);
                    return false;
                }
            }

        }






        private void StartNewIdleScene()
        {
            if (RandomItems.RandomPercent(50))
            {
                PlayingDict = Data.AnimIdleDictionary;
                PlayingAnim = Data.AnimIdle.PickRandom();
            }
            else
            {
                PlayingDict = Data.AnimIdleDictionary2;
                PlayingAnim = Data.AnimIdle2.PickRandom();
            }

            Vector3 Position = Game.LocalPlayer.Character.Position;
            float Heading = Game.LocalPlayer.Character.Heading;
            PlayerScene = NativeFunction.CallByName<int>("CREATE_SYNCHRONIZED_SCENE", Position.X, Position.Y, Game.LocalPlayer.Character.Position.Z, 0.0f, 0.0f, Heading, 2);//270f //old
            NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_LOOPED", PlayerScene, false);
            NativeFunction.CallByName<bool>("TASK_SYNCHRONIZED_SCENE", Game.LocalPlayer.Character, PlayerScene, PlayingDict, PlayingAnim, 1000.0f, -4.0f, 64, 0, 0x447a0000, 0);//std_perp_ds_a
            NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_PHASE", PlayerScene, 0.0f);


            EntryPoint.WriteToConsole($"Sitting Activity Started New Idle {PlayingAnim}", 5);
        }
        private void StartNewBaseScene()
        {
            PlayingDict = Data.AnimBaseDictionary;
            PlayingAnim = Data.AnimBase;
            Vector3 Position = Game.LocalPlayer.Character.Position;
            float Heading = Game.LocalPlayer.Character.Heading;
            PlayerScene = NativeFunction.CallByName<int>("CREATE_SYNCHRONIZED_SCENE", Position.X, Position.Y, Game.LocalPlayer.Character.Position.Z, 0.0f, 0.0f, Heading, 2);//270f //old
            NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_LOOPED", PlayerScene, false);
            NativeFunction.CallByName<bool>("TASK_SYNCHRONIZED_SCENE", Game.LocalPlayer.Character, PlayerScene, PlayingDict, PlayingAnim, 1000.0f, -4.0f, 64, 0, 0x447a0000, 0);//std_perp_ds_a
            NativeFunction.CallByName<bool>("SET_SYNCHRONIZED_SCENE_PHASE", PlayerScene, 0.0f);
            EntryPoint.WriteToConsole($"Sitting Activity Started New Base {PlayingAnim}", 5);
        }
        private void Setup()
        {
            EntryPoint.WriteToConsole($"Sitting Activity SETUP RAN EnterForward {EnterForward}", 5);
            string AnimBase;
            string AnimBaseDictionary;
            string AnimEnter;
            string AnimEnterDictionary;
            string AnimExit;
            string AnimExitDictionary;
            List<string> AnimIdle;
            string AnimIdleDictionary;
            List<string> AnimIdle2;
            string AnimIdleDictionary2;

            if (Player.ModelName.ToLower() == "player_zero" || Player.ModelName.ToLower() == "player_one" || Player.ModelName.ToLower() == "player_two" || Player.IsMale)
            {
                EntryPoint.WriteToConsole("Sitting Activity SETUPO MALE", 5);
                AnimBase = "base";
                AnimBaseDictionary = "amb@prop_human_seat_chair@male@generic@base";
                if (EnterForward)
                {   
                    AnimEnter = "enter_forward";
                    AnimEnterDictionary = "amb@prop_human_seat_chair@male@generic@enter";
                }
                else
                {
                    AnimEnter = "enter_back";
                    AnimEnterDictionary = "amb@prop_human_seat_chair@male@generic@react_aggressive";
                }        
                AnimExit = "exit_forward";
                AnimExitDictionary = "amb@prop_human_seat_chair@male@generic@exit";
                AnimIdle = new List<string>() { "idle_a", "idle_b", "idle_c" };
                AnimIdleDictionary = "amb@prop_human_seat_chair@male@generic@idle_a";
                AnimIdle2 = new List<string>() { "idle_d", "idle_e" };
                AnimIdleDictionary2 = "amb@prop_human_seat_chair@male@generic@idle_b";
            }
            else
            {
                EntryPoint.WriteToConsole("Sitting Activity SETUP FEMALE", 5);
                AnimBase = "base";
                AnimBaseDictionary = "amb@prop_human_seat_chair@female@legs_crossed@base";

                if(EnterForward)
                {
                    AnimEnter = "enter_fwd";
                    AnimEnterDictionary = "amb@prop_human_seat_chair@female@legs_crossed@enter";
                }
                else
                {
                    AnimEnter = "enter_back";
                    AnimEnterDictionary = "amb@prop_human_seat_chair@female@legs_crossed@react_coward";
                } 
                AnimExit = "exit_fwd";
                AnimExitDictionary = "amb@prop_human_seat_chair@female@legs_crossed@exit";
                AnimIdle = new List<string>() { "idle_a", "idle_b", "idle_c" };
                AnimIdleDictionary = "amb@prop_human_seat_chair@female@legs_crossed@idle_a";
                AnimIdle2 = new List<string>() { "idle_d", "idle_e" };
                AnimIdleDictionary2 = "amb@prop_human_seat_chair@female@legs_crossed@idle_b";
            }

            AnimationDictionary.RequestAnimationDictionay(AnimBaseDictionary);
            AnimationDictionary.RequestAnimationDictionay(AnimEnterDictionary);
            AnimationDictionary.RequestAnimationDictionay(AnimIdleDictionary);
            AnimationDictionary.RequestAnimationDictionay(AnimIdleDictionary2);
            AnimationDictionary.RequestAnimationDictionay(AnimExitDictionary);
            EntryPoint.WriteToConsole("Sitting Activity Loaded Dicts", 5);
            Data = new SittingData(AnimBase, AnimBaseDictionary, AnimEnter, AnimEnterDictionary, AnimExit, AnimExitDictionary, AnimIdle, AnimIdleDictionary, AnimIdle2, AnimIdleDictionary2);
            EntryPoint.WriteToConsole("Sitting Activity Data Created", 5);





            //"amb@prop_human_seat_chair@male@generic@react_aggressive" "enter_back"



            SeatModels = new List<SeatModel>() { 
                new SeatModel(0x6ba514ac,-0.45f) {Name = "Iron Bench" }, //sometime float a bit above it
                new SeatModel(0x7facd66f,-0.15f) {Name = "Bus Bench" },
                new SeatModel(0xc0a6cbcd), 
                new SeatModel(0x534bc1bc), 
                new SeatModel(0xa55359b8), 
                new SeatModel(0xe7ed1a59),

                new SeatModel(0xd3c6d323){Name = "Plastic Chair" },

                new SeatModel("PROP_BENCH_05") {Name ="Bus Bench 05"},
                new SeatModel(0x96ff362a){Name = "Wooden Chair With Table" },





                



                new SeatModel(0xd3c6d323){Name = "Plastic Chair" }, 
                new SeatModel(0x3c67ba3f,-0.45f){Name = "Iron Bench" },
                new SeatModel(0xda867f80,-0.45f){Name = "Iron Bench" },
                new SeatModel(0x643d1f90,-0.25f) {Name = "Maze Bus Bench" } };



            TableModels = new List<TableModel>() {
                new TableModel(0xf3a90766),
                                             };
        }
        private class SeatModel
        {
            public SeatModel()
            {

            }
            public SeatModel(uint hash)
            {
                ModelHash = hash;
            }
            public SeatModel(uint hash, float entryOffsetFront)
            {
                ModelHash = hash;
                EntryOffsetFront = entryOffsetFront;
            }
            public SeatModel(string modelName)
            {
                ModelName = modelName;
            }
            public SeatModel(string modelName, float entryOffsetFront)
            {
                ModelName = modelName;
                EntryOffsetFront = entryOffsetFront;
            }
            public string Name { get; set; } = "Unknown";
            public string ModelName { get; set; } = "";
            public uint ModelHash { get; set; } = 0;
            public float EntryOffsetFront { get; set; } = -0.5f;
        }
        private class TableModel
        {
            public TableModel()
            {

            }
            public TableModel(uint hash)
            {
                Hash = hash;
            }
            public TableModel(uint hash, float entryOffsetFront)
            {
                Hash = hash;
                EntryOffsetFront = entryOffsetFront;
            }
            public TableModel(string modelName)
            {
                ModelName = modelName;
            }
            public TableModel(string modelName, float entryOffsetFront)
            {
                ModelName = modelName;
                EntryOffsetFront = entryOffsetFront;
            }
            public string Name { get; set; } = "Unknown";
            public string ModelName { get; set; }
            public uint Hash { get; set; }
            public float EntryOffsetFront { get; set; } = -0.5f;
        }
    }
}