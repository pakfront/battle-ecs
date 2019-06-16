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
        public static readonly float3 forward = new float3(0,0,1);
        public static readonly float3 up = new float3(0,1,0);
        public static readonly float3 unitScale = new float3(1,1,1);
        public static quaternion forwardRotation = quaternion.LookRotationSafe(forward, up);
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

        public static void SetGoalToFormationPosition(float4x4 leaderXform, float3 offset, ref float4x4 goal )
        {
            
            goal = math.mul(
                leaderXform,
                float4x4.TRS(
                    offset,
                    forwardRotation,
                    unitScale
                )
            );
        }

        public static float4x4 CalcGoal(float3 position, float3 heading)
        {
            return float4x4.TRS(position, quaternion.LookRotationSafe(heading, up), unitScale);
        } 

         public static void SetTranslation(float3 translation, ref float4x4 current)
        {
             current.c3 = new float4(translation.x, translation.y, translation.z, 1);
             
            //return float4x4.TRS(position, quaternion.LookRotationSafe(heading, up), unitScale);
        } 

        public static float4x4 CalcGoalPositionOnly(float3 position)
        {
            return float4x4.TRS(position, forwardRotation, unitScale);
        } 

        public static float4x4 CalcGoalHeadingOnly(float3 heading)
        {
            return float4x4.TRS(float3.zero,  quaternion.LookRotationSafe(heading, up), unitScale);
        } 

                // public static void SetGoalToFormationPosition(float4x4 xform, float3 offset, ref float3 goalPosition, ref float3 goalHeading )
        // {
        //         goalPosition = math.transform(xform, offset);
        //         // heterogenous as it's a direction vector;
        //         goalHeading = math.mul( xform, new float4(0,0,1,0)).xyz;
        // }
    }
}