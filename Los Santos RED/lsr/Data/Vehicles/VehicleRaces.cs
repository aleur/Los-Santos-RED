﻿using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class VehicleRaces : IVehicleRaces
{
    private readonly string ConfigFileName = "Plugins\\LosSantosRED\\VehicleRaces.xml";
    public VehicleRaceTypeManager VehicleRaceTypeManager { get; private set; }
    public void ReadConfig(string configName)
    {
        string fileName = string.IsNullOrEmpty(configName) ? "VehicleRaces*.xml" : $"VehicleRaces_{configName}.xml";

        DirectoryInfo LSRDirectory = new DirectoryInfo("Plugins\\LosSantosRED"); 
        FileInfo ConfigFile = LSRDirectory.GetFiles(fileName).OrderByDescending(x => x.Name).FirstOrDefault();
        if (ConfigFile != null && !configName.Equals("Default"))
        {
            EntryPoint.WriteToConsole($"Loaded VehicleRaces config: {ConfigFile.FullName}", 0);
            VehicleRaceTypeManager = Serialization.DeserializeParam<VehicleRaceTypeManager>(ConfigFile.FullName);
        }
        else if (File.Exists(ConfigFileName))
        {
            EntryPoint.WriteToConsole($"Loaded VehicleRaces config  {ConfigFileName}", 0);
            VehicleRaceTypeManager = Serialization.DeserializeParam<VehicleRaceTypeManager>(ConfigFileName);
        }
        else
        {
            EntryPoint.WriteToConsole($"No VehicleRaces config found, creating default", 0);
            DefaultConfig();
            DefaultConfig_Liberty();
        }
    }

    private void DefaultConfig_Liberty()
    {
        VehicleRaces_Liberty vehicleRaces_Liberty = new VehicleRaces_Liberty(this);
        vehicleRaces_Liberty.DefaultConfig();
    }

    private void DefaultConfig()
    {
        VehicleRaceTypeManager = new VehicleRaceTypeManager();
        VehicleRaceTypeManager.VehicleRaceTracks = new List<VehicleRaceTrack>();

        SandyTracks();
        VineWoodTracks();
        PaletoTracks();

        Serialization.SerializeParam(VehicleRaceTypeManager, ConfigFileName);
    }
    private void SandyTracks()
    {
        List<VehicleRaceStartingPosition> vehicleRaceStartingPositions = new List<VehicleRaceStartingPosition>()
        {
            new VehicleRaceStartingPosition(0, new Vector3(1868.027f, 3226.604f, 44.5677f), 39.27186f),
            new VehicleRaceStartingPosition(1, new Vector3(1876.75f, 3215.834f, 44.83164f), 39.09522f),
            new VehicleRaceStartingPosition(2, new Vector3(1866.469f, 3220.694f, 44.6372f), 43.98711f),
            new VehicleRaceStartingPosition(3, new Vector3(1874.058f, 3211.186f, 44.86393f), 38.87733f),
        };
        List<VehicleRaceCheckpoint> vehicleRaceCheckpoints = new List<VehicleRaceCheckpoint>()
        {
            //new VehicleRaceCheckpoint(0, new Vector3(1775.803f, 3375.738f, 39.1007f)),
            //new VehicleRaceCheckpoint(1, new Vector3(1583.795f, 3483.779f, 36.04169f)),
            //new VehicleRaceCheckpoint(2, new Vector3(951.4559f, 3536.375f, 33.44981f)),
            //new VehicleRaceCheckpoint(3,new Vector3(927.9109f, 3630.1f, 31.85251f)),
            //new VehicleRaceCheckpoint(4,new Vector3(1309.275f, 3653.17f, 32.52422f)),
            //new VehicleRaceCheckpoint(5,new Vector3(1663.282f, 3858.711f, 34.23738f)),
            //new VehicleRaceCheckpoint(6,new Vector3(1743.253f, 3758.429f, 33.24446f)),
            //new VehicleRaceCheckpoint(7,new Vector3(1968.987f, 3878.23f, 31.663f)),





            new VehicleRaceCheckpoint(0, new Vector3(1775.803f, 3375.738f, 39.1007f)),
            new VehicleRaceCheckpoint(1,new Vector3(1582.85f, 3480.25f, 36.21559f)),
            new VehicleRaceCheckpoint(2,new Vector3(984.0595f, 3535.763f, 33.54565f)),
            new VehicleRaceCheckpoint(3,new Vector3(956.1648f, 3634.594f, 32.08199f)),
            new VehicleRaceCheckpoint(4,new Vector3(1308.131f, 3652.568f, 32.76283f)),
            new VehicleRaceCheckpoint(5,new Vector3(1628.509f, 3824.556f, 34.63056f)),
            new VehicleRaceCheckpoint(6,new Vector3(1740.17f, 3764.768f, 33.48697f)),
            new VehicleRaceCheckpoint(7,new Vector3(1975.885f, 3888.242f, 31.93978f)),


            //new VehicleRaceCheckpoint(0, new Vector3(1775.803f, 3375.738f, 39.1007f)),
            //new VehicleRaceCheckpoint(1,new Vector3(1702.318f, 3499.852f, 35.91494f)),
            //new VehicleRaceCheckpoint(2,new Vector3(935.6216f, 3535.946f, 33.43148f)),
            //new VehicleRaceCheckpoint(3,new Vector3(931.0949f, 3626.907f, 31.86987f)),
            //new VehicleRaceCheckpoint(4,new Vector3(1540.138f, 3751.758f, 33.91103f)),
            //new VehicleRaceCheckpoint(5,new Vector3(1603.251f, 3673.23f, 33.89013f)),
            //new VehicleRaceCheckpoint(6,new Vector3(1979.965f, 3889.917f, 31.88055f)),

        };
        VehicleRaceTrack sandyDebug = new VehicleRaceTrack("sandyloop1", "Sandy Shores Debug Race", "Simple loop around Sandy Shores", vehicleRaceCheckpoints, vehicleRaceStartingPositions);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(sandyDebug);

        List<VehicleRaceCheckpoint> vehicleRaceCheckpoints7 = new List<VehicleRaceCheckpoint>()
        {
            new VehicleRaceCheckpoint(0, new Vector3(1775.803f, 3375.738f, 39.1007f)),
            new VehicleRaceCheckpoint(1,new Vector3(1582.85f, 3480.25f, 36.21559f)),
            new VehicleRaceCheckpoint(2,new Vector3(984.0595f, 3535.763f, 33.54565f)),
            new VehicleRaceCheckpoint(3,new Vector3(1979.965f, 3889.917f, 31.88055f)),
        };
        VehicleRaceTrack sandyDebug2 = new VehicleRaceTrack("sandyloop2", "Sandy Shores Debug Race 2", "Smaller loop around Sandy Shores", vehicleRaceCheckpoints7, vehicleRaceStartingPositions);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(sandyDebug2);
    }
    private void VineWoodTracks()
    {
        List<VehicleRaceStartingPosition> vehicleRaceStartingPositions2 = new List<VehicleRaceStartingPosition>()
        {
            new VehicleRaceStartingPosition(0,new Vector3(594.9255f, 237.1765f, 102.4954f), 69.08444f),
            new VehicleRaceStartingPosition(1,new Vector3(593.8934f, 231.983f, 102.473f), 76.16933f),
            new VehicleRaceStartingPosition(2,new Vector3(603.9206f, 229.4215f, 101.9056f), 75.98711f),
            new VehicleRaceStartingPosition(3,new Vector3(601.6921f, 235.5364f, 102.1932f), 76.84195f),
        };
        List<VehicleRaceCheckpoint> vehicleRaceCheckpoints2 = new List<VehicleRaceCheckpoint>()
        {
            new VehicleRaceCheckpoint(0,new Vector3(423.3498f, 295.5466f, 102.4895f)),
            new VehicleRaceCheckpoint(1,new Vector3(283.6923f, -73.34487f, 69.56183f)),
            new VehicleRaceCheckpoint(2,new Vector3(-38.75998f, 32.98601f, 71.58444f)),
            new VehicleRaceCheckpoint(3,new Vector3(-82.37482f, -110.0097f, 57.30961f)),
            new VehicleRaceCheckpoint(4,new Vector3(-111.9359f, -220.4267f, 44.26889f)),
            new VehicleRaceCheckpoint(5,new Vector3(-280.4997f, -172.5392f, 39.44448f)),
            new VehicleRaceCheckpoint(6,new Vector3(-343.272f, -191.9327f, 37.79195f)),
            new VehicleRaceCheckpoint(7,new Vector3(-418.2509f, -72.76836f, 42.19181f)),
            new VehicleRaceCheckpoint(8,new Vector3(-391.8105f, 122.2213f, 65.06055f)),
            new VehicleRaceCheckpoint(9,new Vector3(-643.3143f, 130.383f, 56.60829f)),
            new VehicleRaceCheckpoint(10,new Vector3(-996.1489f, 71.36115f, 51.30039f)),
        };
        VehicleRaceTrack vinewoodDebug = new VehicleRaceTrack("vinewoodRace1", "Vinewood Debug Race", "Long race through Vinewood to the coast", vehicleRaceCheckpoints2, vehicleRaceStartingPositions2);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(vinewoodDebug);

        List<VehicleRaceStartingPosition> vehicleRaceStartingPositions3 = new List<VehicleRaceStartingPosition>()
        {
            new VehicleRaceStartingPosition(0,new Vector3(594.9255f, 237.1765f, 102.4954f), 69.08444f),
            new VehicleRaceStartingPosition(1,new Vector3(593.8934f, 231.983f, 102.473f), 76.16933f),
            new VehicleRaceStartingPosition(2,new Vector3(603.9206f, 229.4215f, 101.9056f), 75.98711f),
            new VehicleRaceStartingPosition(3,new Vector3(601.6921f, 235.5364f, 102.1932f), 76.84195f),
        };
        List<VehicleRaceCheckpoint> vehicleRaceCheckpoints3 = new List<VehicleRaceCheckpoint>()
        {
            new VehicleRaceCheckpoint(0,new Vector3(31.48836f, 256.0826f, 109.0203f)),
            new VehicleRaceCheckpoint(1,new Vector3(-2172.701f, -344.3154f, 12.60608f)),

        };
        VehicleRaceTrack vinewoodLONGDebug = new VehicleRaceTrack("vinewoodRace2", "Vinewood LONG Debug Race", "Long race through Vinewood to the coast without many checkpoints.", vehicleRaceCheckpoints3, vehicleRaceStartingPositions3);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(vinewoodLONGDebug);
    }
    private void PaletoTracks()
    {
        List<VehicleRaceStartingPosition> paletoLoop1Starting = new List<VehicleRaceStartingPosition>()
        {
            new VehicleRaceStartingPosition(0,new Vector3(218.0589f, 6565.234f, 31.52061f), 106.4273f),
            new VehicleRaceStartingPosition(1,new Vector3(205.2844f, 6560.81f, 31.64036f), 109.7968f),
            new VehicleRaceStartingPosition(2,new Vector3(190.9381f, 6561.395f, 31.71398f), 110.7484f),
            new VehicleRaceStartingPosition(3,new Vector3(182.8164f, 6552.707f, 31.65496f), 120.4496f),
        };
        List<VehicleRaceCheckpoint> paletoLoop1Checkpoints = new List<VehicleRaceCheckpoint>()
        {
            new VehicleRaceCheckpoint(0,new Vector3(143.2037f, 6526.385f, 31.3439f)),
            new VehicleRaceCheckpoint(1,new Vector3(-202.2245f, 6173.541f, 30.57405f)),
            //new VehicleRaceCheckpoint(2,new Vector3(-341.7607f, 6269.313f, 30.86405f)),
            //new VehicleRaceCheckpoint(3,new Vector3(-358.0514f, 6293.811f, 29.61264f)),
            new VehicleRaceCheckpoint(2,new Vector3(-181.1622f, 6468.749f, 30.21145f)),
            new VehicleRaceCheckpoint(3,new Vector3(150.0628f, 6533.176f, 31.4289f)),
            //new VehicleRaceCheckpoint(0,new Vector3(143.2037f, 6526.385f, 31.3439f)),
            //new VehicleRaceCheckpoint(1,new Vector3(-215.3243f, 6169.238f, 30.88095f)),
            //new VehicleRaceCheckpoint(2,new Vector3(-294.1931f, 6220.725f, 31.19704f)),
            //new VehicleRaceCheckpoint(3,new Vector3(-358.0514f, 6293.811f, 29.61264f)),
            //new VehicleRaceCheckpoint(4,new Vector3(-181.1622f, 6468.749f, 30.21145f)),
            //new VehicleRaceCheckpoint(5,new Vector3(150.0628f, 6533.176f, 31.4289f)),
        };
        VehicleRaceTrack paletoLoop1 = new VehicleRaceTrack("paletoloop1", "Paleto Loop 1", "Paleto Checkpoint Race 1.", paletoLoop1Checkpoints, paletoLoop1Starting);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(paletoLoop1);



        List<VehicleRaceStartingPosition> paletoLoop2Starting = new List<VehicleRaceStartingPosition>()
        {
            new VehicleRaceStartingPosition(0,new Vector3(218.0589f, 6565.234f, 31.52061f), 106.4273f),
            new VehicleRaceStartingPosition(1,new Vector3(205.2844f, 6560.81f, 31.64036f), 109.7968f),
            new VehicleRaceStartingPosition(2,new Vector3(190.9381f, 6561.395f, 31.71398f), 110.7484f),
            new VehicleRaceStartingPosition(3,new Vector3(182.8164f, 6552.707f, 31.65496f), 120.4496f),
        };
        List<VehicleRaceCheckpoint> paletoLoop2Checkpoints = new List<VehicleRaceCheckpoint>()
        {
            new VehicleRaceCheckpoint(0,new Vector3(141.9506f, 6543.649f, 31.27871f)),
            new VehicleRaceCheckpoint(1,new Vector3(87.16878f, 6598.012f, 31.2261f)),
            new VehicleRaceCheckpoint(2,new Vector3(-290.8184f, 6247.322f, 31.10103f)),
            new VehicleRaceCheckpoint(3,new Vector3(-237.6497f, 6163.285f, 31.16849f)),
            new VehicleRaceCheckpoint(4,new Vector3(152.6836f, 6522.836f, 31.30534f)),
        };
        VehicleRaceTrack paletoLoop2 = new VehicleRaceTrack("paletoloop2", "Paleto Loop 2", "Paleto Checkpoint Race 2.", paletoLoop2Checkpoints, paletoLoop2Starting);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(paletoLoop2);



        List<VehicleRaceStartingPosition> paletoDrag1Starting = new List<VehicleRaceStartingPosition>()
        {
            new VehicleRaceStartingPosition(0,new Vector3(218.0589f, 6565.234f, 31.52061f), 106.4273f),
            new VehicleRaceStartingPosition(1,new Vector3(205.2844f, 6560.81f, 31.64036f), 109.7968f),
            new VehicleRaceStartingPosition(2,new Vector3(190.9381f, 6561.395f, 31.71398f), 110.7484f),
            new VehicleRaceStartingPosition(3,new Vector3(182.8164f, 6552.707f, 31.65496f), 120.4496f),
        };
        List<VehicleRaceCheckpoint> paletoDrag1Checkpoints = new List<VehicleRaceCheckpoint>()
        {
            new VehicleRaceCheckpoint(0,new Vector3(143.2037f, 6526.385f, 31.3439f)),
            new VehicleRaceCheckpoint(1, new Vector3(-769.4749f, 5497.793f, 34.48269f)),


        };
        VehicleRaceTrack paletoDrag1 = new VehicleRaceTrack("paletodrag1", "Paleto Drag 1", "Paleto Drag Race 1.", paletoDrag1Checkpoints, paletoDrag1Starting);
        VehicleRaceTypeManager.VehicleRaceTracks.Add(paletoDrag1);

    }
}

