using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{


    
    [Serializable] public struct MoveSettings : IComponentData { 
        public float TranslateSpeed;
        public float RotateSpeed;
    }

    [Serializable] public struct Goal : IComponentData { 
        public float4x4 Value;

        public float3 Position {
            get { return math.transform(Value, float3.zero);}
        }

        public float3 Heading {
            get {
                return math.mul( Value, new float4(0,0,1,0)).xyz;
            }
        }
    }
    [Serializable] public struct MoveToGoal : IComponentData { 
        // public float3 Position;
        // public float3 Heading;
    }
    
   [Serializable] public struct RotateLikeGoal : IComponentData { 
        // public float3 Heading;
    }


}
