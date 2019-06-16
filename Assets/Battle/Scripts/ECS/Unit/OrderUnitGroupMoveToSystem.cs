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
    public class OrderUnitGroupMoveToSystem : ComponentSystem
    {
        private EntityQuery allUnitGroups, fmtUnitGroups;

        public NativeArray<float3> UnitFormationOffsetTable;
        public NativeArray<int> UnitFormationSubIdTable;
        protected override void OnCreate()
        {
            Formation.CalcUnitFormationTables(out float3[] formationOffsets, out int[] formationTypes);
            UnitFormationOffsetTable = new NativeArray<float3>(formationOffsets, Allocator.Persistent);
            UnitFormationSubIdTable = new NativeArray<int>(formationTypes, Allocator.Persistent);
            Debug.Log(this+" FormationOffsetsTable:"+UnitFormationOffsetTable.Length+" SubformationIdsTable:"+UnitFormationSubIdTable.Length);
        

            var rootsQueryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Goal),
                    ComponentType.ReadOnly<OrderedGoal>(),
                    ComponentType.ReadOnly<UnitGroupLeader>(),
                    ComponentType.ReadOnly<UnitGroupChildren>(),
                },
                // None = new ComponentType[]
                // {
                //     typeof(UnitGroupMember)
                // },
            };


            allUnitGroups = GetEntityQuery(rootsQueryDesc);

            var fmtDesc = rootsQueryDesc;
            fmtDesc.All = fmtDesc.All.Concat(new ComponentType[] { ComponentType.ReadOnly<OrderUnitGroupMoveToTag>() }).ToArray();
            fmtUnitGroups = GetEntityQuery(fmtDesc);

        }
        protected override void OnUpdate()
        {
            Entities.With(fmtUnitGroups).ForEach((Entity entity, DynamicBuffer<UnitGroupChildren> children, ref UnitGroupLeader leader, ref OrderedGoal orderedGoal, ref Goal goal, ref OrderedFormation formation) =>
            {
            Debug.Log("OnUpdate " + entity);

                goal.Value = orderedGoal.Value;
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i].Value;

                    if (EntityManager.HasComponent<UnitGroupChildren>(child))
                    {
                        ProcessUnitGroup(child, leader, goal.Value, formation.FormationId);
                    }
                    else
                    {
                        ProcessUnit(child, leader, goal.Value);//, leader, goal.Value, formation.FormationId);
                    }
                }
            });
        }

        void ProcessUnitGroup(Entity entity, UnitGroupLeader leader, float4x4 parentXform, int FormationId)
        {
            Debug.Log("ProcessUnitGroup " + entity);
            // var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);

            // //if it has an order already, skip
            // if (EntityManager.HasComponent<OrderUnitGroupMoveToTag>(entity)) return;

            // var unitGroupMember = EntityManager.GetComponentData<UnitGroupMember>(entity);

            // var orderedGoal = new OrderedGoal();
            // Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref orderedGoal.Value);
            // EntityManager.SetComponentData(entity, orderedGoal);

            // for (int i = 0; i < children.Length; i++)
            // {
            //     var child = children[i].Value;
            //     if (EntityManager.HasComponent<UnitGroupChildren>(child))
            //     {
            //         ProcessUnitGroup(child);
            //     }
            //     else
            //     {
            //         ProcessUnit(child);
            //     }
            // }
        }

        void ProcessUnit(Entity entity, UnitGroupLeader leader, float4x4 leaderXform)
        {
            Debug.Log("ProcessUnit " + entity);
            var unitGroupMember = EntityManager.GetComponentData<UnitGroupMember>(entity);

            int formationTableIndex = leader.FormationStartIndex + unitGroupMember.MemberIndex;
            int formationId = UnitFormationSubIdTable[formationTableIndex];
            float3 positionOffset = UnitFormationOffsetTable[formationTableIndex];
  

            var orderedGoal = new OrderedGoal();
            Movement.SetGoalToFormationPosition(leaderXform, positionOffset, ref orderedGoal.Value);
            EntityManager.SetComponentData(entity, orderedGoal);
            // EntityManager.AddComponent(entity, typeof(OrderMoveTo));
            PostUpdateCommands.AddComponent<OrderMoveToTag>(entity, new OrderMoveToTag { });

            // int formationTableIndex = leader.FormationStartIndex + unitGroupMember.MemberIndex;
            // unitGroupMember.PositionOffset = FormationOffsetsTable[formationTableIndex];
            // unitGroupMember.FormationId = SubformationTable[formationTableIndex];
  
            EntityManager.SetComponentData(entity, new OrderedFormation { FormationId = formationId});
        //     // EntityManager.AddComponent(entity, typeof(OrderMoveTo));
            PostUpdateCommands.AddComponent<OrderChangeFormationTag>(entity, new OrderChangeFormationTag { });

        }

        // void ChildAddOrderMoveTo(float4x4 parentXform, Entity entity)
        // {
        //     Debug.Log("AddOrderMoveToToChild " + entity);

        //     var unitGroupMember = EntityManager.GetComponentData<UnitGroupMember>(entity);

        //     var orderedGoal = new OrderedGoal();
        //     Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref orderedGoal.Value);
        //     EntityManager.SetComponentData(entity, orderedGoal);
        //     // EntityManager.AddComponent(entity, typeof(OrderMoveTo));
        //     PostUpdateCommands.AddComponent<OrderMoveToTag>(entity, new OrderMoveToTag { });

        //     // debug set goal
        //     // var goal = new Goal();
        //     // Movement.SetGoalToFormationPosition(parentXform, unitGroupMember.PositionOffset, ref goal.Value);
        //     // EntityManager.SetComponentData(entity, goal);



        //     if (EntityManager.HasComponent<UnitGroupChildren>(entity))
        //     {
        //         var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);
        //         for (int i = 0; i < children.Length; i++)
        //         {
        //             ChildAddOrderMoveTo(orderedGoal.Value, children[i].Value);
        //             // AddOrderMoveToToChild(order.Goal, children[i].Value);
        //         }
        //     }
        // }
    }
}