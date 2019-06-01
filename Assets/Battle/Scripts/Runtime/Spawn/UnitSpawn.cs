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
        public UnitGoalMarkerProxy unitGoalMarkerPrefab;
        public EOrder initialOrders;

        [Header("Agent")]
        public EFormation agentFormation = EFormation.Line;
        public int agentCount = 60;
        public AgentProxy agentPrefab;
        // public float agentSpacing = 1.3F;
        // public int columns = 6, rows = 2;

        [Header("Movement")]
        public float translationUnitsPerSecond = 1;
        public float rotationsPerSecond = 1;

        private Bounds localBounds;

        public Entity SpawnUnit(EntityManager entityManager)
        {

            // Create entity prefab from the game object hierarchy once
            var unitEntity = CreateSelectableEntity(entityManager, unitPrefab.gameObject);
            entityManager.AddComponentData(unitEntity, new MoveSettings
            {
                TranslateSpeed = translationUnitsPerSecond,
                RotateSpeed = rotationsPerSecond
            });

            // entityManager.AddComponentData(entity, new Opponent { });

            switch (initialOrders)
            {
                case EOrder.PursueUnit:
                    entityManager.AddComponentData(unitEntity, new OrderAttack { });
                    break;
                case EOrder.HoldPosition:
                    entityManager.AddComponentData(unitEntity, new OrderHold { });
                    break;
                default:
                    break;
            }

            SpawnAgents(unitEntity, entityManager);

            var unitGoalMarkerEntity = CreateEntity(entityManager, unitGoalMarkerPrefab.gameObject);
            entityManager.SetComponentData(unitGoalMarkerEntity, new UnitGoalMarker { Unit = unitEntity });

            return unitEntity;
        }


        // spawn multiple agents that follow this unit
        void SpawnAgents(Entity unit, EntityManager entityManager)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab.gameObject, entityManager.World);
            // int count = columns * rows;
            float3[] formationPositions = FormationUtils.CalcAgentFormations();
            float3[] spawnPositions = GetAgentSpawnPositions(formationPositions);

            NativeArray<Entity> agents = new NativeArray<Entity>(formationPositions.Length, Allocator.Temp);
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk

            for (int i = 0; i < agentCount; i++)
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
                entityManager.AddComponentData(agents[i], new AgentFormationMember
                {
                    Index = i,
                    Parent = unit,
                    Offset = formationPosition
                });

                entityManager.SetComponentData(agents[i], new Translation { Value = spawnPosition });
                entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

            }
            agents.Dispose();
        }

        public float3[] GetAgentFormationPositions()
        {
            float3 [] formationPositions = new float3[agentCount];
            int formationIndex = (int)agentFormation;
            if (formationIndex < 0 || formationIndex >= FormationUtils.FormationCount) return formationPositions;

            float3[] formationOffsets = FormationUtils.CalcAgentFormations();
            for (int i = 0; i < agentCount; i++)
            {
                formationPositions[i] = formationOffsets[formationIndex * FormationUtils.UnitOffsetsPerFormation + i];
            }
            return formationPositions;

            // int count = columns * rows;
            // if (formationPositions == null || formationPositions.Length != count)
            // {
            //     formationPositions = new float3[count];
            // }
            // else
            // {
            //     return formationPositions;
            // }

            // for (int x = 0; x < columns; x++)
            // {
            //     for (int y = 0; y < rows; y++)
            //     {
            //         int i = x * rows + y;
            //         // formationPositions[i] = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
            //         formationPositions[i] = new float3((x - columns / 2) * agentSpacing, 0, -y * agentSpacing);
            //     }
            // }

            // for (int i = 0; i < count; i++)
            // {
            //     FormationUtils.DistributeAcrossColumns(columns, i, out int row, out int col);
            //     formationPositions[i] = new float3(col * agentSpacing, 0, row * agentSpacing);
            // }

               
            // return formationPositions;
        }


        public float3[] GetAgentSpawnPositions(float3[] formationPositions)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)gameObject.GetInstanceID());

            float3[] spawnPositions = new float3[agentCount];

            for (int i = 0; i < agentCount; i++)
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
            Bounds bounds = new Bounds( Vector3.zero, Vector3.one);

           float3[] pos = GetAgentFormationPositions();
            for (int i = 0; i < pos.Length; i++)
                bounds.Encapsulate(pos[i]);

            Gizmos.DrawCube(
                bounds.center,
               bounds.size
            );        

            // float agentRadius = agentSpacing / 2f;
            // Gizmos.DrawCube(
            //     new Vector3(-agentRadius, 1, agentRadius - agentSpacing * rows / 2f),
            //     new Vector3(agentSpacing * columns, 2, agentSpacing * rows)
            // );
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.matrix = transform.localToWorldMatrix;
            float3[] pos = GetAgentFormationPositions();
            for (int i = 0; i < pos.Length; i++)
                Gizmos.DrawSphere(
                        pos[i], 1.6f / 2f
                );
        }
    }
}

