using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    public class PlayerUnitSelectSystem : JobComponentSystem
    {
        // EndSimulationBarrier is used to create a command buffer 
        // which will then be played back when that barrier system executes.
        EntityCommandBufferSystem m_EntityCommandBufferSystem;

        // protected override void OnCreate()
        // {
        //     // Cache the EndSimulationBarrier in a field, so we don't have to create it every frame
        //     m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        // }

        [RequireComponentTag(typeof(Unit))]
        struct PlayerUnitSelectJob : IJobForEachWithEntity<AABB>
        {

            // [ReadOnly] public EntityCommandBuffer CommandBuffer;

            // [ReadOnly] public ComponentDataFromEntity<PlayerUnitSelect> Selected;
            public Ray ray;
            public bool LeftClick;

            public void Execute
                (Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (LeftClick)
                {
                    if (RTSPhysics.Intersect(aabb, ray))
                    {
                        Debug.Log("Click on " + index);
                        // CommandBuffer.AddComponent(entity, new PlayerUnitSelect());
                        // CommandBuffer.AddComponent(entity, new Selecting());
                    }
                }

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var leftClick = Input.GetMouseButtonDown(0);
            var rightClick = Input.GetMouseButtonDown(1);
            if (!(leftClick || rightClick)) return inputDeps;

            var job = new PlayerUnitSelectJob
            {
                //  CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
                //  Selected = GetComponentDataFromEntity<PlayerUnitSelect>(),
                LeftClick = leftClick,
                ray = Camera.main.ScreenPointToRay(Input.mousePosition),
            }.Schedule(this, inputDeps);

            // SpawnJob runs in parallel with no sync point until the barrier system executes.
            // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
            // We need to tell the barrier system which job it needs to complete before it can play back the commands.
            // m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

            return job;
        }
    }
}


// using UnityEngine;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Collections;

// namespace UnitAgent
// {
//     public class PlayerUnitMovementSystem : JobComponentSystem
//     {

//         public struct PlayerUnitMovementJob : IJobForEach<Selectable>
//         {
//             public bool LeftClick, RightClick;
//             public float3 MousePos;

//             public void Execute ([ReadOnly] ref Selectable)
//             {
//                 if (RightClick)
//                 {

//                 }
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var leftClick = Input.GetMouseButtonDown(0);
//             var rightClick = Input.GetMouseButtonDown(1);
//             if (! (leftClick || rightClick) ) return inputDeps;

//             var mousePos = Input.mousePosition;
//             // use standard collision
//             Ray ray = Camera.main.ScreenPointToRay(mousePos);

//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 if (hit.collider != null)
//                 {
//                     mousePos = new float3(hit.point.x, 0, hit.point.z);
//                 }
//             }

//             var job = new PlayerUnitMovementJob
//             {
//                 LeftClick = leftClick,
//                 RightClick = rightClick,
//                 MousePos = mousePos
//             };
//             return job.Schedule(this, inputDeps);
//         }
//     }
// }