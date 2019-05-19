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
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerInputSystem))]
    public class PlayerMouseOverSystem : JobComponentSystem
    {
        EntityCommandBufferSystem m_EntityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }

        //  do not burst compile, AddComponent not supported 
        // [BurstCompile]
        [RequireComponentTag(typeof(Unit), typeof(PlayerOwned))]
        struct PlayerSelectionJob : IJobForEachWithEntity<AABB>
        {
            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public Ray ray;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, ray))
                {
                    Debug.Log("PlayerSelectionJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerSelection());
                }
            }
        }

        [RequireComponentTag(typeof(Unit), typeof(PlayerEnemy))]
        struct PlayerTargetJob : IJobForEachWithEntity<AABB>
        {
            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public Ray ray;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, ray))
                {
                    Debug.Log("PlayerTargetJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerTarget());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // early out needs to be refined
            // if ( ! (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) )) return inputDeps;

            JobHandle outputDeps;

            if (Input.GetMouseButtonDown(0))
            {
                outputDeps = new PlayerSelectionJob
                {
                    CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition),
                }.Schedule(this, inputDeps);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                outputDeps = new PlayerTargetJob
                {
                    CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition),
                }.Schedule(this, inputDeps);
            }
            else
            {
                return inputDeps;
            }

            m_EntityCommandBufferSystem.AddJobHandleForProducer(outputDeps);

            return outputDeps;
        }
    }
}
