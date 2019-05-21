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

    public static class Movement
    {
        /// @return true if snapped
        public static bool RotateTowards(quaternion desired, float delta, ref quaternion current)
        {
            if ( math.dot(current,desired) > .98)
            {
                current = desired;
                return true;
            }

            current = math.slerp(current, desired, delta);
            return false;
        }
    }
}
