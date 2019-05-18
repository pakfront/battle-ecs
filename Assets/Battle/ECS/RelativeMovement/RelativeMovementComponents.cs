using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct FormationElement : IComponentData { public float4 Position; }


    // [Serializable] public struct Follow : IComponentData {
    //     public Entity Target;
    //     public float4 Offset;
    // }
}
