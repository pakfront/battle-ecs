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
    [UpdateInGroup(typeof(MovementSystemGroup))]
    public class MovementSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(MoveToGoalTag))]
        [ExcludeComponent(typeof(DeadTag))]
        struct MoveToGoalJob : IJobForEach<Rotation, Translation, MoveSettings, Goal>
        {
            public float DeltaTime;
            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref MoveSettings move, [ReadOnly] ref Goal goal)
            {

                float rotateSpeed = move.RotateSpeed;
                float translateSpeed = move.TranslateSpeed; 

                float3 toGoal = goal.Position - translation.Value;
                toGoal.y = 0;
                float distance = math.length(toGoal);
                float3 desiredForward;
                bool atGoal = false;
                float moveThisTick = translateSpeed * DeltaTime;

                // TODO prevent branching
                if (moveThisTick * 2 > distance)
                {
                    desiredForward = goal.Heading;
                    atGoal = true;
                }
                else if (moveThisTick * 8 > distance)
                {
                    desiredForward = toGoal/distance;
                    moveThisTick /= 2f;
                    rotateSpeed *= 4;
                }
                else {
                    //normalize
                    desiredForward = toGoal/distance;
                }

                quaternion desired = quaternion.LookRotation(desiredForward, math.up());
                quaternion current = rotation.Value;
                Movement.RotateTowards(desired, rotateSpeed*DeltaTime, ref current);
                rotation.Value = current;
            
                if (atGoal)
                    //TODO could just switch to rotate to if goal is no moving
                    translation.Value = goal.Position;
                else
                    translation.Value += moveThisTick * math.mul(rotation.Value, new Vector3 (0,0,1) );
;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(RotateLikeGoalTag))]
        [ExcludeComponent(typeof(DeadTag))]
        struct RotateLikeGoalJob : IJobForEach<Rotation, Translation, MoveSettings, Goal>
        {
            public float DeltaTime;

            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref MoveSettings move, [ReadOnly] ref Goal goal)
            {
                float rotateSpeed = move.RotateSpeed;

                quaternion desired = quaternion.LookRotation(goal.Heading, math.up());
                quaternion current = rotation.Value;
                Movement.RotateTowards(desired, rotateSpeed*DeltaTime, ref current);
                rotation.Value = current;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var outputDeps = new MoveToGoalJob()
            {
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDependencies);

            outputDeps = new RotateLikeGoalJob()
            {
                DeltaTime = Time.deltaTime
            }.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}
