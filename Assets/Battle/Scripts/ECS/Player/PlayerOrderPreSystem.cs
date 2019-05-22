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
    [UpdateAfter(typeof(PlayerPointerSystem))]
    public class PlayerOrderPreSystem : ComponentSystem
    {
        private EntityQuery m_NeedsOrderAttack, m_NeedsOrderMoveTo;

        protected override void OnCreate()
        {
            m_NeedsOrderAttack = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(OrderAttack) },
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>() }
            });

            m_NeedsOrderMoveTo = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(OrderMoveTo) },
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>() }
            });
        }

        protected override void OnUpdate()
        {

            if (Input.GetMouseButtonDown(1))
            {
                // Debug.Log("Adding m_NeedsOrderMoveTo");
                EntityManager.AddComponent(m_NeedsOrderMoveTo, typeof(OrderMoveTo));
            }
        }
    }
}
