using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct Agent : IComponentData { }

    [Serializable]
    public struct AgentGroupPartition : ISharedComponentData
    {
        public Entity Parent;
    }

    [Serializable]
    public struct AgentGroupMember : IComponentData
    {
        public int Index;
        public Entity Parent;
        // public float3 Offset; //debug
    }

    [Serializable]
    public struct AgentGroupLeader : IComponentData
    {
        public int FormationTableId;
        public int FormationId;
        public int FormationStartIndex;

        // could be 3x3 but convience funcs missing
        // public float4x4 Goal;
    }
}
