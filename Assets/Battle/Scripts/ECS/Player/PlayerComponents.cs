using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [FlagsAttribute]
    public enum EClick : uint
    {
        None = 0,
        AABB = 1,
        Terrain = 1<<2,
        PrimaryPointerButton = 1<<3,
        SecondaryPointerButton = 1<<4,
        Shift = 1<<5,
        Ctrl = 1<<6,

        // aliases
        AnyModifier = Shift | Ctrl,
        AnyPointerButton = PrimaryPointerButton | SecondaryPointerButton,
        MoveTo = SecondaryPointerButton | Terrain,
        Attack =  SecondaryPointerButton | AABB,
        FormationMoveTo = MoveTo | Ctrl,
        FormationAttack = Attack | Ctrl
    }

    public enum EFormation : int
    {
        None = -1,
        
        Line = 0,
        Column = 1,
        Reserve = 2,
        Mob = 3
    }

    [Serializable]
    public struct PlayerPointer : IComponentData
    {
        public float3 WorldHitPosition, MousePosition;
        public uint Click;
        public int Formation;

        public Entity CurrentEntity;
    }





    [Serializable] public struct PlayerSelection : IComponentData { }
    [Serializable] public struct PlayerTarget : IComponentData { }
    [Serializable] public struct PlayerFollow : IComponentData { }

    [Serializable] public struct PlayerOwned : ISharedComponentData { }
    [Serializable] public struct PlayerAlly : ISharedComponentData { }
    [Serializable] public struct PlayerEnemy : ISharedComponentData { }
}
