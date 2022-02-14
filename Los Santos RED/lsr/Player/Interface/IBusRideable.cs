﻿using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IBusRideable
    {
        bool IsInVehicle { get; }
        bool IsRidingBus { get; set; }
        bool IsGettingIntoAVehicle { get; }
        Ped Character { get; }
    }
}