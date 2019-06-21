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
    // [DisableAutoCreation]
    //[UpdateAfter(typeof(MoveToGoal))]
    [UpdateInGroup(typeof(CombatSystemGroup))]
    [UpdateAfter(typeof(UnitRangedAttackSystem))]
    public class AgentApplyDamageSystem : JobComponentSystem
    {
   EndSimulationEntityCommandBufferSystem commandBufferSystem;

   protected override void OnCreate()
   {
       // Cache the EndFrameBarrier in a field, so we don't have to get it every frame
       commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
   }

        // burst does not work with command buffer[BurstCompile]
        [ExcludeComponent(typeof(DeadTag))]
        struct AgentDieJob : IJobForEachWithEntity<AgentGroupMember>
        {
            public EntityCommandBuffer Buffer;

            [ReadOnly] public ComponentDataFromEntity<AgentCount> AgentCounts;


            public void Execute(Entity entity, int index, [ReadOnly] ref AgentGroupMember groupMember)
            {            
                // TODO move to chunk so we can do this lookup once    
                Entity parent = groupMember.Parent;
                if ( groupMember.Index >= AgentCounts[parent].Value  )
                {
                    Buffer.AddComponent(entity, new DeadTag {});
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // maybe we should use a query to add DeadTag
            var outputDeps = inputDeps;
            outputDeps = new AgentDieJob
            {
                AgentCounts = GetComponentDataFromEntity<AgentCount>(true),
                Buffer = commandBufferSystem.CreateCommandBuffer()
            }.ScheduleSingle(this, outputDeps);
            commandBufferSystem.AddJobHandleForProducer(outputDeps);
            return outputDeps;
        }
    }
}