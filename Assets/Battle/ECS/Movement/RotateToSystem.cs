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
    // cribbed from 
    // https://forum.unity.com/threads/how-do-you-get-a-bufferfromentity-or-componentdatafromentity-without-inject.587857/#post-3924478
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class RotateToSystem : JobComponentSystem
    {

        [BurstCompile]
        struct RotateToJob : IJobForEach<Rotation, Translation, Move, GoalRotateTo>
        {
            public float DeltaTime;

            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref Move move, [ReadOnly] ref GoalRotateTo goal)
            {

                float rotateSpeed = move.RotateSpeed;

                float3 desiredForward = goal.Heading;
                float3 forward = math.mul(rotation.Value, new Vector3 (0,0,1) );
                float3 nextHeading;
                if ( math.dot(desiredForward,forward) > .98)
                {
                    // close enough, snap
                    nextHeading = desiredForward;
                }
                else
                {
                    nextHeading = math.normalizesafe(forward + rotateSpeed * DeltaTime * (desiredForward-forward));
                }
                rotation.Value = quaternion.LookRotation(nextHeading, math.up());;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var goalMoveToJob = new RotateToJob()
            {
                DeltaTime = Time.deltaTime
            };
            return goalMoveToJob.Schedule(this, inputDependencies);
        }
    }
}
