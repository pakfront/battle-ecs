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
    // cribbed from 
    // https://forum.unity.com/threads/how-do-you-get-a-bufferfromentity-or-componentdatafromentity-without-inject.587857/#post-3924478
    [UpdateAfter(typeof(UnitGoalSystem))]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public class AgentFormationSystem : JobComponentSystem
    {

        // TODO run only when unit has moved
        [BurstCompile]
        [RequireComponentTag(typeof(Agent))]

        struct SetGoalJob : IJobForEach<MoveToGoal, FormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref FormationMember formationElement)
            {
                Entity parent = formationElement.Parent;
                float4x4 xform = Others[parent].Value;
                goal.Position = math.transform(xform, formationElement.Position);
                // heterogenous as it's a direction vector;
                goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var setGoalJob = new SetGoalJob()
            {
                Others = GetComponentDataFromEntity<LocalToWorld>(true)
            };

            return setGoalJob.Schedule(this, inputDependencies);
        }
    }
}