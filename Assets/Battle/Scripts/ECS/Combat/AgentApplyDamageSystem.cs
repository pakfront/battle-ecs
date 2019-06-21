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
        [BurstCompile]
        struct AgentDieJob : IJobForEachWithEntity<AgentGroupLeader>
        {
            public void Execute(Entity entity, int index, [ReadOnly] ref AgentGroupLeader agentMember)
            {

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // look into shedule for hashmap
            var outputDeps = inputDeps;
            outputDeps = new AgentDieJob
            {
            }.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}