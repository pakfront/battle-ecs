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
    public class UnitFindOpponentSystem : JobComponentSystem
    {
        EntityQuery unitGroup;

        protected override void OnCreate()
        {
            unitGroup = GetEntityQuery( new EntityQueryDesc
            {
                All = new ComponentType[] { 
                    typeof(Opponent), ComponentType.ReadOnly<TeamGroup>(),
                    ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Unit>()
                }
            });

        }
        
        [BurstCompile]
        struct FindOpponentJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            // [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> Targets;
            public ArchetypeChunkComponentType<Opponent> OpponentType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<TeamGroup> TeamType;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var chunkOpponent = chunk.GetNativeArray(OpponentType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                int chunkTeamIndex = chunk.GetSharedComponentIndex(TeamType);

                var instanceCount = chunk.Count;

                // initialize
                for (int i = 0; i < instanceCount; i++)
                {
                    chunkOpponent[i] = new Opponent
                    {
                        DistanceSq = float.MaxValue // could be Range?
                    };
                }

                for (int c = 0; c < Chunks.Length; c++)
                {
                    var otherChunk = Chunks[c];

                    // don't test against your own Team/chunk
                    int otherChunkTeamIndex = otherChunk.GetSharedComponentIndex(TeamType);
                    if (otherChunkTeamIndex == chunkTeamIndex) continue;

                    var otherTranslations = otherChunk.GetNativeArray(TranslationType);
                    var otherEntities = otherChunk.GetNativeArray(EntityType);

                    for (int i = 0; i < instanceCount; i++)
                    {
                        float3 position = chunkTranslation[i].Value;
                        // Get from previous loop
                        float nearestDistanceSq = chunkOpponent[i].DistanceSq;
                        int nearestPositionIndex = -1;
                        var target = Entity.Null;
                        for (int j = 0; j < otherChunk.Count; j++)
                        {
                            var targetPosition = otherTranslations[j].Value;
                            var distance = math.lengthsq(position - targetPosition);
                            bool nearest = distance < nearestDistanceSq;
                            nearestDistanceSq = math.select(nearestDistanceSq, distance, nearest);
                            nearestPositionIndex = math.select(nearestPositionIndex, j, nearest);
                        }

                        if (nearestPositionIndex > -1)
                        {
                            // Debug.Log("Found nearest chunk["+chunkIndex+"]: otherChunk["+c+"]"+nearestPositionIndex);
                            chunkOpponent[i] = new Opponent
                            {
                                DistanceSq = nearestDistanceSq,
                                Position = otherTranslations[nearestPositionIndex].Value,
                                Entity = otherEntities[nearestPositionIndex]
                            };
                        }
                        //TODO possibly remove a tag if none found
                    }
                }
            }
        }

        [BurstCompile]
        struct SetGoal : IJobForEach<Opponent, MoveToGoal>
        {
            public void Execute([ReadOnly] ref Opponent opponent, ref MoveToGoal goal)
            {
                goal.Position = opponent.Position;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var opponentType = GetArchetypeChunkComponentType<Opponent>();
            var translationType = GetArchetypeChunkComponentType<Translation>(true);
            var teamType = GetArchetypeChunkSharedComponentType<TeamGroup>();
            var entityType = GetArchetypeChunkEntityType();

            var chunks = unitGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var findOpponentJob = new FindOpponentJob
            {
                Chunks = chunks,
                OpponentType = opponentType,
                TranslationType = translationType,
                EntityType = entityType,
                TeamType = teamType,
            };
            var outputDeps = findOpponentJob.Schedule(chunks.Length, 32, inputDeps);
            
            // // for testing
            // var setGoalJob = new SetGoal();
            // outputDeps = setGoalJob.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}
