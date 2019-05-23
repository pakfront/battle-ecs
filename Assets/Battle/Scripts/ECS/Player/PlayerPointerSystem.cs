using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using static Unity.Physics.Math;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerOrderPreSystem))]
    public class PlayerPointerSystem : JobComponentSystem
    {

        private EntityCommandBufferSystem m_EntityCommandBufferSystem;
        private EntityQuery m_group;
        private UnityEngine.Plane groundplane = new UnityEngine.Plane(Vector3.up, 0);

        Unity.Physics.Systems.BuildPhysicsWorld physicsWorldSystem;


        protected override void OnCreate()
        {
            physicsWorldSystem = Unity.Entities.World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();

            m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            var query = new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Translation>() }
            };

            m_group = GetEntityQuery(query);
        }

        // [BurstCompile]
        // struct CastRayJob : IJobParallelFor
        // {
        //     [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
        //     [ReadOnly] public ArchetypeChunkEntityType EntityType;
        //     [ReadOnly] public ArchetypeChunkComponentType<AABB> AABBType;
        //     [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;

        //     public RTSRay Ray;
        //     public NativeArray<Entity> NearestEntity;
        //     public NativeArray<float> NearestDistanceSq;

        //     public void Execute(int chunkIndex)
        //     {
        //         var chunk = Chunks[chunkIndex];
        //         var entities = chunk.GetNativeArray(EntityType);
        //         var chunkTranslation = chunk.GetNativeArray(TranslationType);
        //         var chunkAABB = chunk.GetNativeArray(AABBType);
        //         var instanceCount = chunk.Count;
        //         float nearestDistanceSq = float.MaxValue;
        //         int nearestPositionIndex = -1;

        //         for (int i = 0; i < instanceCount; i++)
        //         {
        //             var aabb = chunkAABB[i];
        //             bool hit = RTSPhysics.Intersect(aabb, Ray);
        //             // if (hit)
        //             // {
        //             //     Debug.Log("PlayerSelectionJob: Click on " + i);
        //             // }

        //             float distance = math.lengthsq((chunkTranslation[i].Value - Ray.origin));
        //             bool nearest = hit && distance < nearestDistanceSq;
        //             nearestDistanceSq = math.select(nearestDistanceSq, distance, nearest);
        //             nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
        //         }

        //         if (nearestPositionIndex > -1)
        //         {
        //             NearestEntity[chunkIndex] = entities[nearestPositionIndex];
        //             NearestDistanceSq[chunkIndex] = nearestDistanceSq;
        //         }
        //     }
        // }


        [BurstCompile]
        struct Pick : IJob
        {
            [ReadOnly] public Unity.Physics.CollisionWorld CollisionWorld;
            [ReadOnly] public int NumDynamicBodies;
            public Unity.Physics.Ray Ray;
            public PlayerPointer playerPointer;

            public void Execute()
            {
                float fraction = 1.0f;
                RigidBody? hitBody = null;

                var rayCastInput = new RaycastInput { Ray = Ray, Filter = CollisionFilter.Default };
                if (CollisionWorld.CastRay(rayCastInput, out Unity.Physics.RaycastHit hit))
                {
                    if (hit.RigidBodyIndex < NumDynamicBodies)
                    {
                        hitBody = CollisionWorld.Bodies[hit.RigidBodyIndex];
                    }
                }

                // If there was a hit, set up the spring
                if (hitBody != null)
                {
                    playerPointer.Entity = hitBody.Value.Entity;
                    playerPointer.Click |= (uint)EClick.AABB;
                }
                else
                {
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerPointer = GetSingleton<PlayerPointer>();

            // early out if no mouse button clicked
            if ((playerPointer.Click & (uint)EClick.AnyPointerButton) == 0)
                return inputDeps;

            //var handle = JobHandle.CombineDependencies(inputDeps, physicsWorldSystem.FinalJobHandle);

            UnityEngine.Ray unityRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var ray = new Unity.Physics.Ray(unityRay.origin, unityRay.direction * 2000);

            var handle = new Pick
            {
                CollisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld,
                NumDynamicBodies = physicsWorldSystem.PhysicsWorld.NumDynamicBodies,
                Ray = ray,
                playerPointer = playerPointer
                //Near = Camera.main.nearClipPlane,
            }.Schedule();

            // handle = JobHandle.CombineDependencies(handle, physicsWorldSystem.FinalJobHandle);

            handle.Complete();

            Entity nentity = Entity.Null;

            float enter;
            if (playerPointer.Entity == Entity.Null)
            {
                if (groundplane.Raycast(unityRay, out enter))
                {
                    Debug.Log("PlayerPointerSystem Hit Terrain");
                    playerPointer.Position = unityRay.GetPoint(enter);
                    playerPointer.Click |= (uint)EClick.Terrain;
                }
            }
            else
            {
                playerPointer.Click |= (uint)EClick.AABB;
                playerPointer.Entity = nentity;
                Debug.Log("PlayerPointerSystem Hit Entity" + nentity);
                if (EntityManager.HasComponent<PlayerSelection>(nentity))
                    // EntityManager.SetComponentData(nentity, new PlayerSelection { });
                    EntityManager.AddComponentData(nentity, new PlayerSelection { });
                else
                    EntityManager.AddComponentData(nentity, new PlayerSelection { });
            }

            SetSingleton(playerPointer);

            return handle;
        }
    }
}