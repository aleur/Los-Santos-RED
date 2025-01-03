using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IWantedLevels
    {
        List<WantedLevel> WantedLevelList { get; set; }
        void Setup(IHeads heads, IDispatchableVehicles dispatchableVehicles, IDispatchablePeople dispatchablePeople, IIssuableWeapons issuableWeapons);
    }
}