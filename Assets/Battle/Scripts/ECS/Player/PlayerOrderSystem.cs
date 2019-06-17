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
                None = new ComponentType[] { typeof(OrderUnitGroupMoveToTag) },
                All = new ComponentType[] { ComponentType.ReadOnly<UnitGroup>(), ComponentType.ReadOnly<PlayerOwnedTag>() }
            });

            m__NeedsSetFormation = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<UnitGroupLeader>(), ComponentType.ReadOnly<PlayerOwnedTag>() }
            });
        }

        protected override void OnUpdate()
        {
            var playerPointer = GetSingleton<PlayerInput>();

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
                    Debug.Log("Adding OrderUnitGroupMoveToTag: " + playerPointer.CurrentEntity);
                    // HACK set both so that it moves an it's xform are correct for children
                    var goal = EntityManager.GetComponentData<OrderedGoal>(playerPointer.CurrentEntity);
                    Movement.SetTranslation(playerPointer.WorldHitPosition, ref goal.Value);

                    EntityManager.AddComponent(playerPointer.CurrentEntity, typeof(OrderUnitGroupMoveToTag));
                    EntityManager.SetComponentData(playerPointer.CurrentEntity,
                    new OrderedGoal
                    {
                        Value = goal.Value
                    });

                    // EntityManager.SetComponentData(playerPointer.CurrentEntity,
                    //     new Translation { Value = playerPointer.WorldHitPosition }
                    // );
                    // m_FormationGroup.SetFilter(new UnitGroup { Parent = playerPointer.CurrentEntity });
                    // EntityManager.AddComponent(m_FormationGroup, typeof(OrderFormationMoveTo));
                    return;
                }

                if (playerPointer.FormationId != (int)EFormation.None)
                {
                    Debug.Log("Adding OrderChangeFormationTag: " + playerPointer.CurrentEntity + " to " + playerPointer.FormationId + " " + (EFormation)playerPointer.FormationId);
                    EntityManager.AddComponent(playerPointer.CurrentEntity, typeof(OrderChangeFormationTag));                    
                    EntityManager.SetComponentData(playerPointer.CurrentEntity,
                        new OrderedFormation
                        {
                            FormationId = playerPointer.FormationId,
                        });
                    // m_FormationGroup.SetFilter(new UnitGroup { Parent = playerPointer.CurrentEntity });
                    // EntityManager.AddComponent(m_FormationGroup, typeof(OrderFormationMoveTo));
                    return;
                }
                return;
            }


            if (EntityManager.HasComponent<Unit>(playerPointer.CurrentEntity))
            {
                if (playerPointer.Click == (uint)EClick.MoveTo)
                {
                    Debug.Log("Adding OrderUnitMoveToTag: " + playerPointer.CurrentEntity + " to " + playerPointer.WorldHitPosition);
                    var goal = EntityManager.GetComponentData<Goal>(playerPointer.CurrentEntity);
                    Movement.SetTranslation(playerPointer.WorldHitPosition, ref goal.Value);

                    EntityManager.AddComponent(playerPointer.CurrentEntity, typeof(OrderUnitMoveToTag));
                    EntityManager.SetComponentData(playerPointer.CurrentEntity,
                    new OrderedGoal
                    {
                        Value = goal.Value
                    });
                    // EntityManager.AddComponentData(playerPointer.CurrentEntity, new DetachedTag());
                    return;
                }

                if (playerPointer.FormationId != (int)EFormation.None)
                {
                    Debug.Log("Adding OrderChangeFormationTag " + playerPointer.CurrentEntity + " to " + playerPointer.WorldHitPosition);
                    EntityManager.AddComponent(playerPointer.CurrentEntity, typeof(OrderChangeFormationTag));

                    EntityManager.SetComponentData(playerPointer.CurrentEntity,
                    new OrderedFormation
                    {
                        FormationId = playerPointer.FormationId
                    });
                    // EntityManager.AddComponentData(playerPointer.CurrentEntity, new DetachedTag());
                    return;
                }
                return;
            }
            // Debug.Log("PlayerOrderPreSystem: Entity not valid for orders");

        }
    }
}
