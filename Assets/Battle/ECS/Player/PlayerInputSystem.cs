using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace UnitAgent
{
    // use ComponentSystem so we can create entities
    // [DisableAutoCreation] 
    [UpdateAfter(typeof(PlayerSelectionSystem))]
    public class PlayerInputSystem : JobComponentSystem
    {
        private Plane groundplane = new Plane(Vector3.up, 0);

        private EntityQuery m_Group;
        private EntityQuery m_PlayerSelectedNoGoal;

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(
               new EntityQueryDesc
               {
                   All = new ComponentType[] { typeof(GoalMoveTo), ComponentType.ReadOnly<PlayerSelected>() }
               });

            m_PlayerSelectedNoGoal = GetEntityQuery(
               new EntityQueryDesc
               {
                   None = new ComponentType[] { typeof(GoalMoveTo) },
                   All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelected>() }
               });
        }

        [BurstCompile]
        struct SetGoalJob : IJobChunk
        {
            public ArchetypeChunkComponentType<GoalMoveTo> GoalMoveToType;
            [ReadOnly] public float3 ClickLocation;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var goalMoveTo = chunk.GetNativeArray(GoalMoveToType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    // some computed offset from the click location
                    goalMoveTo[i] =
                        new GoalMoveTo { Position = ClickLocation + new float3(i, 0, chunkIndex) };
                }
            }
        }

        //  can burst compile
        // [BurstCompile]
        // [RequireComponentTag(typeof(PlayerSelected))]
        // struct SetGoalJob : IJobForEach<GoalMoveTo>
        // {
        //     [ReadOnly] public float3 ClickLocation;

        //     public void Execute(ref GoalMoveTo goalMoveTo)
        //     {
        //         goalMoveTo.Position = ClickLocation; // + some offset
        //     }
        // }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!Input.GetMouseButtonDown(1)) return inputDeps;

            //Create a ray from the Mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float enter = 0.0f;

            if (!groundplane.Raycast(ray, out enter)) return inputDeps;

            Vector3 clickLocation = ray.GetPoint(enter);
            Debug.Log("PlayerInputSystem clickLocation "+clickLocation);

            // var m_PlayerSelectedNoGoal = GetEntityQuery(
            //    new EntityQueryDesc
            //    {
            //        None = new ComponentType[] { typeof(GoalMoveTo) },
            //        All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelected>() }
            //    });

            EntityManager.AddComponent(m_PlayerSelectedNoGoal, typeof(GoalMoveTo));

            // var job = new SetGoalJob
            // {
            //     ClickLocation = (float3)clickLocation
            // };
            var job = new SetGoalJob
            {
                ClickLocation = (float3)clickLocation,
                GoalMoveToType = GetArchetypeChunkComponentType<GoalMoveTo>(false)
            };

            return job.Schedule(m_Group, inputDeps);
        }
    }
}