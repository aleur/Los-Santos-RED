using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IPropItems
    {
        List<PhysicalItem> PhysicalItemsList { get; set; }
        PhysicalItem Get(string ID);
    }
}
