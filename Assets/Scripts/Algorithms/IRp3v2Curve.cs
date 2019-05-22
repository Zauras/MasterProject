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
                                                                     float3 frontVectorPoint, 
                                                                     float3 backVectorPoint,
                                                                     float[] w1Kof,
                                                                     float[] w2Kof)
        {
            float3 frontVector = frontVectorPoint - controlPoints[0];
            float3 backVector = backVectorPoint - controlPoints[2];
            float3 delta20 = GetDelta(controlPoints[2], controlPoints[0]);
            float3 delta10 = GetDelta(controlPoints[1], controlPoints[0]);
            float3 delta12 = GetDelta(controlPoints[1], controlPoints[2]);          
            // float3 delta21 = GetDelta(controlPoints[2], controlPoints[1]);

            float4 w1 = GenerateWeight(w1Kof[0], w1Kof[1], w1Kof[2], w1Kof[3],
                                        delta20, delta10, backVector, delta12,
                                        delta20, frontVector, backVector, delta20,
                                        w0);

            float4 w2 = GenerateWeight(w2Kof[0], w2Kof[1], w2Kof[2], w2Kof[3],
                                        delta10, delta20, delta12, backVector,
                                        delta20, frontVector, delta12, delta20,
                                        w0);


            //float4 w1t = wTest(w1Kof[0], w1Kof[1], frontVector, delta10, delta20, w0);
            //float4 w2t = wTest(w2Kof[0], w2Kof[1], backVector, delta12, delta20, w1t);
            
           // w0 = new float4(0f, 0f, 0f, 1f); RESET w0
             float4[] weights = { w0, w1, w2 };
           //float4[] weights = { w0, w1t, w2t };
            w0 = w2;
            
            return GenerateCurve(
                curveT, useTengentFix,
                useNormalFix, isRotWithInterpolation,
                controlPoints, weights);
        }
    }
}
