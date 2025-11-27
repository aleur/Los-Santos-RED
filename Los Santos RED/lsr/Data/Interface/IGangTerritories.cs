using LosSantosRED.lsr.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IGangTerritories
    {
        List<GangTerritory> GangTerritoriesList { get; }
        Gang GetRandomGang(string internalGameName, int wantedLevel);
        List<Gang> GetGangs(string internalGameName, int wantedLevel);
        Gang GetMainGang(string internalGameName);
        Gang GetNthGang(string internalGameName, int v);
        List<GangTerritory> GetGangTerritory(string iD);
        List<GangTerritory> GetGangTerritory(string iD, int priority);
    }
}
