﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


namespace UnitAgent
{
    public class SpawnManager : MonoBehaviour
    {

        public UnitGroupProxy [] teamUnitGroupProxy = new UnitGroupProxy[3];
        public UnitProxy [] teamUnitProxy = new UnitProxy[3];
        public UnitGoalMarkerProxy [] teamUnitGoalMarkerProxy = new UnitGoalMarkerProxy[3];

        public AgentProxy [] teamAgentProxy = new AgentProxy[3];

        static SpawnManager _instance;
        public static SpawnManager instance {
            get {
                if (_instance == null) _instance = FindObjectOfType<SpawnManager>();
                return _instance;
            }
        }
        void Start()
        {
            UnitGroupSpawn[] unitGroupSpawns = GameObject.FindObjectsOfType<UnitGroupSpawn>();
            var unitGroupSpawnMap = SpawnFormations(World.Active.EntityManager, unitGroupSpawns);

            UnitSpawn[] unitSpawns = GameObject.FindObjectsOfType<UnitSpawn>();
            SpawnUnits(World.Active.EntityManager, unitGroupSpawnMap, unitSpawns);
            for (int i = 0; i < unitSpawns.Length; i++)
            {
                GameObject.Destroy(unitSpawns[i]);
            }
        }

        public Dictionary<UnitGroupSpawn, Entity> SpawnFormations(EntityManager manager, UnitGroupSpawn[] formationSpawns)
        {
            Dictionary<UnitGroupSpawn, Entity> unitGroupSpawnMap = new Dictionary<UnitGroupSpawn, Entity>();
            foreach (var formationSpawn in formationSpawns)
            {
                unitGroupSpawnMap[formationSpawn] = formationSpawn.SpawnFormation(manager);
            }

            foreach (var outer in unitGroupSpawnMap)
            {
                var spawn = outer.Key;
                var entity = outer.Value;
                TryAssignSuperior(manager, unitGroupSpawnMap, spawn, entity);

            }

            return unitGroupSpawnMap;   
        }

        public void SpawnUnits(EntityManager manager, Dictionary<UnitGroupSpawn, Entity> unitGroupSpawnMap, UnitSpawn[] unitSpawns)
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
                TryAssignSuperior(manager, unitGroupSpawnMap, unitSpawn, unitEntity);     
            }
        }

        public bool TryAssignSuperior(EntityManager manager, Dictionary<UnitGroupSpawn, Entity> unitGroupSpawnMap, Spawn spawn, Entity entity)
        {
                UnitGroupSpawn superior = null;
                if (spawn.transform.parent != null) superior = spawn.transform.parent.GetComponent<UnitGroupSpawn>();

                if (superior == null) return false;

                if (manager.HasComponent<UnitGroupLeader>(entity))
                {
                    Debug.LogError("proper formation heirarchy not supporte for UnitGroupMember UitGroups " + superior, spawn);
                }

                int rank = GetRank(spawn);

                Debug.Log("Setting Superior entity reference to " + superior +" rank:"+rank, spawn);

                var superiorEntity = unitGroupSpawnMap[superior];
                //TODO get in correct position
                int memberIndex = spawn.transform.GetSiblingIndex();
                manager.AddComponentData(entity, new UnitGroupMember
                {
                    MemberIndex = memberIndex,
                    // FormationTableIndex = memberIndex,//TODO get bases on parent formation
                    FormationId = (int)spawn.initialFormation, //TODO set correctly
                    PositionOffset = new float3(0, 0, memberIndex), //TODO get in correct position
                    Parent = superiorEntity
                });

                manager.AddSharedComponentData(entity, new UnitGroup
                {
                    Parent = superiorEntity
                }); 

                manager.AddSharedComponentData(entity, new Rank
                {
                    Value = (byte)rank
                });     

                return true;
        }

        static int GetRank(Spawn spawn)
        {
            UnitGroupSpawn superior = null;
            if (spawn.transform.parent != null) superior = spawn.transform.parent.GetComponent<UnitGroupSpawn>();

            if (superior == null) return 0;

            return 1 + GetRank(superior);
        }
    }
}
