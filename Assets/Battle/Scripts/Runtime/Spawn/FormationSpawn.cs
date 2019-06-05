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

        
        public void ApplyFormation()
        {
            float3 []  formationOffsets = FormationUtils.CalcUnitFormations();
            int formationIndex = (int)initialFormation;
            int startIndex = formationIndex * FormationUtils.UnitOffsetsPerFormation;
            Debug.Log(name+" Applying Formation "+initialFormation+" "+startIndex);
            for (int i = 0; i < transform.childCount; i++)
            {
                var childXform =  transform.GetChild(i);
                Vector3 p = formationOffsets[startIndex +i];
                childXform.position = transform.TransformPoint(p);
                childXform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), Vector3.up);
            
                var childSpawn = childXform.GetComponent<FormationSpawn>();
                if (childSpawn != null) childSpawn.ApplyFormation();
            }
        }

        void OnDrawGizmosSelected()
        {

            int formationIndex = (int)initialFormation;
            if (formationIndex < 0 || formationIndex >= FormationUtils.FormationCount) return;

            UnityEditor.Handles.matrix  = transform.localToWorldMatrix;

            float3[] formationOffsets = FormationUtils.CalcUnitFormations();
            int startIndex = formationIndex * FormationUtils.UnitOffsetsPerFormation; 
            for (int i = 0; i < FormationUtils.UnitOffsetsPerFormation; i++)
            {
                Vector3 p = formationOffsets[startIndex + i];
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