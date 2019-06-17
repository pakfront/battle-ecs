using UnityEditor;
using UnityEngine;
using UnitAgent;
using Unity.Mathematics;

[CustomEditor(typeof(UnitGroupSpawn)), CanEditMultipleObjects]
public class UnitGroupSpawnnEditor : Editor
{
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            UnitGroupSpawn obj = target as UnitGroupSpawn;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Rank", obj.GetRank());
            EditorGUILayout.IntField("Member Index", obj.GetMemberIndex());
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("ApplyTeam"))
            {
                obj.ApplyTeam();
            }
            if (GUILayout.Button("ApplyFormation"))
            {
                obj.ApplyFormation();
            }
        }
}