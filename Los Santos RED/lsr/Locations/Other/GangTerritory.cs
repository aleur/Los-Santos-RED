using LosSantosRED.lsr;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player.ActiveTasks;
using LosSantosRED.lsr.Helper;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable()]
public class GangTerritory
{
    [XmlIgnore]
    public TurfStatus TurfStatus { get; set; }
    [XmlIgnore]
    public GangTask GangTask { get; set; }
    [XmlIgnore]
    public uint LastUpdateTime { get; set; }
    [XmlIgnore]
    public long TimeUntilNextTask { get; set; }
    [XmlIgnore]
    public uint? MinTaskTime { get; set; }
    [XmlIgnore]     
    public uint? MaxTaskTime { get; set; }
    public string ZoneInternalGameName { get; set; } = "";
    public string GangID { get; set; } = "";
    public int Priority { get; set; } = 99;
    public int AmbientSpawnChance { get; set; } = 0;
    public float TaskFrequency { get; set; } = -1f;
    public float DrugDealerPercentage { get; set; } = -1f;
    // Arsenal
    public float PercentageWithLongGuns { get; set; }  = -1f;
    public float PercentageWithSidearms { get; set; }  = -1f;
    public float PercentageWithMelee { get; set; }  = -1f;
    // Violence
    public float FightPercentage { get; set; } = -1f;
    public float FightPolicePercentage { get; set; } = -1f;
    public float AlwaysFightPolicePercentage { get; set; } = -1f;
    public GangTerritory()
    {

    }
    public GangTerritory(string gangID, string zoneInternalName, int priority, int ambientSpawnChance)
    {
        GangID = gangID;
        ZoneInternalGameName = zoneInternalName;
        Priority = priority;
        AmbientSpawnChance = ambientSpawnChance;
    }
    public GangTerritory(string gangID, string zoneInternalName, int priority, int ambientSpawnChance, float drugDealerPercentage, float percentageWithLongGuns, 
        float percentageWithSidearms, float percentageWithMelee, float fightPercentage, float fightPolicePercentage, float alwaysFightPolicePercentage)
    {
        GangID = gangID;
        ZoneInternalGameName = zoneInternalName;
        Priority = priority;
        AmbientSpawnChance = ambientSpawnChance;
        DrugDealerPercentage = drugDealerPercentage;
        PercentageWithLongGuns = percentageWithLongGuns;
        PercentageWithSidearms = percentageWithSidearms;
        PercentageWithMelee = percentageWithMelee;
        FightPercentage = fightPercentage;
        FightPolicePercentage = fightPolicePercentage;
        AlwaysFightPolicePercentage = alwaysFightPolicePercentage;
    }
    public int CurrentSpawnChance()
    {
        return AmbientSpawnChance;
    }
    public void Setup(Gang gang)
    {
        if (DrugDealerPercentage < 0) DrugDealerPercentage = gang.DrugDealerPercentage;
        if (PercentageWithLongGuns < 0) PercentageWithLongGuns = gang.PercentageWithLongGuns;
        if (PercentageWithSidearms < 0) PercentageWithSidearms = gang.PercentageWithSidearms;
        if (PercentageWithMelee < 0) PercentageWithMelee = gang.PercentageWithMelee;
        if (FightPercentage < 0) FightPercentage = gang.FightPercentage;
        if (FightPolicePercentage < 0) FightPolicePercentage = gang.FightPolicePercentage;
        if (AlwaysFightPolicePercentage < 0) AlwaysFightPolicePercentage = gang.AlwaysFightPolicePercentage;

        EntryPoint.WriteToConsole($"{ZoneInternalGameName},{GangID}: {DrugDealerPercentage}");
    }
    public void DefaultTaskSettings(ISettingsProvideable settings)
    {
        if (TaskFrequency < 0)
        {
            TaskFrequency = settings.SettingsManager.GangSettings.GangTaskFrequency;
        }

        if (!MaxTaskTime.HasValue)
        {
            MaxTaskTime = settings.SettingsManager.GangSettings.MaximumTimeBetweenGangTasksTerritories;
        }
        if (!MinTaskTime.HasValue)
        {
            MinTaskTime = settings.SettingsManager.GangSettings.MinimumTimeBetweenGangTasksTerritories;
        }
    }
    public void UpdateTask(IContactInteractable player, Gang gang)
    {
        long deltaTime = Game.GameTime - LastUpdateTime;
        LastUpdateTime = Game.GameTime;

        TimeUntilNextTask -= deltaTime;
        
        if (player.CurrentLocation.CurrentZone.InternalGameName == ZoneInternalGameName)
        {
            EntryPoint.WriteToConsole($"{gang.ID}: {TimeUntilNextTask} left till {GangTask} in {player.CurrentLocation.CurrentZone.DisplayName}");
        }

        if (TimeUntilNextTask <= 0f)
        {
            if (player.RelationshipManager.GangRelationships.GetReputation(gang).IsMember && !player.PlayerTasks.HasTask(gang.Contact?.Name) && !player.PlayerTasks.RecentlyEndedTask(gang.Contact?.Name))
            {
                GangTask.Start();
            }
            GangTask = null;
        }
    }
}
