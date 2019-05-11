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
//     [UpdateBefore(typeof(AgentSystem))] 
//     public class CombatSystem : JobComponentSystem
//     {
//         private EntityQuery m_CombatGroup;

//         protected override void OnCreateManager()
//         {
//             // Cached access to a set of ComponentData based on a specific query
//             m_CombatGroup = GetEntityQuery(
//                 ComponentType.ReadOnly<Combat>(),
//                 ComponentType.ReadOnly<Team>(),
//                 ComponentType.ReadOnly<Translation>());
//         }

//         [BurstCompile]
//         struct FillArrays : IJobForEachWithEntity<Combat, Team, Translation>
//         {
//             public NativeArray<float3> Positions;
//             public NativeArray<int> Teams;

//             public void Execute(Entity entity, int index, [ReadOnly] ref Combat combat, [ReadOnly] ref Team team, [ReadOnly] ref Translation translation)
//             {
//                 Positions[index] = translation.Value;
//                 Teams[index] = team.Value;
//             }
//         }


//         [BurstCompile]
//         struct FindOpponent : IJobForEach<Combat, Team, Translation>
// {           [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> OtherPositions;
//             [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> OtherTeams;

//             public void Execute([ReadOnly] ref Combat combat, [ReadOnly] ref Team team, [ReadOnly] ref Translation translation)
//             {
//                 float3 position = translation.Value;
//                 int teamIndex = team.Value;

//                 int nearestPositionIndex = -1;
//                 float nearestDistance = float.MaxValue; 

//                 for (int i = 1; i < OtherPositions.Length; i++)
//                 {
//                     if ( OtherTeams[i] == teamIndex) continue;

//                     var targetPosition = OtherPositions[i];
//                     var distance       = math.lengthsq(position-targetPosition);
//                     bool nearest        = distance < nearestDistance;
//                     nearestDistance = math.select(nearestDistance, distance, nearest);
//                     nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
//                 }

//                 // nearestDistance = math.sqrt(nearestDistance);
//                 if (nearestPositionIndex > -1)
//                 {
//                     combat.Position = OtherPositions[nearestPositionIndex];
//                 } else {
//                     //temp debug value
//                     combat.Position = new float3(-1,-1,-1);

//                 }
//             }
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDependencies)
//         {
//             var agentCount  = m_CombatGroup.CalculateLength();
//             var otherPositions = new NativeArray<float3>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
//             var otherTeams = new NativeArray<int>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

//             var fillArraysJob = new FillArrays()
//             {
//                 Positions = otherPositions,
//                 Teams = otherTeams
//             };
//             var fillArraysJobHandle = fillArraysJob.Schedule(this, inputDependencies);

//             var findOpponentJob = new FindOpponent()
//             {
//                 OtherPositions = otherPositions,
//                 OtherTeams = otherTeams
//             };
//             return findOpponentJob.Schedule(this, fillArraysJobHandle);
//         }
//     }
// }
