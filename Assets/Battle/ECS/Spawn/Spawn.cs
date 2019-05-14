using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace UnitAgent
{
    public static class Spawn
    {
        public struct UnitSettings
        {
            public enum EOrder { None, InFormation, HoldPosition, MoveToPosition, FollowUnit, PursueUnit }

            [Header("Team")]
            public int team;

            [Header("Unit")]
            public UnitProxy unitPrefab;

            public UnitSpawn superior;
            public float unitTranslationUnitsPerSecond;
            public EOrder initialOrders;

            [Header("Agent")]
            public AgentProxy agentPrefab;
            public float agentSpacing;
            public int columns, rows;
            public float agentTranslationUnitsPerSecond;
            // public UnitSettings()
            // {
            //     unitTranslationUnitsPerSecond = 1;
            //     agentSpacing = 1.3F;
            //     columns = 6;
            //     rows = 2;
            //     agentTranslationUnitsPerSecond = .5f;

            // }
        }
        public static void SpawnUnits(EntityManager manager)
        {
            var entityManager = World.Active.EntityManager;

            Dictionary<UnitSpawn, Entity> map = new Dictionary<UnitSpawn, Entity>();
            foreach (var unitSpawn in GameObject.FindObjectsOfType<UnitSpawn>())
            {
                map[unitSpawn] = unitSpawn.SpawnUnit(entityManager);
            }

            foreach (var outer in map)
            {
                var unitSpawn = outer.Key;
                var unitEntity = outer.Value;

                if (unitSpawn.superior == null) continue;

                Debug.Log("Setting entity reference to " + unitSpawn.superior, unitSpawn);

                var superiorEntity = map[unitSpawn.superior];
                entityManager.AddComponentData(unitEntity, new Subordinate { Superior = superiorEntity });
            }
        }

    }
}
