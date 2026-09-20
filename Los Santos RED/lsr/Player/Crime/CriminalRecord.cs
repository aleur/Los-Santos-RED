using LosSantosRED.lsr;
using LosSantosRED.lsr.Interface;
using LSR.Vehicles;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable]
public class CriminalRecord
{
    [XmlIgnore]
    public Dictionary<string, int> PastCitations { get; set; } // Format: CrimeID, Times Violated


    public int TrafficViolationsFines { get; set; }
    /*
    [XmlArray("PastCitations")]
    [XmlArrayItem("Citation")]
    public List<CitationEntry> PastCitationsList // for saving purposes only
    {
        get => PastCitations
            .Select(kvp => new CitationEntry
            {
                Key = kvp.Key,
                Value = kvp.Value
            })
            .ToList();

        set
        {
            EntryPoint.WriteToConsole(
                $"PastCitationsList SETTER CALLED: {value?.Count ?? -1}"
            );

            PastCitations = value.ToDictionary(x => x.Key, x => x.Value);
        }

    }*/

    [XmlArray("PastCitations")]
    [XmlArrayItem("Citation")]
    public List<CitationEntry> PastCitationsList { get; set; } // saving purposes only
    public class CitationEntry
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public int Value { get; set; }
    }
    public CriminalRecord()
    {
        PastCitations = new Dictionary<string, int>();
        TrafficViolationsFines = 0;
    }
    public void Reset()
    {
        TrafficViolationsFines = 0;
        PastCitations.Clear();
    }

    public void AddCitation(Crime crime)
    {
        PastCitations.TryGetValue(crime.ID, out int count);
        PastCitations[crime.ID] = count + 1;
    }
}