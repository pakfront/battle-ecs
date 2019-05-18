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
    // [UpdateBefore(typeof(PlayerInputSystem))]
    public class PlayerSelectionSystem : JobComponentSystem
    {
        EntityCommandBufferSystem m_EntityCommandBufferSystem;
        // private EntityQuery m_GroupPlayerSelected;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            // m_GroupPlayerSelected = GetEntityQuery(new EntityQueryDesc
            // {
            //     All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelected>() }
            // });
        }

        //  do not burst compile, AddComponent not supported 
        // [BurstCompile]
        [RequireComponentTag(typeof(Unit))]
        [ExcludeComponent(typeof(PlayerSelected))]
        struct PlayerSelectionJob : IJobForEachWithEntity<AABB>
        {

            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public Ray ray;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, ray))
                {
                    Debug.Log("PlayerSelectionJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerSelected());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var leftClick = Input.GetMouseButtonDown(0);

            //if (!leftClick) return inputDeps;

            // clear all old selections unless shift or ctrl
            // if (!(
            //     Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
            //     Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            //     ))
            // {
            //     EntityManager.RemoveComponent(m_GroupPlayerSelected, ComponentType.ReadOnly<PlayerSelected>());
            // }

            var job = new PlayerSelectionJob
            {
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
                ray = Camera.main.ScreenPointToRay(Input.mousePosition),
            };
            var outputDeps = job.Schedule(this, inputDeps);

            m_EntityCommandBufferSystem.AddJobHandleForProducer(outputDeps);

            return outputDeps;
        }
    }
}
