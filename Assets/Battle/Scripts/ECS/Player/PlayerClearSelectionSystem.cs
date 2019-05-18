using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{

    [UpdateBefore(typeof(PlayerSelectionSystem))]
    public class ClearPlayerSelectionSystem : ComponentSystem
    {
        private EntityQuery m_GroupPlayerSelected;
      protected override void OnCreate()
        {
            base.OnCreate();
            m_GroupPlayerSelected = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelected>() }
            });
        }

        protected override void OnUpdate()
        {

            if (!Input.GetMouseButtonDown(0)) return;
            
            if ( Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                 Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                ) return;
            Debug.Log("ClearPlayerSelectionSystem Clearing");
            EntityManager.RemoveComponent(m_GroupPlayerSelected, ComponentType.ReadOnly<PlayerSelected>());
        }
    }
}