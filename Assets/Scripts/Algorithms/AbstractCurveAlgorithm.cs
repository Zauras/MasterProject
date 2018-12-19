using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Master
{
    using H = Master.LibQuaternionAritmetics;
    public class AbstractCurveAlgorithm
    {
        public static float4 GetDelta(float4 iPoint, float4 jPoint)
        {
            return iPoint - jPoint;
        }

        public static float4 GenerateWeight(float kof11, float kof12, float kof21, float kof22,
                                 float4 delta1, float4 delta2, float4 delta3, float4 delta4,
                                  float4 delta5, float4 delta6, float4 delta7, float4 delta8)
        {
            float4 invDelta1 = H.Invers(delta1);
            float4 invDelta3 = H.Invers(delta3);
            float4 invDelta5 = H.Invers(delta5);
            float4 invDelta7 = H.Invers(delta7);

            float4 firstPart = kof11 * H.Mult(invDelta1, delta2) + kof12 * H.Mult(invDelta3, delta4);
            float4 secPart = kof21 * H.Mult(invDelta5, delta6) + kof22 * H.Mult(invDelta7, delta8);

            float4 weight = H.Mult(H.Invers(firstPart), secPart);
            //weight = H.Mult(weight, weights[0]);
            return weight;
        }

        public static List<float> getFiList(float t)
        { // kreives funkcija
          // fi(t) = MultLoop(k!=i): t - t[k]
            float[] TList = BootStrap.Settings.TList;
            List<float> fList = new List<float>();
            for (int i = TList.Length - 2; i >= 0; i--)
            {
                for (int j = TList.Length - 1; j >= 0; j--)
                {
                    if (i < j)
                    {
                        fList.Add((t - TList[i]) * (t - TList[j]));
                        //print("i: "+i+" ; j:"+j);
                    }
                }
            }
            return fList;
        }

        public static FittedMovement GenerateCurve(float4[] points, float4[] weights)
        {
            float[] timePath = PathTimeList.timePath;

            //DualQuaternion[] cDHarr = new DualQuaternion[timePath.Length];
            float3[] positions = new float3[timePath.Length];
            float4[] rotations = new float4[timePath.Length];

            // qt-pwf(t); pt-wf(t);
            // Visis C(t) bus Img(H)
            float3[] curve = new float3[timePath.Length];
            float4 Ft, Wt;
            for (int t = 0; t < timePath.Length; t++)
            {
                Ft = new float4(0, 0, 0, 0); // q(t)
                Wt = new float4(0, 0, 0, 0); // p(t)

                List<float> fiList = getFiList(timePath[t]);

                for (int i = 0; i < BootStrap.Settings.TList.Length; i++)
                {
                    //t yra rezoliucijos delta step
                    if (points.Length == 5)
                        Ft += H.Mult(points[i * 2], weights[i]) * fiList[i];
                    else Ft += H.Mult(points[i], weights[i]) * fiList[i];
                    Wt += weights[i] * fiList[i];
                }
                // Debug.Log(Wt +" ... "+ Ft);
                //cDHarr[t] = new DualQuaternion(Wt, Ft); // Movement & Rotation Quaternions
                float4 displacementPosition = H.Mult(Ft, H.Invers(Wt));
                positions[t] = H.Float4ToFloat3(H.Mult(Ft, H.Invers(Wt))); // Movement
                rotations[t] = Ft; // Rotation

                // float4 curvePoint= H.Mult(Ft, H.Invers(Wt)); // making path of movement
                // curve[t] = new float3(curvePoint.x, curvePoint.y, curvePoint.z);
            }
            return new FittedMovement(positions, rotations);
        }

    }
}
