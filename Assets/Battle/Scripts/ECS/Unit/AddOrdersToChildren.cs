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
                    ComponentType.ReadOnly<Goal>()
                },
                None = new ComponentType[]
                {
                    typeof(UnitGroupMember)
                },
            };


            allRootsGroup = GetEntityQuery(rootsQueryDesc);

            var fmtDesc = rootsQueryDesc;
            fmtDesc.All = fmtDesc.All.Concat(new ComponentType[] { ComponentType.ReadOnly<OrderUnitGroupMoveTo>() }).ToArray();
            fmtRootsGroup = GetEntityQuery(fmtDesc);

        }
        protected override void OnUpdate()
        {
            Entities.With(allRootsGroup).ForEach((Entity entity, DynamicBuffer<UnitGroupChildren> children, ref Goal goal) =>
            {
                // var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);
                if (EntityManager.HasComponent<OrderUnitGroupMoveTo>(entity))
                {
                    var order = EntityManager.GetComponentData<OrderUnitGroupMoveTo>(entity);
                    goal.Value = order.Goal;

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

            var goal = new Goal();
            Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref goal.Value);
            EntityManager.SetComponentData(entity, goal);



            // var order = new NextGoal();
            // Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref order.Goal);
            // EntityManager.SetComponentData(entity, order);
            // EntityManager.AddComponentData(entity, order);


            if (EntityManager.HasComponent<UnitGroupChildren>(entity))
            {
                var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);
                for (int i = 0; i < children.Length; i++)
                {
                    AddOrderMoveToToChild(goal.Value, children[i].Value);
                    // AddOrderMoveToToChild(order.Goal, children[i].Value);
                }
            }
        }
    }
}