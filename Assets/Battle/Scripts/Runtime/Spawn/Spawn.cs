using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent
{
    public class Spawn : MonoBehaviour
    {

        [Header("Team")]
        public int team = 0;

        public FormationSpawn superior;

        protected Entity CreateEntity(EntityManager entityManager, GameObject gameObjectPrefab)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, entityManager.World);
            var entity = entityManager.Instantiate(prefab);
            entityManager.SetName(entity, name);


            // Place the instantiated entity in a grid with some noise
            float3 spawnPosition = transform.TransformPoint(new float3(0, 0, 0));
            entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
            entityManager.SetComponentData(entity, new Rotation { Value = transform.rotation });

            var combinedBounds = new Bounds(new Vector3(0, .5f, 0), new Vector3(1, 1, 1));
            var renderers = gameObjectPrefab.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }

            entityManager.AddComponentData(entity, new AABB
            {
                //TODO add mesh bounds calc
                //0.5f will represent halfwidth for now
                center = combinedBounds.center,
                halfwidth = combinedBounds.extents,
                max = spawnPosition + (float3)combinedBounds.extents,
                min = spawnPosition - (float3)combinedBounds.extents,
            });

            entityManager.AddSharedComponentData(entity, new Team { Value = team });
            if (team == LocalPlayer.Team)
            {
                entityManager.AddSharedComponentData(entity, new PlayerOwned());
            }
            else
            {
                entityManager.AddSharedComponentData(entity, new PlayerEnemy());
            }

            return entity;
        }
    }
}
