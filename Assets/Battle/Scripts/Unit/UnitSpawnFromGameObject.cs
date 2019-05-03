// using System.Collections;
// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace UnitAgent

// {
//     public class UnitSpawnFromGameObject : MonoBehaviour
//     {
//         public UnitProxy unitPrefab;

//         public AgentProxy agentPrefab;

        

//         public int xCount = 4, yCount = 2;
    
//         void Start()
//         {
//             SpawnUnit();
//         }

//         void SpawnUnit()
//         {
//             // Create entity prefab from the game object hierarchy once
//             Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab, World.Active);
//             var entityManager = World.Active.EntityManager;
        
//             for (int x = 0; x < xCount; x++)
//             {
//                 for (int y = 0; y < yCount; y++)
//                 {
//                     // Efficiently instantiate a bunch of entities from the already converted entity prefab
//                     var instance = entityManager.Instantiate(prefab);
                
//                     // Place the instantiated entity in a grid with some noise
//                     var position = transform.TransformPoint(new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));
//                     entityManager.SetComponentData(instance, new Translation {Value = position});
//                 }
//             }
//         }


//     void SpawnAgents(int unitIndex, int count)
//     {

//         Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab, World.Active);
//         var entityManager = World.Active.EntityManager;

//         NativeArray<Entity> agents = new NativeArray<Entity>(count, Allocator.Temp);
//         entityManager.Instantiate(prefab, agents);

//         for (int i = 0; i < count; i++)
//         {
//             var position = transform.TransformPoint(new Vector3(i,0,0));

//             entityManager.SetComponentData(agents[i], new Translation{ Value = position});
//             entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity});
//         }
//         agents.Dispose();
//     }
//     }    
// }

