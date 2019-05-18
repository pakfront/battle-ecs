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
    [DisableAutoCreation]
    [UpdateBefore(typeof(MoveToGoalSystem))]
    public class PlayerTargetSystem : JobComponentSystem
    {
        private EntityQuery m_TargetGroup;

        protected override void OnCreateManager()
        {
            m_TargetGroup = GetEntityQuery(
                ComponentType.ReadOnly<PlayerTarget>());
        }

        [RequireComponentTag(typeof(OrderPursue))]
        [BurstCompile]
        struct FindOpponentJob : IJobForEachWithEntity<PlayerSelection, OrderPursue>
        {

            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> Targets;
            [ReadOnly] public ComponentDataFromEntity<Translation> AllPositions;

            public void Execute([ReadOnly] Entity entity, [ReadOnly] int index, [ReadOnly] ref PlayerSelection playerSelection, ref OrderPursue orderPursue)
            {
                float nearestDistanceSq = float.MaxValue;
                int nearestPositionIndex = -1;
                for (int i = 0; i < Targets.Length; i++)
                {
                    float distance = math.lengthsq(AllPositions[entity].Value - AllPositions[Targets[i]].Value);
                    bool nearest = distance < nearestDistanceSq;
                    nearestDistanceSq = math.select(nearestDistanceSq, distance, nearest);
                    nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);;
                }
                Debug.Assert(nearestPositionIndex > -1);
                Debug.Assert(entity != Targets[nearestPositionIndex]);
                orderPursue = new OrderPursue {
                    Target = Targets[nearestPositionIndex]
                };
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var targetCount = m_TargetGroup.CalculateLength();
            var targetType = GetArchetypeChunkComponentType<PlayerTarget>(true);

            var outputDeps = new FindOpponentJob
            {
                Targets = m_TargetGroup.ToEntityArray(Allocator.TempJob),
                AllPositions = GetComponentDataFromEntity<Translation>()

            }.Schedule(this, inputDeps);
            return outputDeps;
        }
    }
}
