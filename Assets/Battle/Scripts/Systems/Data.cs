using System;
using Unity.Entities;

namespace UnitAgent
{
    // Serializable attribute is for editor support.
    [Serializable]
    public struct TranslationSpeed : IComponentData
    {
        public float unitsPerSecond;
    }
}