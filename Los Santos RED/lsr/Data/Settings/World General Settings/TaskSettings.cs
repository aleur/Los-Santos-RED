using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

public class TaskSettings : ISettingsDefaultable
{
    // Officer Friendly
    [Description("Minimum payment amount for the Gang Hit task from Officer Friendly")]
    public int OfficerFriendlyGangHitPaymentMin { get; set; }
    [Description("Maximum payment amount for the Gang Hit task from Officer Friendly")]
    public int OfficerFriendlyGangHitPaymentMax { get; set; }
    [Description("Complications Percent for the Gang Hit task from Officer Friendly")]
    public float OfficerFriendlyGangHitComplicationsPercentage { get; set; }
    [Description("Minimum payment amount for the Cop Hit task from Officer Friendly")]
    public int OfficerFriendlyCopHitPaymentMin { get; set; }
    [Description("Maximum payment amount for the Cop Hit task from Officer Friendly")]
    public int OfficerFriendlyCopHitPaymentMax { get; set; }
    [Description("Complications Percent for the Cop Hit task from Officer Friendly")]
    public float OfficerFriendlyCopHitComplicationsPercentage { get; set; }
    [Description("Minimum payment amount for the Witness Elimination task from Officer Friendly")]
    public int OfficerFriendlyWitnessEliminationPaymentMin { get; set; }
    [Description("Maximum payment amount for the Witness Elimination task from Officer Friendly")]
    public int OfficerFriendlyWitnessEliminationPaymentMax { get; set; }
    [Description("Complications Percent for the Witness Elimination task from Officer Friendly")]
    public float OfficerFriendlyWitnessEliminationComplicationsPercentage { get; set; }


    // Underground Guns
    [Description("Minimum payment amount for the Gun Dropoff task from Underground Guns")]
    public int UndergroundGunsGunDropoffPaymentMin { get; set; }
    [Description("Maximum payment amount for the Gun Dropoff task from Underground Guns")]
    public int UndergroundGunsGunDropoffPaymentMax { get; set; }
    [Description("Complications Percent for the Gun Dropoff task from Underground Guns")]
    public float UndergroundGunsGunDropoffComplicationsPercentage { get; set; }

    [Description("Minimum payment amount for the Gun Transport task from Underground Guns")]
    public int UndergroundGunsGunTransportPaymentMin { get; set; }
    [Description("Maximum payment amount for the Gun Transport task from Underground Guns")]
    public int UndergroundGunsGunTransportPaymentMax { get; set; }
    [Description("Complications Percent for the Gun Transport task from Underground Guns")]
    public float UndergroundGunsGunTransportComplicationsPercentage { get; set; }

    // QoL
    [Description("Show blips on entities that are related to the task.")]
    public bool ShowEntityBlips { get; set; }
    [Description("Show help text pop ups on task status changes.")]
    public bool DisplayHelpPrompts { get; set; }

    // Vehicle Exporter
    public int VehicleExporterTransferPaymentMin { get; set; }
    public int VehicleExporterTransferPaymentMax { get; set; }
    public float VehicleExporterTransferComplicationsPercentage { get; set; }

    // Gangs
    [Description("Days to complete gang task")]
    public int GangDaysToCompleteTask { get; set; }
    [Description("Reputation lost upon failing gang task")]
    public int GangRepOnFailingTask { get; set; }
    [Description("Reputation gained upon completion of gang task")]
    public int GangRepOnCompletingTask { get; set; }
    [Description("Percent that you will extort stores in Enemy Territory")]
    public float GangRacketeeringExtortionPercentage { get; set; }
    [Description("Complications Percent of stores calling for enemy backup during extortion.")]
    public float GangRacketeeringExtortionComplicationsPercentage { get; set; }
    [Description("Complications Percent of stores calling for police backup during racketeering.")]
    public float GangRacketeeringComplicationsPercentage { get; set; }
    [Description("Percent of torching stores in Enemy Territory")]
    public float GangArsonEnemyTurfPercentage { get; set; }
    [Description("Multiplies reputation gained by the number of enemies killed during lethal tasks")] // ass explanation but ok
    public bool GangMultiplyRepByKillCount { get; set; } // what is the point of this again? why i ad d this
    public float DrugMeetAmbushPercentageNeutral { get; set; }
    public float DrugMeetAmbushPercentageFriendly { get; set; }
    public float DrugMeetPriceScalarMin { get; set; }
    public float DrugMeetPriceScalarMax { get; set; }
    public int DrugMeetMin { get; set; }
    public int DrugMeetMax { get; set; }

    public TaskSettings()
    {
        SetDefault();
    }
    public void SetDefault()
    {

        OfficerFriendlyWitnessEliminationPaymentMin = 1500;// 10000;
        OfficerFriendlyWitnessEliminationPaymentMax = 2500;// 20000;
        OfficerFriendlyWitnessEliminationComplicationsPercentage = 45f;

        OfficerFriendlyGangHitPaymentMin = 2000;// 10000;
        OfficerFriendlyGangHitPaymentMax = 3000;// 15000;
        OfficerFriendlyGangHitComplicationsPercentage = 10f;

        OfficerFriendlyCopHitPaymentMin = 3500;// 15000;
        OfficerFriendlyCopHitPaymentMax = 4500;// 20000;
        OfficerFriendlyCopHitComplicationsPercentage = 25f;



        UndergroundGunsGunTransportPaymentMin = 2000;// 5000;
        UndergroundGunsGunTransportPaymentMax = 4000;// 10000;
        UndergroundGunsGunTransportComplicationsPercentage = 15f;
        UndergroundGunsGunDropoffPaymentMin = 2000;// 5000;
        UndergroundGunsGunDropoffPaymentMax = 4000;// 10000;
        UndergroundGunsGunDropoffComplicationsPercentage = 15f;

        ShowEntityBlips = true;
        DisplayHelpPrompts = true;

        VehicleExporterTransferPaymentMin = 1500;// 2000;
        VehicleExporterTransferPaymentMax = 2500;// 5000;
        VehicleExporterTransferComplicationsPercentage = 25f; 
        GangDaysToCompleteTask = 7;
        GangRepOnFailingTask = -1000;
        GangRepOnCompletingTask = 500;
        GangMultiplyRepByKillCount = true;
        GangRacketeeringExtortionPercentage = 25f;
        GangRacketeeringExtortionComplicationsPercentage = 10f;
        GangRacketeeringComplicationsPercentage = 5f;
        GangArsonEnemyTurfPercentage = 5f;


        DrugMeetAmbushPercentageNeutral = 15f;
        DrugMeetAmbushPercentageFriendly = 1f;


        DrugMeetPriceScalarMin = 0.8f;
        DrugMeetPriceScalarMax = 1.2f;

        DrugMeetMin = 200;
        DrugMeetMax = 2000;
    }
    [OnDeserialized()]
    private void SetValuesOnDeserialized(StreamingContext context)
    {
        SetDefault();
    }

}