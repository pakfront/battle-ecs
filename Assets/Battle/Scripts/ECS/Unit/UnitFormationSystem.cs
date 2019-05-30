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
    [UpdateAfter(typeof(UnitGoalSystem))]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public class UnitFormationSystem : JobComponentSystem
    {

        protected override void OnCreate() {
            var formationSingletonEntity = EntityManager.CreateEntity(typeof(FormationSingleton));
            EntityManager.SetName(formationSingletonEntity, "FormationSingleton");
            SetSingleton(new FormationSingleton{ });

            EntityManager.AddBuffer<FormationOffsets>(formationSingletonEntity);
            var buffer = EntityManager.GetBuffer<FormationOffsets>(formationSingletonEntity);
            for (int i = 0; i < 8; i++)
            {
                // temp data for testing
                buffer.Add(new float3(i*20,0,i*100));
            }
        }

        // TODO run only when unit has moved
        [BurstCompile]
        [RequireComponentTag(typeof(Unit))]
        struct SetGoalJob : IJobForEach<MoveToGoal, FormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref FormationMember formationElement)
            {
                Entity parent = formationElement.Parent;
                float4x4 xform = Others[parent].Value;
                goal.Position = math.transform(xform, formationElement.Position);
                // heterogenous as it's a direction vector;
                goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Unit))]
        struct SetOffsetJob : IJobForEach<FormationMember>
        {
            [ReadOnly] public DynamicBuffer<FormationOffsets> Offsets;
            public void Execute(ref FormationMember formationElement)
            {
                formationElement.Position = Offsets[formationElement.Index];
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {

            var formationSingletonEntity = GetSingletonEntity<FormationSingleton>();
            // this also works, but may not have readonly flag:
            // var buffer = EntityManager.GetBuffer<FormationOffsets>(formationSingletonEntity);

            var lookup = GetBufferFromEntity<FormationOffsets>(true);
            var buffer = lookup[formationSingletonEntity];
            
            var outputDeps = new SetOffsetJob()
            {
                Offsets = buffer
            }.Schedule(this, inputDependencies);

            outputDeps = new SetGoalJob()
            {
                Others = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(this, outputDeps);

            return outputDeps;
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