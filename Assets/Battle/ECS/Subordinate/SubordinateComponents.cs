using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct FormationElement : IComponentData { public float4 Position; }

    // [Serializable] public struct Unit : IComponentData { }

    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Value;}
}
