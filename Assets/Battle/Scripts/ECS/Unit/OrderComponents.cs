  using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

     [Serializable] public struct OrderedGoal : IComponentData { public float4x4 Value; }
     [Serializable] public struct OrderedFormation : IComponentData {  public int FormationId; }
     [Serializable] public struct OrderedTarget : IComponentData {  public Entity Value; }

  // Order Tags
    [Serializable] public struct OrderMoveToTag : IComponentData { }
    [Serializable] public struct OrderAttackTag : IComponentData { }
    // [Serializable] public struct OrderHold : IComponentData { }
    [Serializable] public struct OrderMarchTag : IComponentData { }
    [Serializable] public struct OrderChangeFormationTag : IComponentData { }
    // [Serializable] public struct OrderFormationMoveTo : IComponentData { public float3 Position; public float3 Heading; }
    [Serializable] public struct OrderUnitGroupMoveToTag : IComponentData { }

}