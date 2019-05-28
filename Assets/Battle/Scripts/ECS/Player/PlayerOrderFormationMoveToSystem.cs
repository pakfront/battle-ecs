using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(PlayerOrderPreSystem))]
    [UpdateBefore(typeof(PlayerOrderPostSystem))]
    public class PlayerOrderFormationMoveToSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(PlayerSelection), typeof(PlayerOwned))]
        // [ChangedFilter(typeof(OrderFormationMoveTo))]
        struct SetOrderFormationMoveTo : IJobForEach<OrderFormationMoveTo, FormationMember>
        {
            [ReadOnly] public float4x4 ClickTransform;

            public void Execute(ref OrderFormationMoveTo orderMoveTo, [ReadOnly] ref FormationMember formationMember)
            {
                Movement.SetGoalToFormationPosition(ClickTransform, formationMember.Position, ref orderMoveTo.Position, ref orderMoveTo.Heading);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerPointer = GetSingleton<PlayerPointer>();

            if (playerPointer.Click != (uint)EClick.FormationMoveTo) return inputDeps;

            Debug.Log("PlayerOrderFormationMoveToSystem DidClick:" + playerPointer.Click + " " + playerPointer.Position);

            var outputDeps = new SetOrderFormationMoveTo
            {
                ClickTransform = float4x4.Translate(playerPointer.Position)
            }.Schedule(this, inputDeps);


            return outputDeps;
        }
    }
}