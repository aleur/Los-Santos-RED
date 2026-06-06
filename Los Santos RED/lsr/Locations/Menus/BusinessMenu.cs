using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable]
public class BusinessMenu
{
    public BusinessMenu()
    {

    }
    public BusinessMenu(string iD, string name)
    {
        ID = iD;
        Name = name;
    }
    public BusinessMenu(string iD, string name, string propertyMenuID)
    {
        ID = iD;
        Name = name;
        PropertyMenuID = propertyMenuID;
    }
    public BusinessMenu(string iD, string name, string amenitiesGroupID, List<string> availableAmenities)
    {
        ID = iD;
        Name = name;
        AmenitiesGroupID = amenitiesGroupID;
        AvailableAmenities = availableAmenities;
    }
    public string ID { get; set; }
    public string Name { get; set; }
    public string BannerOverride { get; set; }
    public string PropertyMenuID { get; set; }
    public string AmenitiesGroupID { get; set; } 
    public List<string> AvailableAmenities { get; set; }
    public void SetupBusiness(GameLocation loc, PropertyMenu pm)
    {
        pm.OverrideData(loc);
    }
}

