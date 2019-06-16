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
        public NativeArray<float3> AgentFormationOffsetTable;
        protected override void OnCreate()
        {
            Formation.CalcAgentFormationOffsetTable(out float3[] formationOffsets);
            AgentFormationOffsetTable = new NativeArray<float3>(formationOffsets, Allocator.Persistent);
            Debug.Log("AgentFormationOffsetTable:"+AgentFormationOffsetTable.Length);
        }

        // TODO run only when unit has moved
        [BurstCompile]
        [RequireComponentTag(typeof(MoveToGoalTag))]
        struct SetGoalJob : IJobForEach<Goal, AgentFormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<UnitGroupMember> UnitGroupMembers;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Transforms;
            [ReadOnly] public NativeArray<float3> FormationOffsetsTable;

            public void Execute(ref Goal goal, [ReadOnly] ref AgentFormationMember formationMember)
            {
                Entity parent = formationMember.Parent;
                float4x4 xform = Transforms[parent].Value;

                int startIndex = Formation.CalcAgentFormationStartIndex(
                    UnitGroupMembers[parent].FormationId, UnitGroupMembers[parent].FormationTableId
                    );
                //TODO look into caching 
                float3 offset = FormationOffsetsTable[startIndex + formationMember.Index]; 
                Movement.SetGoalToFormationPosition(xform, offset, ref goal.Value);

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
                UnitGroupMembers = GetComponentDataFromEntity<UnitGroupMember>(true),
                FormationOffsetsTable = AgentFormationOffsetTable,

            };

            return setGoalJob.Schedule(this, inputDependencies);
        }
    }
}