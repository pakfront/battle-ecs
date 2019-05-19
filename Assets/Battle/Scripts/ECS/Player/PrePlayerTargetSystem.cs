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
    [UpdateBefore(typeof(PlayerTargetSystem))]
    public class PrePlayerTargetSystem : ComponentSystem
    {

        private EntityQuery m_NeedsOrderPursue;

        protected override void OnCreate()
        {
            m_NeedsOrderPursue = GetEntityQuery( new EntityQueryDesc
               {
                   None = new ComponentType[] { typeof(OrderPursue) },
                   All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>()  }
               });
        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                EntityManager.AddComponent(m_NeedsOrderPursue, typeof(OrderPursue));
            }
        }
    }
}