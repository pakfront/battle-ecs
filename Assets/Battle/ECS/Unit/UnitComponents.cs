using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct OrderHold : IComponentData {}
    [Serializable] public struct OrderMove : IComponentData {}
    [Serializable] public struct OrderPursue : IComponentData {}

}
