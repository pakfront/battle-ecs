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
    [UpdateBefore(typeof(TranslationSpeedSystem))]
    public class AgentSystem : JobComponentSystem
    {

        [BurstCompile]
        struct SetGoalJob : IJobForEach<Goal, Agent, FormationElement>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Units;
            public void Execute(ref Goal goal, [ReadOnly] ref Agent agent, [ReadOnly] ref FormationElement formationElement)
            {
                Entity e = agent.Unit;
                float4x4 xform = Units[e].Value;
                goal.Position = math.mul (xform, formationElement.Position).xyz;
                // goal.Position = math.mul (xform, new float4 (0,0,0,1f)).xyz;
                //TODO set goal rotation from formation too
            }
        }

        // [BurstCompile]
        // struct RotationJob : IJobForEach<Rotation, Translation, Goal >
        // {
        //     public void Execute(ref Rotation rotation, [ReadOnly] ref Translation translation, [ReadOnly] ref Goal goal)
        //     {
        //         float3 heading = goal.Value - translation.Value;
        //         heading.y = 0;
        //         rotation.Value = quaternion.LookRotation(heading, math.up());                   
        //     }
        // }

        // [BurstCompile]
        // LocalToWorld not working the way i think it does
        // [RequireComponentTag(typeof(Agent))]
        // struct MoveTowardGoalJob : IJobForEach<LocalToWorld, Goal>
        // {
        //     public float DeltaTime;

        //     public void Execute(ref LocalToWorld localToWorld, [ReadOnly] ref Goal goal)
        //     {
        //         var currentPosition                   = localToWorld.Position;

        //         float moveSpeed = .2f;
        //         float3 nextHeading = goal.Position - currentPosition;
        //         nextHeading.y = 0;
        //         nextHeading = math.normalizesafe(nextHeading);

        //         localToWorld = new LocalToWorld
        //         {
        //             Value = float4x4.TRS(
        //                 currentPosition + nextHeading * moveSpeed * DeltaTime,
        //                 quaternion.identity,
        //                 new float3(1.0f, 1.0f, 1.0f))
        //         };                  
        //     }
        // }

        [BurstCompile]
        [RequireComponentTag(typeof(Agent))]
        struct TowardGoalJob : IJobForEach<Rotation, Translation, Goal>
        {
            public float DeltaTime;

            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref Goal goal)
            {

                float turnSpeed = .8f;
                float moveSpeed = .2f; 

                float3 toGoal = goal.Position - translation.Value;
                toGoal.y = 0;
                float distance = math.length(toGoal);
                float3 desiredForward;
                bool atGoal = false;
                float moveThisTick = moveSpeed * DeltaTime;

                // if (distance <= moveThisTick)
                if (moveThisTick * 2 > distance)
                {
                    //TODO use goalForward
                    desiredForward = new float3(0,0,1);
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
                    nextHeading = math.normalizesafe(forward + turnSpeed * DeltaTime * (desiredForward-forward));
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
            var setGoalJob = new SetGoalJob()
            {
                Units = GetComponentDataFromEntity<LocalToWorld>(true)
            };

            var setGoalJobHandle = setGoalJob.Schedule(this, inputDependencies);

            var rotateTowardGoalJob = new TowardGoalJob()
            {
                DeltaTime = Time.deltaTime
            };

            return rotateTowardGoalJob.Schedule(this, setGoalJobHandle);
        }
    }
}

// namespace UnitAgent
// {
//     public class AgentSystem : JobComponentSystem
//     {

//         private EntityQuery m_UnitGroup, m_AgentGroup;
//         private Unity.Collections.NativeHashMap<int, float3> targets;

//         protected override void OnCreate()
//         {
//             // Cached access to a set of ComponentData based on a specific query
//             m_UnitGroup = GetEntityQuery(
//                 ComponentType.ReadOnly<Unit>(),
//                 ComponentType.ReadOnly<UnitId>(),
//                 ComponentType.ReadOnly<Translation>());

//             m_AgentGroup = GetEntityQuery(
//                 typeof(Rotation),
//                 ComponentType.ReadOnly<Agent>(),
//                 ComponentType.ReadOnly<UnitId>()
//                 );
//         }


//         /// <summary>
//         /// Put each Unit's position in a common table
//         /// </summary>
//         [RequireComponentTag(typeof(Unit))]
//         [BurstCompile]
//         struct TargetJob : IJobForEach<UnitId,Translation>
//         {
//             [WriteOnly] public NativeHashMap<int, float3>.Concurrent targets;

//             public void Execute([ReadOnly] ref UnitId unitId, [ReadOnly] ref Translation translation)
//             {
//                 // can't remove from Concurrent
//                 // targets.Remove(unitId.Value);
//                 if (! targets.TryAdd(unitId.Value,translation.Value))
//                 {
//                     Debug.LogError("Failed to add value for key "+unitId.Value);
//                 }
//             }
//         }

//         // lookup each agents unit position and look at it
//         [RequireComponentTag(typeof(Agent))]
//         [BurstCompile]
//         struct AgentRotationJob : IJobChunk
//         {
//             public float DeltaTime;
//             public ArchetypeChunkComponentType<Rotation> RotationType;
//             [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
//             [ReadOnly] public ArchetypeChunkComponentType<UnitId> UnitIdType;
//             [ReadOnly] public NativeHashMap<int, float3> targets;

//             public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//             {
//                 var chunkRotations = chunk.GetNativeArray(RotationType);
//                 var chunkTranslations = chunk.GetNativeArray(TranslationType);
//                 var chunkUnitIds = chunk.GetNativeArray(UnitIdType);

//                 for (var i = 0; i < chunk.Count; i++)
//                 {
//                     // float3 target = new float3(0,0,0);
//                     float3 target = targets[ chunkUnitIds[i].Value ];
//                     var rotation = chunkRotations[i];
//                     var pos = chunkTranslations[i];

//                     float3 heading = target - pos.Value;
//                     heading.y = 0;
//                     chunkRotations[i] = new Rotation
//                     {
//                         Value = quaternion.LookRotation(heading, math.up())                   
//                     };
//                 }
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDependencies)
//         {
//             var rotationType = GetArchetypeChunkComponentType<Rotation>(false); 
//             var translationType = GetArchetypeChunkComponentType<Translation>(true);
//             var unitIdType = GetArchetypeChunkComponentType<UnitId>(true);

//             // Need to clear so we can write new values this update
//             targets.Clear();
//             var targetJob = new TargetJob()
//             {
//                 targets = targets.ToConcurrent()
//             };

//             var targetJobHandle = targetJob.Schedule(m_UnitGroup, inputDependencies);

//             var agentRotationJob = new AgentRotationJob()
//             {
//                 RotationType = rotationType,
//                 TranslationType = translationType,
//                 UnitIdType = unitIdType,
//                 targets = targets,
//                 DeltaTime = Time.deltaTime
//             };

//             return agentRotationJob.Schedule(m_AgentGroup, targetJobHandle);
//         }

//         protected override void OnStartRunning()
//         {
//             int unitCount = m_UnitGroup.CalculateLength();
//             Debug.Log("Creating NativeHashMap "+unitCount);
//             targets = new NativeHashMap<int,float3>(24, Allocator.Persistent);
//         }
//         protected override void OnStopRunning()
//         {
//             targets.Dispose();
//         }

//     }

// }