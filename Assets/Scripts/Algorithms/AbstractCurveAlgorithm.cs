﻿using System.Collections;
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

        public static float3 GetDelta(float3 iPoint, float3 jPoint)
        {
            return iPoint - jPoint;
        }

        public static float4 GenerateWeight(float kof11, float kof12, float kof21, float kof22,
                                 float3 delta1, float3 delta2, float3 delta3, float3 delta4,
                                  float3 delta5, float3 delta6, float3 delta7, float3 delta8)
        {
            float3 invDelta1 = H.Invers(delta1);
            float3 invDelta3 = H.Invers(delta3);
            float3 invDelta5 = H.Invers(delta5);
            float3 invDelta7 = H.Invers(delta7);

            float4 firstPart = kof11 * H.Mult(invDelta1, delta2) 
                               + kof12 * H.Mult(invDelta3, delta4);
            float4 secPart = kof21 * H.Mult(invDelta5, delta6)
                               + kof22 * H.Mult(invDelta7, delta8);

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

        public static (List<float3>, List<quaternion>) GenerateCurve(float3[] points, float4[] weights)
        {
            float[] timePath = PathTimeList.timePath;

            //DualQuaternion[] cDHarr = new DualQuaternion[timePath.Length];
            List<float3> positions = new List<float3>();
            List<quaternion> rotations = new List<quaternion>();

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
                    if (points.Length == 5) {
                        Ft += H.Mult(points[i * 2], weights[i]) * fiList[i];
                    } else {
                        Ft += H.Mult(points[i], weights[i]) * fiList[i];
                    }
                    Wt += weights[i] * fiList[i];
                }
                // Debug.Log(Wt +" ... "+ Ft);
                positions.Add( H.Float4ToFloat3(H.Mult(Ft, H.Invers(Wt)))); // Movement
                rotations.Add( H.Float4ToQuat(Ft) ); // Rotation
            }
            return (positions, rotations);
        }

    }
}
