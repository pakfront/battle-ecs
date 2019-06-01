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
    public class PlayerOrderMoveToSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(PlayerSelection), typeof(PlayerOwned))]
        struct SetOrderMoveTo : IJobForEach<OrderMoveTo>
        {
            [ReadOnly] public float3 ClickLocation;

            public void Execute(ref OrderMoveTo orderMoveTo)
            {
                orderMoveTo.Position = ClickLocation; // + some offset
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerPointer = GetSingleton<PlayerPointer>();

            if (playerPointer.Click != (uint)EClick.MoveTo) return inputDeps;

            Debug.Log("PlayerOrderMoveSystem DidClick:" + playerPointer.Click + " " + playerPointer.WorldHitPosition);

            var outputDeps = new SetOrderMoveTo
            {
                ClickLocation = playerPointer.WorldHitPosition
            }.Schedule(this, inputDeps);


            return outputDeps;
        }
    }
}