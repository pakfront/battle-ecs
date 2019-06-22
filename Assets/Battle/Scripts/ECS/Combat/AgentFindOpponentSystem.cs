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
    [UpdateAfter(typeof(UnitFindOpponentSystem))]
    [UpdateInGroup(typeof(CombatSystemGroup))]
    public class AgentFindOpponentSystem : JobComponentSystem
    {
  
        EntityQuery agentGroup;

        protected override void OnCreate()
        {
            agentGroup = GetEntityQuery( new EntityQueryDesc
            {
                All = new ComponentType[] { 
                    typeof(Opponent), ComponentType.ReadOnly<AgentGroupPartition>(),
                    ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Agent>()
                }
            });

        }

        [BurstCompile]
        struct FindOpponentJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            // [ReadOnly, DeallocateOnJobCompletion] public NativeHashMap<Entity,int> EntityToIndex;
            // [ReadOnly, DeallocateOnJobCompletion] public NativeHashMap<int,Entity> IndexToEntity;

            [ReadOnly] public NativeHashMap<int,int> IndexToIndex;
            public ArchetypeChunkComponentType<Opponent> OpponentType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkSharedComponentType<AgentGroupPartition> AgentGroupPartitionType;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var chunkOpponent = chunk.GetNativeArray(OpponentType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                int chunkAgentGroupPartionIndex = chunk.GetSharedComponentIndex(AgentGroupPartitionType);
                int parentOpponentPartitionIndex = IndexToIndex[chunkAgentGroupPartionIndex];

                if (parentOpponentPartitionIndex <= 0) return;

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

                    // only search against my AgentGroupPartition's Opponent
                    int otherAgentGroupPartitionIndex = otherChunk.GetSharedComponentIndex(AgentGroupPartitionType);
                    if (otherAgentGroupPartitionIndex != parentOpponentPartitionIndex) continue;

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

        [RequireComponentTag(typeof(AgentGroupMember))]
        [BurstCompile]
        struct SetGoal : IJobForEach<Opponent, Goal>
        {
            public void Execute([ReadOnly] ref Opponent opponent, ref Goal goal)
            {
                goal.Value = Movement.CalcGoalPositionOnly(opponent.Position);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var outputDeps = inputDeps;
            var agentGroupPartionData = new List<AgentGroupPartition>();
            var sharedComponentIndices = new List<int>();
            
            EntityManager.GetAllUniqueSharedComponentData(agentGroupPartionData, sharedComponentIndices);
            // make these managed?
            var indexFromParent = new NativeHashMap<Entity,int>(sharedComponentIndices.Count, Allocator.TempJob);
            var parentFromIndex = new NativeHashMap<int,Entity>(sharedComponentIndices.Count, Allocator.TempJob);
            
            // start at 1 to skip default
            for (int i = 1; i < sharedComponentIndices.Count; i++)
            {
                int index = sharedComponentIndices[i];
                var parent = agentGroupPartionData[i].Parent;
                // Debug.Log(i+" index:"+index+" parent:"+parent);

                indexFromParent.TryAdd(parent, index);
                parentFromIndex.TryAdd(index, parent);
            }

            var indexToOpponentAgentGroupPartionIndex = new NativeHashMap<int,int>(sharedComponentIndices.Count, Allocator.TempJob);
            for (int i = 1; i < sharedComponentIndices.Count; i++)
            {
                var index = sharedComponentIndices[i];
                var parent = parentFromIndex[index];
                // if (! EntityManager.HasComponent<Opponent>(parent))
                // {
                //     Debug.LogError("No Opponent on"+i+" index:"+index+" parent:"+parent);
                //     continue;
                // }
                var opponent = EntityManager.GetComponentData<Opponent>(parent);
                var opponentEntity = opponent.Entity;
                var opponentIndex = indexFromParent[opponentEntity];
                // Debug.Log("IndexToIndex:"+i+" index:"+index+" parent:"+parent);

                indexToOpponentAgentGroupPartionIndex.TryAdd(index, opponentIndex);
            }
            
            var chunks = agentGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var findOpponentJob = new FindOpponentJob
            {
                Chunks = chunks,
                OpponentType = GetArchetypeChunkComponentType<Opponent>(),
                TranslationType = GetArchetypeChunkComponentType<Translation>(true),
                EntityType = GetArchetypeChunkEntityType(),
                AgentGroupPartitionType = GetArchetypeChunkSharedComponentType<AgentGroupPartition>(),

                IndexToIndex = indexToOpponentAgentGroupPartionIndex
                // EntityToIndex = entityToIndex,
                // IndexToEntity = parentFromIndex
            };
            outputDeps = findOpponentJob.Schedule(chunks.Length, 32, outputDeps);
            outputDeps.Complete();

            // for testing
            // Debug.LogWarning("Setting up SetGoal for testing");
            var setGoalJob = new SetGoal();
            outputDeps = setGoalJob.Schedule(this, outputDeps);

            indexFromParent.Dispose();
            parentFromIndex.Dispose();
            indexToOpponentAgentGroupPartionIndex.Dispose();

            return outputDeps;
        }
    }
}
