// using Unity.Burst;
// using UnityEngine;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Collections;

// namespace UnitAgent
// {
//     // [DisableAutoCreation] 
//     [UpdateInGroup(typeof(PlayerSystemGroup))]
//     [UpdateAfter(typeof(PlayerOrderPreSystem))]
//     [UpdateBefore(typeof(PlayerOrderPostSystem))]
//     public class PlayerOrderMoveToSystem : JobComponentSystem
//     {
//         [BurstCompile]
//         [RequireComponentTag(typeof(PlayerOwned))]
//         struct SetOrderMoveTo : IJobForEach<OrderMoveTo>
//         {
//             [ReadOnly] public float3 ClickLocation;

//             public void Execute(ref OrderMoveTo orderMoveTo)
//             {
//                 orderMoveTo.Position = ClickLocation; // + some offset
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var playerPointer = GetSingleton<PlayerPointer>();

//             var outputDeps = new SetOrderMoveTo
//             {
//                 ClickLocation = playerPointer.WorldHitPosition
//             }.Schedule(this, inputDeps);


//             return outputDeps;
//         }
//     }
// }