﻿using LosSantosRED.lsr.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

[Serializable]
public class CraftableItem
{

    public CraftableItem()
    {
    }
    public CraftableItem(string name, string resultant, List<Ingredient> ingredients)
    {
        Name = name;
        Resultant = resultant;
        Ingredients = ingredients;
    }
    public string Name { get; set; }
    public string Resultant { get; set; }
    public string AnimationDictionary { get; set; }
    public string AnimationName { get; set; }
    public int ResultantAmount { get; set; }
    public bool SingleUnit { get; set; }
    public string Category { get; set; }
    public List<Ingredient> Ingredients { get; set; }
    public int Cooldown { get; set; } = 2000;
    public string CrimeId { get; set; }
    public HashSet<string> CraftingFlags { get; set; }



    public bool HasCustomAnimations => !string.IsNullOrEmpty(AnimationDictionary) && !string.IsNullOrEmpty(AnimationName);

    //[XmlIgnore]
    //private string _ingredientList;
    //[XmlIgnore]
    //public string IngredientList
    //{
    //    get
    //    {
    //        if (_ingredientList == null)
    //        {
    //            _ingredientList = GetIngredients();
    //        }
    //        return "Ingredients Required: ~n~" + _ingredientList;
    //    }
    //}
    public string GetIngredientDescription(int quantity, IModItems modItems)
    {
        ModItem resultantItem = modItems?.Get(Resultant);
        StringBuilder ingredientStringBuilder = new StringBuilder();
        foreach (var ingredient in Ingredients)
        {
            if (!ingredient.IsConsumed)
            {
                quantity = 1;
            }
            ModItem relatedItem = modItems?.Get(ingredient.IngredientName);
            string modItemDescription = "Item(s)";
            if (relatedItem != null)
            {
                modItemDescription = relatedItem.MeasurementName + "(s)";
            }
            ingredientStringBuilder.Append($"{ingredient.IngredientName} - {ingredient.Quantity * quantity} {modItemDescription}~n~");
        }
        string extendeddescription = "";
        if(resultantItem != null && !string.IsNullOrEmpty(resultantItem.Description))
        {
            extendeddescription = resultantItem.Description + "~n~~n~";
        }


        return extendeddescription + "~r~Ingredients Required:~s~ ~n~" + ingredientStringBuilder.ToString().Trim();
    }

}
