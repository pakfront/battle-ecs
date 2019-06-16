using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;


namespace UnitAgent
{
    // cribbed from 
    // https://forum.unity.com/threads/how-do-you-get-a-bufferfromentity-or-componentdatafromentity-without-inject.587857/#post-3924478
    [DisableAutoCreation]
    [UpdateAfter(typeof(UnitOrderSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UnitHeirarchalGoalSystem : JobComponentSystem
    {
        // private EntityQuery[] m_Groups;
        private EntityQuery m_Group0, m_Group1; // m_Group2, m_Group3, m_Group4;
        protected override void OnCreate()
        {

            m_Group0 = GetEntityQuery(
            new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(Goal),
                    ComponentType.ReadOnly<Rank>(),
                    }
            }

            );
            m_Group0.SetFilter(new Rank { Value = (byte)0 });

            m_Group1 = GetEntityQuery(
           new EntityQueryDesc
           {
               All = new ComponentType[] {
                            typeof(Goal),
                            ComponentType.ReadOnly<Rank>(),
                            ComponentType.ReadOnly<UnitGroupMember>()
                   },
               None = new ComponentType[] {
                            ComponentType.ReadOnly<DetachedTag>(),
               }

           });
            m_Group1.SetFilter(new Rank { Value = (byte)1 });

            //     m_Group2 = GetEntityQuery(
            //    new EntityQueryDesc
            //    {
            //        All = new ComponentType[] {
            //                     typeof(Goal),
            //                     ComponentType.ReadOnly<Rank>(),
            //                     ComponentType.ReadOnly<UnitGroupMember>()
            //            },
            //        None = new ComponentType[] {
            //                     ComponentType.ReadOnly<Detached>(),
            //        }

            //    });
            //     m_Group2.SetFilter(new Rank { Value = (byte)2 });

            //     m_Group3 = GetEntityQuery(
            //     new EntityQueryDesc
            //     {
            //         All = new ComponentType[] {
            //                                 typeof(Goal),
            //                                 ComponentType.ReadOnly<Rank>(),
            //                                 ComponentType.ReadOnly<UnitGroupMember>()
            //         },
            //         None = new ComponentType[] {
            //                                 ComponentType.ReadOnly<Detached>(),
            //     }

            //     });
            //     m_Group3.SetFilter(new Rank { Value = (byte)3 });



            //     m_Group4 = GetEntityQuery(
            //     new EntityQueryDesc
            //     {
            //         All = new ComponentType[] {
            //                                 typeof(Goal),
            //                                 ComponentType.ReadOnly<Rank>(),
            //                                 ComponentType.ReadOnly<UnitGroupMember>()
            //         },
            //         None = new ComponentType[] {
            //                                 ComponentType.ReadOnly<Detached>(),
            //     }

            //     });
            //     m_Group4.SetFilter(new Rank { Value = (byte)4 });

            // m_Groups = new EntityQuery[Rank.MaxRank];

            // m_Groups[0] = GetEntityQuery(
            //         new EntityQueryDesc
            //         {
            //             All = new ComponentType[] {
            //                 ComponentType.ReadOnly<Rank>(),
            //                 },

            //         });
            // m_Groups[0].SetFilter(new Rank { Value = (byte)0 });


            // for (int i = 1; i < Rank.MaxRank; i++)
            // {
            //     m_Groups[i] = GetEntityQuery(
            //         //typeof(Goal), ComponentType.ReadOnly<Rank>());
            //         new EntityQueryDesc
            //         {
            //             All = new ComponentType[] {
            //                 ComponentType.ReadOnly<Rank>(),
            //                 ComponentType.ReadOnly<UnitGroupMember>()
            //                 },
            //             None = new ComponentType[] {
            //                 ComponentType.ReadOnly<Detached>(),
            //             }


            //         });
            //     m_Groups[i].SetFilter(new Rank { Value = (byte)i });
            // }
        }

        // [BurstCompile]
        // struct SetRootGoalJob : IJobChunk
        // {
        //     public ArchetypeChunkComponentType<Goal> GoalType;
        //     [ReadOnly] public ArchetypeChunkComponentType<UnitGroupMember> UnitGroupMemberType;

        //     public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        //     {
        //         var goals = chunk.GetNativeArray(GoalType);
        //         var unitGroupMembers = chunk.GetNativeArray(UnitGroupMemberType);
        //         for (var i = 0; i < chunk.Count; i++)
        //         {
        //         }
        //     }
        // }

        [BurstCompile]
        struct SetGoalJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Goal> GoalType;
            [ReadOnly] public ArchetypeChunkComponentType<UnitGroupMember> UnitGroupMemberType;

            // we know that all parents have been processed already
            [NativeDisableContainerSafetyRestriction] [ReadOnly] public ComponentDataFromEntity<Goal> Superiors;

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

            // var job0 = new SetRootGoalJob()
            // {
            //     GoalType = GetArchetypeChunkComponentType<Goal>(false),
            //     UnitGroupMemberType = GetArchetypeChunkComponentType<UnitGroupMember>(true),
            // };

            // outputDeps = job0.Schedule(m_Group0, inputDependencies);
            // outputDeps.Complete();

            var job1 = new SetGoalJob()
            {
                GoalType = GetArchetypeChunkComponentType<Goal>(false),
                UnitGroupMemberType = GetArchetypeChunkComponentType<UnitGroupMember>(true),
                Superiors = GetComponentDataFromEntity<Goal>(true)
            };

            outputDeps = job1.Schedule(m_Group1, inputDependencies);
            // outputDeps.Complete();

            // for (int i = 1; i < m_Groups.Length; i++)
            // {
            //     var job = new SetGoalJob()
            //     {
            //         GoalType = GetArchetypeChunkComponentType<Goal>(false),
            //         UnitGroupMemberType = GetArchetypeChunkComponentType<UnitGroupMember>(true),
            //         Superiors = GetComponentDataFromEntity<Goal>(true)
            //     };

            //     outputDeps = job.Schedule(m_Groups[i], inputDependencies);
            //     outputDeps.Complete();

            // }
            return outputDeps;

        }
    }
}
