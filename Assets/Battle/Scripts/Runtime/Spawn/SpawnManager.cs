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
            FormationSpawn[] formationSpawns = GameObject.FindObjectsOfType<FormationSpawn>();
            var formationSpawnMap = SpawnFormations(World.Active.EntityManager, formationSpawns);

            UnitSpawn[] unitSpawns = GameObject.FindObjectsOfType<UnitSpawn>();
            SpawnUnits(World.Active.EntityManager, formationSpawnMap, unitSpawns);
            for (int i = 0; i < unitSpawns.Length; i++)
            {
                GameObject.Destroy(unitSpawns[i]);
            }
        }

        public Dictionary<FormationSpawn, Entity> SpawnFormations(EntityManager manager, FormationSpawn[] formationSpawns)
        {
            Dictionary<FormationSpawn, Entity> spawnMap = new Dictionary<FormationSpawn, Entity>();
            foreach (var formationSpawn in formationSpawns)
            {
                spawnMap[formationSpawn] = formationSpawn.SpawnFormation(manager);
            }

            int i = 0;
            foreach (var outer in spawnMap)
            {
                var spawn = outer.Key;
                var entity = outer.Value;

                if (spawn.superior == null) continue;

                Debug.Log("Setting Superior entity reference to " + spawn.superior, spawn);

                var superiorEntity = spawnMap[spawn.superior];
                manager.AddComponentData(entity, new FormationMember
                {
                    Index = i++,
                    Offset = new float3(0, 0, i),
                    Parent = superiorEntity
                });

                manager.AddSharedComponentData(entity, new FormationGroup
                {
                    Parent = superiorEntity
                });
            }

            return spawnMap;   
        }

        public void SpawnUnits(EntityManager manager, Dictionary<FormationSpawn, Entity> formationSpawnMap, UnitSpawn[] unitSpawns)
        {
            Dictionary<UnitSpawn, Entity> unitSpawnMap = new Dictionary<UnitSpawn, Entity>();
            foreach (var unitSpawn in unitSpawns)
            {
                unitSpawnMap[unitSpawn] = unitSpawn.SpawnUnit(manager);
            }

            int i = 0;
            foreach (var outer in unitSpawnMap)
            {
                var unitSpawn = outer.Key;
                var unitEntity = outer.Value;

                if (unitSpawn.superior == null) continue;

                Debug.Log("Setting Superior entity reference to " + unitSpawn.superior, unitSpawn);

                var superiorEntity = formationSpawnMap[unitSpawn.superior];
                manager.AddComponentData(unitEntity, new FormationMember
                {
                    Index = i++,
                    Offset = new float3(0, 0, i),
                    Parent = superiorEntity
                });

                manager.AddSharedComponentData(unitEntity, new FormationGroup
                {
                    Parent = superiorEntity
                });
            }
        }
    }
}
