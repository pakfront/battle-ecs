using System;
using Unity.Entities;

namespace UnitAgent
{
    [Serializable] public struct TranslationSpeed : IComponentData{ public float UnitsPerSecond; }

    [Serializable] public struct RotationSpeed : IComponentData {public float RadiansPerSecond; }

    [Serializable] public struct Agent : IComponentData { public Entity Unit; }

    [Serializable] public struct Unit : IComponentData { }
    
    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Value;}

}
