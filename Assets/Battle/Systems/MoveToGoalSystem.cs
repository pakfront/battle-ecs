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
    public class MoveToGoalSystem : JobComponentSystem
    {
        [BurstCompile]
        struct TowardGoalJob : IJobForEach<Rotation, Translation, Goal>
        {
            public float DeltaTime;

            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref Goal goal)
            {

                float rotateSpeed = goal.RotateSpeed;
                float translateSpeed = goal.TranslateSpeed; 

                float3 toGoal = goal.Position - translation.Value;
                toGoal.y = 0;
                float distance = math.length(toGoal);
                float3 desiredForward;
                bool atGoal = false;
                float moveThisTick = translateSpeed * DeltaTime;

                // if (distance <= moveThisTick)
                if (moveThisTick * 2 > distance)
                {
                    //TODO use goalForward
                    desiredForward = goal.Heading;
                    atGoal = true;
                }
                else {
                    //normalize
                    desiredForward = toGoal/distance;
                }

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

                if (atGoal)
                    translation.Value = goal.Position;
                else
                    translation.Value += moveThisTick * nextHeading;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var rotateTowardGoalJob = new TowardGoalJob()
            {
                DeltaTime = Time.deltaTime
            };

            return rotateTowardGoalJob.Schedule(this, inputDependencies);
        }
    }
}
