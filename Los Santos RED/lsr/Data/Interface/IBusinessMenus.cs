using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IBusinessMenus
    {
        BusinessMenuTypes PossibleBusinessMenus { get; }
        BusinessMenu GetSpecificBusinessMenu(string menuID);
        PropertyMenu GetSpecificPropertyMenu(string menuID);
    }
}
