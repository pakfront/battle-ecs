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
        Primary = 1<<3,
        Secondary = 1<<4,
        Shift = 1<<5,
        Ctrl = 1<<6,
        AnyModifier = Shift | Ctrl,
        AnyButton = Primary | Secondary
    }
    [Serializable]
    public struct PlayerPointer : IComponentData
    {
        public float3 Position;
        public uint Click;
    }

    [Serializable] public struct PlayerSelection : IComponentData { }
    [Serializable] public struct PlayerTarget : IComponentData { }
    [Serializable] public struct PlayerFollow : IComponentData { }

    [Serializable] public struct PlayerOwned : ISharedComponentData { }
    [Serializable] public struct PlayerAlly : ISharedComponentData { }
    [Serializable] public struct PlayerEnemy : ISharedComponentData { }
}
