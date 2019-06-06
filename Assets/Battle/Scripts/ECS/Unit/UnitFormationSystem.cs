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
    [UpdateAfter(typeof(UnitOrderPostSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UnitFormationSystem : JobComponentSystem
    {
        // public static readonly int FormationCount = 4;
        // public static readonly int OffsetsPerFormation = 12;
        // public static readonly int FormationOffsetsLength = FormationCount * OffsetsPerFormation;

        public NativeArray<float3> FormationOffsets;
        public NativeArray<int> FormationTypes;
        protected override void OnCreate()
        {
            
            FormationUtils.CalcUnitFormations(out float3[] formationOffsets, out int[] formationTypes);
            FormationOffsets = new NativeArray<float3>(formationOffsets, Allocator.Persistent);
            FormationTypes = new NativeArray<int>(formationTypes, Allocator.Persistent);
            // FormationTypes = new NativeArray<int>(FormationOffsetsLength, Allocator.Persistent);

            // // these could be read from disk and there could be a lot more than these few variations
            // int f = (int)EFormation.Mob;
            // for (int i = 0; i < OffsetsPerFormation; i++)
            // {
            //     FormationOffsets[f * OffsetsPerFormation + i] = new float3(i * 20, 0, i * 20);
            //     FormationTypes[f * OffsetsPerFormation + i] = (int)EFormation.Line;
            // }

            // f = (int)EFormation.Line;
            // for (int i = 0; i < OffsetsPerFormation; i++)
            // {
            //     FormationUtils.DistributeAcrossColumns(5, i, out int row, out int col);
            //     FormationOffsets[f * OffsetsPerFormation + i] = new float3(col * 80, 0, row * 20);
            //     FormationTypes[f * OffsetsPerFormation + i] = (int)EFormation.Line;
            // }

            // f = (int)EFormation.Column;
            // for (int i = 0; i < OffsetsPerFormation; i++)
            // {
            //     FormationOffsets[f * OffsetsPerFormation + i] = new float3(0, 0, i * -20);
            //     FormationTypes[f * OffsetsPerFormation + i] = (int)EFormation.Column;

            // }

            // f = (int)EFormation.Reserve;
            // for (int i = 0; i < OffsetsPerFormation; i++)
            // {
            //     FormationUtils.DistributeAcrossColumns(5, i, out int row, out int col);
            //     FormationOffsets[f * OffsetsPerFormation + i] = new float3(col * 80, 0, row * 20);
            //     FormationTypes[f * OffsetsPerFormation + i] = (int)EFormation.Column;
            // }
        }


        protected override void OnDestroy()
        {
            FormationOffsets.Dispose();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Unit))]
        struct SetOffsetJob : IJobForEach<FormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<FormationLeader> Leaders;
            [ReadOnly] public NativeArray<float3> Offsets;
            public void Execute(ref FormationMember formationElement)
            {
                Entity parent = formationElement.Parent;
                int formationIndex = Leaders[parent].FormationIndex;
                
                formationElement.PositionOffset = Offsets[formationIndex + formationElement.IndexOffset];
            }
        }

        // TODO run only when parent has moved
        [BurstCompile]
        [RequireComponentTag(typeof(Unit))]
        struct SetGoalJob : IJobForEach<MoveToGoal, FormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref FormationMember formationElement)
            {
                Entity parent = formationElement.Parent;
                float4x4 xform = Others[parent].Value;
                goal.Position = math.transform(xform, formationElement.PositionOffset);
                // heterogenous as it's a direction vector;
                goal.Heading = math.mul(xform, new float4(0, 0, 1, 0)).xyz;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {

            var outputDeps = inputDependencies;
            
            outputDeps = new SetOffsetJob()
            {
                Leaders = GetComponentDataFromEntity<FormationLeader>(true),
                Offsets = FormationOffsets
            }.Schedule(this, outputDeps);

            //if moved or formation changed
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