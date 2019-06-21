using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable]
    public struct UnitGroup : ISharedComponentData
    {
        public Entity Parent;
    }

    [Serializable]
    public struct Rank : ISharedComponentData
    {
        public static readonly byte MaxRank = 8;
        // Rank 0 is root, higher numbers mean greater depth
        public byte Value;
    }

    [Serializable]
    public struct UnitGroupLeader : IComponentData
    {
        public int FormationTableId;
        public int FormationId;
        public int FormationStartIndex;

        // could be 3x3 but convience funcs missing
        public float4x4 Goal;
    }


     //used element in buffer list on group leader
    [Serializable]
    [InternalBufferCapacity(8)]
    public struct UnitGroupChildren :  ISystemStateBufferElementData
    {
        public Entity Value;
    }

    //used  on children
    [Serializable]
    public struct UnitGroupMember : IComponentData
    {
        public int MemberIndex;
        // public int FormationTableIndex;//used for debug only

        public Entity Parent;
        public float3 PositionOffset;

        // these maybe should be copied into an AgentGroupLeader for agent stuff
        // public int FormationId; //corresponds to EFormation
        // public int FormationTableId; //offset into compound table
    }

    [Serializable] public struct DetachedTag : IComponentData { }


}