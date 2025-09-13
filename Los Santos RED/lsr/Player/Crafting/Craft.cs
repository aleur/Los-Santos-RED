using LosSantosRED.lsr.Interface;
using Rage;
using System;
using System.Linq;
using LosSantosRED.lsr.Helper.Crafting;
using System.Collections.Generic;
using Rage.Native;
using LosSantosRED.lsr.Helper;
using RAGENativeUI;
using System.Drawing;
using RAGENativeUI.Elements;

namespace Mod
{
    public class Crafting
    {
        private Player Player;
        private IModItems ModItems;
        private ISettingsProvideable Settings;
        private IWeapons Weapons;

        private TimerBarPool TimerBarPool;


        public bool IsCrafting { get; private set; } = false;
        public CraftingMenu CraftingMenu { get; set; }
        public ICraftableItems CraftableItems;
        private BarTimerBar ProgressBar;
        public List<(int, CraftableItem)> UnfinishedCrafts { get; set; } = new List<(int, CraftableItem)> ();

        public Crafting(Player player, ICraftableItems craftableItems, IModItems modItems, ISettingsProvideable settings, IWeapons weapons)
        {
            Player = player;
            CraftableItems = craftableItems;
            ModItems = modItems;
            Settings = settings;
            Weapons = weapons;
        }
        public void Setup()
        {
            SetupCraftableLookup();
            Player.Crafting = this;

            TimerBarPool= new TimerBarPool();
            ProgressBar = new BarTimerBar("Progress");
            ProgressBar.BackgroundColor = Color.FromArgb(100, 142, 50, 50);
            ProgressBar.ForegroundColor = Color.FromArgb(255, 181, 48, 48);//Red

        }
        public void Reset()
        {
            UnfinishedCrafts.Clear();
        }
        private void SetupCraftableLookup()
        {
            CraftableItems.CraftablesLookup = new System.Collections.Generic.Dictionary<string, CraftableItemLookupModel>();

            //Just holding reference to the craftable item in case any of the other details are required anywhere else.
            foreach (CraftableItem craftableItem in CraftableItems.Items)
            {
                CraftableItems.CraftablesLookup.Add(
                    craftableItem.Name,
                    new CraftableItemLookupModel()
                    {
                        RecipeName = craftableItem.Name,
                        IngredientLookup = GetIngredientLookup(craftableItem.Ingredients),
                        CraftableItem = craftableItem,
                    });
            }
        }
        private Dictionary<string, Ingredient> GetIngredientLookup(List<Ingredient> ingredient)
        {
            Dictionary<string, Ingredient> ingredientLookup = new Dictionary<string, Ingredient>();
            foreach (var _ingredient in ingredient)
            {
                ingredientLookup.Add(_ingredient.IngredientName, _ingredient);
            }
            return ingredientLookup;
        }
        private void DeductIngredientsFromInventory(CraftableItemLookupModel craftItem, int quantity)
        {
            foreach (Ingredient ingredient in craftItem.CraftableItem.Ingredients)
            {
                if (ingredient.IsConsumed && ingredient.Quantity * quantity > 0)
                {
                    Player.Inventory.Remove(Player.Inventory.ItemsList.Find(x=>x.ModItem.Name == ingredient.IngredientName).ModItem, ingredient.Quantity * quantity);
                }
            }
        }
        private void PerformAnimation(CraftableItem craftItem)
        {
            if(Player.ActivityManager.IsInteractingWithLocation && !Player.InteriorManager.IsInsideTeleportInterior)
            {
                return;
            }
            string dictionary = "missmechanic";
            string animation = "work2_base";

            //anim@scripted@ulp_missions@paperwork@male@
            //action
            if (craftItem != null && craftItem.HasCustomAnimations)
            {
                dictionary = craftItem.AnimationDictionary;
                animation = craftItem.AnimationName;
            }
            if(!AnimationDictionary.RequestAnimationDictionayResult(dictionary))
            {
                return;
            }

            NativeFunction.CallByName<uint>("TASK_PLAY_ANIM", Player.Character, dictionary, animation, 4.0f, -4.0f, -1, 1, 0, false, false, false);
        }
        public void CraftItem(CraftableItem craftableItem, int quantity, int multiplier = 1, string craftingFlag = null)
        {
            if (IsCrafting)
            {
                Game.DisplayHelp("Cannot start crafting, ~r~Cooldown active.");
                return;
            }
            Player.ActivityManager.StopDynamicActivity();

            if (craftableItem == null)
            {
                Game.DisplayHelp("Cannot start crafting.");
                return;
            }

            string productName = craftableItem.Name;
            ModItem itemToGive = ModItems.Get(craftableItem.Resultant);

            if (itemToGive == null)
            {
                Game.DisplayHelp("Cannot start crafting.");
                return;
            }

            CraftingMenu.Hide(); 

            CraftableItemLookupModel craftItem = CraftableItems.CraftablesLookup[craftableItem.Name];
            DeductIngredientsFromInventory(craftItem, multiplier);

            Player.IsSetDisabledControlsWithCamera = true;
            IsCrafting = true;

            PerformAnimation(craftableItem);

            if (!string.IsNullOrEmpty(craftableItem.CrimeId))
            {
                Player.Violations.SetContinuouslyViolating(craftableItem.CrimeId);
            }

            TimerBarPool.Add(ProgressBar);

            uint GameTimeStartedCrafting = Game.GameTime;

            Player.ButtonPrompts.AddPrompt("craftingStop", "Stop Crafting", "stopcraftingprompt1", Settings.SettingsManager.KeySettings.InteractCancel, 999);
            Player.ButtonPrompts.AddPrompt("craftingPause", "Put Away", "putawayprompt1", Settings.SettingsManager.KeySettings.InteractStart, 999);

            int craftedQuantity = 0;
            EntryPoint.WriteToConsole($"craftedQuantity{craftedQuantity} quantity{quantity}");
            while (craftedQuantity < quantity)//Game.GameTime - GameTimeStartedCrafting <= (craftableItem.Cooldown * quantity))
            {
                if (!Player.IsAliveAndFree || Player.IsUnconscious || Player.ButtonPrompts.IsPressed("stopcraftingprompt1") || Player.ButtonPrompts.IsPressed("putawayprompt1"))
                {
                    if (!string.IsNullOrEmpty(craftableItem.CrimeId)) Player.Violations.StopContinuouslyViolating(craftableItem.CrimeId);

                    Player.IsSetDisabledControlsWithCamera = false;

                    string Message;
                    if (Player.ButtonPrompts.IsPressed("stopcraftingprompt1"))
                    {
                        TimerBarPool.Remove(ProgressBar);
                        Message = "Crafting cancelled.";
                        // Give back ingredients if none crafted
                        if (craftedQuantity == 0)
                        {
                            foreach (Ingredient ingredient in craftableItem.Ingredients) Player.Inventory.Add(ModItems.Get(ingredient.IngredientName), ingredient.Quantity * multiplier);
                        }
                    }
                    else if (Player.ButtonPrompts.IsPressed("putawayprompt1"))
                    {
                        TimerBarPool.Remove(ProgressBar);
                        Message = "Crafting paused: item put away.";
                        UnfinishedCrafts.Add((quantity-craftedQuantity, craftableItem));
                        Game.DisplaySubtitle($"Crafted {productName} - {craftedQuantity} {itemToGive.MeasurementName}(s)");
                    }
                    else
                    {
                        TimerBarPool.Remove(ProgressBar);
                        Message = "Crafting failed.";
                    }
                    Game.DisplayHelp(Message);


                    Player.ButtonPrompts.RemovePrompts("craftingPause");
                    Player.ButtonPrompts.RemovePrompts("craftingStop");
                    NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
                    IsCrafting = false;

                    return;
                }
                float currentPercentage = (float)(Game.GameTime - GameTimeStartedCrafting) / (float)craftableItem.Cooldown;
                ProgressBar.Percentage = currentPercentage;
                if (Game.GameTime - GameTimeStartedCrafting >= craftableItem.Cooldown)
                {
                    GameTimeStartedCrafting = Game.GameTime;
                    itemToGive.AddToPlayerInventory(Player, 1);
                    NativeHelper.PlaySuccessSound();
                    craftedQuantity++;
                    if (craftedQuantity < quantity)
                    {
                        Game.DisplaySubtitle($"Crafted {productName} {craftedQuantity}/{quantity} {itemToGive.MeasurementName}(s)", craftableItem.Cooldown);
                    }
                    EntryPoint.WriteToConsole($"CRAFTED ONE craftedQuantity{craftedQuantity} quantity{quantity}");
                }

                TimerBarPool.Draw();

                GameFiber.Yield();
            }
            TimerBarPool.Remove(ProgressBar);

            Player.ButtonPrompts.RemovePrompts("craftingPause");
            Player.ButtonPrompts.RemovePrompts("craftingStop");   
            NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
            IsCrafting = false; 

            if (!string.IsNullOrEmpty(craftableItem.CrimeId)) Player.Violations.StopContinuouslyViolating(craftableItem.CrimeId);
            Player.IsSetDisabledControlsWithCamera = false;
            Game.DisplaySubtitle($"Crafted {productName} - {quantity} {itemToGive.MeasurementName}(s)");
        }
    }
}