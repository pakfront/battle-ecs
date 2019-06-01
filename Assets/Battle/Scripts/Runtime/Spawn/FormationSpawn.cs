using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent

{
    public class FormationSpawn : Spawn
    {
        public FormationProxy formationPrefab;

      public Entity SpawnFormation(EntityManager entityManager)
        {
            var entity = CreateSelectableEntity(entityManager, formationPrefab.gameObject);
            return entity;
        }
    }
}