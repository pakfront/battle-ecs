using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent
{
    // cribbed from 
    // https://forum.unity.com/threads/how-do-you-get-a-bufferfromentity-or-componentdatafromentity-without-inject.587857/#post-3924478
    [UpdateAfter(typeof(UnitOrderPreSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UnitOrderFormationMoveToSystem : JobComponentSystem
    {
        public NativeArray<float3> UnitFormationOffsetTable;
        public NativeArray<int> UnitFormationSubIdTable;
        protected override void OnCreate()
        {
            Formation.CalcUnitFormationTables(out float3[] formationOffsets, out int[] formationTypes);
            UnitFormationOffsetTable = new NativeArray<float3>(formationOffsets, Allocator.Persistent);
            UnitFormationSubIdTable = new NativeArray<int>(formationTypes, Allocator.Persistent);
            Debug.Log("FormationOffsetsTable:"+UnitFormationOffsetTable.Length+" SubformationIdsTable:"+UnitFormationSubIdTable.Length);
        }

        protected override void OnDestroy()
        {
            UnitFormationOffsetTable.Dispose();
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OrderFormationMoveTo))]
        struct SetFormationMemberDataJob : IJobForEach<UnitGroupMember>
        {
            [ReadOnly] public ComponentDataFromEntity<UnitGroupLeader> Leaders;
            [ReadOnly] public NativeArray<float3> FormationOffsetsTable;
            [ReadOnly] public NativeArray<int> SubformationTable;
            public void Execute(ref UnitGroupMember formationElement)
            {
                Entity parent = formationElement.Parent;
                int formationTableIndex = Leaders[parent].FormationStartIndex + formationElement.MemberIndex;
                formationElement.PositionOffset = FormationOffsetsTable[formationTableIndex];
                formationElement.FormationId = SubformationTable[formationTableIndex];
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OrderFormationMoveTo),typeof(LocalToWorld))]
        [ExcludeComponent(typeof(Detached))]
        struct SetGoalJob : IJobForEach<MoveToGoal, UnitGroupMember>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref UnitGroupMember formationMember)
            {
                Entity parent = formationMember.Parent;
                float4x4 xform = Others[parent].Value;
                Movement.SetGoalToFormationPosition(xform, formationMember.PositionOffset, ref goal.Position, ref goal.Heading);
                // // goal.Position = math.transform(xform, new float3(0,0,0));
                // goal.Position = math.transform(xform, formationMember.PositionOffset);
                // // heterogenous as it's a direction vector;
                // goal.Heading = math.mul(xform, new float4(0, 0, 1, 0)).xyz;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {

            var outputDeps = inputDependencies;
            
            outputDeps = new SetFormationMemberDataJob()
            {
                Leaders = GetComponentDataFromEntity<UnitGroupLeader>(true),
                FormationOffsetsTable = UnitFormationOffsetTable,
                SubformationTable = UnitFormationSubIdTable
            }.Schedule(this, outputDeps);

            //if moved or formation changed
            outputDeps = new SetGoalJob()
            {
                Others = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}
