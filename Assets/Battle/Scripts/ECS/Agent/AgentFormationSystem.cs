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
    [UpdateAfter(typeof(UnitSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class AgentFormationSystem : JobComponentSystem
    {

        // TODO run only when unit has moved
        [BurstCompile]
        [RequireComponentTag(typeof(Agent))]

        struct SetGoalJob : IJobForEach<MoveToGoal, AgentFormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<UnitGroupMember> UnitGroupMembers;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Transforms;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref AgentFormationMember formationMember)
            {
                Entity parent = formationMember.Parent;
                float4x4 xform = Transforms[parent].Value;
                int startIndex = Formation.CalcAgentFormationStartIndex(
                    UnitGroupMembers[parent].FormationId, UnitGroupMembers[parent].FormationTableId
                    );
                
                Movement.SetGoalToFormationPosition(xform, startIndex + formationMember.Index, ref goal.Position, ref goal.Heading);


                // Movement.SetGoalToFormationPosition(xform, formationMember.Offset, ref goal.Position, ref goal.Heading);

                // goal.Position = math.transform(xform, formationElement.Position);
                // // heterogenous as it's a direction vector;
                // goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var setGoalJob = new SetGoalJob()
            {
                Transforms = GetComponentDataFromEntity<LocalToWorld>(true),
                UnitGroupMembers = GetComponentDataFromEntity<UnitGroupMember>(true)
            };

            return setGoalJob.Schedule(this, inputDependencies);
        }
    }
}