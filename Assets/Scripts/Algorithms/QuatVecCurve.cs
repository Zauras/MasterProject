using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Master
{
    public class QuatVecCurve : AbstractCurveAlgorithm
    {
        public static FittedMovement CalcQuatVecCurve(
                                            float4[] controlPoints,
                                            float4 frontVector, 
                                            float4 backVector)
        {
            float4 delta40 = GetDelta(controlPoints[2], controlPoints[0]);
            float4 delta20 = GetDelta(controlPoints[1], controlPoints[0]);
            float4 delta24 = GetDelta(controlPoints[1], controlPoints[2]);

            float4 wx = new float4(0f, 0f, 0f, 1f); // IGNOR w0 (experiment) !!!
            float4 w1 = GenerateWeight(-8.0f, -2.0f, 16.0f, -1.0f,
                                        delta40, delta20, backVector, delta24,
                                        delta40, frontVector, backVector, delta40);
            float4 w2 = GenerateWeight(-1.0f, -4.0f, 4.0f, 1.0f,
                                        delta20, delta40, delta24, backVector,
                                        delta20, frontVector, delta24, delta40);

            float4[] weights = { wx, w1, w2 };
            FittedMovement movement = GenerateCurve(controlPoints, weights);

            return movement;
        }
    }
}
