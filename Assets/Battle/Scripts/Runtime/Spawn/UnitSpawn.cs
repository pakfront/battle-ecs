using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent

{
    public class UnitSpawn : Spawn
    {
        public enum EOrder { None, InFormation, HoldPosition, MoveToPosition, FollowUnit, PursueUnit }

        [Header("Unit")]
        public UnitProxy unitPrefab;
        public EOrder initialOrders;

        [Header("Agent")]
        public AgentProxy agentPrefab;
        public float agentSpacing = 1.3F;
        public int columns = 6, rows = 2;

        [Header("Movement")]
        public float translationUnitsPerSecond = 1;
        public float rotationsPerSecond = 1;

        private float3[] formationPositions = null;
        private Bounds localBounds;

        public Entity SpawnUnit(EntityManager entityManager)
        {

            // Create entity prefab from the game object hierarchy once
            var entity = CreateEntity(entityManager, unitPrefab.gameObject);

            entityManager.AddComponentData(entity, new MoveSettings
            {
                TranslateSpeed = translationUnitsPerSecond,
                RotateSpeed = rotationsPerSecond
            });

            // entityManager.AddComponentData(entity, new Opponent { });

            switch (initialOrders)
            {
                case EOrder.PursueUnit:
                    entityManager.AddComponentData(entity, new OrderAttack { });
                    break;
                case EOrder.HoldPosition:
                    entityManager.AddComponentData(entity, new OrderHold { });
                    break;
                default:
                    break;
            }

            SpawnAgents(entity, entityManager);

            return entity;
        }


        // spawn multiple agents that follow this unit
        void SpawnAgents(Entity unit, EntityManager entityManager)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, entityManager.World);
            // int count = columns * rows;
            float3[] formationPositions = GetAgentFormationPositions();
            float3[] spawnPositions = GetAgentSpawnPositions(formationPositions);
            int count = formationPositions.Length;

            NativeArray<Entity> agents = new NativeArray<Entity>(formationPositions.Length, Allocator.Temp);
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk

            for (int i = 0; i < count; i++)
            {
                // float3 formationPosition = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                float3 formationPosition = formationPositions[i];
                float3 spawnPosition = spawnPositions[i];
                entityManager.SetName(agents[i], name + "_" + i);

                // entityManager.SetComponentData(agents[i], new Agent { });
                //TODO only add when move needed
                entityManager.AddComponentData(agents[i], new MoveToGoal());

                entityManager.AddComponentData(agents[i], new MoveSettings
                {
                    TranslateSpeed = translationUnitsPerSecond,
                    RotateSpeed = rotationsPerSecond
                });
                entityManager.AddComponentData(agents[i], new FormationMember
                {
                    Index = i,
                    Parent = unit,
                    Position = formationPosition
                });

                entityManager.SetComponentData(agents[i], new Translation { Value = spawnPosition });
                entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

            }
            agents.Dispose();
        }

        public float3[] GetAgentFormationPositions()
        {
            int count = columns * rows;
            if (formationPositions == null || formationPositions.Length != count)
            {
                formationPositions = new float3[count];
            }
            else
            {
                return formationPositions;
            }

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int i = x * rows + y;
                    // formationPositions[i] = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                    formationPositions[i] = new float3((x - columns / 2) * agentSpacing, 0, -y * agentSpacing);
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
            float agentRadius = agentSpacing / 2f;
            Gizmos.DrawCube(
                new Vector3(-agentRadius, 1, agentRadius - agentSpacing * rows / 2f),
                new Vector3(agentSpacing * columns, 2, agentSpacing * rows)
            );
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.matrix = transform.localToWorldMatrix;
            float3[] pos = GetAgentFormationPositions();
            for (int i = 0; i < pos.Length; i++)
                Gizmos.DrawSphere(
                        pos[i], agentSpacing / 2f
                );
        }
    }
}

