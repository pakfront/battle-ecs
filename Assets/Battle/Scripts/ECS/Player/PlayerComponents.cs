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
        public int Formation;

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


    [Serializable] public struct PlayerSelection : IComponentData { }
    [Serializable] public struct PlayerTarget : IComponentData { }
    [Serializable] public struct PlayerFollow : IComponentData { }

    [Serializable] public struct PlayerOwned : ISharedComponentData { }
    [Serializable] public struct PlayerAlly : ISharedComponentData { }
    [Serializable] public struct PlayerEnemy : ISharedComponentData { }
}
