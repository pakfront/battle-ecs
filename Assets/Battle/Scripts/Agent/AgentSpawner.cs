// using System.Collections;
// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace UnitAgent
// {
//     public class AgentSpawner : MonoBehaviour
//     {

//         public int count = 10;

//         public GameObject agentPrefab;

//         void Start()
//         {
//             SpawnAgents();
//         }

//         void SpawnAgents()
//         {
//             Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(agentPrefab, World.Active);
//             var entityManager = World.Active.EntityManager;

//             NativeArray<Entity> agents = new NativeArray<Entity>(count, Allocator.Temp);
//             entityManager.Instantiate(prefab, agents);

//             var target = new TargetPosition { Value = transform.position };

//             for (int i = 0; i < count; i++)
//             {
//                 var position = transform.TransformPoint(new Vector3(i, 0, 0));

//                 entityManager.SetComponentData(agents[i], new Translation { Value = position });
//                 entityManager.SetComponentData(agents[i], new Rotation { Value = Quaternion.identity });
//                 entityManager.AddSharedComponentData(agents[i], target);
//             }
//             agents.Dispose();
//         }
//     }
// }