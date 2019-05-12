using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct UnitInFormation : IComponentData {}
    [Serializable] public struct UnitHoldPosition : IComponentData {}
    [Serializable] public struct UnitMoveToPosition : IComponentData {}
    [Serializable] public struct UnitFollowUnit : IComponentData {}
    [Serializable] public struct UnitPursueUnit : IComponentData {}

}
