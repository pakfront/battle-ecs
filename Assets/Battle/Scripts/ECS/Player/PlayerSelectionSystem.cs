using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateAfter(typeof(PlayerMouseOverSystem))]
    [UpdateBefore(typeof(PlayerInputSystem))]
    public class PlayerSelectionSystem : ComponentSystem
    {
    private EntityQuery m_GroupPlayerMouseOver;
      protected override void OnCreate()
        {
            base.OnCreate();
            m_GroupPlayerMouseOver = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerMouseOver>(), ComponentType.ReadOnly<PlayerOwned>()  }
            });
        }

        protected override void OnUpdate()
        {            
            if (!Input.GetMouseButtonDown(0)) return;
            Debug.Log("PlayerSelectionSystem Adding");
            EntityManager.AddComponent(m_GroupPlayerMouseOver, ComponentType.ReadOnly<PlayerSelection>());
        }
    }
}

// namespace UnitAgent
// {
//     // [DisableAutoCreation] 
//     [UpdateAfter(typeof(PlayerMouseOverSystem))]
//     public class PlayerSelectionSystem : JobComponentSystem
//     {
//         EntityCommandBufferSystem m_EntityCommandBufferSystem;
//         protected override void OnCreate()
//         {
//             base.OnCreate();
//             m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
//         }

//         //  do not burst compile, AddComponent not supported 
//         // [BurstCompile]
//         [RequireComponentTag(typeof(Unit))]
//         [ExcludeComponent(typeof(PlayerSelected))]
//         struct PlayerSelectionJob : IJobForEachWithEntity<AABB>
//         {
//             [ReadOnly] public EntityCommandBuffer CommandBuffer;
//             public Ray ray;

//             public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
//             {
//                 if (RTSPhysics.Intersect(aabb, ray))
//                 {
//                     Debug.Log("PlayerSelectionJob: Click on " + index);
//                     CommandBuffer.AddComponent(entity, new PlayerSelected());
//                 }
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var leftClick = Input.GetMouseButtonDown(0);

//             if (!leftClick) return inputDeps;

//             var job = new PlayerSelectionJob
//             {
//                 CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
//                 ray = Camera.main.ScreenPointToRay(Input.mousePosition),
//             };
//             var outputDeps = job.Schedule(this, inputDeps);

//             m_EntityCommandBufferSystem.AddJobHandleForProducer(outputDeps);

//             return outputDeps;
//         }
//     }
// }
