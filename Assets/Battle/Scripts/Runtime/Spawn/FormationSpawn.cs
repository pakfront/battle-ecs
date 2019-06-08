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
            FormationUtils.CalcUnitFormations(out float3[] formationOffsets, out int[] formationTypes);

            int formationIndex = (int)initialFormation;
            int startIndex = formationIndex * FormationUtils.MaxUnitsPerFormation;
            Debug.Log(name+" Applying Formation "+initialFormation+" "+startIndex);
            for (int i = 0; i < transform.childCount; i++)
            {
                var childXform =  transform.GetChild(i);
                Vector3 p = formationOffsets[startIndex +i];
                childXform.position = transform.TransformPoint(p);
                childXform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), Vector3.up);
            
                var childFormationSpawn = childXform.GetComponent<FormationSpawn>();
                if (childFormationSpawn != null) {
                    childFormationSpawn.initialFormation = (EFormation)formationTypes[startIndex +i];
                    childFormationSpawn.ApplyFormation();
                }

                var childUnitSpawn = childXform.GetComponent<UnitSpawn>();
                if (childUnitSpawn != null) childUnitSpawn.agentFormation = (EFormation)formationTypes[startIndex +i];
            }
        }

        public void ApplyTeam()
        {
            foreach (var child in GetComponentsInChildren<Spawn>())
            {
                child.team = this.team;
            }
        }

        void OnDrawGizmosSelected()
        {

            int formationIndex = (int)initialFormation;
            if (formationIndex < 0 || formationIndex >= FormationUtils.FormationCount) return;

            UnityEditor.Handles.matrix  = transform.localToWorldMatrix;

            FormationUtils.CalcUnitFormations(out float3[] formationOffsets, out int[] formationTypes);

            int startIndex = formationIndex * FormationUtils.MaxUnitsPerFormation; 
            for (int i = 0; i < FormationUtils.MaxUnitsPerFormation; i++)
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