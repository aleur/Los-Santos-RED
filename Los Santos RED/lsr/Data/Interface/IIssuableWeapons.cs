using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IIssuableWeapons
    {
        List<IssuableWeaponsGroup> IssuableWeaponsGroupLookup { get; set; }
        List<IssuableWeapon> GetWeaponData(string issuableWeaponsID);
    }
}