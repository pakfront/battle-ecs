using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent

{
    public class FormationSpawn : Spawn
    {
        public FormationProxy formationPrefab;
        public EFormation initialFormation;

        public Entity SpawnFormation(EntityManager entityManager)
        {
            var entity = CreateSelectableEntity(entityManager, formationPrefab.gameObject);
            return entity;
        }

        void OnDrawGizmosSelected()
        {

            int formationIndex = (int)initialFormation;
            if (formationIndex < 0 || formationIndex >= FormationUtils.FormationCount) return;

            UnityEditor.Handles.matrix  = transform.localToWorldMatrix;

            float3[] formationOffsets = FormationUtils.CalcUnitFormations();
            for (int i = 0; i < FormationUtils.UnitOffsetsPerFormation; i++)
            {
                Vector3 p = formationOffsets[formationIndex * FormationUtils.UnitOffsetsPerFormation + i];
#if UNITY_EDITOR
                UnityEditor.Handles.Label(p, i.ToString());
#endif

            }

        }

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
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
                Gizmos.DrawCube(
                  Vector3.zero,
                  Vector3.one
              );

        }
    }

}