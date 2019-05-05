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
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class TranslationSpeedSystem : JobComponentSystem
    {

        [ReadOnly] static float3 forward = new float3(0,0,1);
        // private EntityQuery m_Group;

        // protected override void OnCreate()
        // {
        //     // Cached access to a set of ComponentData based on a specific query
        //     m_Group = GetEntityQuery(
        //         typeof(Translation),
        //         ComponentType.ReadOnly<Rotation>(),
        //         ComponentType.ReadOnly<TranslationSpeed>());
        // }


        /// <summary>
        /// Put each Unit's position in a common table
        /// </summary>
        [BurstCompile]
        struct TranslateJob : IJobForEach<Translation,Rotation,TranslationSpeed>
        {
            public float DeltaTime;

            public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation, [ReadOnly] ref TranslationSpeed translationSpeed)
            {
                translation.Value += math.mul(rotation.Value, forward) * translationSpeed.UnitsPerSecond * DeltaTime;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            // intermediate storage for unit position so they can be read by agents
            // Unity.Collections.NativeHashMap<int, float3> targets; 
            // targets.Clear();
            

            var translateJob = new TranslateJob()
            {
                DeltaTime = Time.deltaTime
            };

            return translateJob.Schedule(this, inputDependencies);
        }
    }

}