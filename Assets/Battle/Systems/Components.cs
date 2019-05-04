using System;
using Unity.Entities;

namespace UnitAgent
{
    [Serializable] public struct TranslationSpeed : IComponentData{ public float UnitsPerSecond; }

    [Serializable] public struct RotationSpeed : IComponentData {public float RadiansPerSecond; }

    [Serializable] public struct Agent : IComponentData { }

    [Serializable] public struct Unit : IComponentData { }
    
    [Serializable] public struct UnitId : IComponentData { public int Value;}
    
    [Serializable] public struct UnitMembership : ISharedComponentData { public int Value;}

}
