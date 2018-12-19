using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Transforms;
using System.Threading;
using System;


namespace Master {
    using H = Master.LibQuaternionAritmetics;
    using m = Unity.Mathematics.math;

    public sealed class PHodCurve
    {
        public static List<float3> CalcPHodCurve(Transform pT0, Transform pT1, 
                              quaternion v0, quaternion v1)
        {
            Rotations rotations = GetRFrames(v0, v1);
            pT0.rotation = H.Float4ToQuat(rotations.q0);
            pT1.rotation = H.Float4ToQuat(rotations.q1);

            List<float3> curve = GetPHCUrve(pT0.position, pT1.position, rotations.q0, rotations.q1);
            return curve;
        }

        private struct Rotations
        {
            public Rotations(float4 q0, float4 q1) : this() { this.q0 = q0; this.q1 = q1; }
            public float4 q0, q1;
        }

        private static Rotations GetRFrames(quaternion v0, quaternion v1)
        {
            Debug.Log("===========R-FRAMES begin ===========");
            //two frames of p0 & p1:

            quaternion F01h = m.normalizesafe(v0);
            float4 F01 = H.QuatToFloat4(F01h);
            //quaternion F02h = m.normalizesafe(new quaternion(-F01h.value.y, F01h.value.x, 0f, 0f));
            //quaternion F03h = H.Mult(F01h, F02h);
            //Debug.Log(F01 + " | " + F02 + " | " + F03);

            quaternion F11h = m.normalizesafe(v1);
            float4 F11 = H.QuatToFloat4(F11h);
            //quaternion F12 = m.normalizesafe(new quaternion(-F11.value.y, F11.value.x, 0f, 0f));
            //quaternion F13 = H.Mult(F11, F12);
            //Debug.Log(F11 + " | " + F12 + " | " + F13);

            float4 one = new float4(0f, 0f, 0f, 1f);

            float4 oq1 = one - H.Mult(F01, H.iif);
            float4 oq2 = F01 + H.iif;
            //Debug.Log(oq1 + " | " + oq2);
            float io1 = m.sqrt(H.QuatLength(oq1));
            float io2 = m.sqrt(H.QuatLength(oq2));
            // Debug.Log(io1 + " | " + io2);

            float4 oq3 = one - H.Mult(F11, H.iif);
            float4 oq4 = F11 + H.iif;
            //Debug.Log(oq3 + " | " + oq4);

            float io3 = m.sqrt(H.QuatLength(oq3));
            float io4 = m.sqrt(H.QuatLength(oq4));
            //Debug.Log(io3 + " | " + io4);

            float4 nq1 = oq1 / io1;
            float4 nq2 = oq2 / io2;
            float4 nq3 = oq3 / io3;
            float4 nq4 = oq4 / io4;
            //Debug.Log(nq1 + " | " + nq2);
            //Debug.Log(nq3 + " | " + nq4);

            float t1 = -Mathf.PI / 12f;
            float t2 = -Mathf.PI / 8f;
            float4 q0 = GetQuatOnSphere(nq1, nq2, t1);
            float4 q1 = GetQuatOnSphere(nq3, nq4, t2);
            //Debug.Log(q0 + " | " + q1);
            Debug.Log("===========R-FRAMES ends ===========");
            return new Rotations(q0, q1);
        }

        private static float4 GetQuatOnSphere(float4 nqBeg, float4 nqEnd, float t)
        {
            return m.cos(t) * nqBeg + m.sin(t) * nqEnd;
        }

        private static List<float3> GetPHCUrve(float3 point0, float3 point1, float4 q0, float4 q1)
        {
            //float lmd0 = lmd1 = 1.2f;
            //float4 postumis = new float4(1f, 2f, -2f, 0f);
            float4 p0 = new float4(point0, 0f);
            float4 p1 = new float4(point1, 0f);
            float lmd0 = m.sqrt(H.QuatLength(p0 - p1));
            float lmd1 = lmd0;

            //Debug.Log("Turi buti teigiamas: " + (q0.x * q1.x + q0.y * q1.y + q0.z * q1.z + q0.w * q1.w));

            float4 bb = 3f * (lmd0*q0 + lmd1*q1) / 2f;
            //Debug.Log("bb:= " + bb);

            float4 cc = 1.5f * (m.pow(lmd0, 2f) * H.StarDub(q0)
                      + m.pow(lmd1, 2f) * H.StarDub(q1)
                      + (lmd0 * lmd1 * H.StarOpr(q0, q1)) / 3f
                      - 5f * (p1 - p0)); 
            //Debug.Log("cc:= " + cc);

            // Isdalintas Y skaiciavimas:
            float4 bc = 0.25f * H.StarDub(bb) - cc;
            //Debug.Log("bc:= " + bc);
            var nbc = m.length(bc); // norm of bc
            //Debug.Log("nbc:= " + nbc);
            float4 virsus = H.iif + bc / nbc;
           // Debug.Log("virsus:= " + virsus);
            float4 Y = m.sqrt(nbc) * virsus / m.length(virsus);
            //Debug.Log("Y:= " + Y);

            float phi = -Mathf.PI / 2.0f;
           // Debug.Log("phi:= " + phi);
            //float4 qphi = new float4(m.sin(phi), 0f, 0f, m.cos(phi));
            float4 qphi = H.Round(new float4(m.sin(phi), 0f, 0f, m.cos(phi)));
           //Debug.Log("qphi:= " + qphi);

            float4 A0 = lmd0 * q0;
            float4 A1 = -0.5f * bb + H.Mult(Y, qphi);
            var o = H.Mult(Y, qphi);
           // Debug.Log("A1pat1:= " + (-0.5f * bb));
           // Debug.Log("A1pat2:= " + o);
            float4 A2 = lmd1 * q1;
           // Debug.Log("A0: " + A0);
           // Debug.Log("A1: " + A1);
           // Debug.Log("A2: " + A2);

            // bezier control points:
            float4 cp0 = H.StarDub(A0);
            float4 cp1 = H.StarOpr(A1, A0);
            float4 cp2 = 1f / 3f * (2 * H.StarDub(A1) + H.StarOpr(A2, A0));
            float4 cp3 = H.StarOpr(A1, A2);
            float4 cp4 = H.StarDub(A2);

            // integrated bezier control points:
            float4 icp0 = p0;
            float4 icp1 = icp0 + 0.2f * cp0;
            float4 icp2 = icp1 + 0.2f * cp1;
            float4 icp3 = icp2 + 0.2f * cp2;
            float4 icp4 = icp3 + 0.2f * cp3;
            float4 icp5 = icp4 + 0.2f * cp4;

           // Debug.Log(cp0 + "; " + cp1 + "; " + cp2 + "; " + cp3 + "; " + cp4);
           // Debug.Log(icp0 + "; " + icp1 + "; " + icp2 + "; " + icp3 + "; " + icp4 + "; " + icp5); 
            float4[] bezPoints = { cp0, cp1, cp2, cp3, cp4 };
            float4[] intBezPoints = { icp0, icp1, icp2, icp3, icp4, icp5 };

            // Bezier stuff
            float3[] curvePoints = new float3[50];
            float t = 0f;
            float tStep = 1f / curvePoints.Length;

            List<float3> pathPoints = new List<float3>();
            for (int i = 0; i <= curvePoints.Length; i++)
            {
                var point = H.Bezier(t, bezPoints, intBezPoints);
                pathPoints.Add(new float3(point.x, point.y, point.z));
                t += tStep;
            }
            return pathPoints;
        }

    }
}
