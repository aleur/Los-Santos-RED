﻿using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosSantosRED.lsr.Interface
{
    public interface IIntoxicatable : IActivityPerformable
    {
        bool IsIntoxicated { get; set; }
        float IntoxicatedIntensity { get; set; }
        Vector3 Position { get; }
        bool IsMoveControlPressed { get; }
        Scenario ClosestScenario { get; }

        void SetUnarmed();
    }
}