using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.ComponentModel;

namespace Master
{
    public class TravelerData : MonoBehaviour
    {
        public GameObjectEntity motion;
        public float3 futurePosition;
        public quaternion futureRotation;
        public int pathIndex = 0;

        void Start()
        {
            futurePosition = transform.position;
            futureRotation = transform.rotation;
        }
    }
}

