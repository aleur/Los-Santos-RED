using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public class BlankLocation : GameLocation
{
    public BlankLocation() : base()
    {

    }
    public override bool ShowsOnDirectory { get; set; } = false;
    public override bool ShowsOnTaxi { get; set; } = false;
    public override string TypeName { get; set; } = "Blank Location";
    public override bool ShowsMarker { get; set; } = false;
    public override bool IsBlipEnabled { get; set; } = false;
    public override float ActivateDistance { get; set; } = 300f;
    public string MissionTargetType { get; set; }
    public bool CanBeAmbushableTarget { get; set; } = true;
    public List<string> AvailablePedAmbushMissions { get; set; }
    /* Missions
     * Kill
     * Kidnap
     * Assault
     */
    public bool CanVehiclesBeTarget { get; set; } = false;
    public List<string> AvailableVehicleAmbushMissions { get; set; }
    /* Missions
     * Steal
     * Destroy
     */
    public bool WillSelectMultipleTargetGroups { get; set; } = true;
    [XmlIgnore]
    public bool IsAmbushTarget { get; set; } = false;
    [XmlIgnore]
    public bool AreVehiclesTargeted { get; set; } = false;
    public BlankLocation(Vector3 _EntrancePosition, float _EntranceHeading, string _Name, string _Description) : base(_EntrancePosition, _EntranceHeading, _Name, _Description)
    {

    }
    public override bool CanCurrentlyInteract(ILocationInteractable player)
    {
        return false;
    }
    public override void AddLocation(PossibleLocations possibleLocations)
    {
        possibleLocations.BlankLocations.Add(this);
        base.AddLocation(possibleLocations);
    }
}

