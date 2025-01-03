using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IHeads
    {
        List<HeadDataGroup> RandomHeadDataLookup { get; set; }
        void DefaultConfig();
        List<RandomHeadData> GetHeadData(string headDataGroupID);
    }
}