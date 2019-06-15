using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnitAgent
{
    [RequiresEntityConversion]
    public class AgentProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var agent = new Agent { };
            dstManager.AddComponentData(entity, agent);
            dstManager.AddComponentData(entity, new Goal {} );
        }
    }
}