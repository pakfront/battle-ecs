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
    [UpdateBefore(typeof(UnitOrderSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UnitGroupOrderSystem : ComponentSystem
    {
        private EntityQuery allUnitGroups, UGMTUnitGroups, CFUnitGroups;

        public NativeArray<float3> UnitFormationOffsetTable;
        public NativeArray<int> UnitFormationSubIdTable;
        protected override void OnCreate()
        {
            Formation.CalcUnitFormationTables(out float3[] formationOffsets, out int[] formationTypes);
            UnitFormationOffsetTable = new NativeArray<float3>(formationOffsets, Allocator.Persistent);
            UnitFormationSubIdTable = new NativeArray<int>(formationTypes, Allocator.Persistent);
            Debug.Log(this + " FormationOffsetsTable:" + UnitFormationOffsetTable.Length + " SubformationIdsTable:" + UnitFormationSubIdTable.Length);

            var all = new ComponentType[]
                {
                    // typeof(Goal),
                    ComponentType.ReadOnly<OrderedGoal>(),
                    ComponentType.ReadOnly<UnitGroupLeader>(),
                    ComponentType.ReadOnly<UnitGroupChildren>(),
                };

            var allUnitGroupsDesc = new EntityQueryDesc {
                All = all
            };
            allUnitGroups = GetEntityQuery(allUnitGroupsDesc);


            var CFDesc = new EntityQueryDesc {
                All = all.Concat(new ComponentType[] { ComponentType.ReadOnly<OrderChangeFormationTag>() }).ToArray()
            };
            CFUnitGroups = GetEntityQuery(CFDesc);

            var UGMTDesc = new EntityQueryDesc {
                All = all.Concat(new ComponentType[] { ComponentType.ReadOnly<OrderUnitGroupMoveToTag>() }).ToArray()
            };
            UGMTUnitGroups = GetEntityQuery(UGMTDesc);


        }
        protected override void OnUpdate()
        {
            Entities.With(UGMTUnitGroups).ForEach((Entity entity, DynamicBuffer<UnitGroupChildren> children, ref UnitGroupLeader leader, ref OrderedGoal orderedGoal, ref OrderedFormation orderedFormation) =>
            {

                Formation.SetFormation(orderedFormation.FormationId, ref leader);
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i].Value;

                    if (EntityManager.HasComponent<UnitGroupChildren>(child))
                    {
                        ProcessUnitGroup(child, leader, orderedGoal.Value);
                    }
                    else
                    {
                        ProcessUnit(child, leader, orderedGoal.Value);//, leader, goal.Value, orderedFormation.FormationId);
                    }
                }
            });

            Entities.With(CFUnitGroups).ForEach((Entity entity, DynamicBuffer<UnitGroupChildren> children, ref UnitGroupLeader leader, ref OrderedGoal orderedGoal, ref OrderedFormation orderedFormation) =>
            {
                Formation.SetFormation(orderedFormation.FormationId, ref leader);
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i].Value;

                    if (EntityManager.HasComponent<UnitGroupChildren>(child))
                    {
                        ProcessUnitGroup(child, leader, orderedGoal.Value);
                    }
                    else
                    {
                        ProcessUnit(child, leader, orderedGoal.Value);//, leader, goal.Value, orderedFormation.FormationId);
                    }
                }
            });
        }

        void ProcessUnitGroup(Entity entity, UnitGroupLeader parent, float4x4 parentXform)
        {
            Debug.Log("ProcessUnitGroup " + entity);
            var unitGroupMember = EntityManager.GetComponentData<UnitGroupMember>(entity);
            int formationTableIndex = parent.FormationStartIndex + unitGroupMember.MemberIndex;
            int formationId =  UnitFormationSubIdTable[formationTableIndex];
            float3 positionOffset = UnitFormationOffsetTable[formationTableIndex];

            var orderedGoal = new OrderedGoal();
            Movement.SetGoalToFormationPosition(parentXform, positionOffset, ref orderedGoal.Value);
            EntityManager.SetComponentData(entity, orderedGoal);

            var unitGroupLeader = EntityManager.GetComponentData<UnitGroupLeader>(entity);
            Formation.SetFormation(formationId, ref unitGroupLeader);


            EntityManager.SetComponentData(entity, new OrderedFormation { FormationId = formationId });
            
            // this is a bit of a hack, it should be set via handling an order if there is a delay
            unitGroupMember.FormationId = formationId;

            EntityManager.SetComponentData(entity, unitGroupMember);

            var children = EntityManager.GetBuffer<UnitGroupChildren>(entity);

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i].Value;

                if (EntityManager.HasComponent<UnitGroupChildren>(child))
                {
                    ProcessUnitGroup(child, parent, orderedGoal.Value);
                }
                else
                {
                    ProcessUnit(child, parent, orderedGoal.Value);//, leader, goal.Value, formation.FormationId);
                }
            }
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
            PostUpdateCommands.AddComponent<OrderUnitMoveToTag>(entity, new OrderUnitMoveToTag { });

            EntityManager.SetComponentData(entity, new OrderedFormation { FormationId = formationId });
            // EntityManager.AddComponent(entity, typeof(OrderMoveTo));
            PostUpdateCommands.AddComponent<OrderChangeFormationTag>(entity, new OrderChangeFormationTag { });
        }

        protected override void OnDestroy()
        {
            UnitFormationOffsetTable.Dispose();
            UnitFormationSubIdTable.Dispose();
        }
    }
}