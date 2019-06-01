// using UnityEditor;
// using UnityEngine;
// using UnitAgent;
// using Unity.Mathematics;

// [CustomEditor(typeof(FormationSpawn)), CanEditMultipleObjects]
// public class FormationSpawnEditor : Editor
// {
//     protected virtual void OnSceneGUI()
//     {
//         FormationSpawn example = (FormationSpawn)target;

//         Handles.color = Color.black;
//         Handles.Label(example.transform.position + Vector3.up * 2, example.name );

//         float3 [] formationPositions = example.GetFormationPositions();
//         float3 [] spawnPositions = example.GetSpawnPositions(formationPositions);

//         for (int i = 0; i < formationPositions.Length; i++)
//         {
//             Handles.color = Color.black;
//             Handles.matrix = example.transform.localToWorldMatrix;
// 			// UnityEditor.Handles.Label( (Vector3)(TransformPoint(formationPositions[i])), i.ToString());
// 			// UnityEditor.Handles.Label( formationPositions[i], i.ToString());

//             // Handles.color = Color.yellow;
// 			// UnityEditor.Handles.Label( spawnPositions[i], i.ToString());
//         }


//         // EditorGUI.BeginChangeCheck();
//         // Vector3 p;
//         // //Handles.matrix = example.transform.localToWorldMatrix;
//         // for (int i = 0; i < example.positions.Length; i++)
//         // {
//         //     //p = example.positions[i];
//         //     //// p = Handles.PositionHandle(p, Quaternion.identity); // 1, Vector3.one, Handles.DotHandleCap);
//         //     //p = Handles.FreeMoveHandle(p, Quaternion.identity, 1, Vector3.one, Handles.RectangleHandleCap);
//         //     //example.positions[i] = p;
//         // 	Handles.color = Color.yellow;
//         //     p = example.transform.TransformPoint(example.positions[i]);
//         //     p = Handles.FreeMoveHandle(p, Quaternion.identity, 1, Vector3.one, Handles.SphereHandleCap);
//         //     example.positions[i] = example.transform.InverseTransformPoint(p);
//         //     Handles.color = Color.black;
// 		// 	UnityEditor.Handles.Label( p, i.ToString());							


//         // }
//         // //Handles.matrix = Matrix4x4.identity;

//         // if (EditorGUI.EndChangeCheck())
//         // {
//         //     Undo.RecordObject(example, "Change Look At Target Position");        
//         // }
//     }
// }