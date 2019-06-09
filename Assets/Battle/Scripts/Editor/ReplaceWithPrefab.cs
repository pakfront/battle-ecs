using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
// COMMUNITY THREAD LINK https://forum.unity.com/threads/replace-game-object-with-prefab.24311/
// CopyComponents - by Michael L. Croswell for Colorado Game Coders, LLC
// March 2010
//Modified by Kristian Helle Jespersen
//June 2011
//Modified by Connor Cadellin McKee for Excamedia
//April 2015
//Modified by Fernando Medina (fermmmm)
//April 2015
//Modified by Julien Tonsuso (www.julientonsuso.com)
//July 2015
//Changed into editor window and added instant preview in scene view
//Modified by Alex Dovgodko
//June 2017
//Made changes to make things work with Unity 5.6.1
//March 2018
//Added link to community thread, booleans to chose if scale and rotation are applied, mark scene as dirty, changed menu item to tools. By Hyper
//May 2018
//Added KeepPlaceInHeirarchy self explanatory
//Modified by Virgil Iordan
//Added unique numbering identifier in the hierarchy to each newly instantiated prefab, also accounts for existing numbers
//Modified by Dev Bye-A-Jee, Sanjay Sen & Nick Rodriguez for Ravensbourne University London
//January 2019
public class ReplaceWithPrefab:EditorWindow {
    public GameObject Prefab;
    public GameObject[] ObjectsToReplace;
    public List<GameObject> TempObjects = new List<GameObject>();
    public bool KeepOriginalNames = true;
    public bool EditMode = false;
    public bool ApplyRotation = true;
    public bool ApplyScale = true;
    public bool KeepPlaceInHeirarchy = false;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/ReplaceWithPrefab")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        ReplaceWithPrefab window = (ReplaceWithPrefab)EditorWindow.GetWindow(typeof(ReplaceWithPrefab));
        window.Show();
    }
    void OnSelectionChange() {
        GetSelection();
        Repaint();
    }
    void CheckParent() {
    }
    void OnGUI() {
        EditMode = GUILayout.Toggle(EditMode,"Edit");
        if (GUI.changed) {
            if (EditMode)
                GetSelection();
            else
                ResetPreview();
        }
        KeepOriginalNames = GUILayout.Toggle(KeepOriginalNames,"Keep names");
        ApplyRotation = GUILayout.Toggle(ApplyRotation,"Apply rotation");
        ApplyScale = GUILayout.Toggle(ApplyScale,"Apply scale");
        KeepPlaceInHeirarchy = GUILayout.Toggle(KeepPlaceInHeirarchy,"Keep Place In Hierarchy");
        GUILayout.Space(5);
        if (EditMode) {
            ResetPreview();
            GUI.color = Color.yellow;
            if (Prefab != null) {
                GUILayout.Label("Prefab: ");
                GUILayout.Label(Prefab.name);
            } else {
                GUILayout.Label("No prefab selected");
            }
            GUI.color = Color.white;
            GUILayout.Space(5);
            GUILayout.BeginScrollView(new Vector2());
            foreach (GameObject go in ObjectsToReplace) {
                GUILayout.Label(go.name);
                if (Prefab != null) {
                    GameObject newObject;
                    newObject = (GameObject)PrefabUtility.InstantiatePrefab(Prefab);
                    newObject.transform.SetParent(go.transform.parent,true);
                    newObject.transform.localPosition = go.transform.localPosition;
                    if (ApplyRotation) {
                        newObject.transform.localRotation = go.transform.localRotation;
                    }
                    if (ApplyScale) {
                        newObject.transform.localScale = go.transform.localScale;
                    }
                    TempObjects.Add(newObject);
             
                    if (KeepOriginalNames) {
                        newObject.transform.name = go.transform.name;
                    }
                    go.SetActive(false);
                 
                    if (KeepPlaceInHeirarchy) {
                        newObject.transform.SetSiblingIndex(go.transform.GetSiblingIndex());
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply")) {
                if (!KeepOriginalNames) {
                    int count = 0;
                    List<int> ExistingNumbers = new List<int>();
                    foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
                    {
                        if (obj.name.Contains(Prefab.name))
                        {
                            string[] stringSplit = obj.name.Split(' '); // number deliminator, setup for default Unity naming convention i.e "Cube (1)"
                            if (stringSplit.Length > 1) {
                                char[] charsToTrim = {'(', ')'}; // extra characters to trim
                                 string num = stringSplit[1].Trim(charsToTrim); // substring which contains number
                                int x = int.Parse(num); // convert string to number
                                ExistingNumbers.Add(x);
                             }
                        }
                    }
                    foreach (GameObject go in TempObjects) {
                        count++;
                         if (ExistingNumbers.Count > 0) {
                            int i = 0;
                            while (i < (ExistingNumbers.Count)) {
                                if (count == (ExistingNumbers[i])) {
                                    count++;
                                    i = 0;
                                } else {
                                    i++;
                                }
                            }
                        }
                        go.transform.name = go.transform.name + " (" + count + ")";
                    }
                }
                foreach (GameObject go in ObjectsToReplace) {
                    DestroyImmediate(go);
                }
                EditMode = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // So that we don't forget to save...
            };
            if (GUILayout.Button("Cancel")) {
                ResetPreview();
                EditMode = false;
            };
            GUILayout.EndHorizontal();
        } else {
            ObjectsToReplace = new GameObject[0];
            TempObjects.Clear();
            Prefab = null;
        }
    }
    void OnDestroy() {
        ResetPreview();
    }
    void GetSelection() {
        if (EditMode && Selection.activeGameObject != null) {
            PrefabType t = PrefabUtility.GetPrefabType(Selection.activeGameObject);
            if (t == PrefabType.Prefab) //Here goes the fix
            {
                Prefab = Selection.activeGameObject;
            } else {
                ResetPreview();
                ObjectsToReplace = Selection.gameObjects;
            }
        }
    }
    void ResetPreview() {
        if (TempObjects != null) {
            foreach (GameObject go in TempObjects) {
                DestroyImmediate(go);
            }
        }
        foreach (GameObject go in ObjectsToReplace) {
            go.SetActive(true);
        }
        TempObjects.Clear();
    }
}
