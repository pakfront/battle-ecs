using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace UnitAgent
{
    public class InputSystem : JobComponentSystem
    {

        Plane groundplane = new Plane(Vector3.up, 0);

        public struct SetGoal : IJobForEach<Goal>
        {
            public float3 HitPoint;

            public void Execute(ref Goal goal)
            {
                goal.Position = HitPoint;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!Input.GetMouseButton(0)) return inputDeps;

            //Create a ray from the Mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float enter = 0.0f;

            if (!groundplane.Raycast(ray, out enter)) return inputDeps;

            Vector3 hitPoint = ray.GetPoint(enter);

            var job = new SetGoal
            {
                HitPoint = (float3)hitPoint
            };
            return job.Schedule(this, inputDeps);
        }
    }
}