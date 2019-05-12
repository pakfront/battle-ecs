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
    [UpdateBefore(typeof(AgentSystem))]
    public class UnitCombatSystem : JobComponentSystem
    {
        [BurstCompile]
        struct FindOpponent : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            public ArchetypeChunkComponentType<Opponent> OpponentType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<Team> TeamType;
            public float DeltaTime;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var chunkOpponent = chunk.GetNativeArray(OpponentType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                int chunkTeamIndex = chunk.GetSharedComponentIndex(TeamType);
                var instanceCount = chunk.Count;

                //create arrays for holding distance and entity?
                for (int i = 0; i < instanceCount; i++)
                {
                    chunkOpponent[i] =  new Opponent
                    {
                        DistanceSq = float.MaxValue
                    };
                }

                for (int c = 0; c < Chunks.Length; c++)
                {
                    var otherChunk = Chunks[c];
                    // don't test against your own Team/chunk
                    int otherChunkTeamIndex = otherChunk.GetSharedComponentIndex(TeamType);
                    if (otherChunkTeamIndex == chunkTeamIndex) continue;

                    var otherTranslations = otherChunk.GetNativeArray(TranslationType);

                    for (int i = 0; i < instanceCount; i++)
                    {
                        float3 position = chunkTranslation[i].Value;
                        // Get from previous pass
                        float nearestDistanceSq = chunkOpponent[i].DistanceSq;
                        int nearestPositionIndex = -1;

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
                            chunkOpponent[i] =  new Opponent
                            {
                                DistanceSq = nearestDistanceSq,
                                Position = otherTranslations[nearestPositionIndex].Value
                            };
                        }
                        //TODO possibly remove a tag if none found
                    }
                }
            }
        }

    [BurstCompile]
    struct SetGoal : IJobForEach<Opponent, GoalMoveTo>
    {
        public void Execute([ReadOnly] ref Opponent opponent, ref GoalMoveTo goal)
        {
            goal.Position = opponent.Position;
        }
    }


        EntityQuery m_group;

        protected override void OnCreate()
        {
            var query = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Opponent), ComponentType.ReadOnly<Team>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Unit>() }
            };

            m_group = GetEntityQuery(query);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var opponentType = GetArchetypeChunkComponentType<Opponent>();
            var translationType = GetArchetypeChunkComponentType<Translation>(true);
            var teamType = GetArchetypeChunkSharedComponentType<Team>();
            var chunks = m_group.CreateArchetypeChunkArray(Allocator.TempJob);

            var findOpponentJob = new FindOpponent
            {
                Chunks = chunks,
                OpponentType = opponentType,
                TranslationType = translationType,
                TeamType = teamType,
                DeltaTime = Time.deltaTime
            };
            var findOpponentJobHandle = findOpponentJob.Schedule(chunks.Length, 32, inputDeps);

            var setGoalJob = new SetGoal();
            return setGoalJob.Schedule(this,findOpponentJobHandle); 
        }
    }
}
