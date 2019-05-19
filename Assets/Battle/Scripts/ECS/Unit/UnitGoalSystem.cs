using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace UnitAgent
{

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(PlayerAddOrderSystem))]
    public class UnitGoalSystem : JobComponentSystem
    {
        [BurstCompile]
        struct OrderMoveToJob : IJobForEach<MoveToGoal, OrderMoveTo>
        {
            public void Execute(ref MoveToGoal goalMoveTo, [ReadOnly] ref OrderMoveTo detachedMoveTo)
            {
                goalMoveTo.Position = detachedMoveTo.Position;
                goalMoveTo.Heading = new float3(0,0,1);
            }
        }

        // TODO run only when target has moved
        [BurstCompile]
        [ExcludeComponent(typeof(OrderMoveTo))]
        struct OrderPursueJob : IJobForEach<MoveToGoal, OrderPursue>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Others;
            public void Execute(ref MoveToGoal goal, [ReadOnly] ref OrderPursue orderPursue)
            {
                Entity target = orderPursue.Target;
                UnityEngine.Debug.Log("OrderPursueJob target:"+target);
                float4x4 xform = Others[target].Value;
                goal.Position = math.mul (xform, new float4(0,0,0,1)).xyz;
                // heterogenous as it's a direction vector;
                goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
            }
        }

        // TODO run only when target has moved
        // [BurstCompile]
        [ExcludeComponent(typeof(OrderMoveTo),typeof(OrderPursue))]
        struct OrderHoldJob : IJobForEach<OrderHold>
        {
            public void Execute([ReadOnly] ref OrderHold orderPursue)
            {
                // UnityEngine.Debug.Log("OrderHold");
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
 

            var allXforms = GetComponentDataFromEntity<LocalToWorld>(true);

            var outputDeps = new OrderMoveToJob {}.Schedule(this, inputDependencies);

            outputDeps = new OrderPursueJob
            {
                Others = allXforms
            }.Schedule(this, outputDeps);
 
            outputDeps = new OrderHoldJob
            {
            }.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}