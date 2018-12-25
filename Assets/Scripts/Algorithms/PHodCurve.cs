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
        public static (List<float3>, List<float4>, List<EulerRodriguesFrame>) FindPHmotion(Transform pT0, Transform pT1, quaternion v0, quaternion v1)
        {
            // dummy data beg
            /*
             v0 = new quaternion(3f, 4f, 1f, 0f);
             v1 = new quaternion(2f, 2f, 0f, 0f);
             var postumis = new float3(1f, 2f, -2f);
             pT0.position = new float3(0f,0f,0f) + postumis;
             pT1.position = new float3(0.6f, 0.8f, 0.5f) + postumis;
             */
            // dummy data end
            Rotations rotations = GetRFrames(v0, v1);
            pT0.rotation = H.Float4ToQuat(rotations.q0);
            pT1.rotation = H.Float4ToQuat(rotations.q1);

            (List<float3>, List<float4>, List<EulerRodriguesFrame>) movementData 
                = PHCalgorithm(pT0.position, pT1.position, rotations.q0, rotations.q1);
            return movementData;
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
            //Debug.Log(io1 + " | " + io2);

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
           // Debug.Log(nq1 + " | " + nq2);
           // Debug.Log(nq3 + " | " + nq4);

            float t1 = Mathf.PI / 12f;
            float t2 = -1f*Mathf.PI / 8f;
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

        private static float4[] FindBezierControlPoints(float4[] rotPolinomial)
        {
            // bezier^4 control points:
            float4[] bezierCPs = new float4[5];
            bezierCPs[0] = H.StarDub(rotPolinomial[0]);
            bezierCPs[1] = H.StarOpr(rotPolinomial[1], rotPolinomial[0]);
            bezierCPs[2] = 1f / 3f * (2f * H.StarDub(rotPolinomial[1]) 
                           + H.StarOpr(rotPolinomial[2], rotPolinomial[0]));
            bezierCPs[3] = H.StarOpr(rotPolinomial[1], rotPolinomial[2]);
            bezierCPs[4] = H.StarDub(rotPolinomial[2]);
            return bezierCPs;
        }

        private static float4[] IntegrateBezierControlPoints(float4 p0, float4[] bezierCPs)
        {
            // integrated bezier^4 control points -> bezier^5
            float4[] intBezierCPs = new float4[6];
            intBezierCPs[0] = p0;
            intBezierCPs[1] = intBezierCPs[0] + 0.2f * bezierCPs[0];
            intBezierCPs[2] = intBezierCPs[1] + 0.2f * bezierCPs[1];
            intBezierCPs[3] = intBezierCPs[2] + 0.2f * bezierCPs[2];
            intBezierCPs[4] = intBezierCPs[3] + 0.2f * bezierCPs[3];
            intBezierCPs[5] = intBezierCPs[4] + 0.2f * bezierCPs[4];
            return intBezierCPs;
        }

        private static float4 FindMidRotationPolynomial(float4 p0, float4 p1, float4 q0, float4 q1, float lmd0, float lmd1 )
        {
            float4 bb = 3f * (lmd0 * q0 + lmd1 * q1) / 2f;
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
            return -0.5f * bb + H.Mult(Y, qphi);
        }

        private static (List<float3>, List<float4>, List<EulerRodriguesFrame>) PHCalgorithm (
                float3 point0, float3 point1, float4 q0, float4 q1)
        {
            //float lmd0 = lmd1 = 1.2f;
            //float4 postumis = new float4(1f, 2f, -2f, 0f);
            float4 p0 = new float4(point0, 0f);
            float4 p1 = new float4(point1, 0f);
            float lmd0 = m.sqrt(H.QuatLength(p0 - p1));
            float lmd1 = lmd0;
            //Debug.Log(lmd0);s

            //Debug.Log("Turi buti teigiamas: " + (q0.x * q1.x + q0.y * q1.y + q0.z * q1.z + q0.w * q1.w));

            float4 A0 = lmd0 * q0;
            float4 A1 = FindMidRotationPolynomial(p0, p1, q0, q1, lmd0, lmd1);
            float4 A2 = lmd1 * q1;

            float4[] rotationPolynomial = new float4[]
            {
                lmd0 * q0,
                FindMidRotationPolynomial(p0, p1, q0, q1, lmd0, lmd1),
                lmd1 * q1
            };
            float4[] bezPoints = FindBezierControlPoints(rotationPolynomial);
            float4[] intBezPoints = IntegrateBezierControlPoints(p0, bezPoints);

            // Calculate PH curve in Bezier form
            int rez = 10;
            float time = 0f;
            float timeStep = 1f / rez;
            ;

            // List<float3> pathHod = new List<float3>();
            List<float3> pathPoints = new List<float3>();
            List<float4> pathRotations = new List<float4>();
            List<EulerRodriguesFrame> ERFs = new List<EulerRodriguesFrame>();

            for (int i = 0; i <= rez; i++)
            {
                float timeDelta = -1f * (time - 1f);

                // float3 hodPoint = FindHodographPoint(time, timeDelta, bezPoints); //hodograph point (hod(t))
                float4 rotation = FindRotation(time, timeDelta, rotationPolynomial); //rotations? aa in maple
                float3 curvePoint = FindCurvePoint(time, timeDelta, intBezPoints); // PH curve points (pp(t))
                EulerRodriguesFrame ERF = FindEulerRodriguesFrame(rotation);

                pathPoints.Add(curvePoint);
                pathRotations.Add(rotation);
                ERFs.Add(ERF);
                time += timeStep;
            }
            return (pathPoints, pathRotations, ERFs);
        }

        private static float4 FindRotation(float time, float timeDelta, float4[] rotPolym)
        {   // rotPolym = [A0, A1, A2]
            return rotPolym[0] * m.pow(timeDelta, 2f)
                   + rotPolym[1] * (2f * timeDelta * time)
                   + rotPolym[2] * m.pow(time, 2f);
        }

        private static float3 FindHodographPoint(float time, float timeDelta, float4[] bezierCPs)
        { 
            return H.Float4ToFloat3(
                        bezierCPs[0] * m.pow(timeDelta, 4f)
                        + bezierCPs[1] * (4f * time * m.pow(timeDelta, 3f))
                        + bezierCPs[2] * (6f * m.pow(time, 2f) * m.pow(timeDelta, 2f))
                        + bezierCPs[3] * (4f * m.pow(time, 3f) * timeDelta)
                        + bezierCPs[4] * m.pow(time, 4f)
                    );
        }

        private static float3 FindCurvePoint(float time, float timeDelta, float4[] integratedBezierCPs)
        {   
            return H.Float4ToFloat3(
                       integratedBezierCPs[0] * m.pow(timeDelta, 5f)
                       + integratedBezierCPs[1] * (5f * time * m.pow(timeDelta, 4f))
                       + integratedBezierCPs[2] * (10f * m.pow(time, 2f) * m.pow(timeDelta, 3f))
                       + integratedBezierCPs[3] * (10f * m.pow(time, 3f) * m.pow(timeDelta, 2f))
                       + integratedBezierCPs[4] * (5f * m.pow(time, 4f) * timeDelta)
                       + integratedBezierCPs[5] * m.pow(time, 5f)
                   );
        }

        private static EulerRodriguesFrame FindEulerRodriguesFrame(float4 rotation)
        {
            float rotLength = H.QuatLength(rotation);

            float4 ERFx = H.Mult(H.Mult(rotation, H.iif), H.Conj(rotation)) / rotLength;
            float4 ERFy = H.Mult(H.Mult(rotation, H.jjf), H.Conj(rotation)) / rotLength;
            float4 ERFz = H.Mult(H.Mult(rotation, H.kkf), H.Conj(rotation)) / rotLength;

            return new EulerRodriguesFrame ( H.Float4ToFloat3(ERFx),
                                             H.Float4ToFloat3(ERFy),
                                             H.Float4ToFloat3(ERFz) );
        }

    }
}
