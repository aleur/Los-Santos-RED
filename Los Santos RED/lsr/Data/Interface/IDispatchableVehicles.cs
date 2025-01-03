using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IDispatchableVehicles
    {
        List<DispatchableVehicleGroup> VehicleGroupLookup { get; set; }
        List<DispatchableVehicle> GetVehicleData(string dispatchableVehicleGroupID);
        void Setup(IPlateTypes plateTypes);
    }
}