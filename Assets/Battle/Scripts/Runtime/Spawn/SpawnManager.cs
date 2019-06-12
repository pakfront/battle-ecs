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
            Dictionary<FormationSpawn, Entity> formationSpawnMap = new Dictionary<FormationSpawn, Entity>();
            foreach (var formationSpawn in formationSpawns)
            {
                formationSpawnMap[formationSpawn] = formationSpawn.SpawnFormation(manager);
            }

            foreach (var outer in formationSpawnMap)
            {
                var spawn = outer.Key;
                var entity = outer.Value;
                TryAssignSuperior(manager, formationSpawnMap, spawn, entity);

            }

            return formationSpawnMap;   
        }

        public void SpawnUnits(EntityManager manager, Dictionary<FormationSpawn, Entity> formationSpawnMap, UnitSpawn[] unitSpawns)
        {
            Dictionary<UnitSpawn, Entity> unitSpawnMap = new Dictionary<UnitSpawn, Entity>();
            foreach (var unitSpawn in unitSpawns)
            {
                unitSpawnMap[unitSpawn] = unitSpawn.SpawnUnit(manager);
            }

            foreach (var outer in unitSpawnMap)
            {
                var unitSpawn = outer.Key;
                var unitEntity = outer.Value;
                TryAssignSuperior(manager, formationSpawnMap, unitSpawn, unitEntity);     
            }
        }

        public bool TryAssignSuperior(EntityManager manager, Dictionary<FormationSpawn, Entity> formationSpawnMap, Spawn spawn, Entity entity)
        {
                FormationSpawn superior = null;
                if (spawn.transform.parent != null) superior = spawn.transform.parent.GetComponent<FormationSpawn>();

                if (superior == null) return false;

                Debug.Log("Setting Superior entity reference to " + superior, spawn);

                var superiorEntity = formationSpawnMap[superior];
                //TODO get in correct position
                int memberIndex = spawn.transform.GetSiblingIndex();
                manager.AddComponentData(entity, new UnitGroupMember
                {
                    MemberIndex = memberIndex,
                    // FormationTableIndex = memberIndex,//TODO get bases on parent formation
                    FormationId = 1, //TODO set correctly
                    PositionOffset = new float3(0, 0, memberIndex), //TODO get in correct position
                    Parent = superiorEntity
                });

                manager.AddSharedComponentData(entity, new UnitGroup
                {
                    Parent = superiorEntity
                });   

                return true;
        }
    }
}
