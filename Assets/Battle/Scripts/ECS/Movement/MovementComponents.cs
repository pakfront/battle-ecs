using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct MoveSettings : IComponentData { 
        public float TranslateSpeed;
        public float RotateSpeed;
    }

    [Serializable] public struct MoveToGoal : IComponentData { 
        public float3 Position;
        public float3 Heading;
    }
    
   [Serializable] public struct RotateToGoal : IComponentData { 
        public float3 Heading;
    }


}
