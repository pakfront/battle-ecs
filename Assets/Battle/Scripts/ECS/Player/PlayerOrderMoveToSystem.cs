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

            public void Execute(ref OrderMoveTo goalMoveTo)
            {
                goalMoveTo.Position = ClickLocation; // + some offset
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerClickTerrain = GetSingleton<PlayerPointer>();
            if ( (playerClickTerrain.Click & (uint)EClick.Terrain) != (uint)EClick.Terrain ) return inputDeps;

            Debug.Log("PlayerOrderMoveSystem DidClick:"+playerClickTerrain.Click+" "+playerClickTerrain.Position );

            var outputDeps = new SetOrderMoveTo
            {
                ClickLocation = playerClickTerrain.Position
            }.Schedule(this, inputDeps);
 
            return outputDeps;
        }
    }
}