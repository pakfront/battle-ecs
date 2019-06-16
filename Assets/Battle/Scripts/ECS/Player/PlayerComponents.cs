using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [FlagsAttribute]
    public enum EClick : uint
    {
        None = 0,
        PlayerSelectable = 1,
        Terrain = 1 << 2,
        PrimaryPointerButton = 1 << 3,
        SecondaryPointerButton = 1 << 4,
        Shift = 1 << 5,
        Ctrl = 1 << 6,

        // aliases
        AnyModifier = Shift | Ctrl,
        AnyPointerButton = PrimaryPointerButton | SecondaryPointerButton,
        MoveTo = SecondaryPointerButton | Terrain,
        Attack = SecondaryPointerButton | PlayerSelectable,
        // FormationMoveTo = MoveTo | Ctrl,
        // FormationAttack = Attack | Ctrl
    }



    [Serializable]
    public struct PlayerInput : IComponentData
    {
        public float3 WorldHitPosition, MousePosition;
        public uint Click;
        public int FormationId;

        public Entity CurrentEntity;
    }




    [Serializable]
    public struct PlayerSelectable : IComponentData
    {
        public float3 min;
        public float3 max;
        public float3 center;
        public float3 halfwidth;
    }


    [Serializable] public struct PlayerSelectionTag : IComponentData { }
    [Serializable] public struct PlayerTargetTag : IComponentData { }
    [Serializable] public struct PlayerFollowTag : IComponentData { }

    [Serializable] public struct PlayerOwnedTag : ISharedComponentData { }
    [Serializable] public struct PlayerAllyTag : ISharedComponentData { }
    [Serializable] public struct PlayerEnemyTag : ISharedComponentData { }
}
