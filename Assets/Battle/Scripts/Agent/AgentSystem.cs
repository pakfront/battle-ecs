// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Burst;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace UnitAgent
// {
//     public class AgentSystem : JobComponentSystem
//     {

//         private EntityQuery m_Group;

//         protected override void OnCreate()
//         {
//             // Cached access to a set of ComponentData based on a specific query
//             m_Group = GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<TargetPosition>());
//         }

//         // [BurstCompile]
//         // struct AgentRotationJob : IJobForEach<Translation, Rotation>
//         // {
//         //     public float3 target;
//         //     public float dt;

//         //     public void Execute(ref Translation pos, ref Rotation rot)
//         //     {
//         //         float3 heading = target - pos.Value;
//         //         heading.y = 0;
//         //         rot.Value = quaternion.LookRotation(heading, math.up());
//         //     }
//         // }

//         [BurstCompile]
//         struct TargetPositionJob : IJobChunk
//         {
//             public float DeltaTime;
//             public ArchetypeChunkComponentType<Rotation> RotationType;
//             [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
//             [ReadOnly] public ArchetypeChunkSharedComponentType<TargetPosition> TargetPositionType;
    
//             public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//             {
//                 var chunkRotations = chunk.GetNativeArray(RotationType);
//                 var chunkTranslations = chunk.GetNativeArray(TranslationType);

//                 // wrong
//                 // var chunkTargetPosition = chunk.GetChunkComponentData(TargetPositionType);

//                 // can get shadedINdex, but can't figure out how to get at data
//                 // int sharedIndex = chunk.GetSharedComponentIndex(TargetPositionType);
//                 // var chunkTargetPositions = chunk.GetNativeArray(TargetPositionType);                
//                 // float3 target = chunkTargetPositions(sharedIndex);

//                 // requires entity manager
//                 // var chunkTargetPosition = chunk.GetSharedComponentData(TargetPositionType);
//                 // float3 target = chunkTargetPosition.Value;
                
//                 // placeholder until I figure it out
//                 float3 target = new float3 (1,1,1);

//                 for (var i = 0; i < chunk.Count; i++)
//                 {
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
//             // Explicitly declare:
//             // - Read-Write access to Rotation
//             // - Read-Only access to TargetPosition
//             var rotationType = GetArchetypeChunkComponentType<Rotation>(false); 
//             var translationType = GetArchetypeChunkComponentType<Translation>(true);
//             var targetPositionType = GetArchetypeChunkSharedComponentType<TargetPosition>();
            
//             var job = new TargetPositionJob()
//             {
//                 RotationType = rotationType,
//                 TranslationType = translationType,
//                 TargetPositionType = targetPositionType,
//                 DeltaTime = Time.deltaTime
//             };
    
//             return job.Schedule(m_Group, inputDependencies);
//         }

//         // protected override JobHandle OnUpdate(JobHandle inputDeps)
//         // {
//         //     if (Globals.main == null)
//         //         return inputDeps;

//         //     var job = new AgentRotationJob
//         //     {
//         //         target = Globals.main.target,// so what if this varies?
//         //         dt = Time.deltaTime
//         //     };

//         //     return job.Schedule(this, inputDeps);
//         // }
//     }

// }