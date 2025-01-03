using ExtensionsMethods;
using LosSantosRED.lsr.Helper;
using LosSantosRED.lsr.Interface;
using LosSantosRED.lsr.Player;
using LSR.Vehicles;
using Mod;
using Rage;
using Rage.Native;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace LosSantosRED.lsr.Data
{
    public class WorldSave
    {
        public WorldSave()
        {

        }
        public WorldSave(string playerName, int money, string modelName, bool isMale, PedVariation currentModelVariation, List<StoredWeapon> weaponInventory, List<VehicleSaveStatus> vehicleVariations)
        {
            PlayerName = playerName;
            Money = money;
            ModelName = modelName;
            IsMale = isMale;
            CurrentModelVariation = currentModelVariation;
            WeaponInventory = weaponInventory;
            OwnedVehicleVariations = vehicleVariations;
            SaveDateTime = DateTime.Now;
        }
        public string PlayerName { get; set; }
        public int Money { get; set; }
        public List<BankAccount> SavedBankAccounts { get; set; } = new List<BankAccount>();
        public string ModelName { get; set; }
        public Vector3 PlayerPosition { get; set; }
        public float PlayerHeading { get; set; }
        public bool IsMale { get; set; }
        public int SaveNumber { get; set; }
        public DateTime SaveDateTime { get; set; }
        public DateTime CurrentDateTime { get; set; }
        public int UndergroundGunsMoneySpent { get; set; }
        public int UndergroundGunsDebt { get; set; }
        public int UndergroundGunsReputation { get; set; }
        public int OfficerFriendlyMoneySpent { get; set; }
        public int OfficerFriendlyDebt { get; set; }
        public int OfficerFriendlyReputation { get; set; }
        public float HungerValue { get; set; }
        public float ThirstValue { get; set; }
        public float SleepValue { get; set; }
        public int SpeechSkill { get; set; }
        public bool IsCop { get; set; }
        public bool IsEMT { get; set; }
        public bool IsFireFighter { get; set; }
        public bool IsSecurityGuard {get;set;}
        public string AssignedAgencyID { get; set; }
        public string VoiceName { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public int MaxHealth { get; set; }
        public string CurrentTeleportInterior { get; set; }
        public PedVariation CurrentModelVariation { get; set; }
        public DriversLicense DriversLicense { get; set; }
        public CCWLicense CCWLicense { get; set; }
        public PilotsLicense PilotsLicense { get; set; }
        public List<SavedTextMessage> TextMessages { get; set; } = new List<SavedTextMessage>();
        public List<PhoneContact> Contacts { get; set; } = new List<PhoneContact>();
        public List<ContactRelationship> ContactRelationships { get; set; } = new List<ContactRelationship>();
        public List<GangRepSave> GangReputationsSave { get; set; } = new List<GangRepSave>();
        public GangKickSave GangKickSave { get; set; }
        public List<StoredWeapon> WeaponInventory { get; set; } = new List<StoredWeapon>();
        public List<InventorySave> InventoryItems { get; set; } = new List<InventorySave>();
        public List<VehicleSaveStatus> OwnedVehicleVariations { get; set; } = new List<VehicleSaveStatus>();
        public List<SavedResidence> SavedResidences { get; set; } = new List<SavedResidence>();
        public CellPhoneSave CellPhoneSave { get; set; } = new CellPhoneSave();

        public List<GangLoanSave> GangLoanSaves { get; set; } = new List<GangLoanSave>();

        [OnDeserialized()]
        private void SetValuesOnDeserialized(StreamingContext context)
        {
            MaxHealth = 200;
        }

        private DirectoryInfo worldDirectory { get; set; }



        //Save
        public void Save(string worldName, List<FileInfo> files)
        {
            DirectoryInfo baseDirectory = new DirectoryInfo("Plugins\\LosSantosRED\\Worlds");

            worldDirectory = baseDirectory.CreateSubdirectory(worldName);

            foreach (FileInfo file in files)
            {
                string destinationPath = Path.Combine(worldDirectory.FullName, file.Name);
                file.CopyTo(destinationPath, overwrite: true);
            }

            EntryPoint.WriteToConsole($"World '{worldName}' saved with {files.Count} files in {worldDirectory.FullName}", 0);
        }
        /*Load
        public void Load(IWeapons weapons,IPedSwap pedSwap, IInventoryable player, ISettingsProvideable settings, IEntityProvideable world, IGangs gangs, IAgencies agencies, ITimeControllable time, IPlacesOfInterest placesOfInterest, IModItems modItems, IContacts contacts, IInteractionable interactionable)
        {
            try
            {
                Game.FadeScreenOut(1000, true);

                FileInfo ConfigFile = worldDirectory.GetFiles("Weapons*.xml").OrderByDescending(x => x.Name).FirstOrDefault();
                if (ConfigFile != null)
                {
                    EntryPoint.WriteToConsole($"Loaded Weapons config: {ConfigFile.FullName}", 0);
                    WeaponsList = Serialization.DeserializeParams<WeaponInformation>(ConfigFile.FullName);
                }
                else if (File.Exists(ConfigFileName))
                {
                    EntryPoint.WriteToConsole($"Loaded Issuable Weapons config  {ConfigFileName}", 0);
                    WeaponsList = Serialization.DeserializeParams<WeaponInformation>(ConfigFileName);
                }
                else
                {
                    EntryPoint.WriteToConsole($"No Weapons config found, creating default", 0);
                    DefaultConfig();
                }

                GameFiber.Sleep(1000);
                Game.FadeScreenIn(1500, true);
                player.DisplayPlayerNotification();
            }
            catch (Exception e)
            {
                Game.FadeScreenIn(0);
                EntryPoint.WriteToConsole("Error Loading World Save: " + e.Message + " " + e.StackTrace, 0);
                Game.DisplayNotification("Error Loading World Save");
            }
        }*/
        public string Title => $"{SaveNumber.ToString("D2")} - {PlayerName} ({(Money + (SavedBankAccounts == null ? 0 : SavedBankAccounts.Sum(x=> x.Money))).ToString("C0")}) - {CurrentDateTime.ToString("MM/dd/yyyy HH:mm")}";
        public string RightLabel => SaveDateTime.ToString("MM/dd/yyyy HH:mm");
    }

}
