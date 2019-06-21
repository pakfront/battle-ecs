using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


namespace UnitAgent
{
    public class SpawnManager : MonoBehaviour
    {

        public UnitGroupProxy[] teamUnitGroupProxy = new UnitGroupProxy[3];
        public UnitProxy[] teamUnitProxy = new UnitProxy[3];
        public UnitGoalMarkerProxy[] teamUnitGoalMarkerProxy = new UnitGoalMarkerProxy[3];

        public AgentProxy[] teamAgentProxy = new AgentProxy[3];

        static SpawnManager _instance;
        public static SpawnManager instance
        {
            get
            {
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

        public bool TryAssignSuperior(EntityManager manager, Dictionary<UnitGroupSpawn, Entity> unitGroupSpawnMap, Spawn spawn, Entity childEntity)
        {

            int rank = spawn.GetRankAndSuperior(out UnitGroupSpawn superior);
            manager.AddSharedComponentData(childEntity, new Rank
            {
                Value = (byte)rank
            });


            if (superior == null) return false;

            // if (manager.HasComponent<UnitGroupLeader>(entity))
            // {
            //     Debug.LogError("proper formation heirarchy not supporte for UnitGroupMember UitGroups " + superior, spawn);
            // }


            Debug.Log("Setting Superior entity reference to " + superior + " rank:" + rank, spawn);

            var superiorEntity = unitGroupSpawnMap[superior];

            if (!manager.HasComponent(superiorEntity, typeof(UnitGroupChildren)))
            {
                var children = manager.AddBuffer<UnitGroupChildren>(superiorEntity);
                children.Add(new UnitGroupChildren {Value = childEntity});
            }
            else
            {
                var children = manager.GetBuffer<UnitGroupChildren>(superiorEntity);
                children.Add(new UnitGroupChildren {Value = childEntity});
            }


            //TODO get in correct position
            int memberIndex = spawn.GetMemberIndex(); //transform.GetSiblingIndex();
            manager.AddComponentData(childEntity, new UnitGroupMember
            {
                MemberIndex = memberIndex,
                // FormationTableIndex = memberIndex,//TODO get bases on parent formation
                // FormationId = (int)spawn.initialFormation, //TODO set correctly
                PositionOffset = new float3(0, 0, memberIndex), //TODO get in correct position
                Parent = superiorEntity
            });


            if (manager.HasComponent<AgentGroupLeader>(childEntity))
            {
                var agentGroupLeader = manager.GetComponentData<AgentGroupLeader>(childEntity);
                Formation.SetFormation((int)spawn.initialFormation, ref agentGroupLeader);
                manager.SetComponentData(childEntity, agentGroupLeader);
                manager.SetComponentData(childEntity, new OrderedFormation {
                    FormationId = agentGroupLeader.FormationId
                });
            } 


            if (manager.HasComponent<UnitGroupLeader>(childEntity))
            {
                var unitGroupLeader = manager.GetComponentData<UnitGroupLeader>(childEntity);
                Formation.SetFormation((int)spawn.initialFormation, ref unitGroupLeader);
                manager.SetComponentData(childEntity, unitGroupLeader);
                manager.SetComponentData(childEntity, new OrderedFormation {
                    FormationId = unitGroupLeader.FormationId
                });
            } 

            manager.AddSharedComponentData(childEntity, new UnitGroup
            {
                Parent = superiorEntity
            });

            return true;
        }

        // static int GetRank(Spawn spawn, out UnitGroupSpawn superior)
        // {
        //     superior = null;
        //     if (spawn.transform.parent != null) superior = spawn.transform.parent.GetComponent<UnitGroupSpawn>();
        //     if (superior == null) return 0;


        //     return 1 + GetRank(superior, out UnitGroupSpawn nil);
        // }
    }
}
