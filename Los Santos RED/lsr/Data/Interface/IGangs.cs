using Rage;
using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IGangs
    {
        List<Gang> AllGangs { get; }
        List<Gang> GangsList { get; set; }
        Gang GetGang(string GangInitials);
        List<Gang> GetGangs(Ped cop);
        List<Gang> GetGangs(Vehicle CopCar);
        List<Gang> GetSpawnableGangs(int wantedLevel);
        List<Gang> GetAllGangs();
        Gang GetGangByContact(string contactName);
        void CheckTerritory(IGangTerritories gangTerritories);
        void Setup(IHeads heads, IDispatchableVehicles dispatchableVehicles, IDispatchablePeople dispatchablePeople, IIssuableWeapons issuableWeapons, IContacts contacts);
    }
}
