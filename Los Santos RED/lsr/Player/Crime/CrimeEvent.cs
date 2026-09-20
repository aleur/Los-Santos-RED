using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable]
public class CrimeEvent
{
    private uint GameTimeLastOccurred;
    private uint InstanceDuration = 20000;
    public CrimeEvent()
    {

    }
    public CrimeEvent(Crime crimeToReport, CrimeSceneDescription currentInfo)
    {
        AssociatedCrime = crimeToReport;
        AssociatedCrimeID = AssociatedCrime.ID;
        CurrentInformation = currentInfo;
        GameTimeLastOccurred = Game.GameTime;
    }
    [XmlIgnore]
    public Crime AssociatedCrime { get; set; }
    public string AssociatedCrimeID { get; set; }
    [XmlIgnore]
    public CrimeSceneDescription CurrentInformation { get; set; }
    public int Instances { get; set; } = 1;
    public bool CanAddInstance
    {
        get
        {
            if (Game.GameTime - GameTimeLastOccurred >= InstanceDuration)
                return true;
            else
                return false;
        }
    }
    public bool RecentlyOccurred(uint TimeSince)
    {
        if (Game.GameTime - GameTimeLastOccurred <= TimeSince)
            return true;
        else
            return false;
    }
    public void AddInstance()
    {
        if (CanAddInstance && AssociatedCrime.CanViolateMultipleTimes)
        {    
            GameTimeLastOccurred = Game.GameTime;
            Instances++;
            //EntryPoint.WriteToConsole($"PLAYER EVENT: ADD TO EXISTING CRIME: {AssociatedCrime.Name} Instances {Instances}", 3);
        }
    }
    public void ForceAddInstance()
    {
        GameTimeLastOccurred = Game.GameTime;
        Instances++;
    }
}