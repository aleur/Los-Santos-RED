using ExtensionsMethods;
using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class FollowInVehicleTaskState : TaskState
{
    private PedExt PedGeneral;
    private IEntityProvideable World;
    private SeatAssigner SeatAssigner;
    private bool ForceGuard;
    private uint GameTimeBetweenScenarios;
    private uint GameTimeLastStartedScenario;
    private uint GameTimeLastStartedFootPatrol;
    private uint GameTimeBetweenFootPatrols;

    private bool IsFastDriving = false;
    private bool PrevIsFastDriving = false;
    private float MoveSpeed = 1.0f;
    private float PrevMoveSpeed = 1.0f;

    private bool isGuarding = false;
    private bool isPatrolling = false;
    private ISettingsProvideable Settings;
    private ITargetable Player;
    private int GroupMemberNumber;
    public FollowInVehicleTaskState(PedExt pedGeneral, IEntityProvideable world, SeatAssigner seatAssigner, ISettingsProvideable settings, ITargetable player)
    {
        PedGeneral = pedGeneral;
        World = world;
        SeatAssigner = seatAssigner;
        Settings = settings;
        Player = player;
    }

    public bool IsValid => PedGeneral != null && PedGeneral.Pedestrian.Exists() && PedGeneral.IsInVehicle && Player.IsInVehicle;
    public string DebugName { get; } = "FollowInVehicleTaskState";
    public void Dispose()
    {

    }
    public void Start()
    {
        SetEscortTask();
    }
    public void Stop()
    {

    }
    public void Update()
    {
        //bool isPlayerDrivingFast = false;
        //if(Player.IsInVehicle && Player.CurrentVehicle != null && Player.CurrentVehicle.Vehicle.Exists() && Player.CurrentVehicle.Vehicle.Speed >= Settings.SettingsManager.GangSettings.EscortSpeedNormal)
        //{
        //    isPlayerDrivingFast = true;
        //}
        //IsFastDriving = Player.IsWanted || isPlayerDrivingFast || PedGeneral.IsWanted || PedGeneral.Pedestrian.IsInCombat;
        //if(PrevIsFastDriving != IsFastDriving)
        //{
        //    SetEscortTask();
        //    PrevIsFastDriving = IsFastDriving;
        //}
        UpdateChaseTask();
    }
    private void UpdateChaseTask()
    {
        if(PedGeneral == null || !PedGeneral.Pedestrian.Exists() || PedGeneral.Pedestrian.CurrentVehicle == null)
        {
            return;
        }


        float PedVehicleSpeed = PedGeneral.Pedestrian.CurrentVehicle.Speed;
        float TargetSpeed = MathHelper.Lerp(PedVehicleSpeed, Player.VehicleSpeed - 5f, 0.1f);
        float MaxDistance = BrakingDistance(Player.VehicleSpeed, PedVehicleSpeed);

        bool PlayerFelonySpeeding = World.Streets.GetStreet(PedGeneral.Position)?.SpeedLimitMPH != null &&
                                    Player.VehicleSpeedMPH > World.Streets.GetStreet(PedGeneral.Position)?.SpeedLimitMPH + Settings.SettingsManager.ViolationSettings.OverLimitFelonySpeedingAmount;

        //brakingDistance = Extensions.Clamp(brakingDistance, 5f, 60f);5

        NativeFunction.Natives.SET_DRIVER_ABILITY(PedGeneral.Pedestrian, Settings.SettingsManager.GroupSettings.MemberVehAbility);
        NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(PedGeneral.Pedestrian, Settings.SettingsManager.GroupSettings.MemberVehAggressiveness);

        //NativeFunction.Natives.SET_DRIVE_TASK_MAX_CRUISE_SPEED(PedGeneral.Pedestrian, Player.VehicleSpeed);
        // NativeFunction.Natives.SET_DRIVE_TASK_CRUISE_SPEED(PedGeneral.Pedestrian, 70f);
        // NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(PedGeneral.Pedestrian, (int)eCustomDrivingStyles.Code3);

        // Speeding with Player
        if (Player.VehicleSpeedMPH >= 60f || PlayerFelonySpeeding)
        {
            EntryPoint.WriteToConsole($"Speeding! - VehicleSpeed: {PedVehicleSpeed}, TargetSpeed: {TargetSpeed}, PlayerSpeed: {Player.VehicleSpeed}, BrakingDistance: {MaxDistance}, PedDistance: {PedGeneral.DistanceToPlayer}");
            NativeFunction.Natives.SET_DRIVE_TASK_CRUISE_SPEED(PedGeneral.Pedestrian, 55f);

            if (PedGeneral.DistanceToPlayer <= MaxDistance)
            {
                //NativeFunction.Natives.TASK_VEHICLE_FOLLOW(PedGeneral.Pedestrian, PedGeneral.Pedestrian.CurrentVehicle, Player.Character, Player.VehicleSpeed, (int)eCustomDrivingStyles.RacingNew, MaxDistance);
                NativeFunction.Natives.TASK_VEHICLE_ESCORT(PedGeneral.Pedestrian, PedGeneral.Pedestrian.CurrentVehicle, Player.CurrentVehicle.Vehicle, -1, TargetSpeed, (int)eCustomDrivingStyles.RacingNew, MaxDistance, 20, 20.0f);
                return;
            }

            NativeFunction.Natives.TASK_VEHICLE_CHASE(PedGeneral.Pedestrian, Player.Pedestrian);
            NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(PedGeneral.Pedestrian, MaxDistance);
        }
        else if (PedGeneral.DistanceToPlayer <= 35f)
        {
            float speedLimit = World.Streets.GetStreet(PedGeneral.Position)?.SpeedLimitMS ?? 25f;
            float safeSpeed = speedLimit > 0f ? speedLimit : 25f;

            NativeFunction.Natives.TASK_VEHICLE_FOLLOW(PedGeneral.Pedestrian, PedGeneral.Pedestrian.CurrentVehicle, Player.Character, safeSpeed, (int)eCustomDrivingStyles.PoliceFollow, 8);
            NativeFunction.Natives.SET_DRIVE_TASK_CRUISE_SPEED(PedGeneral.Pedestrian, safeSpeed);
            EntryPoint.WriteToConsole($"Following! - VehicleSpeed: {PedVehicleSpeed}, TargetSpeed: {safeSpeed}, PlayerSpeed: {Player.VehicleSpeed}, PedDistance: {PedGeneral.DistanceToPlayer}");
        }
        else
        {
            EntryPoint.WriteToConsole($"Falling Behind! - VehicleSpeed: {PedVehicleSpeed}, TargetSpeed: {TargetSpeed}, PlayerSpeed: {Player.VehicleSpeed}, BrakingDistance: {MaxDistance}, PedDistance: {PedGeneral.DistanceToPlayer}");
            NativeFunction.Natives.TASK_VEHICLE_CHASE(PedGeneral.Pedestrian, Player.Pedestrian);
            NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(PedGeneral.Pedestrian, MaxDistance);
        }

        NativeFunction.Natives.SET_PED_COMBAT_ATTRIBUTES(PedGeneral.Pedestrian, (int)eCombatAttributes.BF_DisableCruiseInFrontDuringBlockDuringVehicleChase, true);
        NativeFunction.Natives.SET_PED_COMBAT_ATTRIBUTES(PedGeneral.Pedestrian, (int)eCombatAttributes.BF_DisableSpinOutDuringVehicleChase, true);
        NativeFunction.Natives.SET_PED_COMBAT_ATTRIBUTES(PedGeneral.Pedestrian, (int)eCombatAttributes.BF_DisableBlockFromPursueDuringVehicleChase, true);

        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.CantPullAlongsideInFront, true);
        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.UseContinuousRam, false);
        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.CantPullAlongside, true);

        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.FullContact, false);
        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.MediumContact, false);
        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.LowContact, false);
        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.PIT, false);
        NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.NoContact, true);
    }
    private void SetEscortTask()
    {
        if (PedGeneral != null && PedGeneral.IsInVehicle && PedGeneral.Pedestrian.Exists() && PedGeneral.Pedestrian.CurrentVehicle.Exists())
        {
            NativeFunction.Natives.SET_DRIVER_ABILITY(PedGeneral.Pedestrian, Settings.SettingsManager.GroupSettings.MemberVehAbility);
            NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(PedGeneral.Pedestrian, Settings.SettingsManager.GroupSettings.MemberVehAggressiveness);


            //  NativeFunction.Natives.TASK_VEHICLE_ESCORT(PedGeneral.Pedestrian, PedGeneral.Pedestrian.CurrentVehicle, Player.Character, -1, 100f, (int)eCustomDrivingStyles.Code3, -1.0f, 20, 20.0f);


            NativeFunction.Natives.TASK_VEHICLE_FOLLOW(PedGeneral.Pedestrian, PedGeneral.Pedestrian.CurrentVehicle, Player.Character, 25f, (int)eCustomDrivingStyles.PoliceFollow, 8);
            // NativeFunction.Natives.TASK_VEHICLE_FOLLOW(PedGeneral.Pedestrian, PedGeneral.Pedestrian.CurrentVehicle, Player.Character, 100f, (int)eCustomDrivingStyles.Code3, 20);

            // NativeFunction.Natives.TASK_VEHICLE_CHASE(PedGeneral.Pedestrian, Player.Pedestrian);



            //NativeFunction.Natives.TASK_VEHICLE_CHASE(PedGeneral.Pedestrian, Player.Character);

            // NativeFunction.Natives.SET_DRIVE_TASK_MAX_CRUISE_SPEED(PedGeneral.Pedestrian, 100.0f);//reset?

            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE(PedGeneral.Pedestrian, 8f);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.FullContact, false);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.MediumContact, false);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.LowContact, false);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.PIT, false);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.NoContact, true);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.CantPullAlongsideInFront, true);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.UseContinuousRam, false);
            // NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(PedGeneral.Pedestrian, (int)eChaseBehaviorFlag.CantPullAlongside, true);
            // NativeFunction.Natives.SET_PED_COMBAT_ATTRIBUTES(PedGeneral.Pedestrian, (int)eCombatAttributes.BF_DisableCruiseInFrontDuringBlockDuringVehicleChase, true);
            // NativeFunction.Natives.SET_PED_COMBAT_ATTRIBUTES(PedGeneral.Pedestrian, (int)eCombatAttributes.BF_DisableSpinOutDuringVehicleChase, true);
            // NativeFunction.Natives.SET_PED_COMBAT_ATTRIBUTES(PedGeneral.Pedestrian, (int)eCombatAttributes.BF_DisableBlockFromPursueDuringVehicleChase, true);
            // NativeFunction.Natives.SET_DRIVE_TASK_MAX_CRUISE_SPEED(PedGeneral.Pedestrian, 100f);
            // NativeFunction.Natives.SET_DRIVE_TASK_CRUISE_SPEED(PedGeneral.Pedestrian, 100f);
            // NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(PedGeneral.Pedestrian, (int)eCustomDrivingStyles.Code3);
            //unsafe
            //{
            //    int lol = 0;
            //    NativeFunction.CallByName<bool>("OPEN_SEQUENCE_TASK", &lol);
            //    NativeFunction.CallByName<bool>("TASK_VEHICLE_ESCORT", 0, PedGeneral.Pedestrian.CurrentVehicle, Player.Character, -1, 100f, (int)eCustomDrivingStyles.Code3, -1.0f * Math.Abs(Settings.SettingsManager.GangSettings.EscortOffsetValue) * GroupMemberNumber, 20, 20.0f);
            //    NativeFunction.CallByName<bool>("SET_SEQUENCE_TO_REPEAT", lol, true);
            //    NativeFunction.CallByName<bool>("CLOSE_SEQUENCE_TASK", lol);
            //    NativeFunction.CallByName<bool>("TASK_PERFORM_SEQUENCE", PedGeneral.Pedestrian, lol);
            //    NativeFunction.CallByName<bool>("CLEAR_SEQUENCE_TASK", &lol);
            //}
        }
    }

    private float BrakingDistance(float TargetSpeed, float PedSpeed)
    {
        if (PedSpeed <= TargetSpeed) return Settings.SettingsManager.GroupSettings.MinBrakingDistance;

        float bd = ((PedSpeed * PedSpeed) - (TargetSpeed * TargetSpeed)) / (2f * Settings.SettingsManager.GroupSettings.DecelerationValue);

        return bd > Settings.SettingsManager.GroupSettings.MinBrakingDistance ? bd : Settings.SettingsManager.GroupSettings.MinBrakingDistance;
    }
}