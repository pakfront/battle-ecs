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
            var playerPointer = GetSingleton<PlayerPointer>();

            // selection modifier pressed, early exit;
            if ( (playerPointer.Click &  (uint)(EClick.AnyModifier)) != 0) 
            {
                return;
            }

            // clear current selections
            if ( (playerPointer.Click &  (uint)(EClick.PrimaryPointerButton)) != 0) 
            {
                Debug.Log("Clearing PlayerSelection");
                EntityManager.RemoveComponent(m_PlayerSelection, ComponentType.ReadOnly<PlayerSelection>());
            }

            if ( (playerPointer.Click &  (uint)(EClick.SecondaryPointerButton)) != 0) 
            {
                Debug.Log("Clearing PlayerTarget & PlayerFollow");
                EntityManager.RemoveComponent(m_PlayerTarget, ComponentType.ReadOnly<PlayerTarget>());
                EntityManager.RemoveComponent(m_PlayerFollow, ComponentType.ReadOnly<PlayerFollow>());
            }
        }
    }
}
