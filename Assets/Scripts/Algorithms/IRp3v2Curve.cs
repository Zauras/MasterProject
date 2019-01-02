using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Master
{
    public class IRp3v2Curve : AbstractCurveAlgorithm
    {
        // ##### API & Setup ######################################################
        public static (List<float3>, List<quaternion>) FindIRcMotion(Transform curveT,
                                                                     ref float4 w0,
                                                                     bool isRotWithWeight,
                                                                     float3[] controlPoints,
                                                                     float3 frontVector, 
                                                                     float3 backVector)
        {
            float3 delta40 = GetDelta(controlPoints[2], controlPoints[0]);
            float3 delta20 = GetDelta(controlPoints[1], controlPoints[0]);
            float3 delta24 = GetDelta(controlPoints[1], controlPoints[2]);

           // float4 wx = new float4(0f, 0f, 0f, 1f); // IGNOR w0 (experiment) !!!
            float4 w1 = GenerateWeight(-8.0f, -2.0f, 16.0f, -1.0f,
                                        delta40, delta20, backVector, delta24,
                                        delta40, frontVector, backVector, delta40);
            float4 w2 = GenerateWeight(-1.0f, -4.0f, 4.0f, 1.0f,
                                        delta20, delta40, delta24, backVector,
                                        delta20, frontVector, delta24, delta40);

            float4[] weights = { w0, w1, w2 };
            w0 = w2;

            return GenerateCurve(curveT, isRotWithWeight, controlPoints, weights); ;
        }
    }
}
