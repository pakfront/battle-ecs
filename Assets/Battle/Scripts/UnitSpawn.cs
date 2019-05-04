using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent

{
    public class UnitSpawn : MonoBehaviour
    {

        public int agentCountX = 6, agentCountY = 2;

        public int unitIndex;

        public float translationUnitsPerSecond = 1;

        public UnitProxy unitPrefab;

        public AgentProxy agentPrefab;

        void Start()
        {
            SpawnUnit();
        }

        void SpawnUnit()
        {
            var entityManager = World.Active.EntityManager;

            // Create entity prefab from the game object hierarchy once
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab.gameObject, World.Active);
            var unit = entityManager.Instantiate(prefab);

            // Place the instantiated entity in a grid with some noise
            var position = transform.TransformPoint(new float3(0,0,0));
            entityManager.SetComponentData(unit, new Translation { Value = position });
            entityManager.SetComponentData(unit, new UnitId { Value = unitIndex });
            entityManager.AddComponentData(unit, new TranslationSpeed { UnitsPerSecond = translationUnitsPerSecond });
            SpawnAgents();
        }


        // spawn multiple agents taht follow this unit
        void SpawnAgents()
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, World.Active);
            int count = agentCountX * agentCountY;

            NativeArray<Entity> agents = new NativeArray<Entity>(count, Allocator.Temp);
            var entityManager = World.Active.EntityManager;
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk
            var unitMembership = new UnitMembership { Value = unitIndex };

            for (int x = 0; x < agentCountX; x++)
            {
                for (int y = 0; y < agentCountY; y++)
                {
                    int i = x * agentCountY + y;
                    var position = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                    entityManager.SetComponentData(agents[i], new Translation { Value = position });
                    entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

                    // creates a chunk per unitId, but data is not accessible in job
                    entityManager.AddSharedComponentData(agents[i], unitMembership);

                    // since we can't access the UnitMembership in the JobChunk, 
                    // create an index local to the agent  
                    entityManager.AddComponentData(agents[i], new UnitId { Value = unitIndex });
                }
            }
            agents.Dispose();
        }
    }
}

