using UnityEditor;
using UnityEngine;
using UnitAgent;
using Unity.Mathematics;

[CustomEditor(typeof(FormationSpawn)), CanEditMultipleObjects]
public class FormationSpawnEditor : Editor
{
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            FormationSpawn obj = target as FormationSpawn;
            if (GUILayout.Button("Apply Formation"))
            {
                obj.ApplyFormation();
            }
        }
}