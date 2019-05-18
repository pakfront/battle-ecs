using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    // [Serializable] public struct UnitInFormation : IComponentData {}
    // [Serializable] public struct UnitMoveToPosition : IComponentData {}
    // [Serializable] public struct UnitFollow : IComponentData { }
    // [Serializable] public struct UnitPursueAny : IComponentData {}
    // [Serializable] public struct UnitPursueUnit : IComponentData {}

    [Serializable] public struct OrderMoveTo : IComponentData { public float3 Position; }
    [Serializable] public struct OrderPursue : IComponentData { public Entity Target;}
    [Serializable] public struct OrderHold : IComponentData {}
    [Serializable] public struct OrderMarch : IComponentData { public Entity Next; }
    [Serializable] public struct OrderFormation : IComponentData {}

    [Serializable] public struct Detached : IComponentData {}

}
