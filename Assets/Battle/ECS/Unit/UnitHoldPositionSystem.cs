using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace UnitAgent
{

    public class UnitHoldPositionSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem m_EndSimulationBarrier;

        protected override void OnCreate()
        {
            m_EndSimulationBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        [RequireComponentTag(typeof(GoalMoveTo))]
        // [BurstCompile] - burst does not support RemoveComponent yet
        struct ClearGoalsJob : IJobForEachWithEntity<UnitHoldPosition>
        {
            public EntityCommandBuffer CommandBuffer;
            public void Execute(Entity entity, int index, [ReadOnly] ref UnitHoldPosition orderHold)
            {
                CommandBuffer.RemoveComponent<GoalMoveTo>(entity);
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ClearGoalsJob
            {
                CommandBuffer = m_EndSimulationBarrier.CreateCommandBuffer()
            }.ScheduleSingle(this, inputDependencies);

            // We need to tell the barrier system which job it needs to complete before it can play back the commands.
            m_EndSimulationBarrier.AddJobHandleForProducer(job);

            return job;
        }
    }
}