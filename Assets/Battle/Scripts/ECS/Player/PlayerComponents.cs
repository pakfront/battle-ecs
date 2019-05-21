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
        Terrain = 2,
        Primary = 4,
        Secondary = 8
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
