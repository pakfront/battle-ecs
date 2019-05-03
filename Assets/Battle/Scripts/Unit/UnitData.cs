using System;
using Unity.Entities;

namespace UnitAgent
{
    [Serializable] public struct Unit : IComponentData { }
    [Serializable] public struct UnitId : IComponentData { public int Value;}
    [Serializable] public struct UnitMembership : ISharedComponentData { public int Value;}
}

