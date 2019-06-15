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
    [UpdateInGroup(typeof(PlayerSystemGroup))]
    [UpdateBefore(typeof(PlayerOrderSystem))]
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
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerSelectable>(), ComponentType.ReadOnly<Translation>() }
            };

            m_group = GetEntityQuery(query);
        }

        [BurstCompile]
        struct CastRayJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<PlayerSelectable> PlayerSelectableType;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;

            public RTSRay Ray;
            public NativeArray<Entity> NearestEntity;
            public NativeArray<float> NearestDistanceSq;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var entities = chunk.GetNativeArray(EntityType);
                var chunkTranslation = chunk.GetNativeArray(TranslationType);
                var chunkPlayerSelectable = chunk.GetNativeArray(PlayerSelectableType);
                var instanceCount = chunk.Count;
                float nearestDistanceSq = float.MaxValue;
                int nearestPositionIndex = -1;

                for (int i = 0; i < instanceCount; i++)
                {
                    var aabb = chunkPlayerSelectable[i];
                    bool hit = RTSPhysics.Intersect(aabb, Ray);

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
            var playerPointer = GetSingleton<PlayerInput>();

            // early out if no mouse button clicked
            if ((playerPointer.Click & (uint)EClick.AnyPointerButton) == 0)
                return inputDeps;


            var chunks = m_group.CreateArchetypeChunkArray(Allocator.TempJob);
            var nchunks = chunks.Length;

            // TODO move to 
            Ray ray = Camera.main.ScreenPointToRay(playerPointer.MousePosition);
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
                PlayerSelectableType = GetArchetypeChunkComponentType<PlayerSelectable>(true),
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
                    playerPointer.WorldHitPosition = ray.GetPoint(enter);
                    playerPointer.Click |= (uint)EClick.Terrain;
                }
                else
                {
                    // Debug.Log("PlayerPointerSystem clickLocation MISS ");
 
                }
            }
            else
            {
                playerPointer.Click |= (uint)EClick.PlayerSelectable;
                playerPointer.CurrentEntity = nentity;
                Debug.Log("PlayerPointerSystem Hit " + nentity);
                if (!EntityManager.HasComponent<PlayerSelection>(nentity))
                    EntityManager.AddComponentData(nentity, new PlayerSelection { });
            }

            nearestDistanceSq.Dispose();
            nearestEntity.Dispose();

            SetSingleton(playerPointer);

            return outputDeps;
        }
    }

    public struct RTSRay
    {
        public float3 origin;
        public float3 direction;
    }
    public static class RTSPhysics
    {

        public static bool Intersect(PlayerSelectable box, RTSRay ray)
        {
            double tx1 = (box.min.x - ray.origin.x) * (1 / ray.direction.x);
            double tx2 = (box.max.x - ray.origin.x) * (1 / ray.direction.x);

            double tmin = math.min(tx1, tx2);
            double tmax = math.max(tx1, tx2);

            double ty1 = (box.min.y - ray.origin.y) * (1 / ray.direction.y);
            double ty2 = (box.max.y - ray.origin.y) * (1 / ray.direction.y);

            tmin = math.max(tmin, math.min(ty1, ty2));
            tmax = math.min(tmax, math.max(ty1, ty2));

            double tz1 = (box.min.z - ray.origin.z) * (1 / ray.direction.z);
            double tz2 = (box.max.z - ray.origin.z) * (1 / ray.direction.z);

            tmin = math.max(tmin, math.min(tz1, tz2));
            tmax = math.min(tmax, math.max(tz1, tz2));

            return tmax >= tmin;
        }
    }
}