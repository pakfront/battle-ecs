using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct Agent : IComponentData { }

    [Serializable]
    public struct AgentFormationMember : IComponentData
    {
        public int Index;
        public Entity Parent;
        public float3 Offset;
    }

    //using SCD triggers chunking per unit and chunk data lookup



    // [Serializable] public struct Follow : IComponentData {
    //     public Entity Target;
    //     public float4 Offset;
    // }
}
