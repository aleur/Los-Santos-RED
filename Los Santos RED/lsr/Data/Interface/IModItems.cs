﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IModItems
    {
        PossibleItems PossibleItems { get; set; }
        WeaponItem GetWeapon(string modelName);
        WeaponItem GetWeapon(uint modelHash);
        ModItem Get(string text);
        ModItem GetRandomItem(bool allowIllegal, bool allowCellphones);
        List<ModItem> AllItems();
        List<ModItem> InventoryItems();
        void WriteToFile();
        void Setup(IPropItems physicalItems, IWeapons weapons, IIntoxicants intoxicants, ICellphones cellphones);
        WeaponItem GetRandomWeapon(bool allowIllegal);
        VehicleItem GetVehicle(string modelName);
        VehicleItem GetVehicle(uint modelHash);
    }
}
