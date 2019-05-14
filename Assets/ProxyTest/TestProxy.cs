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
    public class TestProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        
        public Samples.HelloCube_02.RotationSpeedProxy rotationSpeedProxyPrefab;

        // this is not needed, I think
       public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
        {
            gameObjects.Add(rotationSpeedProxyPrefab.gameObject);
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TestComponentData { });
            // SetName does not fail, but no name is seen in debugger
            dstManager.SetName(entity,"Parent");

            Entity prefabEntity = conversionSystem.GetPrimaryEntity(rotationSpeedProxyPrefab.gameObject);
            // I don't think instantiation of the prefab entity is needed
            // dstManager.Instantiate(prefabEntity);
            int count = 4;
            for (int i = 0; i < count; i++)
            {
                var kid = conversionSystem.CreateAdditionalEntity(rotationSpeedProxyPrefab);
                // SetName does not fail, but no name is seen in debugger
                dstManager.SetName(kid,"Kid"+i);
                // Error: ArgumentException: A component with type:RotationSpeed has not been added to the entity.
                dstManager.SetComponentData(kid, new Samples.HelloCube_02.RotationSpeed { RadiansPerSecond  = 20*i });
            }
        }
    }

    [Serializable] public struct TestComponentData : IComponentData {}
}
