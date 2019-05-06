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
        public UnitProxy unitPrefab;
        public float unitTranslationUnitsPerSecond = 1;



        [Header("Agent")]
        public AgentProxy agentPrefab;
        public int agentCountX = 6, agentCountY = 2;
        public float agentTranslationUnitsPerSecond = .5f;




        void Start()
        {
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
            entityManager.AddComponentData(entity, new Goal { 
                Position = (float3)(
                    transform.TransformPoint( transform.right * 20 + transform.forward * 10)), 
                });
            entityManager.AddComponentData(entity, new Move { 
                TranslateSpeed = unitTranslationUnitsPerSecond,
                RotateSpeed = .5f
                });
            SpawnAgents(entity);
        }


        // spawn multiple agents taht follow this unit
        void SpawnAgents(Entity unit)
        {

            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, World.Active);
            // int count = agentCountX * agentCountY;
            float3 [] formationPositions = GetFormationPositions();
            float3 [] spawnPositions = GetSpawnPositions(formationPositions);
            int count = formationPositions.Length;

            NativeArray<Entity> agents = new NativeArray<Entity>(formationPositions.Length, Allocator.Temp);
            var entityManager = World.Active.EntityManager;
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk
            // var unitMembership = new UnitMembership { Value = unit };

            for (int i = 0; i < count; i++)
            {

                // float3 formationPosition = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                float3 formationPosition = formationPositions[i];
                float3 spawnPosition = spawnPositions[i];

                entityManager.SetComponentData(agents[i], new Agent { Unit = unit });
                entityManager.AddComponentData(agents[i], new Goal ());
                entityManager.AddComponentData(agents[i], new Move {
                    TranslateSpeed = agentTranslationUnitsPerSecond,
                    RotateSpeed = .5f
                });
                entityManager.AddComponentData(agents[i], new FormationElement { Position = new float4(formationPosition, 1)});
                
                entityManager.SetComponentData(agents[i], new Translation { Value = spawnPosition });
                entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

                // creates a chunk per unitId, but data is not accessible in job
                // entityManager.AddSharedComponentData(agents[i], unitMembership);
                
            }
            agents.Dispose();
        }

        public float3 [] GetFormationPositions()
        {
            int count = agentCountX * agentCountY;
            float3 [] formationPositions = new float3 [count];

            for (int x = 0; x < agentCountX; x++)
            {
                for (int y = 0; y < agentCountY; y++)
                {
                    int i = x * agentCountY + y;
                    // formationPositions[i] = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                    formationPositions[i] = new float3(x * 1.3F, 0, y * 1.3F);
                }
            }
            return formationPositions;
        }


        public float3 [] GetSpawnPositions(float3 [] formationPositions)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random( (uint) gameObject.GetInstanceID());

            int count = formationPositions.Length;
            float3 [] spawnPositions = new float3 [count];

            for (int i = 0; i < count; i++)
            {
                spawnPositions[i] = (float3)(transform.TransformPoint(formationPositions[i])) + new float3( random.NextFloat(-4, 4), 0,  random.NextFloat(-4, 4));
            }
            return spawnPositions;
        }
    } 
}

