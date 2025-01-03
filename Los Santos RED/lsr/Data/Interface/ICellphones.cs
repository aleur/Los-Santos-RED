using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface ICellphones
    {
        List<CellphoneData> CellphoneList { get; set; }
        CellphoneData GetDefault();
        CellphoneData GetPhone(string name);
        CellphoneData GetRandomRegular();
    }
}
