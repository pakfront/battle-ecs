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
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(PlayerMouseOverSystem))]
    [UpdateBefore(typeof(UnitGoalSystem))]
    public class PlayerTargetSystem : JobComponentSystem
    {
        private EntityQuery m_TargetGroup;

        protected override void OnCreateManager()
        {
            m_TargetGroup = GetEntityQuery(
                ComponentType.ReadOnly<PlayerTarget>());
        }

        [BurstCompile]
        struct SetOrderPursueTarget : IJobForEachWithEntity<PlayerSelection, OrderPursue>
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
                Debug.Log("SetOrderPursueTarget:"+nearestPositionIndex);
                // orderPursue = new OrderPursue {
                //     Target = Targets[nearestPositionIndex]
                // };
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // EntityManager.AddComponent(m_PlayerSelectionNoOrderPursue, typeof(OrderPursue));
            var targets = m_TargetGroup.ToEntityArray(Allocator.TempJob);
            if (targets.Length == 0)
            {
                targets.Dispose();
                return inputDeps;
            }

            var outputDeps = new SetOrderPursueTarget
            {
                Targets = targets,
                AllPositions = GetComponentDataFromEntity<Translation>()

            }.Schedule(this, inputDeps);

            //then remove all the PlayerTarget?
            return outputDeps;
        }
    }
}
