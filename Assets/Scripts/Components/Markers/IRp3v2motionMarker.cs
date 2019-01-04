using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Master
{
    public class IRp3v2motionMarker : MonoBehaviour
    {
        public readonly string TYPE = "IRp3v2";
        public bool useRotWithInterpolation = false;
        public bool useTangentFix = false;
        public bool useNormalFix = false;

        [Range(-50, 50)]
        public float[] w1Kofs = { -8.0f, -2.0f, 8.0f, 60.0f };
        [Range(-50, 50)]
        public float[] w2Kofs = { -1.0f, -4.0f,  -4.0f,  -50.0f };
    }
}
