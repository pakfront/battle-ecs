using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct Unit : IComponentData { }

    [Serializable] public struct Team : ISharedComponentData { public int Value; }

    // Formations
    [Serializable] public struct FormationGroup : ISharedComponentData { public Entity Parent; }

    [Serializable] public struct FormationLeader : IComponentData {
        public int FormationIndex;
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
    [Serializable] public struct OrderHold : IComponentData { }
    [Serializable] public struct OrderMarch : IComponentData { public Entity Next; }
    // [Serializable] public struct OrderFormation : IComponentData { }

    [Serializable] public struct OrderFormationMoveTo : IComponentData { public float3 Position; public float3 Heading; }

}
