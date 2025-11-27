using ExtensionsMethods;
using LosSantosRED.lsr.Interface;
using Mod;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

public class MerchantConditionalLocation : ConditionalLocation
{
    //protected GameLocation GameLocation;
    [XmlIgnore]
    public MerchantSpawnTask MerchantSpawnTask { get; set; }
    public MerchantConditionalLocation(Vector3 location, float heading, float percentage) : base(location, heading, percentage)
    {

    }
    public MerchantConditionalLocation()
    {

    }
    public override void RunSpawnTask()
    {
        try
        {
            MerchantSpawnTask = new MerchantSpawnTask(SpawnLocation, null, DispatchablePerson, false, false, true, Settings, Crimes, Weapons, Names, World, ModItems, ShopMenus, GameLocation);//, Names, true, Crimes, PedGroups, ShopMenus, World, ModItems, ForceMelee, ForceSidearm, ForceLongGun);// Settings.SettingsManager.Police.SpawnedAmbientPoliceHaveBlip);
            MerchantSpawnTask.PossibleHeads = GameLocation.VendorPossibleHeads;
            MerchantSpawnTask.AllowAnySpawn = true;
            MerchantSpawnTask.AllowBuddySpawn = false;
            MerchantSpawnTask.SetupMenus = false;
            MerchantSpawnTask.SpawnRequirement = TaskRequirements;
            //MerchantSpawnTask.ClearVehicleArea = true;
            //MerchantSpawnTask.PlacePedOnGround = true;
            MerchantSpawnTask.IsAmbushTarget = IsAmbushTarget;
            MerchantSpawnTask.AreVehiclesTargeted = AreVehiclesTargeted;
            MerchantSpawnTask.AttemptSpawn();
            MerchantSpawnTask.PostRun(this, GameLocation);
            //merchantSpawnTask.CreatedPeople.ForEach(x => { World.Pedestrians.AddEntity(x); x.IsLocationSpawned = true; AddLocationRequirements(x); });
        }
        catch (Exception ex)
        {
            EntryPoint.WriteToConsole($"Merchant Dispatcher Spawn Error: {ex.Message} : {ex.StackTrace}", 0);
        }
    }
    public override bool DetermineRun(bool force)
    {
        if (!Settings.SettingsManager.CivilianSettings.ManageDispatching)
        {
            return false;
        }
        if (World.Pedestrians.TotalSpawnedServiceWorkers >= Settings.SettingsManager.CivilianSettings.TotalSpawnedServiceMembersLimit)
        {
            return false;
        }
        if(GameLocation == null)
        {
            return false;
        }
        return base.DetermineRun(force);
    }
    public override void GenerateSpawnTypes()
    {
        if (GameLocation == null)
        {
            return;
        }
        if (IsPerson || !IsEmpty)
        {
            DispatchablePerson = GameLocation.VendorPersonnel.PickRandom();
        }
        if(!IsPerson)
        {
            //do a car spawn here if you wanna store car spawn stuff
        }
    }
}
