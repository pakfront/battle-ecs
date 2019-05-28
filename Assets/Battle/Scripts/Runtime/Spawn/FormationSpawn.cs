using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent

{
    public class FormationSpawn : MonoBehaviour
    {
        public FormationProxy formationPrefab;

        public FormationSpawn superior;

      public Entity SpawnFormation(EntityManager entityManager)
        {

            // Create entity prefab from the game object hierarchy once
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(formationPrefab.gameObject, entityManager.World);
            var entity = entityManager.Instantiate(prefab);

            entityManager.SetName(entity, name);

            // Place the instantiated entity in a grid with some noise
            float3 spawnPosition = transform.TransformPoint(new float3(0, 0, 0));
            entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
            entityManager.SetComponentData(entity, new Rotation { Value = transform.rotation });

            return entity;
        }
    }
}