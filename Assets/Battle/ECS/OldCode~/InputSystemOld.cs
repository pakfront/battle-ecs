// using Unity.Burst;
// using UnityEngine;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Collections;

// namespace UnitAgent
// {
//     // use ComponentSystem so we can create entities
//     public class InputSystem : JobComponentSystem
//     {
//         private Plane groundplane = new Plane(Vector3.up, 0);

//         private EntityQuery m_Group;

//         private EntityCommandBufferSystem m_EntityCommandBufferSystem;

//         protected override void OnCreate()
//         {
//             // Cached access to a set of ComponentData based on a specific query
//             m_Group = GetEntityQuery( ComponentType.ReadOnly<PlayerSelected>() );

//             // Cache the EndSimulationBarrier in a field, so we don't have to create it every frame
//             m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
//         }


//         [RequireComponentTag(typeof(PlayerSelected))]
//         [BurstCompile]
//         struct SetOrAddGoalJob : IJobChunk
//         {
//             [ReadOnly] public ArchetypeChunkEntityType EntityType;
//             [ReadOnly] public ArchetypeChunkComponentType<GoalMoveTo> GoalMoveToType;
//             [ReadOnly] public EntityCommandBuffer CommandBuffer;
//             [ReadOnly] public float3 HitPoint;

//             public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//             {

//                 // var entities = chunk.GetNativeArray<EntityType>();
//                 // if (chunk.Has(GoalMoveToType))
//                 // {
//                 //     for (var i = 0; i < chunk.Count; i++)
//                 //     {
//                 //         CommandBuffer.SetComponent(entities[i], new GoalMoveTo { Position = HitPoint + new float3(i,0,chunkIndex) });
//                 //     }
//                 // } 
//                 // else
//                 // {
//                 //     for (var i = 0; i < chunk.Count; i++)
//                 //     {
//                 //         CommandBuffer.AddComponent(entities[i], new GoalMoveTo { Position = HitPoint + new float3(i,0,chunkIndex) });
//                 //     }

//                 // }
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             if (!Input.GetMouseButtonDown(1)) return inputDeps;

//             //Create a ray from the Mouse click position
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//             float enter = 0.0f;

//             if (!groundplane.Raycast(ray, out enter)) return inputDeps;

//             Vector3 hitPoint = ray.GetPoint(enter);

//             var job = new SetOrAddGoalJob
//             {
//                 EntityType = GetArchetypeChunkEntityType (),
//                 GoalMoveToType = GetArchetypeChunkComponentType<GoalMoveTo>(),
//                 CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
//                 HitPoint = (float3)hitPoint
//             };
            
//             job.Schedule(m_Group, inputDeps);

//             m_EntityCommandBufferSystem.AddJobHandleForProducer(job);
//             return job;
//         }
//     }
// }