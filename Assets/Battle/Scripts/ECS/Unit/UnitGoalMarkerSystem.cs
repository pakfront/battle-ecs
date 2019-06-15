using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace UnitAgent
{

    [UpdateInGroup(typeof(UnitSystemGroup))]
    // [UpdateAfter(typeof(UnitOrderClearExpiredSystem))]
    [UpdateAfter(typeof(UnitOrderSystem))]
    public class UnitGoalMarkerSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UnitGoalMarkerJob : IJobForEach<Rotation, Translation, UnitGoalMarker>
        {
            [ReadOnly] public ComponentDataFromEntity<Goal> Goals;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Xforms;

            public void Execute(ref Rotation rotation, ref Translation translation, [ReadOnly] ref UnitGoalMarker marker)
            {
                Entity target = marker.Unit;
                // is branching an issue here?
                if (Goals.Exists(target))
                {
                    translation.Value = Goals[target].Position;
                    rotation.Value = quaternion.LookRotation(Goals[target].Heading, math.up());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var moveToGoals = GetComponentDataFromEntity<Goal>(true);
            var xforms = GetComponentDataFromEntity<LocalToWorld>(true);

            var outputDeps = new UnitGoalMarkerJob
            {
                Goals = moveToGoals,
                Xforms = xforms
            }.Schedule(this, inputDependencies);

            return outputDeps;
        }
    }
}