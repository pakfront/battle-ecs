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
    public class PlayerOrderAttackSystem : JobComponentSystem
    {
        private EntityQuery m_PlayerTargetGroup;

        protected override void OnCreateManager()
        {
            m_PlayerTargetGroup = GetEntityQuery(
                ComponentType.ReadOnly<PlayerTarget>());
        }

        [BurstCompile]
        struct SetOrderAttackTarget : IJobForEachWithEntity<PlayerSelection, OrderAttack>
        {
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> Targets;
            [ReadOnly] public ComponentDataFromEntity<Translation> AllPositions;

            public void Execute([ReadOnly] Entity entity, [ReadOnly] int index, [ReadOnly] ref PlayerSelection playerSelection, ref OrderAttack OrderAttack)
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
                Debug.Log("SetOrderAttackTarget:"+nearestPositionIndex);
                OrderAttack = new OrderAttack {
                    Target = Targets[nearestPositionIndex]
                };
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!Input.GetKeyDown(KeyCode.Y)) return inputDeps;

            // EntityManager.AddComponent(m_PlayerSelectionNoOrderAttack, typeof(OrderAttack));
            var targets = m_PlayerTargetGroup.ToEntityArray(Allocator.TempJob);
            if (targets.Length == 0)
            {
                targets.Dispose();
                return inputDeps;
            }

            var outputDeps = new SetOrderAttackTarget
            {
                Targets = targets,
                AllPositions = GetComponentDataFromEntity<Translation>()

            }.Schedule(this, inputDeps);

            //then remove all the PlayerTarget?
            return outputDeps;
        }
    }


    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(PlayerMouseOverSystem))]
    [UpdateBefore(typeof(PlayerOrderAttackSystem))]
    public class PrePlayerOrderAttackSystem : ComponentSystem
    {

        private EntityQuery m_NeedsOrderAttack;

        protected override void OnCreate()
        {
            m_NeedsOrderAttack = GetEntityQuery( new EntityQueryDesc
               {
                   None = new ComponentType[] { typeof(OrderAttack) },
                   All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>()  }
               });
        }

        protected override void OnUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.Y)) return;
            
            EntityManager.AddComponent(m_NeedsOrderAttack, typeof(OrderAttack));
        }
    }

}
