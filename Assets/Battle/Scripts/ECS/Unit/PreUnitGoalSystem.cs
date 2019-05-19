using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(PlayerMouseOverSystem))]
    [UpdateBefore(typeof(UnitGoalSystem))]
    public class PreUnitGoalSystem : ComponentSystem
    {

        private EntityQuery m_NeedsMoveToGoal, m_RemoveMoveToGoal;

        protected override void OnCreate()
        {
            m_RemoveMoveToGoal = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(MoveToGoal) },
                Any = new ComponentType[] {
                    ComponentType.ReadOnly<OrderHold>(),
                },
                None = new ComponentType[] {
                    ComponentType.ReadOnly<OrderMoveTo>(),
                    ComponentType.ReadOnly<OrderPursue>(),
                    ComponentType.ReadOnly<OrderMarch>(),
                    ComponentType.ReadOnly<OrderFormation>()
                }
            });

            m_NeedsMoveToGoal = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(MoveToGoal) },
                Any = new ComponentType[] {
                    ComponentType.ReadOnly<OrderMoveTo>(),
                    ComponentType.ReadOnly<OrderPursue>(),
                    ComponentType.ReadOnly<OrderMarch>(),
                    ComponentType.ReadOnly<OrderFormation>()
                }
            });
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent(m_RemoveMoveToGoal, typeof(MoveToGoal));
            EntityManager.AddComponent(m_NeedsMoveToGoal, typeof(MoveToGoal));
        }
    }
}