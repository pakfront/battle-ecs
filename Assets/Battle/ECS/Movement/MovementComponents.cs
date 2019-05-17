using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
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
}
