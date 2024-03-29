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
//     public class PlayerOrderFormationMoveToSystem : JobComponentSystem
//     {
//         [BurstCompile]
//         [RequireComponentTag(typeof(PlayerOwned))]
//         // [ChangedFilter(typeof(OrderFormationMoveTo))]
//         struct SetOrderFormationMoveTo : IJobForEach<OrderFormationMoveTo, FormationMember>
//         {
//             [ReadOnly] public float4x4 ClickTransform;

//             public void Execute(ref OrderFormationMoveTo orderMoveTo, [ReadOnly] ref FormationMember formationMember)
//             {
//                 Movement.SetGoalToFormationPosition(ClickTransform, formationMember.PositionOffset, ref orderMoveTo.Position, ref orderMoveTo.Heading);
//                 // Debug.Log("Execute "+orderMoveTo.Position);
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var playerPointer = GetSingleton<PlayerPointer>();
            
//             var clickTransform = float4x4.Translate(playerPointer.WorldHitPosition);

//             // Debug.Log("PlayerOrderFormationMoveToSystem DidClick:" + playerPointer.Click + " " +clickTransform);

//             var outputDeps = new SetOrderFormationMoveTo
//             {
//                 ClickTransform = clickTransform
//             }.Schedule(this, inputDeps);

//             return outputDeps;
//         }
//     }
// }