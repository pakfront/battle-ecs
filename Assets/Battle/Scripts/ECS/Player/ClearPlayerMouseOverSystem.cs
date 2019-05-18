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
      private EntityQuery m_PlayerSelection, m_PlayerTarget, m_PlayerFollow;
      protected override void OnCreate()
        {
            base.OnCreate();
            m_PlayerSelection = GetEntityQuery(ComponentType.ReadOnly<PlayerSelection>());
            m_PlayerTarget = GetEntityQuery(ComponentType.ReadOnly<PlayerSelection>());
            m_PlayerFollow = GetEntityQuery(ComponentType.ReadOnly<PlayerFollow>());
        }

        protected override void OnUpdate()
        {
            // selection modifier pressed, early exit;
            if ( Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                 Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                ) return;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Clearing PlayerSelection");
                EntityManager.RemoveComponent(m_PlayerSelection, ComponentType.ReadOnly<PlayerSelection>());
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("Clearing PlayerTarget");
                EntityManager.RemoveComponent(m_PlayerTarget, ComponentType.ReadOnly<PlayerTarget>());
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Clearing PlayerFollow");
                EntityManager.RemoveComponent(m_PlayerFollow, ComponentType.ReadOnly<PlayerFollow>());
            }
            


        }
    }
}