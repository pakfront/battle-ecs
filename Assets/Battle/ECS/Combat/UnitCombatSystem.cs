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
        struct FindOpponentJob : IJobParallelFor
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

                for (int c = 0; c < Chunks.Length; c++)
                {
                    var otherChunk = Chunks[c];

                    // don't test against your own Team/chunk
                    int otherChunkTeamIndex = otherChunk.GetSharedComponentIndex(TeamType);
                    if (otherChunkTeamIndex == chunkTeamIndex) continue;

                    // assuming getnativearray has a cost so putting in outer loop
                    var otherTranslations = otherChunk.GetNativeArray(TranslationType);

                    for (int i = 0; i < instanceCount; i++)
                    {
                        var rotation = chunkOpponent[i];
                        float3 position = chunkTranslation[i].Value;
                        int nearestPositionIndex = -1;
                        //TODO get from previous pass
                        float nearestDistance = float.MaxValue;

                        for (int j = 0; j < otherChunk.Count; j++)
                        {
                            var targetPosition = otherTranslations[j].Value;
                            var distance = math.lengthsq(position - targetPosition);
                            bool nearest = distance < nearestDistance;
                            nearestDistance = math.select(nearestDistance, distance, nearest);
                            nearestPositionIndex = math.select(nearestPositionIndex, j, nearest);
                        }

                        if (nearestPositionIndex > -1)
                        {
                            // Debug.Log("Found nearest chunk["+chunkIndex+"]: otherChunk["+c+"]"+nearestPositionIndex);

                            // nearestDistance = math.sqrt(nearestDistance);
                            // chunkOpponent 
                            // combat.Position = OtherPositions[nearestPositionIndex];
                        }
                        // else
                        // {
                        //     //temp debug value
                        //     combat.Position = new float3(-1, -1, -1);

                        // }
                    }
                }
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

            var rotationsSpeedJob = new FindOpponentJob
            {
                Chunks = chunks,
                OpponentType = opponentType,
                TranslationType = translationType,
                TeamType = teamType,
                DeltaTime = Time.deltaTime
            };
            return rotationsSpeedJob.Schedule(chunks.Length, 32, inputDeps);
        }
    }
}
