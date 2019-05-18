// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Burst;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace UnitAgent
// {
//     // cribbed from 
//     // https://forum.unity.com/threads/how-do-you-get-a-bufferfromentity-or-componentdatafromentity-without-inject.587857/#post-3924478
//     [UpdateBefore(typeof(MoveToGoalSystem))]
//     public class FollowToGoalSystem : JobComponentSystem
//     {

//         // TODO run only when unit has moved
//         [BurstCompile]
//         struct SetGoalJob : IJobForEach<MoveToGoal, Follow>
//         {
//             [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Units;
//             public void Execute(ref MoveToGoal goal, [ReadOnly] ref Follow follow)
//             {
//                 Entity target = follow.Target;
//                 float4x4 xform = Units[target].Value;
//                 goal.Position = math.mul(xform, follow.Offset).xyz;
//                 // heterogenous as it's a direction vector;
//                 goal.Heading = math.mul( xform, new float4(0,0,1,0) ).xyz;
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDependencies)
//         {
//             var setGoalJob = new SetGoalJob()
//             {
//                 Units = GetComponentDataFromEntity<LocalToWorld>(true)
//             };

//             return setGoalJob.Schedule(this, inputDependencies);
//         }
//     }
// }
