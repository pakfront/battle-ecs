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
        public NativeArray<float3> FormationOffsets;
        public NativeArray<int> FormationTypes;
        protected override void OnCreate()
        {
            Formation.CalcUnitFormations(out float3[] formationOffsets, out int[] formationTypes);
            FormationOffsets = new NativeArray<float3>(formationOffsets, Allocator.Persistent);
            FormationTypes = new NativeArray<int>(formationTypes, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            FormationOffsets.Dispose();
        }

        [BurstCompile]
        // TODO run only when parent has changed formation
        [RequireComponentTag(typeof(OrderFormationMoveTo))]
        struct SetFormationMemberDataJob : IJobForEach<FormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<FormationLeader> Leaders;
            [ReadOnly] public NativeArray<float3> Offsets;
            public void Execute(ref FormationMember formationElement)
            {
                Entity parent = formationElement.Parent;
                int formationStartIndex = Leaders[parent].FormationStartIndex;
                formationElement.FormationIndex = formationStartIndex + formationElement.MemberIndex;
                formationElement.PositionOffset = Offsets[formationElement.FormationIndex];
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OrderFormationMoveTo))]
        [ExcludeComponent(typeof(Detached))]
        struct SetGoalJob : IJobForEach<MoveToGoal, FormationMember>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref FormationMember formationMember)
            {
                Entity parent = formationMember.Parent;
                float4x4 xform = Others[parent].Value;
                // goal.Position = math.transform(xform, new float3(0,0,0));
                goal.Position = math.transform(xform, formationMember.PositionOffset);
                // heterogenous as it's a direction vector;
                goal.Heading = math.mul(xform, new float4(0, 0, 1, 0)).xyz;
                // Movement.SetGoalToFormationPosition(xform, formationElement.PositionOffset, ref goal.Position, ref goal.Heading);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {

            var outputDeps = inputDependencies;
            
            outputDeps = new SetFormationMemberDataJob()
            {
                Leaders = GetComponentDataFromEntity<FormationLeader>(true),
                Offsets = FormationOffsets
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
