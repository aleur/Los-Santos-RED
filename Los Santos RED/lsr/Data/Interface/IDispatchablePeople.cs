using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IDispatchablePeople
    {
        List<DispatchablePersonGroup> PeopleGroupLookup { get; set; }
        List<DispatchablePersonGroup> AllPeople { get; }
        List<DispatchablePerson> GetPersonData(string dispatchablePersonGroupID);
        void Setup(IIssuableWeapons issuableWeapons);
    }
}