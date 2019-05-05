using System;
using Unity.Entities;

namespace UnitAgent
{
    [Serializable] public struct TranslationSpeed : IComponentData{ public float UnitsPerSecond; }

    [Serializable] public struct RotationSpeed : IComponentData {public float RadiansPerSecond; }

    [Serializable] public struct Agent : IComponentData { }

    [Serializable] public struct Unit : IComponentData { }
    
    // UnitId and UnitMembership are set when the unit and it's agents are spawned.
    // It is unique per unit, but all agents 'attached' to a unit have the same value as the unit
    [Serializable] public struct UnitId : IComponentData { public int Value;}
    //using SCD triggers chunking per unit
    [Serializable] public struct UnitMembership : ISharedComponentData { public int Value;}

}
