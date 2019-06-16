using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct Unit : IComponentData { }

    [Serializable] public struct UnitGoalMarker : IComponentData { public Entity Unit; }

    // [Serializable] public struct TeamMember : IComponentData { public int Value; }
    [Serializable] public struct TeamGroup : ISharedComponentData { public int Value; }

    // Types
    public enum EUnitType { Foot, Horse, Artillery, Train, HQ }
    [Serializable] public struct Foot : IComponentData { }
    [Serializable] public struct Horse : IComponentData { }
    [Serializable] public struct Artillery : IComponentData { }
    [Serializable] public struct Train : IComponentData { }
    [Serializable] public struct HQ : IComponentData { }
    [Serializable] public struct Ranged : IComponentData {
        public float Range;
     }
    [Serializable] public struct Melee : IComponentData { }

}
