using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable]
public class BOLO
{
    public int WantedLevel { get; set; }
    public Vector3 LastSeenLocation { get; set; }
    public DateTime DateTimeLastWantedEnded { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<CrimeEvent> Crimes { get; set; } = new List<CrimeEvent>();
    public BOLO(Vector3 lastSeenLocation, List<CrimeEvent> crimes, int wantedLevel)
    {
        LastSeenLocation = lastSeenLocation;
        if (crimes == null)
        {
            Crimes = new List<CrimeEvent>();
        }
        else
        {
            Crimes = crimes;
        }
        WantedLevel = wantedLevel;
    }

    public BOLO()
    {
    }

}

