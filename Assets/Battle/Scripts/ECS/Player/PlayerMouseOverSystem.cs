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
        [RequireComponentTag(typeof(Unit))]
        struct PlayerMouseOverJob : IJobForEachWithEntity<AABB>
        {

            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public Ray ray;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, ray))
                {
                    Debug.Log("PlayerMouseOverJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerMouseOver());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // early out needs to be refined
            if ( ! (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) )) return inputDeps;

            var job = new PlayerMouseOverJob
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
