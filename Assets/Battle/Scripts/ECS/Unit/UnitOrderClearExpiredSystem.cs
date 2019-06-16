using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    // [UpdateInGroup(typeof(UnitSystemGroup))]
    // [UpdateAfter(typeof(UnitOrderSystem))]
    [UpdateInGroup(typeof(ClearPreviousFrameSystemGroup))]
    public class UnitOrderClearExpiredSystem : ComponentSystem
    {

        private EntityQuery m_OrderAttack, m_OrderMoveTo, m_OrderFormationMoveTo, m_OrderFormation;
        protected override void OnCreate()
        {
            //eventually there may be timers to simulate order delay

            m_OrderAttack = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderAttackTag>()}
            });
            m_OrderMoveTo = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderUnitMoveToTag>()}
            });
            m_OrderFormationMoveTo = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderUnitGroupMoveToTag>()}
            });
            m_OrderFormation = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderChangeFormationTag>()}
            });

        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent(m_OrderAttack, typeof(OrderAttackTag));
            EntityManager.RemoveComponent(m_OrderMoveTo, typeof(OrderUnitMoveToTag));
            EntityManager.RemoveComponent(m_OrderFormationMoveTo, typeof(OrderUnitGroupMoveToTag));
            EntityManager.RemoveComponent(m_OrderFormation, typeof(OrderChangeFormationTag));
        }
        
    }
}
