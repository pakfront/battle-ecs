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


        [Header("Unit")]
        public float unitTranslationUnitsPerSecond = 1;


        public UnitProxy unitPrefab;

        [Header("Agent")]
        public int agentCountX = 6, agentCountY = 2;
        public float agentTranslationUnitsPerSecond = .5f;

        public AgentProxy agentPrefab;

        Unity.Mathematics.Random random;


        void Start()
        {
            random = new Unity.Mathematics.Random( (uint) gameObject.GetInstanceID());
            SpawnUnit();
        }

        void SpawnUnit()
        {
            var entityManager = World.Active.EntityManager;

            // Create entity prefab from the game object hierarchy once
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab.gameObject, World.Active);
            var entity = entityManager.Instantiate(prefab);

            // Place the instantiated entity in a grid with some noise
            var spawnPosition = transform.TransformPoint(new float3(0,0,0));
            entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
            entityManager.AddComponentData(entity, new TranslationSpeed { UnitsPerSecond = unitTranslationUnitsPerSecond });
            SpawnAgents(entity);
        }


        // spawn multiple agents taht follow this unit
        void SpawnAgents(Entity unit)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, World.Active);
            int count = agentCountX * agentCountY;

            NativeArray<Entity> agents = new NativeArray<Entity>(count, Allocator.Temp);
            var entityManager = World.Active.EntityManager;
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk
            // var unitMembership = new UnitMembership { Value = unit };

            for (int x = 0; x < agentCountX; x++)
            {
                for (int y = 0; y < agentCountY; y++)
                {
                    int i = x * agentCountY + y;
                    float3 formationPosition = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                    float3 spawnPosition = formationPosition + new float3( random.NextFloat(-4, 4), 0,  random.NextFloat(-4, 4));

                    entityManager.SetComponentData(agents[i], new Agent { Unit = unit });
                    entityManager.AddComponentData(agents[i], new Goal());
                    entityManager.AddComponentData(agents[i], new FormationElement { Position = new float4(formationPosition, 1)});
                    // entityManager.AddComponentData(agents[i], new TranslationSpeed { UnitsPerSecond = agentTranslationUnitsPerSecond });
                    
                    entityManager.SetComponentData(agents[i], new Translation { Value = spawnPosition });
                    // entityManager.SetComponentData(agents[i], new Translation { Value = formationPosition });
                    entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

                    // creates a chunk per unitId, but data is not accessible in job
                    // entityManager.AddSharedComponentData(agents[i], unitMembership);
                }
            }
            agents.Dispose();
        }
    }
}

