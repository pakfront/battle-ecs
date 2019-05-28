using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct FormationGroup : ISharedComponentData { public Entity Parent; }   

    [Serializable]
    public struct FormationMember : IComponentData
    {
        public int Index;
        public Entity Parent;
        public float3 Position;
    }

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

            // public static void SetGoalToFormationPosition(float4x4 xform, [REa])
            // {
            //     Entity superior = formationElement.Parent;
            //     float4x4 xform = Others[superior].Value;
            //     math.transform(xform, ormationElement.Position);
            //     goal.Position = math.mul (xform, formationElement.Position).xyz;
            //     // heterogenous as it's a direction vector;
            //     goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
            // }
    }
}
