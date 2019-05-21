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
    [UpdateBefore(typeof(MoveToGoalSystem))]
    public class PlayerPointerSystem : JobComponentSystem
    {
        EntityQuery m_group;


        protected override void OnCreate()
        {
            var query = new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AABB>(), ComponentType.ReadOnly<Translation>() }
            };

            m_group = GetEntityQuery(query);
        }

        [BurstCompile]
        struct FindOpponentJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<AABB> AABBType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var chunkOpponent = chunk.GetNativeArray(OpponentType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                int chunkTeamIndex = chunk.GetSharedComponentIndex(TeamType);
                var instanceCount = chunk.Count;
                float nearestDistanceSq = chunkOpponent[i].DistanceSq;
                int nearestPositionIndex = -1;


                for (int i = 0; i < instanceCount; i++)
                {
                    float3 position = chunkTranslation[i].Value;
                    // Get from previous pass

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
                            Position = otherTranslations[nearestPositionIndex].Value
                        };
                    }
                    //TODO possibly remove a tag if none found

                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var closestIndex = NativeArray
            var chunks = m_group.CreateArchetypeChunkArray(Allocator.TempJob);

            var findOpponentJob = new FindOpponentJob
            {
                Chunks = chunks,
                OpponentType = opponentType,
                TranslationType = translationType,
                TeamType = teamType,
            };
            var outputDeps = findOpponentJob.Schedule(chunks.Length, 32, inputDeps);
            
            return outputDeps;

            // var setGoalJob = new SetGoal();
            // return setGoalJob.Schedule(this, outputDeps);
        }
    }
}