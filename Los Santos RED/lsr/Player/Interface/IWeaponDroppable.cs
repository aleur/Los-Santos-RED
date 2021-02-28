﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IWeaponDroppable
    {
        bool IsInVehicle { get; }
        bool IsVisiblyArmed { get; }
    }
}