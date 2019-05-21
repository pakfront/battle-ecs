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
    [UpdateAfter(typeof(PlayerOrderAttackSystem))]
    public class PlayerOrderMoveToSystem : JobComponentSystem
    {
        private Plane groundplane = new Plane(Vector3.up, 0);

        private EntityQuery m_PlayerSelectedPlayerOwnedNoMoveToGoal;

        protected override void OnCreate()
        {
            m_PlayerSelectedPlayerOwnedNoMoveToGoal = GetEntityQuery( new EntityQueryDesc
               {
                   None = new ComponentType[] { typeof(OrderMoveTo) },
                   All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelection>(), ComponentType.ReadOnly<PlayerOwned>()  }
               });
        }

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
            if (!Input.GetMouseButtonDown(1)) return inputDeps;

            //TODO make sure we didn't click on gui, etc.

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float enter = 0.0f;

            if (!groundplane.Raycast(ray, out enter)) return inputDeps;

            Vector3 clickLocation = ray.GetPoint(enter);
            Debug.Log("PlayerInputSystem clickLocation "+clickLocation);

            EntityManager.AddComponent(m_PlayerSelectedPlayerOwnedNoMoveToGoal, typeof(OrderMoveTo));

            var job = new SetOrderMoveTo
            {
                ClickLocation = (float3)clickLocation
            };
 
            return job.Schedule(this, inputDeps);
        }
    }
}