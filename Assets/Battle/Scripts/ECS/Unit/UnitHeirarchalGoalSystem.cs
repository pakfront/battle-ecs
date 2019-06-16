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
    [UpdateAfter(typeof(UnitOrderSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UnitHeirarchalGoalSystem : JobComponentSystem
    {
        private EntityQuery[] m_Groups;
        protected override void OnCreate()
        {
            m_Groups = new EntityQuery[Rank.MaxRank];
            for (int i = 0; i < Rank.MaxRank; i++)
            {
                m_Groups[i] = GetEntityQuery(typeof(Goal), ComponentType.ReadOnly<Rank>());
                m_Groups[i].SetFilter(new Rank { Value = (byte)i });
            }
        }

        [BurstCompile]
        struct SetGoalJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Goal> GoalType;
            [ReadOnly] public ArchetypeChunkComponentType<UnitGroupMember> UnitGroupMemberType;
            [ReadOnly] public ComponentDataFromEntity<Goal> Superiors;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var goals = chunk.GetNativeArray(GoalType);
                var unitGroupMembers = chunk.GetNativeArray(UnitGroupMemberType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    Entity parent = unitGroupMembers[i].Parent;
                    float4x4 xform = Superiors[parent].Value;
                    Goal goal = goals[i];
                    Movement.SetGoalToFormationPosition(xform, unitGroupMembers[i].PositionOffset, ref goal.Value);
                    goals[i] = goal;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var outputDeps = inputDependencies;
            for (int i = 1; i < m_Groups.Length; i++)
            {
                var job = new SetGoalJob()
                {
                    GoalType = GetArchetypeChunkComponentType<Goal>(false),
                    UnitGroupMemberType = GetArchetypeChunkComponentType<UnitGroupMember>(true),
                    Superiors = GetComponentDataFromEntity<Goal>(true)
                };

                outputDeps = job.Schedule(m_Groups[i], inputDependencies);

            }
            return outputDeps;

        }
    }
}
