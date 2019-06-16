  using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    
  // Orders
    [Serializable] public struct OrderMoveTo : IComponentData { public float4x4 Goal; }
    [Serializable] public struct OrderAttack : IComponentData { public Entity Target; }
    // [Serializable] public struct OrderHold : IComponentData { }
    [Serializable] public struct OrderMarch : IComponentData { public Entity Next; }
    [Serializable] public struct OrderChangeFormation : IComponentData { public int FormationId; }
    // [Serializable] public struct OrderFormationMoveTo : IComponentData { public float3 Position; public float3 Heading; }
    [Serializable] public struct OrderUnitGroupMoveTo : IComponentData { public float4x4 Goal; }

}