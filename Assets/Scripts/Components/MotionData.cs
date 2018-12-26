using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Master
{
    public class MotionData : MonoBehaviour
    {
        public bool isClosedSpline = false;
        public int curveCount;
        public float3[] positions;
        public quaternion[] rotations;
    }
}
