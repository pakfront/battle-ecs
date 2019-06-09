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

        private EntityQuery m_OrderAttack, m_OrderMoveTo, m_OrderFormationMoveTo;
        protected override void OnCreate()
        {

            m_OrderAttack = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderAttack>()}
            });
            m_OrderMoveTo = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderMoveTo>()}
            });
            m_OrderFormationMoveTo = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<OrderFormationMoveTo>()}
            });

        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent(m_OrderAttack, typeof(OrderAttack));
            EntityManager.RemoveComponent(m_OrderMoveTo, typeof(OrderMoveTo));
            EntityManager.RemoveComponent(m_OrderFormationMoveTo, typeof(OrderFormationMoveTo));
        }
        
    }
}
