using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    [UpdateInGroup(typeof(PlayerSystemGroup))]
    [UpdateAfter(typeof(PlayerPointerSystem))]
    public class PlayerOrderSystem : ComponentSystem
    {
        // private EntityQuery m_NeedsSnapTo, m_NeedsOrderAttack, m_NeedsOrderMoveTo,
        private EntityQuery m_FormationGroup, m__NeedsSetFormation;

        protected override void OnCreate()
        {

            // m_NeedsSnapTo = GetEntityQuery(new EntityQueryDesc
            // {
            //     All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>() }
            // });


            // m_NeedsOrderAttack = GetEntityQuery(new EntityQueryDesc
            // {
            //     None = new ComponentType[] { typeof(OrderAttack) },
            //     All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>() }
            // });

            // m_NeedsOrderMoveTo = GetEntityQuery(new EntityQueryDesc
            // {
            //     None = new ComponentType[] { typeof(OrderMoveTo) },
            //     All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>() }
            // });


            m_FormationGroup = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(OrderFormationMoveTo) },
                All = new ComponentType[] { ComponentType.ReadOnly<UnitGroup>(), ComponentType.ReadOnly<PlayerOwned>() }
            });

            m__NeedsSetFormation = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<UnitGroupLeader>(), ComponentType.ReadOnly<PlayerOwned>() }
            });
        }

        protected override void OnUpdate()
        {
            var playerPointer = GetSingleton<PlayerInput>();

            // // MultiSelection add to all with PlayerSelection
            // if (playerPointer.Click == (uint)EClick.MoveTo)
            // {
            //     Debug.Log("PlayerOrderPreSystem EClick.MoveTo");
            //     EntityManager.AddComponent(m_NeedsOrderMoveTo, typeof(OrderMoveTo));
            //     // EntityManager.SetComponent(m_NeedsSnapTo, new Translation { Value = playerPointer.WorldHitPosition} );
            //     return;
            // }



            // Apply to single selection only
            if (playerPointer.CurrentEntity == Entity.Null)
            {
                // Debug.Log("PlayerOrderPreSystem: No Entity Selected");
                return;
            }

            if (EntityManager.HasComponent<UnitGroupLeader>(playerPointer.CurrentEntity))
            {
                // m_NeedsOrderFormationMoveTo.ClearFilter();
                if (playerPointer.Click == (uint)EClick.MoveTo)
                {
                    Debug.Log("Adding FormationMoveTo " + playerPointer.CurrentEntity);
                    EntityManager.SetComponentData(playerPointer.CurrentEntity, new Translation { Value = playerPointer.WorldHitPosition });
                    m_FormationGroup.SetFilter(new UnitGroup { Parent = playerPointer.CurrentEntity });
                    EntityManager.AddComponent(m_FormationGroup, typeof(OrderFormationMoveTo));
                    return;
                }

                if (playerPointer.FormationId != (int)EFormation.None)
                {
                    Debug.Log("Setting FormationLeader Formation " + playerPointer.CurrentEntity + " to " + playerPointer.FormationId + " " + (EFormation)playerPointer.FormationId);
                    EntityManager.SetComponentData(playerPointer.CurrentEntity,
                        new UnitGroupLeader
                        {
                            CurrentFormation = playerPointer.FormationId,
                            FormationStartIndex = Formation.CalcUnitFormationStartIndex(playerPointer.FormationId, 0)
                        });
                    m_FormationGroup.SetFilter(new UnitGroup { Parent = playerPointer.CurrentEntity });
                    EntityManager.AddComponent(m_FormationGroup, typeof(OrderFormationMoveTo));
                    return;
                }
                return;
            }


            if (EntityManager.HasComponent<Unit>(playerPointer.CurrentEntity))
            {
                if (playerPointer.Click == (uint)EClick.MoveTo)
                {
                    Debug.Log("Adding OrderMoveTo " + playerPointer.CurrentEntity + " to " + playerPointer.WorldHitPosition);
                    EntityManager.AddComponentData(playerPointer.CurrentEntity,
                        new OrderMoveTo
                        {
                            Position = playerPointer.WorldHitPosition,
                            Heading = new float3(0, 0, 1) // TODO get current/best heading
                        });
                    return;
                }

                if (playerPointer.FormationId != (int)EFormation.None)
                {
                    Debug.Log("Adding OrderFormation " + playerPointer.CurrentEntity + " to " + playerPointer.WorldHitPosition);
                    EntityManager.AddComponentData(playerPointer.CurrentEntity,
                        new OrderFormation
                        {
                            FormationId = playerPointer.FormationId
                        });
                    return;
                }
                return;
            }
            // Debug.Log("PlayerOrderPreSystem: Entity not valid for orders");

        }
    }
}
