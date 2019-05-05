using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct TranslationSpeed : IComponentData{ public float UnitsPerSecond; }

    [Serializable] public struct RotationSpeed : IComponentData {public float RadiansPerSecond; }

    [Serializable] public struct Agent : IComponentData { public Entity Unit; }

    [Serializable] public struct FormationElement : IComponentData { public float4 Position; }

    [Serializable] public struct Goal : IComponentData { 
        public float3 Position;
        public quaternion Rotation;
    }
    
    [Serializable] public struct Unit : IComponentData { }
    
    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Value;}

}
