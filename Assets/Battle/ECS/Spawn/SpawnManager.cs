using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace UnitAgent
{
    public class SpawnManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SpawnUnits();
        }

        void SpawnUnits()
        {
            var entityManager = World.Active.EntityManager;

            Dictionary<UnitSpawn, Entity> map = new Dictionary<UnitSpawn, Entity>();
            foreach( var unitSpawn in FindObjectsOfType<UnitSpawn>())
            {
                map[unitSpawn] = unitSpawn.SpawnUnit(entityManager);
            }

            foreach (var outer in map )
            {
                var unitSpawn = outer.Key;
                var unitEntity = outer.Value;

                if (unitSpawn.superior == null) continue;

                var superiorEntity = map[unitSpawn.superior];
                //unitEntity.Set( )unitSpawn

                
            }

        }
  
    }
}
