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

        public enum EOrder {None, Hold, Move, Pursue}

        [Header("Team")]
        public int team = 0;

        [Header("Unit")]
        public UnitProxy unitPrefab;
        public float unitTranslationUnitsPerSecond = 1;
        public EOrder initialOrders;

        [Header("Agent")]
        public AgentProxy agentPrefab;
        public float agentSpacing = 1.3F;
        public int columns = 6, rows = 2;
        public float agentTranslationUnitsPerSecond = .5f;

        private float3[] formationPositions = null;
        private Bounds localBounds; 


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
            float3 spawnPosition = transform.TransformPoint(new float3(0, 0, 0));
            entityManager.SetComponentData(entity, new Translation { Value = spawnPosition });
            entityManager.AddComponentData(entity, new Goal
            {
                Position = (float3)(
                    transform.TransformPoint(transform.right * 20 + transform.forward * 10)),
            });
            entityManager.AddComponentData(entity, new Move
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
                case EOrder.Hold:
                    entityManager.AddComponentData(entity, new OrderHold{});
                    break;
                default:
                    break;
            }


            SpawnAgents(entity);
            Destroy(gameObject);
        }


        // spawn multiple agents taht follow this unit
        void SpawnAgents(Entity unit)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, World.Active);
            // int count = columns * rows;
            float3[] formationPositions = GetFormationPositions();
            float3[] spawnPositions = GetSpawnPositions(formationPositions);
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
                entityManager.AddComponentData(agents[i], new Goal());
                entityManager.AddComponentData(agents[i], new Move
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

        public float3[] GetFormationPositions()
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


        public float3[] GetSpawnPositions(float3[] formationPositions)
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

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            switch (team)
            {
                case 0:
                    Gizmos.color = Color.red;
                    break;
                case 1:
                    Gizmos.color = Color.blue;
                    break;
                default:
                    Gizmos.color = Color.yellow;
                    break;

            }
            Gizmos.matrix = transform.localToWorldMatrix;
            float agentRadius = agentSpacing/2f;
            Gizmos.DrawCube(
                new Vector3(-agentRadius, 1, agentRadius - agentSpacing*rows/2f),
                new Vector3(agentSpacing*columns, 2, agentSpacing*rows)
            );
        }

       void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.matrix = transform.localToWorldMatrix;
            float3 [] pos = GetFormationPositions();
            for (int i = 0; i < pos.Length; i++)
            Gizmos.DrawSphere(
                    pos[i], agentSpacing/2f
            );
        }
    }
}
