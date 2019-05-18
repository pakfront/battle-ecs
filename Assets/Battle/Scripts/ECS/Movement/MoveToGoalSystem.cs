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
        struct MoveToGoalJob : IJobForEach<Rotation, Translation, MoveSettings, MoveToGoal>
        {
            public float DeltaTime;
            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref MoveSettings move, [ReadOnly] ref MoveToGoal goal)
            {

                float rotateSpeed = move.RotateSpeed;
                float translateSpeed = move.TranslateSpeed; 

                float3 toGoal = goal.Position - translation.Value;
                toGoal.y = 0;
                float distance = math.length(toGoal);
                float3 desiredForward;
                bool atGoal = false;
                float moveThisTick = translateSpeed * DeltaTime;

                // if (distance <= moveThisTick)
                if (moveThisTick * 2 > distance)
                {
                    desiredForward = goal.Heading;
                    atGoal = true;
                }
                else if (moveThisTick * 4 > distance)
                {
                    desiredForward = toGoal/distance;
                    distance /= 2;
                    rotateSpeed *= 2;
                }
                else {
                    //normalize
                    desiredForward = toGoal/distance;
                }

                // float3 forward = math.mul(rotation.Value, new Vector3 (0,0,1) );
                // quaternion desiredRotation = quaternion.LookRotation(goal.Heading, math.up());
                // if ( math.dot(desiredForward,forward) > .98)
                // {
                //     // close enough, snap
                //     rotation.Value = desiredRotation;
                //     return;
                // }
                // else
                // {
                //     rotation.Value = math.slerp(rotation.Value, desiredRotation, 100 * rotateSpeed * DeltaTime);
                // }
                
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
                    //TODO could just switch to rotate to if goal is no moving
                    translation.Value = goal.Position;
                else
                    translation.Value += moveThisTick * math.mul(rotation.Value, new Vector3 (0,0,1) );
;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var goalMoveToJob = new MoveToGoalJob()
            {
                DeltaTime = Time.deltaTime
            };

            return goalMoveToJob.Schedule(this, inputDependencies);
        }
    }
}
