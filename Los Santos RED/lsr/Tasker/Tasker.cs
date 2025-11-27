using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod
{
    public class Tasker : ITaskerable
    {
        private IEntityProvideable PedProvider;
        private ITargetable Player;
        private IWeapons Weapons;
        private ISettingsProvideable Settings;
        //private List<PedExt> PossibleTargets;
        //private Cop ClosestCopToPlayer;
        private IPlacesOfInterest PlacesOfInterest;
        //private List<AssignedSeat> SeatAssignments = new List<AssignedSeat>();


        private CopTasker CopTasker;
        private GangTasker GangTasker;
        private CivilianTasker CivilianTasker;
        private EMTTasker EMTTasker;
        private FirefighterTasker FirefighterTasker;



        private double AverageTimeBetweenCopUpdates = 0;
        private double AverageTimeBetweenCivUpdates = 0;
        private uint MaxTimeBetweenCopUpdates = 0;
        private uint MaxTimeBetweenCivUpdates = 0;
        private uint GameTimeLastTaskedPolice;
        private uint GameTimeLastTaskedCivilians;

        public RelationshipGroup ZombiesRG { get; set; }
        public string TaskerDebug => $"Cop Max: {MaxTimeBetweenCopUpdates} Avg: {AverageTimeBetweenCopUpdates} Civ Max: {MaxTimeBetweenCivUpdates} Avg: {AverageTimeBetweenCivUpdates}";
        public Tasker(IEntityProvideable pedProvider, ITargetable player, IWeapons weapons, ISettingsProvideable settings, IPlacesOfInterest placesOfInterest)
        {
            PedProvider = pedProvider;
            Player = player;
            Weapons = weapons;
            Settings = settings;
            PlacesOfInterest = placesOfInterest;
            CopTasker = new CopTasker(this, PedProvider, player, weapons, settings, PlacesOfInterest);
            GangTasker = new GangTasker(this, PedProvider, player, weapons, settings, PlacesOfInterest);
            CivilianTasker = new CivilianTasker(this, PedProvider, player, weapons, settings, PlacesOfInterest);

            EMTTasker = new EMTTasker(this, PedProvider, player, weapons, settings, PlacesOfInterest);
            EMTTasker.Setup();


            FirefighterTasker = new FirefighterTasker(this, PedProvider, player, weapons, settings, PlacesOfInterest);
            FirefighterTasker.Setup();
        }
        public void Setup()
        {
            ZombiesRG = new RelationshipGroup("ZOMBIES");


            RelationshipGroup CIVMALERG = new RelationshipGroup("CIVMALE");
            RelationshipGroup CIVFEMALERG = new RelationshipGroup("CIVFEMALE");

            RelationshipGroup.Cop.SetRelationshipWith(ZombiesRG, Relationship.Hate);
            ZombiesRG.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            RelationshipGroup.Player.SetRelationshipWith(ZombiesRG, Relationship.Hate);
            ZombiesRG.SetRelationshipWith(RelationshipGroup.Player, Relationship.Hate);

            Game.SetRelationshipBetweenRelationshipGroups(CIVMALERG, ZombiesRG, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(ZombiesRG, CIVMALERG, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(CIVFEMALERG, ZombiesRG, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(ZombiesRG, CIVFEMALERG, Relationship.Hate);

            NativeFunction.Natives.REQUEST_ANIM_SET<bool>("move_m@drunk@verydrunk");
        }
        public void UpdateCivilians()
        {
            CivilianTasker.Update();
            GameFiber.Yield();//TR 29
            GangTasker.Update();
            if (Settings.SettingsManager.PerformanceSettings.EnableHighPerformanceMode)
            {
                GameFiber.Yield();//TR 29
            }
            EMTTasker.Update();
            if (Settings.SettingsManager.PerformanceSettings.EnableHighPerformanceMode)
            {
                GameFiber.Yield();//TR 29
            }
            FirefighterTasker.Update();
            GameFiber.Yield();
            if (Settings.SettingsManager.PerformanceSettings.PrintUpdateTimes)
            {
                EntryPoint.WriteToConsole($"Tasker.UpdateCivilians Ran Time Since {Game.GameTime - GameTimeLastTaskedCivilians}", 5);
            }
            GameTimeLastTaskedCivilians = Game.GameTime;
        }
        public void UpdatePolice()
        {
            CopTasker.Update();
            if (Settings.SettingsManager.PerformanceSettings.PrintUpdateTimes)
            {
                EntryPoint.WriteToConsole($"Tasker.UpdatePolice Ran Time Since {Game.GameTime - GameTimeLastTaskedPolice}", 5);
            }
            GameTimeLastTaskedPolice = Game.GameTime;
        }
    }
}
