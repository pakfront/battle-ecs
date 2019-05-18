using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateAfter(typeof(PlayerSelectionSystem))]
    public class PlayerInputSystem : JobComponentSystem
    {
        private Plane groundplane = new Plane(Vector3.up, 0);

        private EntityQuery m_PlayerSelectedPlayerOwnedNoGoal;

        protected override void OnCreate()
        {
            m_PlayerSelectedPlayerOwnedNoGoal = GetEntityQuery( new EntityQueryDesc
               {
                   None = new ComponentType[] { typeof(OrderMoveTo) },
                   All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>()  }
               });
        }

        [BurstCompile]
        [RequireComponentTag(typeof(PlayerSelection), typeof(PlayerOwned))]
        struct SetGoalOnPlayerOwned : IJobForEach<OrderMoveTo>
        {
            [ReadOnly] public float3 ClickLocation;

            public void Execute(ref OrderMoveTo goalMoveTo)
            {
                goalMoveTo.Position = ClickLocation; // + some offset
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!Input.GetMouseButtonDown(1)) return inputDeps;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float enter = 0.0f;

            if (!groundplane.Raycast(ray, out enter)) return inputDeps;

            Vector3 clickLocation = ray.GetPoint(enter);
            Debug.Log("PlayerInputSystem clickLocation "+clickLocation);

            EntityManager.AddComponent(m_PlayerSelectedPlayerOwnedNoGoal, typeof(OrderMoveTo));

            var job = new SetGoalOnPlayerOwned
            {
                ClickLocation = (float3)clickLocation
            };
 
            return job.Schedule(this, inputDeps);
        }
    }
}