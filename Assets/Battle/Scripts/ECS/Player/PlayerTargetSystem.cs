using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateAfter(typeof(PlayerMouseOverSystem))]
    [UpdateBefore(typeof(PlayerInputSystem))]
    public class PlayerTargetSystem : ComponentSystem
    {
    private EntityQuery m_GroupPlayerMouseOver;
      protected override void OnCreate()
        {
            base.OnCreate();
            m_GroupPlayerMouseOver = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerMouseOver>(), ComponentType.ReadOnly<PlayerEnemy>() }
            });
        }

        protected override void OnUpdate()
        {            
            if (!Input.GetKeyDown(KeyCode.T)) return;
            Debug.Log("PlayerTargetSystem Adding");
            EntityManager.AddComponent(m_GroupPlayerMouseOver, ComponentType.ReadOnly<PlayerTarget>());
        }
    }
}
