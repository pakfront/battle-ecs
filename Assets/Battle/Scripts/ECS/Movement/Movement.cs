using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent
{

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

        public static void SetGoalToFormationPosition(float4x4 xform, float3 offset, ref float3 goalPosition, ref float3 goalHeading )
        {
                goalPosition = math.transform(xform, offset);
                // heterogenous as it's a direction vector;
                goalHeading = math.mul( xform, new float4(0,0,1,0)).xyz;
        }
    }
}