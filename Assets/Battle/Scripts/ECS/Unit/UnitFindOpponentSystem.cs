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
    [UpdateInGroup(typeof(CombatSystemGroup))]
    public class UnitFindOpponentSystem : JobComponentSystem
    {
        private EntityQuery m_PlayerTargetGroup;
        private EntityQuery m_NeedsOrderAttack;

        protected override void OnCreateManager()
        {
            m_PlayerTargetGroup = GetEntityQuery(
                ComponentType.ReadOnly<Unit>(), ComponentType.ReadOnly<Opponent>()
                );
        }

        [BurstCompile]
        struct FindClosestOpponentJob : IJobForEachWithEntity<Opponent, Unit>
        {
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> Targets;
            [ReadOnly] public ComponentDataFromEntity<Unit> AllUnits;
            [ReadOnly] public ComponentDataFromEntity<Translation> AllPositions;

            public void Execute([ReadOnly] Entity entity, [ReadOnly] int index, [ReadOnly] ref Opponent opponent, ref Unit unit)
            {
                float nearestDistanceSq = float.MaxValue;//could be range
                int nearestPositionIndex = -1;
                for (int i = 0; i < Targets.Length; i++)
                {
                    if (unit.Team == AllUnits[Targets[i]].Team) continue;

                    float distance = math.lengthsq(AllPositions[entity].Value - AllPositions[Targets[i]].Value);
                    bool nearest = distance < nearestDistanceSq;
                    nearestDistanceSq = math.select(nearestDistanceSq, distance, nearest);
                    nearestPositionIndex = math.select(nearestPositionIndex, i, nearest); ;
                }
#if !BurstCompile
                Debug.Assert(nearestPositionIndex > -1);
                Debug.Assert(entity != Targets[nearestPositionIndex]);
#endif
                opponent = new Opponent
                {
                   
                };
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var targets = m_PlayerTargetGroup.ToEntityArray(Allocator.TempJob);
            if (targets.Length == 0)
            {
                Debug.Log("UnitFindOpponentSystem No Targets");
                targets.Dispose();
                return inputDeps;
            }

            var outputDeps = new FindClosestOpponentJob
            {
                Targets = targets,
                AllPositions = GetComponentDataFromEntity<Translation>(),
                AllUnits = GetComponentDataFromEntity<Unit>()


            }.Schedule(this, inputDeps);

            return outputDeps;
        }
    }
}
