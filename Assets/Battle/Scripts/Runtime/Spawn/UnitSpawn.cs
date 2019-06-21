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
        // public enum EOrder { None, InFormation, HoldPosition, MoveToPosition, FollowUnit, PursueUnit }
        // public enum EMobility { None, Foot, Horse, Wheeled }

        [Header("Unit")]
        public UnitProxy unitPrefab;
        public UnitGoalMarkerProxy unitGoalMarkerPrefab;

        public EUnitType unitType;
        public bool hasRanged;
        public bool hasMelee;
        // public EMobility mobility = EMobility.Foot;
        // public EOrder initialOrders;

        [Header("Agent")]
        public int agentCount = 60;
        public AgentProxy agentPrefab;
        // public float agentSpacing = 1.3F;
        // public int columns = 6, rows = 2;

        [Header("Movement")]
        public float translationUnitsPerSecond = 1;
        public float rotationsPerSecond = 1;

        private Bounds localBounds;

        public override void SetTeam(ETeam value)
        {
            #if UNITY_EDITOR
            // UnityEditor.Undo.RecordObject(this, "SetTeam");
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            this.team = value;
            unitPrefab = SpawnManager.instance.teamUnitProxy[(int)team];
            unitGoalMarkerPrefab = SpawnManager.instance.teamUnitGoalMarkerProxy[(int)team];
            agentPrefab = SpawnManager.instance.teamAgentProxy[(int)team];
        }

        void OnValidate()
        {
            if (agentCount > Formation.MaxAgentsPerFormation) agentCount = Formation.MaxAgentsPerFormation;
            else if (agentCount < 0) agentCount = 0;
        }
        public Entity SpawnUnit(EntityManager entityManager)
        {

            // Create entity prefab from the game object hierarchy once
            var unitEntity = CreateSelectableEntity(entityManager, unitPrefab.gameObject);
            AddOrderableComponents(entityManager, unitEntity);
            entityManager.AddComponentData(unitEntity, new MoveSettings
            {
                TranslateSpeed = translationUnitsPerSecond,
                RotateSpeed = rotationsPerSecond
            });

            entityManager.SetComponentData(unitEntity,
            new Goal
            {
                Value = float4x4.TRS(this.transform.position, this.transform.rotation, Movement.unitScale)
            });

            switch (unitType)
            {
                case EUnitType.Foot:
                    entityManager.AddComponentData(unitEntity, new Foot());
                    break;
                case EUnitType.Horse:
                    entityManager.AddComponentData(unitEntity, new Horse());
                    break;
                case EUnitType.Artillery:
                    entityManager.AddComponentData(unitEntity, new Artillery());
                    break;
                case EUnitType.Train:
                    entityManager.AddComponentData(unitEntity, new Train());
                    break;
                case EUnitType.HQ:
                    entityManager.AddComponentData(unitEntity, new HQ());
                    break;
            }

            if (hasMelee) 
                entityManager.AddComponentData(unitEntity, new Melee());
            
            if (true) 
                entityManager.AddComponentData(unitEntity, new Ranged{
                    Range = 80
                });

            entityManager.AddComponentData(unitEntity, new Opponent { });

            // switch (initialOrders)
            // {
            //     case EOrder.PursueUnit:
            //         entityManager.AddComponentData(unitEntity, new OrderAttack { });
            //         break;
            //     case EOrder.HoldPosition:
            //         entityManager.AddComponentData(unitEntity, new OrderHold { });
            //         break;
            //     default:
            //         break;
            // }



            SpawnAgents(unitEntity, entityManager);

            // create a secondary entity that draws the goal
            var unitGoalMarkerEntity = CreateEntity(entityManager, unitGoalMarkerPrefab.gameObject);
            entityManager.SetComponentData(unitGoalMarkerEntity, new UnitGoalMarker { Unit = unitEntity });

            return unitEntity;
        }


        // spawn multiple agents that follow this unit
        void SpawnAgents(Entity unit, EntityManager entityManager)
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                agentPrefab.gameObject, entityManager.World);
            Formation.CalcAgentFormationOffsetTable(out float3[] formationPositions);
            float3[] spawnPositions = GetAgentSpawnPositions(formationPositions);

            NativeArray<Entity> agents = new NativeArray<Entity>(agentCount, Allocator.Temp);
            entityManager.Instantiate(prefab, agents);

            // SharedComponent placed on Agents o we can process by chunk
            int startIndex = Formation.CalcAgentFormationStartIndex((int)initialFormation, formationTable);
            Debug.Log(name+" Spawning Agents "+initialFormation+" startIndex:"+startIndex);
            entityManager.AddComponentData(unit, new AgentCount { Value = agentCount});
            for (int i = 0; i < agentCount; i++)
            {
                // float3 formationPosition = transform.TransformPoint(new float3(x * 1.3F, 0, y * 1.3F));
                float3 formationOffset = formationPositions[startIndex+i];
                float3 spawnPosition = spawnPositions[startIndex+i];
                entityManager.SetName(agents[i], name + "_" + i);

                // entityManager.SetComponentData(agents[i], new Agent { });
                //TODO only add when move needed
                entityManager.AddComponentData(agents[i], new MoveToGoalTag());

                entityManager.AddComponentData(agents[i], new MoveSettings
                {
                    TranslateSpeed = translationUnitsPerSecond,
                    RotateSpeed = rotationsPerSecond
                });
                entityManager.AddComponentData(agents[i], new AgentFormationMember
                {
                    Index = i,
                    Parent = unit,
                    // Offset = formationOffset
                });

                entityManager.SetComponentData(agents[i], new Translation { Value = spawnPosition });
                entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });

            }
            agents.Dispose();
        }

        public float3[] GetAgentFormationPositions()
        {
            float3[] formationPositions = new float3[agentCount];
            int formationIndex = (int)initialFormation;
            if (formationIndex < 0 || formationIndex >= Formation.FormationCount) return formationPositions;

            Formation.CalcAgentFormationOffsetTable(out float3[] formationOffsets);
            for (int i = 0; i < agentCount; i++)
            {
                formationPositions[i] = formationOffsets[formationIndex * Formation.MaxAgentsPerFormation + i];
            }
            return formationPositions;
        }


        public float3[] GetAgentSpawnPositions(float3[] formationPositions)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)gameObject.GetInstanceID());

            float3[] spawnPositions = new float3[formationPositions.Length];

            for (int i = 0; i < spawnPositions.Length; i++)
            {
                spawnPositions[i] = (float3)(transform.TransformPoint(formationPositions[i])) + new float3(random.NextFloat(-4, 4), 0, random.NextFloat(-4, 4));
            }
            return spawnPositions;
        }

        void OnDrawGizmos()
        {

            switch (team)
            {
                case ETeam.Red:
                    Gizmos.color = Color.red;
                    break;
                case ETeam.Blue:
                    Gizmos.color = Color.blue;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;

            }
            Gizmos.matrix = transform.localToWorldMatrix;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

            float3[] pos = GetAgentFormationPositions();
            for (int i = 0; i < pos.Length; i++)
            {
                bounds.Encapsulate(pos[i]);
            }

            if (UnityEditor.Selection.activeGameObject == this.gameObject)
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            else
                Gizmos.DrawCube(bounds.center, bounds.size);

            // float agentRadius = agentSpacing / 2f;
            // Gizmos.DrawCube(
            //     new Vector3(-agentRadius, 1, agentRadius - agentSpacing * rows / 2f),
            //     new Vector3(agentSpacing * columns, 2, agentSpacing * rows)
            // );
        }

        void OnDrawGizmosSelected()
        {
            // if (UnityEditor.Selection.activeGameObject != this.gameObject) return;

            Gizmos.color = Color.gray;
            Gizmos.matrix = transform.localToWorldMatrix;
            switch (team)
            {
                case ETeam.Red:
                    Gizmos.color = Color.red;
                    break;
                case ETeam.Blue:
                    Gizmos.color = Color.blue;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;

            }
            float3[] pos = GetAgentFormationPositions();
            if (UnityEditor.Selection.activeGameObject == this.gameObject)
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    Gizmos.DrawSphere(
                            pos[i], 1.6f / 2f
                    );
                }
            }
            else
            {
                Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
                for (int i = 0; i < pos.Length; i++)
                {
                    bounds.Encapsulate(pos[i]);
                }
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }

        }
    }
}

