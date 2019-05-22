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

        public static float3 GetDelta(float3 iPoint, float3 jPoint)
        {
            return iPoint - jPoint;
        }

        public static float4 GenerateWeight(float kof11, float kof12, float kof21, float kof22,
            float3 delta1, float3 delta2, float3 delta3, float3 delta4,
            float3 delta5, float3 delta6, float3 delta7, float3 delta8,
            float4 weight0)
        {
            float3 invDelta1 = H.Invers(delta1);
            float3 invDelta3 = H.Invers(delta3);
            float3 invDelta5 = H.Invers(delta5);
            float3 invDelta7 = H.Invers(delta7);

            float4 firstPart = kof11 * H.Mult(invDelta1, delta2)
                               + kof12 * H.Mult(invDelta3, delta4);

            float4 secPart = kof21 * H.Mult(invDelta5, delta6)
                             + kof22 * H.Mult(invDelta7, delta8);

            // EXAMING:
            // float4 weight = H.Mult(H.Mult(H.Invers(firstPart), secPart), weight0);
            float4 weight = H.Mult(H.Invers(firstPart), secPart);
            weight = H.Mult(weight, weight0);
            return weight;
        }


        public static float4 wTest(
            float k1, float k2,
            float3 v, float3 d, float3 df,
            float4 w)
        {
            float3 invV = H.Invers(v);
            float3 invDF = H.Invers(df);
            float3 invD = H.Invers(d);

            float4 firstPart = k1 * H.Mult(invD, v);
            float4 secPart = k2 * H.Mult(invV, d);

            float4 weight = firstPart + secPart;
           //float4 weight = H.Mult(H.Invers(secPart), firstPart);
           //float4 weight = k1 * H.Mult(invDF, v);
            weight = H.Mult(weight, w);
            return weight;
        }

        public static List<float> getFiList(float[] TList, float t)
        { // kreives funkcija
          // fi(t) = MultLoop(k!=i): t - t[k]
            //float[] TList = BootStrap.Settings.TList;
            List<float> fList = new List<float>();
            for (int i = TList.Length - 2; i >= 0; i--)
            {
                for (int j = TList.Length - 1; j >= 0; j--)
                {
                    if (i < j)
                    {
                        fList.Add((t - TList[i]) * (t - TList[j]));
                        //Debug.Log("i: "+i+" ; j:"+j);
                    }
                }
            }
            return fList;
        }
        
        private static float[] GetTimeList(float[] TList)
        {
            int rez = BootStrap.Settings.pathResolution;
            float deltaStep = TList[TList.Length - 1] / rez;

            //float rez1 = TList[2] / TList[1] * rez;
            //float rez2 = TList[2] / (TList[2] - TList[1]) * rez;
            
            
            //float deltaStep1 = TList[1] / rez1;
            //float deltaStep2 = (TList[1] - TList[2]) / rez2;

           float t = 0.0f;
           float[] timePath = new float[rez+1];
            
            for (int i = 0; i < rez + 1; i++)
            {
                timePath[i] = t;
                t += deltaStep;
                //if (i < rez1) t += deltaStep1;
                //else t += deltaStep2;
            }
            return timePath;
        }


        private static float[] GetDeltaList(float3[] points)
        {
            float[] deltaList = new float[points.Length];
            deltaList[0] = 0f;
            for (int i = 0; i < points.Length-1; i++)
            {
                deltaList[i + 1] = math.distance(points[i + 1], points[i]);
            }
            return deltaList;
        }
        

        public static (List<float3>, List<quaternion>) GenerateCurve(Transform curveT,
                                                                    bool useTengentFix,
                                                                    bool useNormalFix,
                                                                    bool useRotWithWeight,
                                                                    float3[] points,
                                                                    float4[] weights)
        {
           float[] timePath = PathTimeList.timePath;
           float[] TList = BootStrap.Settings.TList;
          // float[] TList = GetDeltaList(points);
           //float[] timePath = GetTimeList(TList);

            //DualQuaternion[] cDHarr = new DualQuaternion[timePath.Length];
            List<float3> positions = new List<float3>();
            List<quaternion> rotations = new List<quaternion>();

            // qt-pwf(t); pt-wf(t);
            // Visis C(t) bus Img(H)
            float3[] curve = new float3[timePath.Length];
            float4 q, p;

            for (int t = 0; t < timePath.Length; t++)
            {
                q = new float4(0, 0, 0, 0); // Ft(t)
                p = new float4(0, 0, 0, 0); // Wt(t)

                List<float> fiList = getFiList(TList, timePath[t]);

                for (int i = 0; i < BootStrap.Settings.TList.Length; i++)
                {
                    //t yra rezoliucijos delta step
                    if (points.Length == 5) {
                        q += H.Mult(points[i * 2], weights[i]) * fiList[i];
                    } else {
                        q += H.Mult(points[i], weights[i]) * fiList[i];
                    }
                    p += weights[i] * fiList[i];
                }
                float4 x = H.Mult(q, H.Invers(p));
                positions.Add(curveT.TransformPoint(H.Float4ToFloat3(x))); // Movement

                float4 rot = new float4();
                if (!useRotWithWeight) {
                    rot = H.Mult(H.Mult(p, x), H.Invers(p)); // Rotation
                } else {
                    rot = H.Mult(H.Mult(q, x), H.Invers(q)); // Rotation
                }

                quaternion rotH = H.Float4ToQuat(rot);

                if (useTengentFix)
                {
                    rotH *= Quaternion.Euler(0f, 180f, 0f);
                }
                if (useNormalFix)
                {
                    rotH *= Quaternion.Euler(0f, 0, 180f);
                }
                // Geriau su su q arba qxq^-1
                // rotH = new quaternion(0, rotH.value.y, rotH.value.z, rotH.value.w);
                //rotH = new quaternion(rotH.value.x, 0, rotH.value.z, rotH.value.w);s
                //rotH = new quaternion(rotH.value.x, rotH.value.y, 0, rotH.value.w);
                //rotH = new quaternion(rotH.value.x, rotH.value.y, rotH.value.z, 0);  
                rotations.Add(rotH); // Rotation
            }
            return (positions, rotations);
        }

    }
}
