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
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerPointerSystem))]

    public class PlayerPointerPreSystem : ComponentSystem
    {
        private EntityQuery m_PlayerClicked, m_PlayerSelection, m_PlayerTarget, m_PlayerFollow;
        protected override void OnCreate()
        {
            base.OnCreate();
            // m_PlayerClicked = GetEntityQuery(ComponentType.ReadOnly<PlayerClicked>());
            m_PlayerSelection = GetEntityQuery(ComponentType.ReadOnly<PlayerSelection>());
            m_PlayerTarget = GetEntityQuery(ComponentType.ReadOnly<PlayerTarget>());
            m_PlayerFollow = GetEntityQuery(ComponentType.ReadOnly<PlayerFollow>());
        }

        protected override void OnUpdate()
        {
            // clear previous clicks
            // EntityManager.RemoveComponent(m_PlayerClicked, ComponentType.ReadOnly<PlayerClicked>());
            
            // selection modifier pressed, early exit;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                 Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                ) return;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Clearing PlayerSelection");
                EntityManager.RemoveComponent(m_PlayerSelection, ComponentType.ReadOnly<PlayerSelection>());
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Clearing PlayerTarget");
                EntityManager.RemoveComponent(m_PlayerTarget, ComponentType.ReadOnly<PlayerTarget>());
                Debug.Log("Clearing PlayerFollow");
                EntityManager.RemoveComponent(m_PlayerFollow, ComponentType.ReadOnly<PlayerFollow>());
            }
        }
    }
}
