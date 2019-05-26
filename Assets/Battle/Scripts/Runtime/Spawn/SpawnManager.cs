using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


namespace UnitAgent
{
    public class SpawnManager : MonoBehaviour
    {
        void Start()
        {
            UnitSpawn[] unitSpawns = GameObject.FindObjectsOfType<UnitSpawn>();
            SpawnUnits(World.Active.EntityManager, unitSpawns);
            for (int i = 0; i < unitSpawns.Length; i++)
            {
                GameObject.Destroy(unitSpawns[i]);
            }
        }

        public void SpawnUnits(EntityManager manager, UnitSpawn[] unitSpawns)
        {
            var entityManager = World.Active.EntityManager;

            Dictionary<UnitSpawn, Entity> map = new Dictionary<UnitSpawn, Entity>();
            foreach (var unitSpawn in unitSpawns)
            {
                map[unitSpawn] = unitSpawn.SpawnUnit(entityManager);
            }

            int i = 0;
            foreach (var outer in map)
            {
                var unitSpawn = outer.Key;
                var unitEntity = outer.Value;

                if (unitSpawn.superior == null) continue;

                Debug.Log("Setting entity reference to " + unitSpawn.superior, unitSpawn);

                var superiorEntity = map[unitSpawn.superior];
                entityManager.AddComponentData(unitEntity, new FormationElement
                {
                    Index = i++,
                    Position = new float3(0, 0, i),
                    Parent = superiorEntity
                });
            }
        }
    }
}
