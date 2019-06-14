using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent

{
    public class UnitGroupSpawn : Spawn
    {
        public UnitGroupProxy unitGroupPrefab;

        public Entity SpawnFormation(EntityManager entityManager)
        {
            var entity = CreateSelectableEntity(entityManager, unitGroupPrefab.gameObject);
            entityManager.SetComponentData(entity,
            new UnitGroupLeader
            {
                CurrentFormation = (int)initialFormation,
                FormationStartIndex = Formation.CalcUnitFormationStartIndex((int)initialFormation, formationTable)
            });

            return entity;
        }


        public void ApplyFormation()
        {
            Formation.CalcUnitFormationTables(out float3[] formationOffsets, out int[] formationTypes);

            int formationIndex = (int)initialFormation;
            int startIndex = Formation.CalcUnitFormationStartIndex(formationIndex, formationTable); //formationIndex * FormationUtils.MaxUnitsPerFormation;
            Debug.Log(name + " Applying Formation " + initialFormation + " " + startIndex);
            for (int i = 0; i < transform.childCount; i++)
            {
                var childXform = transform.GetChild(i);
                Vector3 p = formationOffsets[startIndex + i];
                childXform.position = transform.TransformPoint(p);
                childXform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), Vector3.up);

                var childFormationSpawn = childXform.GetComponent<UnitGroupSpawn>();
                if (childFormationSpawn != null)
                {
                    childFormationSpawn.initialFormation = (EFormation)formationTypes[startIndex + i];
                    childFormationSpawn.ApplyFormation();
                }

                var childUnitSpawn = childXform.GetComponent<UnitSpawn>();
                if (childUnitSpawn != null) childUnitSpawn.initialFormation = (EFormation)formationTypes[startIndex + i];
            }
        }

        public void ApplyTeam()
        {
            SetTeam(this.team);
        }

        public override void SetTeam(ETeam value)
        {
            #if UNITY_EDITOR
            // UnityEditor.Undo.RecordObject(this, "SetTeam");
            UnityEditor.EditorUtility.SetDirty(this);
            #endif

            this.team = value;
            unitGroupPrefab = SpawnManager.instance.teamUnitGroupProxy[(int)team];

            foreach (var child in GetComponentsInChildren<Spawn>())
            {
                if (child == this) continue;
                child.SetTeam(this.team);
            }
        }

        void OnDrawGizmosSelected()
        {

            int formationIndex = (int)initialFormation;
            if (formationIndex < 0 || formationIndex >= Formation.FormationCount) return;

            UnityEditor.Handles.matrix = transform.localToWorldMatrix;

            Formation.CalcUnitFormationTables(out float3[] formationOffsets, out int[] formationTypes);

            int startIndex = Formation.CalcUnitFormationStartIndex(formationIndex, formationTable); //formationIndex * FormationUtils.MaxUnitsPerFormation;
            for (int i = 0; i < Formation.MaxUnitsPerFormation; i++)
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