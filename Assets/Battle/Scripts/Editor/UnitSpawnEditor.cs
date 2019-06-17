using UnityEditor;
using UnityEngine;
using UnitAgent;
using Unity.Mathematics;

[CustomEditor(typeof(UnitSpawn)), CanEditMultipleObjects]
public class UnitSpawnnEditor : Editor
{
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            UnitSpawn obj = target as UnitSpawn;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Rank", obj.GetRank());
            EditorGUILayout.IntField("Member Index", obj.GetMemberIndex());
            EditorGUI.EndDisabledGroup();
        }
}