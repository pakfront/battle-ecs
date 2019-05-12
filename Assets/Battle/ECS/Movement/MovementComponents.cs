using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    // [Serializable] public struct Agent : IComponentData { public Entity Unit; }

    [Serializable] public struct FormationElement : IComponentData { public float4 Position; }
    [Serializable] public struct Move : IComponentData { 
        public float TranslateSpeed;
        public float RotateSpeed;
    }



    [Serializable] public struct GoalMoveTo : IComponentData { 
        public float3 Position;
        public float3 Heading;
    }
    

   [Serializable] public struct GoalRotateTo : IComponentData { 
        public float3 Heading;
    }

    // [Serializable] public struct Unit : IComponentData { }
    
    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Value;}

}
