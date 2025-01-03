using LosSantosRED.lsr.Data;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class WorldSaves : IWorldSaves
{
    private readonly string ConfigFileName = "Plugins\\LosSantosRED\\SaveGames.xml";
    private WorldSave PlayingSave;
    public WorldSaves()
    {
    }
    public List<WorldSave> WorldSaveList { get; private set; } = new List<WorldSave>();
    public int NextSaveGameNumber => WorldSaveList.Count + 1;
    public void ReadConfig()
    {
        DirectoryInfo LSRDirectory = new DirectoryInfo("Plugins\\LosSantosRED");
        DetectAlternativeConfigs(LSRDirectory);
        /*
        FileInfo ConfigFile = LSRDirectory.GetFiles("SaveGames*.xml").OrderByDescending(x => x.Name).FirstOrDefault();
        if (ConfigFile != null)
        {
            EntryPoint.WriteToConsole($"Loaded Games Saves config: {ConfigFile.FullName}", 0);
            WorldSaveList = Serialization.DeserializeParams<WorldSave>(ConfigFile.FullName);
        }
        else if (File.Exists(ConfigFileName))
        {
            EntryPoint.WriteToConsole($"Loaded Game Saves config  {ConfigFileName}", 0);
            WorldSaveList = Serialization.DeserializeParams<WorldSave>(ConfigFileName);
        }
        else
        {
            EntryPoint.WriteToConsole($"No Game Saves config found, creating default", 0);
        }*/
    }
    public void Save(ISaveable player, IWeapons weapons, ITimeControllable time, IPlacesOfInterest placesOfInterest, IModItems modItems, int saveNumber)
    {
        String promptName = "PLACEHOLDER";
        //EntryPoint.WriteToConsoleTestLong($"NEW SAVE GAME save number {saveNumber}");
        WorldSave mySave = new WorldSave();
        mySave.SaveNumber = saveNumber;
        WorldSaveList.Add(mySave);
        //mySave.Save(promptName,groupFiles);
        Serialization.SerializeParams(WorldSaveList, ConfigFileName);
        PlayingSave = mySave;
    }
    public void Load(WorldSave gameSave, IWeapons weapons, IPedSwap pedSwap, IInventoryable player, ISettingsProvideable settings, IEntityProvideable world, IGangs gangs, ITimeControllable time, IPlacesOfInterest placesOfInterest,
        IModItems modItems, IAgencies agencies, IContacts contacts, IInteractionable interactionable)
    {
        //gameSave.Load(weapons, pedSwap, player, settings, world, gangs, agencies, time, placesOfInterest, modItems, contacts, interactionable);
        PlayingSave = gameSave;
    }
    public void DeleteSave(WorldSave toDelete)
    {
        if (toDelete != null)
        {
            if (PlayingSave != null && PlayingSave == toDelete)
            {
                PlayingSave = null;
            }
            WorldSaveList.Remove(toDelete);
        }
        Serialization.SerializeParams(WorldSaveList, ConfigFileName);
    }
    public void DeleteSave()
    {
        if (PlayingSave != null)
        {
            EntryPoint.WriteToConsole($"Game Save Deleted {PlayingSave.SaveNumber} {PlayingSave.ModelName} {PlayingSave.PlayerName}");
            WorldSaveList.Remove(PlayingSave);
            PlayingSave = null;
        }
        Serialization.SerializeParams(WorldSaveList, ConfigFileName);
    }
    public bool IsPlaying(WorldSave toCheck) => PlayingSave != null && toCheck != null && PlayingSave == toCheck;

    public void OnChangedPlayer()
    {
        PlayingSave = null;
    }
    public void DetectAlternativeConfigs(DirectoryInfo directory)
    {
        List<FileInfo> allFiles = directory.GetFiles("*.xml").ToList();

        Dictionary<string, List<FileInfo>> groupedConfigs = new Dictionary<string, List<FileInfo>>();

        foreach (FileInfo file in allFiles)
        {
            if (file.Name.StartsWith("SavedVariation", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
            int lastUnderscoreIndex = fileNameWithoutExtension.LastIndexOf('_');
            if (lastUnderscoreIndex != -1)
            {
                string configSuffix = fileNameWithoutExtension.Substring(lastUnderscoreIndex + 1);

                if (!groupedConfigs.ContainsKey(configSuffix))
                {
                    groupedConfigs[configSuffix] = new List<FileInfo>();
                }
                groupedConfigs[configSuffix].Add(file);
            }
            else
            {
                if (!groupedConfigs.ContainsKey("Default"))
                {
                    groupedConfigs["Default"] = new List<FileInfo>();
                }
                groupedConfigs["Default"].Add(file);
            }
        }

        List<string> groupKeys = groupedConfigs.Keys.ToList();

        for (int i = 0; i < groupKeys.Count; i++)
        {
            string groupKey = groupKeys[i];
            List<FileInfo> groupFiles = groupedConfigs[groupKey];

            EntryPoint.WriteToConsole($"Config Group: {groupKey}", 0);

            WorldSave template = new WorldSave();
            template.SaveNumber = i + 1;
            WorldSaveList.Add(template);
            template.Save(groupKey,groupFiles);
        }
    }
}
