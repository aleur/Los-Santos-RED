using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IPedGroups
    {
        List<PedGroup> PedGroupList { get; set; }
        PedGroup GetPedGroup(string name);
    }
}
