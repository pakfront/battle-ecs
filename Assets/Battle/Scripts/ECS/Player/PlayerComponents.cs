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
        AnyModifier = Shift | Ctrl,
        AnyPointerButton = PrimaryPointerButton | SecondaryPointerButton
    }
    [Serializable]
    public struct PlayerPointer : IComponentData
    {
        public float3 Position;
        public uint Click;
        public Entity Entity;
    }

    [Serializable] public struct PlayerSelection : IComponentData { }
    [Serializable] public struct PlayerTarget : IComponentData { }
    [Serializable] public struct PlayerFollow : IComponentData { }

    [Serializable] public struct PlayerOwned : ISharedComponentData { }
    [Serializable] public struct PlayerAlly : ISharedComponentData { }
    [Serializable] public struct PlayerEnemy : ISharedComponentData { }
}
