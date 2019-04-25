using System;
using Unity.Entities;

namespace Dux.Battle
{
    // Serializable attribute is for editor support.
    [Serializable]
    public struct UnitSpeed : IComponentData
    {
        public float RadiansPerSecond;
    }
}

