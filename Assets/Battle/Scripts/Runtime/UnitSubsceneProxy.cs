using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Samples.HelloCube_02;

namespace UnitAgent
{
    [RequiresEntityConversion]
    public class UnitSubsceneProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float DegreesPerSecond = 360;
        
        public enum EOrder {None, InFormation, HoldPosition, MoveToPosition, FollowUnit, PursueUnit}

        [Header("Team")]
        public int team = 0;

        [Header("Unit")]
        public float unitTranslationUnitsPerSecond = 1;
        public EOrder initialOrders;

        [Header("Agent")]
        public AgentProxy agentPrefab;
        public float agentSpacing = 1.3F;
        public int columns = 6, rows = 2;
        public float agentTranslationUnitsPerSecond = .5f;

        private float3[] formationPositions = null;
        void SpawnUnit(Entity entity, EntityManager entityManager)
        {
            entityManager.AddComponentData(entity, new Unit());
            
            // Place the instantiated entity in a grid with some noise
            float3 spawnPosition = transform.TransformPoint(new float3(0, 0, 0));
            entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
            entityManager.AddComponentData(entity, new MoveToGoal
            {
                Position = (float3)(
                    transform.TransformPoint(transform.right * 20 + transform.forward * 10)),
            });
            entityManager.AddComponentData(entity, new MoveSettings
            {
                TranslateSpeed = unitTranslationUnitsPerSecond,
                RotateSpeed = .5f
            });

            entityManager.AddComponentData(entity, new AABB
            {
                //0.5f will represent halfwidth for now
                max = spawnPosition + 0.5f,
                min = spawnPosition - 0.5f,
            });

            entityManager.AddComponentData(entity, new Opponent { });

            entityManager.AddSharedComponentData(entity, new Team { Value = team });

            switch (initialOrders)
            {
                case EOrder.HoldPosition:
                    entityManager.AddComponentData(entity, new OrderHold{});
                    break;
                default:
                    break;
            }

            SpawnAgents(entity, entityManager);
        }

        // spawn multiple agents taht follow this unit
        void SpawnAgents(Entity unit, EntityManager entityManager)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, World.Active);
            // int count = columns * rows;
            float3[] formationPositions = GetAgentFormationPositions();
            float3[] spawnPositions = GetAgentSpawnPositions(formationPositions);
            int count = formationPositions.Length;

            NativeArray<Entity> agents = new NativeArray<Entity>(formationPositions.Length, Allocator.Temp);
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk
            // var unitMembership = new UnitMembership { Value = unit };

            for (int i = 0; i < count; i++)
            {
                // float3 formationPosition = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                float3 formationPosition = formationPositions[i];
                float3 spawnPosition = spawnPositions[i];

                entityManager.SetComponentData(agents[i], new Agent { });
                entityManager.SetComponentData(agents[i], new Subordinate { Superior = unit });
                entityManager.AddComponentData(agents[i], new MoveToGoal());
                entityManager.AddComponentData(agents[i], new MoveSettings
                {
                    TranslateSpeed = agentTranslationUnitsPerSecond,
                    RotateSpeed = .5f
                });
                entityManager.AddComponentData(agents[i], new FormationElement { Position = new float4(formationPosition, 1) });

                entityManager.SetComponentData(agents[i], new Translation { Value = spawnPosition });
                entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

                // creates a chunk per unitId, but data is not accessible in job
                // entityManager.AddSharedComponentData(agents[i], unitMembership);

            }
            agents.Dispose();
        }

        public float3[] GetAgentFormationPositions()
        {
            int count = columns * rows;
            if (formationPositions == null || formationPositions.Length != count)
            {
                formationPositions = new float3[count];
            } else {
                return formationPositions;
            }

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int i = x * rows + y;
                    // formationPositions[i] = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                    formationPositions[i] = new float3((x-columns/2) * agentSpacing, 0, -y * agentSpacing);
                }
            }
            return formationPositions;
        }


        public float3[] GetAgentSpawnPositions(float3[] formationPositions)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)gameObject.GetInstanceID());

            int count = formationPositions.Length;
            float3[] spawnPositions = new float3[count];

            for (int i = 0; i < count; i++)
            {
                spawnPositions[i] = (float3)(transform.TransformPoint(formationPositions[i])) + new float3(random.NextFloat(-4, 4), 0, random.NextFloat(-4, 4));
            }
            return spawnPositions;
        }

        // The MonoBehaviour data is converted to ComponentData on the entity.
        // We are specifically transforming from a good editor representation of the data (Represented in degrees)
        // To a good runtime representation (Represented in radians)
        public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
        {
            // var data = new RotationSpeed { RadiansPerSecond = math.radians(DegreesPerSecond) };
            // entityManager.AddComponentData(entity, data);

            SpawnUnit(entity, entityManager);            
        }
    }
}
