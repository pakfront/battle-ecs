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
        private EntityQuery m_NeedsOrderAttack, m_NeedsOrderMoveTo, m_NeedsOrderFormationMoveTo;

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


            m_NeedsOrderFormationMoveTo = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(OrderFormationMoveTo) },
                All = new ComponentType[] { ComponentType.ReadOnly<FormationGroup>(), ComponentType.ReadOnly<PlayerOwned>() }
            });
        }

        protected override void OnUpdate()
        {
            var playerPointer = GetSingleton<PlayerPointer>();
            if (playerPointer.Click == (uint)EClick.MoveTo)
            {
                Debug.Log("Adding OrderMoveTo");
                EntityManager.AddComponent(m_NeedsOrderMoveTo, typeof(OrderMoveTo));
                return;
            }
            if (playerPointer.Click == (uint)EClick.FormationMoveTo)
            {
                Debug.Log("Adding FormationMoveTo "+playerPointer.CurrentEntity);
                m_NeedsOrderFormationMoveTo.SetFilter( new FormationGroup { Parent = playerPointer.CurrentEntity} );
                EntityManager.AddComponent(m_NeedsOrderFormationMoveTo, typeof(OrderFormationMoveTo));
                EntityManager.SetComponentData(playerPointer.CurrentEntity, new Translation { Value = playerPointer.Position} );
                return;
            }
        }
    }
}
