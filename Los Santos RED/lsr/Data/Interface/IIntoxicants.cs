using Rage;
using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IIntoxicants
    {
        List<Intoxicant> IntoxicantList { get; set; }
        List<Intoxicant> Items { get; }

        Intoxicant Get(string name);
        void SerializeAllSettings();
    }
}
