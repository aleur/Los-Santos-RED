using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BusinessMenuTypes
{
    public BusinessMenuTypes()
    {
    }
    public List<BusinessMenu> BusinessMenuList { get; private set; } = new List<BusinessMenu>();
    // Other Business Menus
    public List<PropShopMenu> PropShopMenus { get; private set; } = new List<PropShopMenu>();
    public List<TreatmentOptions> TreatmentOptionsList { get; private set; } = new List<TreatmentOptions>();
    public List<PedVariationShopMenu> PedVariationShopMenus { get; private set; } = new List<PedVariationShopMenu>();
    public List<VehicleVariationShopMenu> VehicleVariationShopMenus { get; private set; } = new List<VehicleVariationShopMenu>();


    public List<PedClothingShopMenu> PedClothingShopMenus { get; private set; } = new List<PedClothingShopMenu>();

}

