using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;


namespace UnitAgent
{
    // [DisableAutoCreation]
    [UpdateBefore(typeof(UnitOrderPreSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class AddOrdersToChildrenSystem : ComponentSystem
    {
        private EntityQuery allRootsGroup, fmtRootsGroup;

        protected override void OnCreate()
        {
            var rootsQueryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    // ComponentType.ReadOnly<UnitGroupLeader>(),
                    ComponentType.ReadOnly<UnitGroupChildren>(),
                    ComponentType.ReadOnly<OrderedGoal>()
                },
                None = new ComponentType[]
                {
                    typeof(UnitGroupMember)
                },
            };


            allRootsGroup = GetEntityQuery(rootsQueryDesc);

            var fmtDesc = rootsQueryDesc;
            fmtDesc.All = fmtDesc.All.Concat(new ComponentType[] { ComponentType.ReadOnly<OrderUnitGroupMoveToTag>() }).ToArray();
            fmtRootsGroup = GetEntityQuery(fmtDesc);

        }
        protected override void OnUpdate()
        {
            Entities.With(allRootsGroup).ForEach((Entity entity, DynamicBuffer<UnitGroupChildren> children, ref OrderedGoal goal) =>
            {
                // var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);
                //FIXME this is broken if oders not on root!
                if (EntityManager.HasComponent<OrderUnitGroupMoveToTag>(entity))
                {
                    // var orderedGoal = EntityManager.GetComponentData<OrderedGoal>(entity);
                    // goal.Value = orderedGoal.Goal;

                    for (int i = 0; i < children.Length; i++)
                    {
                        AddOrderMoveToToChild(goal.Value, children[i].Value);
                    }
                }
            });
        }

        void AddOrderMoveToToChild(float4x4 parentXform, Entity entity)
        {
            Debug.Log("AddOrderMoveToToChild "+entity);

            var unitGroupMember = EntityManager.GetComponentData<UnitGroupMember>(entity);

            var orderedGoal = new OrderedGoal();
            Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref orderedGoal.Value);
            EntityManager.SetComponentData(entity, orderedGoal);
            // EntityManager.AddComponent(entity, typeof(OrderMoveTo));
            PostUpdateCommands.AddComponent<OrderMoveToTag>(entity, new OrderMoveToTag {});

            // var order = new NextGoal();
            // Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref order.Goal);
            // EntityManager.SetComponentData(entity, order);
            // EntityManager.AddComponentData(entity, order);


            if (EntityManager.HasComponent<UnitGroupChildren>(entity))
            {
                var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);
                for (int i = 0; i < children.Length; i++)
                {
                    AddOrderMoveToToChild(orderedGoal.Value, children[i].Value);
                    // AddOrderMoveToToChild(order.Goal, children[i].Value);
                }
            }
        }
    }
}