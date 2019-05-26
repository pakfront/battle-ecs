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
    // [DisableAutoCreation] 
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerOrderPreSystem))]
    public class PlayerPointerSystem : JobComponentSystem
    {

        private EntityCommandBufferSystem m_EntityCommandBufferSystem;
        private EntityQuery m_group;
        private Plane groundplane = new Plane(Vector3.up, 0);


        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            var query = new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<AABB>(), ComponentType.ReadOnly<Translation>() }
            };

            m_group = GetEntityQuery(query);
        }

        [BurstCompile]
        struct CastRayJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<AABB> AABBType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;

            public RTSRay Ray;
            public NativeArray<Entity> NearestEntity;
            public NativeArray<float> NearestDistanceSq;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var entities = chunk.GetNativeArray(EntityType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                var chunkAABB = chunk.GetNativeArray(AABBType);
                var instanceCount = chunk.Count;
                float nearestDistanceSq = float.MaxValue;
                int nearestPositionIndex = -1;

                for (int i = 0; i < instanceCount; i++)
                {
                    var aabb = chunkAABB[i];
                    bool hit = RTSPhysics.Intersect(aabb, Ray);
                    // if (hit)
                    // {
                    //     Debug.Log("PlayerSelectionJob: Click on " + i);
                    // }

                    float distance = math.lengthsq((chunkTranslation[i].Value - Ray.origin));
                    bool nearest = hit && distance < nearestDistanceSq;
                    nearestDistanceSq = math.select(nearestDistanceSq, distance, nearest);
                    nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
                }

                if (nearestPositionIndex > -1)
                {
                    NearestEntity[chunkIndex] = entities[nearestPositionIndex];
                    NearestDistanceSq[chunkIndex] = nearestDistanceSq;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerPointer = GetSingleton<PlayerPointer>();

            // early out if no mouse button clicked
            if ( (playerPointer.Click &  (uint)EClick.AnyPointerButton) == 0) 
                return inputDeps;


            var chunks = m_group.CreateArchetypeChunkArray(Allocator.TempJob);
            var nchunks = chunks.Length;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RTSRay rtsRay = new RTSRay
            {
                origin = ray.origin,
                direction = ray.direction
            };

            var nearestDistanceSq = new NativeArray<float>(nchunks, Allocator.TempJob);
            var nearestEntity = new NativeArray<Entity>(nchunks, Allocator.TempJob);

            var outputDeps = new CastRayJob
            {
                Chunks = chunks,
                Ray = rtsRay,
                EntityType = GetArchetypeChunkEntityType(),
                TranslationType = GetArchetypeChunkComponentType<Translation>(true),
                AABBType = GetArchetypeChunkComponentType<AABB>(true),
                NearestDistanceSq = nearestDistanceSq,
                NearestEntity = nearestEntity
            }.Schedule(nchunks, 32, inputDeps);
            outputDeps.Complete();

            float nsq = float.MaxValue;
            Entity nentity = Entity.Null;
            for (int i = 0; i < nchunks; i++)
            {
                if (nearestEntity[i] != Entity.Null && nearestDistanceSq[i] < nsq)
                {
                    nsq = nearestDistanceSq[i];
                    nentity = nearestEntity[i];
                }
            }

            float enter;
            if (nentity == Entity.Null)
            {
                if (groundplane.Raycast(ray, out enter))
                {
                    playerPointer.Position = ray.GetPoint(enter);
                    playerPointer.Click |=  (uint)EClick.Terrain;
                }
                // else
                // {
                //     // Debug.Log("PlayerPointerSystem clickLocation MISS ");
                //     SetSingleton(new PlayerPointer
                //     {
                //         Click = click
                //     });
                // }
            }
            else
            {
                playerPointer.Click |=  (uint)EClick.AABB;
                playerPointer.CurrentEntity = nentity;
                Debug.Log("PlayerPointerSystem Hit "+nentity);
                if ( ! EntityManager.HasComponent<PlayerSelection>(nentity) )
                    EntityManager.AddComponentData(nentity, new PlayerSelection { });
            }

            nearestDistanceSq.Dispose();
            nearestEntity.Dispose();

            SetSingleton(playerPointer);

            return outputDeps;
        }
    }
}