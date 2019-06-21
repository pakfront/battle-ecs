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
    [UpdateBefore(typeof(AddGoalTags))]
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
        [RequireComponentTag(typeof(OrderUnitMoveToTag))]
        struct OrderMoveToJob : IJobForEach<Goal, OrderedGoal>
        {
            public void Execute(ref Goal goal, [ReadOnly] ref OrderedGoal orderedGoal)
            {
                goal.Value = orderedGoal.Value;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(OrderChangeFormationTag))]
        struct OrderChangeFormationJob : IJobForEach<AgentGroupLeader, OrderedFormation>
        {
            public void Execute(ref AgentGroupLeader agentGroupLeader, [ReadOnly] ref OrderedFormation orderedFormation)
            {
                Formation.SetFormation(orderedFormation.FormationId, ref agentGroupLeader);
                // agentGroupLeader.FormationId = orderedFormation.FormationId;
                // agentGroupLeader.FormationStartIndex = Formation.SetFormation()
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var outputDeps = new OrderMoveToJob
            {
            }.Schedule(this, inputDependencies);

            outputDeps = new OrderChangeFormationJob
            {
            }.Schedule(this, outputDeps);

            return outputDeps;
        }
    }
}