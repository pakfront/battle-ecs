using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{

    [UpdateBefore(typeof(PlayerMouseOverSystem))]
    public class ClearPlayerMouseOverSystem : ComponentSystem
    {
      private EntityQuery m_GroupPlayerSelection;
      protected override void OnCreate()
        {
            base.OnCreate();
            m_GroupPlayerSelection = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerMouseOver>() }
            });
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent(m_GroupPlayerSelection, ComponentType.ReadOnly<PlayerMouseOver>());
        }
    }
}