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
    public class PlayerOrderPreSystem : ComponentSystem
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
                All = new ComponentType[] { ComponentType.ReadOnly<FormationGroup>(), ComponentType.ReadOnly<PlayerOwned>() }
            });

            m__NeedsSetFormation = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<FormationLeader>(), ComponentType.ReadOnly<PlayerOwned>() }
            });
        }

        protected override void OnUpdate()
        {
            var playerPointer = GetSingleton<PlayerPointer>();

            // // MultiSelection add to all with PlayerSelection
            // if (playerPointer.Click == (uint)EClick.MoveTo)
            // {
            //     Debug.Log("PlayerOrderPreSystem EClick.MoveTo");
            //     EntityManager.AddComponent(m_NeedsOrderMoveTo, typeof(OrderMoveTo));
            //     // EntityManager.SetComponent(m_NeedsSnapTo, new Translation { Value = playerPointer.WorldHitPosition} );
            //     return;
            // }



            // Apply to single selection only
            if (playerPointer.CurrentEntity == Entity.Null) {
                // Debug.Log("PlayerOrderPreSystem: No Entity Selected");
                return;
            }

            if (EntityManager.HasComponent<FormationLeader>(playerPointer.CurrentEntity))
            {
                // m_NeedsOrderFormationMoveTo.ClearFilter();
                if (playerPointer.Click == (uint)EClick.MoveTo)
                {
                    Debug.Log("Adding FormationMoveTo "+playerPointer.CurrentEntity);
                    EntityManager.SetComponentData(playerPointer.CurrentEntity, new Translation { Value = playerPointer.WorldHitPosition} );
                    m_FormationGroup.SetFilter( new FormationGroup { Parent = playerPointer.CurrentEntity} );
                    EntityManager.AddComponent(m_FormationGroup, typeof(OrderFormationMoveTo));
                    return;
                }

                if (playerPointer.FormationIndex != (int)EFormation.None)
                {
                    Debug.Log("Setting Unit Formation "+playerPointer.CurrentEntity+" to "+(EFormation)playerPointer.FormationIndex);
                    EntityManager.SetComponentData(playerPointer.CurrentEntity, new FormationLeader { FormationIndex = playerPointer.FormationIndex} );
                    m_FormationGroup.SetFilter( new FormationGroup { Parent = playerPointer.CurrentEntity} );
                    EntityManager.AddComponent(m_FormationGroup, typeof(OrderFormationMoveTo));
                    return;
                }
                return;
            }


            if (EntityManager.HasComponent<Unit>(playerPointer.CurrentEntity))
            {
                if (playerPointer.Click == (uint)EClick.MoveTo)
                {
                    Debug.Log("Adding MoveTo "+playerPointer.CurrentEntity);
                    EntityManager.AddComponentData(playerPointer.CurrentEntity, 
                        new OrderMoveTo { 
                            Position = playerPointer.WorldHitPosition,
                            Heading = new float3(0,0,1) // TODO get current/best heading
                        });
                    return;
                }
                return;
            }
            // Debug.Log("PlayerOrderPreSystem: Entity not valid for orders");

        }
    }
}
