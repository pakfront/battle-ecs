using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace UnitAgent
{

    [UpdateInGroup(typeof(UnitSystemGroup))]
    [UpdateAfter(typeof(UnitOrderPreSystem))]
    public class UnitOrderSystem : JobComponentSystem
    {
        // private EntityQuery m_Group;
        // protected override void OnCreate()
        // {
        //     m_Group = GetEntityQuery(typeof(Goal), ComponentType.ReadOnly<OrderMoveTo>());
        // }
        // [BurstCompile]
        // struct OrderMoveToJob : IJobChunk
        // {
        //     [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
        //     [ReadOnly] public ArchetypeChunkComponentType<OrderMoveTo> OrderMoveToType;
        //     public ArchetypeChunkComponentType<Goal> GoalType;

        //     public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        //     {
        //         var moveToGoal = chunk.GetNativeArray(GoalType);
        //         var orderMoveTo = chunk.GetNativeArray(OrderMoveToType);

        //         for (int i = 0; i < chunk.Count; i++)
        //         {
        //             moveToGoal[i] = new Goal
        //             {
        //                 Position = orderMoveTo[i].Position,
        //                 Heading = orderMoveTo[i].Heading
        //             };
        //         }
        //     }
        // }


        [BurstCompile]
        struct OrderMoveToJob : IJobForEach<Goal, OrderMoveTo>
        {
            public void Execute(ref Goal goal, [ReadOnly] ref OrderMoveTo orderMoveTo)
            {
                goal.Value = Movement.CalcGoalPositionOnly(orderMoveTo.Position);
                // goal  = new GoalTag
                // {
                //     Position = orderMoveTo.Position,
                //     Heading = new float3(0,0,1)
                // };
                // UnityEngine.Debug.Log("OrderMoveToJob "+goal.Position +" ?= "+ orderMoveTo.Position);
            }
        }

        [BurstCompile]
        struct OrderFormationJob : IJobForEach<UnitGroupMember, OrderFormation>
        {
            public void Execute(ref UnitGroupMember unitFormationMember, [ReadOnly] ref OrderFormation orderFormation)
            {
                unitFormationMember.FormationId = orderFormation.FormationId;
            }
        }

        // // [BurstCompile]
        // struct OrderFormationMoveToJob : IJobForEach<Goal, OrderFormationMoveTo>
        // {
        //     public void Execute(ref Goal goal, [ReadOnly] ref OrderFormationMoveTo orderMoveTo)
        //     {
        //         goal.Position = orderMoveTo.Position;
        //         // goal.Heading = orderMoveTo.Heading;
        //         UnityEngine.Debug.Log("OrderFormationMoveToJob "+goal.Position +" ?= "+ orderMoveTo.Position);
        //     }
        // }

        // [BurstCompile]
        // [ExcludeComponent(typeof(OrderMoveTo))]
        // //TODO only update if target has moved?
        // struct OrderAttackJob : IJobForEach<Goal, OrderAttack>
        // {
        //     [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
        //     public void Execute(ref Goal goal, [ReadOnly] ref OrderAttack OrderAttack)
        //     {
        //         Entity target = OrderAttack.Target;
        //         float4x4 xform = Others[target].Value;
        //         goal.Position = math.mul (xform, new float4(0,0,0,1)).xyz;
        //         // heterogenous as it's a direction vector;
        //         goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
        //     }
        // }

        // // TODO run only when target has moved
        // // [BurstCompile]
        // [ExcludeComponent(typeof(OrderMoveTo),typeof(OrderAttack))]
        // struct OrderHoldJob : IJobForEach<OrderHold>
        // {
        //     public void Execute([ReadOnly] ref OrderHold OrderAttack)
        //     {
        //         // UnityEngine.Debug.Log("OrderHold");
        //     }
        // }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {


            // var job = new OrderMoveToJob
            // {
            //     GoalType = GetArchetypeChunkComponentType<Goal>(false),
            //     OrderMoveToType = GetArchetypeChunkComponentType<OrderMoveTo>(true)
            // };

            // var outputDeps = job.Schedule(m_Group, inputDependencies);

            var outputDeps = new OrderMoveToJob {
            }.Schedule(this, inputDependencies);

            outputDeps = new OrderFormationJob {
            }.Schedule(this, outputDeps);

            // outputDeps = new OrderFormationMoveToJob
            // {
            // }.Schedule(this, outputDeps);

            // var allXforms = GetComponentDataFromEntity<LocalToWorld>(true);
            // outputDeps = new OrderAttackJob
            // {
            //     Others = allXforms
            // }.Schedule(this, outputDeps);

            // outputDeps = new OrderHoldJob
            // {
            // }.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}