﻿using System.Collections.Generic;
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
                                                                     bool useTengentFix,
                                                                     bool useNormalFix,
                                                                     bool isRotWithInterpolation,
                                                                     float3[] controlPoints,
                                                                     float3 frontVector, 
                                                                     float3 backVector,
                                                                     float[] w1Kof,
                                                                     float[] w2Kof)
        {
            /*
            frontVector = math.abs(frontVector - controlPoints[0]);
            backVector = backVector - controlPoints[2];
>>>>>>> Migration to Unity 2019.1
            float3 delta40 = GetDelta(controlPoints[2], controlPoints[0]);
            float3 delta20 = GetDelta(controlPoints[1], controlPoints[0]);
            float3 delta24 = GetDelta(controlPoints[1], controlPoints[2]);

<<<<<<< 62a68c1b7c7f32ca055ac82cbabb802c58577240
           // float4 wx = new float4(0f, 0f, 0f, 1f); // IGNOR w0 (experiment) !!!
            float4 w1 = GenerateWeight(-8.0f, -2.0f, 16.0f, -1.0f,
                                        delta40, delta20, backVector, delta24,
                                        delta40, frontVector, backVector, delta40);
            float4 w2 = GenerateWeight(-1.0f, -4.0f, 4.0f, 1.0f,
=======
            float3 delta42 = GetDelta(controlPoints[2], controlPoints[1]);
            */

            /*
             float4 w1 = GenerateWeight(-8.0f, -2.0f, 16.0f, -1.0f,
                                         delta40, delta20, backVector, delta24,
                                         delta40, frontVector, backVector, delta40,
                                         w0);
             float4 w2 = GenerateWeight(-1.0f, -4.0f, 4.0f, 1.0f,
                                         delta20, delta40, delta24, backVector,
                                         delta20, frontVector, delta24, delta40,
                                         w0);
             */
             

            /*
            float4 w1 = GenerateWeight(w1Kof[0], w1Kof[1], w1Kof[2], w1Kof[3],
                                        delta40, delta20, backVector, delta24,
                                        delta40, frontVector, backVector, delta40,
                                        w0);

            float4 w2 = GenerateWeight(w2Kof[0], w2Kof[1], w2Kof[2], w2Kof[3],
>>>>>>> Migration to Unity 2019.1
                                        delta20, delta40, delta24, backVector,
                                        delta20, frontVector, delta24, delta40,
                                        w0);
           */


            frontVector = (frontVector - controlPoints[0]);
            backVector = (backVector - controlPoints[2]);
            float3 delta20 = GetDelta(controlPoints[2], controlPoints[0]);
            float3 delta10 = GetDelta(controlPoints[1], controlPoints[0]);
            float3 delta12 = GetDelta(controlPoints[1], controlPoints[2]);
            float3 delta21 = GetDelta(controlPoints[2], controlPoints[1]);

            float4 w1 = GenerateWeightTest1(w1Kof[0], w1Kof[1], w1Kof[2], w1Kof[3],
                                        delta20, delta10, backVector, delta12,
                                        delta20, frontVector, backVector, delta20,
                                        w0);

            float4 w2 = GenerateWeightTest1(w2Kof[0], w2Kof[1], w2Kof[2], w2Kof[3],
                                        delta10, delta20, delta12, backVector,
                                        delta20, frontVector, delta12, delta20,
                                        w0);

            float4[] weights = { w0, w1, w2 };
            w0 = w2;
            
            return GenerateCurve(
                curveT, useTengentFix,
                useNormalFix, isRotWithInterpolation,
                controlPoints, weights);
        }
    }
}
