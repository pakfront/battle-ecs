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
    public class AgentSystem : JobComponentSystem
    {

        private EntityQuery m_UnitGroup, m_AgentGroup;
        private Unity.Collections.NativeHashMap<int, float3> targets;

        protected override void OnCreate()
        {
            // Cached access to a set of ComponentData based on a specific query
            m_UnitGroup = GetEntityQuery(
                ComponentType.ReadOnly<Unit>(),
                ComponentType.ReadOnly<UnitId>(),
                ComponentType.ReadOnly<Translation>());

            m_AgentGroup = GetEntityQuery(
                typeof(Rotation),
                ComponentType.ReadOnly<Agent>(),
                ComponentType.ReadOnly<UnitId>()
                );
        }


        /// <summary>
        /// Put each Unit's position in a common table
        /// </summary>
        [RequireComponentTag(typeof(Unit))]
        [BurstCompile]
        struct TargetJob : IJobForEach<UnitId,Translation>
        {
            [WriteOnly] public NativeHashMap<int, float3>.Concurrent targets;

            public void Execute([ReadOnly] ref UnitId unitId, [ReadOnly] ref Translation translation)
            {
                // SortedDictionary what if it fails?
                targets.TryAdd(unitId.Value,translation.Value);
            }
        }

        // lookup each agents unit position and look at it
        [RequireComponentTag(typeof(Agent))]
        [BurstCompile]
        struct AgentRotationJob : IJobChunk
        {
            public float DeltaTime;
            public ArchetypeChunkComponentType<Rotation> RotationType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkComponentType<UnitId> UnitIdType;
            [ReadOnly] public NativeHashMap<int, float3> targets;
    
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkRotations = chunk.GetNativeArray(RotationType);
                var chunkTranslations = chunk.GetNativeArray(TranslationType);
                var chunkUnitIds = chunk.GetNativeArray(UnitIdType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    // float3 target = new float3(0,0,0);
                    float3 target = targets[ chunkUnitIds[i].Value ];
                    var rotation = chunkRotations[i];
                    var pos = chunkTranslations[i];
                    
                    float3 heading = target - pos.Value;
                    heading.y = 0;
                    chunkRotations[i] = new Rotation
                    {
                        Value = quaternion.LookRotation(heading, math.up())                   
                    };
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            // intermediate storage for unit position so they can be read by agents
            // Unity.Collections.NativeHashMap<int, float3> targets; 
            // targets.Clear();
            
            int unitCount = m_UnitGroup.CalculateLength();
            Debug.Log("Creating OnUpdate "+unitCount);

            var rotationType = GetArchetypeChunkComponentType<Rotation>(false); 
            var translationType = GetArchetypeChunkComponentType<Translation>(true);
            var unitIdType = GetArchetypeChunkComponentType<UnitId>(true);

            var targetJob = new TargetJob()
            {
                targets = targets.ToConcurrent()
            };

            var targetJobHandle = targetJob.Schedule(m_UnitGroup, inputDependencies);

            var agentRotationJob = new AgentRotationJob()
            {
                RotationType = rotationType,
                TranslationType = translationType,
                UnitIdType = unitIdType,
                targets = targets,
                DeltaTime = Time.deltaTime
            };
    
            return agentRotationJob.Schedule(m_AgentGroup, targetJobHandle);
        }

        protected override void OnStartRunning()
        {
            int unitCount = m_UnitGroup.CalculateLength();
            Debug.Log("Creating NativeHashMap "+unitCount);
            targets = new NativeHashMap<int,float3>(24, Allocator.Persistent);
        }
        protected override void OnStopRunning()
        {
            targets.Dispose();
        }

    }

}