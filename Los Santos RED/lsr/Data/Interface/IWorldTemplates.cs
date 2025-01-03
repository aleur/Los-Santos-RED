using LosSantosRED.lsr.Data;
using Rage;
using System.Collections.Generic;

namespace LosSantosRED.lsr.Interface
{
    public interface IWorldTemplates
    {
        List<WorldTemplate> WorldTemplateList { get; }

        void Load(WorldTemplate template);
    }
}