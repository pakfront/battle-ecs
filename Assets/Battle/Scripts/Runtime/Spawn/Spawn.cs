using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent
{
    public enum ETeam : byte {None=0, Red, Blue}

    public class Spawn : MonoBehaviour
    {

        [Header("Team")]
        public ETeam team;

        // public FormationSpawn superior;

        protected Entity CreateEntity(EntityManager entityManager, GameObject gameObjectPrefab)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, entityManager.World);
            var entity = entityManager.Instantiate(prefab);
            entityManager.SetName(entity, name);

            // Place the instantiated entity in a grid with some noise
            float3 spawnPosition = transform.TransformPoint(new float3(0, 0, 0));
            entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
            entityManager.SetComponentData(entity, new Rotation { Value = transform.rotation });

            return entity;
        }

        protected Entity CreateSelectableEntity(EntityManager entityManager, GameObject gameObjectPrefab)
        {
            var entity = CreateEntity(entityManager, gameObjectPrefab);

            var combinedBounds = new Bounds(new Vector3(0, .125f, 0), new Vector3(1, 1, 0.25f));
            var renderers = gameObjectPrefab.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }

            float3 spawnPosition = transform.TransformPoint(new float3(0, 0, 0));

            entityManager.AddComponentData(entity, new PlayerSelectable
            {
                //TODO add mesh bounds calc
                //0.5f will represent halfwidth for now
                center = combinedBounds.center,
                halfwidth = combinedBounds.extents,
                max = spawnPosition + (float3)combinedBounds.extents,
                min = spawnPosition - (float3)combinedBounds.extents,
            });

            entityManager.AddSharedComponentData(entity, new TeamGroup { Value = (int)team });
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
