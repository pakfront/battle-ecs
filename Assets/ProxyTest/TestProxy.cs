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
    public class TestProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
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
            dstManager.SetName(entity,"Parent");

            //Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(rotationSpeedProxyPrefab, dstManager.World);
            int count = 4;

            // NativeArray<Entity> kids = new NativeArray<Entity>(count, Allocator.Temp);
            // dstManager.Instantiate(prefab, kids);
            var kidPrimary = conversionSystem.GetPrimaryEntity(rotationSpeedProxyPrefab);
            for (int i = 0; i < count; i++)
            {
                var kid = conversionSystem.CreateAdditionalEntity(rotationSpeedProxyPrefab);
                dstManager.SetName(kid,"Kid"+i);
                dstManager.SetComponentData(kid, new RotationSpeed { RadiansPerSecond  = 20*i });
            }
            // kids.Dispose();
        }
    }

    [Serializable] public struct TestComponentData : IComponentData {}
     
}
