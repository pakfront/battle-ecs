using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerOrderMoveToSystem))]
    public class PlayerMouseOverSystem : JobComponentSystem
    {
        private Plane groundplane = new Plane(Vector3.up, 0);

        EntityCommandBufferSystem m_EntityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }

        //  do not burst compile, AddComponent not supported 
        // [BurstCompile]
        [RequireComponentTag(typeof(Unit), typeof(PlayerOwned))]
        struct PlayerSelectionJob : IJobForEachWithEntity<AABB>
        {
            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public RTSRay CameraRay;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, CameraRay))
                {
                    Debug.Log("PlayerSelectionJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerSelection());
                }
            }
        }

        [RequireComponentTag(typeof(Unit), typeof(PlayerOwned))]
        struct PlayerFollowJob : IJobForEachWithEntity<AABB>
        {
            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public RTSRay CameraRay;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, CameraRay))
                {
                    Debug.Log("PlayerSelectionJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerFollow());
                }
            }
        }

        [RequireComponentTag(typeof(Unit), typeof(PlayerEnemy))]
        struct PlayerTargetJob : IJobForEachWithEntity<AABB>
        {
            [ReadOnly] public EntityCommandBuffer CommandBuffer;
            public RTSRay CameraRay;

            public void Execute(Entity entity, int index, [ReadOnly] ref AABB aabb)
            {
                if (RTSPhysics.Intersect(aabb, CameraRay))
                {
                    Debug.Log("PlayerTargetJob: Click on " + index);
                    CommandBuffer.AddComponent(entity, new PlayerTarget());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if ( ! (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) )) return inputDeps;

            return inputDeps;

            // JobHandle outputDeps;
            // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // RTSRay rtsRay = new RTSRay
            // {
            //     origin = ray.origin,
            //     direction = ray.direction
            // };
            // // Vector3 clickLocation = ray.GetPoint(enter);
            // // if (Input.GetMouseButtonDown(0))
            // // {
            // //     outputDeps = new PlayerSelectionJob
            // //     {
            // //         CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            // //         CameraRay = rtsRay,
            // //     }.Schedule(this, inputDeps);
            // // }
            // // else if (Input.GetKeyDown(KeyCode.T))
            // // {
            // //     outputDeps = new PlayerTargetJob
            // //     {
            // //         CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            // //         CameraRay = rtsRay,
            // //     }.Schedule(this, inputDeps);
            // // }
            // // else 
            // if (Input.GetKeyDown(KeyCode.F))
            // {
            //     outputDeps = new PlayerFollowJob
            //     {
            //         CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            //         CameraRay = rtsRay,
            //     }.Schedule(this, inputDeps);
            // }
            // else
            // {
            //     return inputDeps;
            // }

            // m_EntityCommandBufferSystem.AddJobHandleForProducer(outputDeps);

            // return outputDeps;
        }
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerMouseOverSystem))]
    public class PrePlayerMouseOverSystem : ComponentSystem
    {
        private EntityQuery m_PlayerClicked, m_PlayerSelection, m_PlayerTarget, m_PlayerFollow;
        protected override void OnCreate()
        {
            base.OnCreate();
            // m_PlayerClicked = GetEntityQuery(ComponentType.ReadOnly<PlayerClicked>());
            m_PlayerSelection = GetEntityQuery(ComponentType.ReadOnly<PlayerSelection>());
            m_PlayerTarget = GetEntityQuery(ComponentType.ReadOnly<PlayerTarget>());
            m_PlayerFollow = GetEntityQuery(ComponentType.ReadOnly<PlayerFollow>());
        }

        protected override void OnUpdate()
        {
            // clear previous clicks
            // EntityManager.RemoveComponent(m_PlayerClicked, ComponentType.ReadOnly<PlayerClicked>());
            
            // selection modifier pressed, early exit;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                 Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                ) return;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Clearing PlayerSelection");
                EntityManager.RemoveComponent(m_PlayerSelection, ComponentType.ReadOnly<PlayerSelection>());
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Clearing PlayerTarget");
                EntityManager.RemoveComponent(m_PlayerTarget, ComponentType.ReadOnly<PlayerTarget>());
                Debug.Log("Clearing PlayerFollow");
                EntityManager.RemoveComponent(m_PlayerFollow, ComponentType.ReadOnly<PlayerFollow>());
            }
        }
    }
}
