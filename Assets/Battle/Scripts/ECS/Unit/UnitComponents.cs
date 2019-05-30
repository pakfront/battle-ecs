using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct Unit : IComponentData { }

    [Serializable] public struct Team : ISharedComponentData { public int Value; }

    [Serializable] public struct FormationGroup : ISharedComponentData { public Entity Parent; }

    [Serializable] public struct FormationMarker : IComponentData { }
    [Serializable]
    public struct FormationMember : IComponentData
    {
        public int Index;
        public Entity Parent;
        public float3 Position;
    }

    // This describes the number of buffer elements that should be reserved
    // in chunk data for each instance of a buffer. In this case, 8 integers
    // will be reserved (32 bytes) along with the size of the buffer header
    // (currently 16 bytes on 64-bit targets)
    [InternalBufferCapacity(8)]
    public struct FormationOffsets : IBufferElementData
    {
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator float3(FormationOffsets e) { return e.Value; }
        public static implicit operator FormationOffsets(float3 e) { return new FormationOffsets { Value = e }; }
        // Actual value each buffer element will store.
        public float3 Value;
    }

    [Serializable] public struct FormationSingleton : IComponentData { }


    [Serializable] public struct Detached : IComponentData { }

    // Orders
    [Serializable] public struct OrderMoveTo : IComponentData { public float3 Position; public float3 Heading; }
    [Serializable] public struct OrderAttack : IComponentData { public Entity Target; }
    [Serializable] public struct OrderHold : IComponentData { }
    [Serializable] public struct OrderMarch : IComponentData { public Entity Next; }
    // [Serializable] public struct OrderFormation : IComponentData { }

    [Serializable] public struct OrderFormationMoveTo : IComponentData { public float3 Position; public float3 Heading; }

}
