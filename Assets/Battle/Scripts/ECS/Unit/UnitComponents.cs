using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct Unit : IComponentData {
        public int Team;
    }

    [Serializable] public struct UnitGoalMarker : IComponentData { public Entity Unit; }

    // [Serializable] public struct TeamMember : IComponentData { public int Value; }
    [Serializable] public struct TeamGroup : ISharedComponentData { public int Value; }

    // Formations
    [Serializable] public struct FormationGroup : ISharedComponentData { public Entity Parent; }

    [Serializable] public struct FormationLeader : IComponentData {
        public int FormationTable;
        public int CurrentFormation;
        public int FormationStartIndex;
     }
    [Serializable]
    public struct FormationMember : IComponentData
    {
        public int IndexOffset;
        public Entity Parent;
        public float3 PositionOffset;
    }

    [Serializable] public struct Detached : IComponentData { }

    // Orders
    [Serializable] public struct OrderMoveTo : IComponentData { public float3 Position; public float3 Heading; }
    [Serializable] public struct OrderAttack : IComponentData { public Entity Target; }
    // [Serializable] public struct OrderHold : IComponentData { }
    [Serializable] public struct OrderMarch : IComponentData { public Entity Next; }
    // [Serializable] public struct OrderFormation : IComponentData { }

    [Serializable] public struct OrderFormationMoveTo : IComponentData { public float3 Position; public float3 Heading; }

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
