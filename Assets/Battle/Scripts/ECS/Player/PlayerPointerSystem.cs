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
        EntityCommandBufferSystem m_EntityCommandBufferSystem;
        EntityQuery m_group;


        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

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

            public Ray Ray;
            public NativeArray<int> NearestEntity;
            public NativeArray<float> NearestDistanceSq;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                int chunkAABB = chunk.GetNativeArray(AABBType);
                var instanceCount = chunk.Count;
                float nearestDistanceSq = float.MaxValue;
                int nearestPositionIndex = -1;

                for (int i = 0; i < instanceCount; i++)
                {
                    var aabb = chunkAABB[i];
                    bool hit = RTSPhysics.Intersect(aabb, Ray);
                    if (hit)
                    {
                        Debug.Log("PlayerSelectionJob: Click on " + i);
                    }

                    float distance = (chunkTranslation[i].Value - ray.origin).lengthsq;
                    var distance = math.lengthsq(position - targetPosition);
                    bool nearest = hit && distance < nearestDistanceSq;
                    nearestDistanceSq = math.select(nearestDistanceSq, distance, nearest);
                    nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
                }

                // FIXME is not an actual Enitty
                NearestEntity[chunkIndex] = nearestPositionIndex;
                NearestDistanceSq[chunkIndex] = nearestPositionIndex;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = m_group.CreateArchetypeChunkArray(Allocator.TempJob);
            var nchunks = chunks.Length;


            var findOpponentJob = new FindOpponentJob
            {
                Chunks = chunks,
                OpponentType = opponentType,
                TranslationType = translationType,
                TeamType = teamType,
            };
            var outputDeps = findOpponentJob.Schedule(nchunks, 32, inputDeps);
            
            return outputDeps;

            // var setGoalJob = new SetGoal();
            // return setGoalJob.Schedule(this, outputDeps);
        }
    }
}