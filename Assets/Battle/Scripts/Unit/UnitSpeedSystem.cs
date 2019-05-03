// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace UnitAgent

// {
//     // This system updates all entities in the scene with both a UnitSpeed and Rotation component.
//     public class UnitSpeedSystem : JobComponentSystem
//     {
//         private EntityQuery m_Group;

//         protected override void OnCreate()
//         {
//             // Cached access to a set of ComponentData based on a specific query
//             m_Group = GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<UnitSpeed>());
//         }

//         // Use the [BurstCompile] attribute to compile a job with Burst. You may see significant speed ups, so try it!
//         [BurstCompile]
//         struct UnitSpeedJob : IJobChunk
//         {
//             public float DeltaTime;
//             public ArchetypeChunkComponentType<Rotation> RotationType;
//             [ReadOnly] public ArchetypeChunkComponentType<UnitSpeed> UnitSpeedType;
    
//             public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//             {
//                 var chunkRotations = chunk.GetNativeArray(RotationType);
//                 var chunkUnitSpeeds = chunk.GetNativeArray(UnitSpeedType);
//                 for (var i = 0; i < chunk.Count; i++)
//                 {
//                     var rotation = chunkRotations[i];
//                     var rotationSpeed = chunkUnitSpeeds[i];
                    
//                     // Rotate something about its up vector at the speed given by UnitSpeed.
//                     chunkRotations[i] = new Rotation
//                     {
//                         Value = math.mul(math.normalize(rotation.Value),
//                             quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * DeltaTime))
//                     };
//                 }
//             }
//         }
    
//         // OnUpdate runs on the main thread.
//         protected override JobHandle OnUpdate(JobHandle inputDependencies)
//         {
//             // Explicitly declare:
//             // - Read-Write access to Rotation
//             // - Read-Only access to UnitSpeed
//             var rotationType = GetArchetypeChunkComponentType<Rotation>(false); 
//             var rotationSpeedType = GetArchetypeChunkComponentType<UnitSpeed>(true);
            
//             var job = new UnitSpeedJob()
//             {
//                 RotationType = rotationType,
//                 UnitSpeedType = rotationSpeedType,
//                 DeltaTime = Time.deltaTime
//             };
    
//             return job.Schedule(m_Group, inputDependencies);
//         }
//     }
// }
