using System;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Samples.HelloCube_02;

namespace ProxyTest
{
        [RequiresEntityConversion]
    public class TestProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject rotationSpeedProxyPrefab;
        // Start is called before the first frame update
       public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
        {
            gameObjects.Add(rotationSpeedProxyPrefab);
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TestComponentData { });

            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(rotationSpeedProxyPrefab, dstManager.World);
            int count = 4;

            NativeArray<Entity> kids = new NativeArray<Entity>(count, Allocator.Temp);
            for (int i = 0; i < count; i++)
            {
                dstManager.SetComponentData(kids[i], new RotationSpeed { RadiansPerSecond  = 20*i });
            }
            kids.Dispose();
        }
    }

    [Serializable] public struct TestComponentData : IComponentData {}
     
}
