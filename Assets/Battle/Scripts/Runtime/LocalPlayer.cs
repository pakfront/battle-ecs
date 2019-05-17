using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitAgent
{
    public class LocalPlayer : MonoBehaviour
    {
        static LocalPlayer instance;
        public static int Team => (instance == null ? 0 : instance.team);
        [SerializeField] private int team = 0;


        void Awake()
        {
            instance = this;
        }

    }
}
