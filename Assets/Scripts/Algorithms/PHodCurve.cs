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
        //##### API ##############################################################################

        /// IF: Spline begining || Independent Curve
        public static (List<float3>, List<quaternion>) FindPHCmotion(Transform pT0, Transform pT1,
                                                                      quaternion v0, quaternion v1)
        {
            float4 q0 = FindEndPointRotations(v0, Mathf.PI / 8f);
            float4 q1 = FindEndPointRotations(v1, Mathf.PI / 8f);
            pT0.rotation = H.Float4ToQuat(q0);
            pT1.rotation = H.Float4ToQuat(q1);

            (List<float3>, List<quaternion>) movementData = CalcPHcurveMotion(pT0.position, q0,
                                                                               pT1.position, q1);
            return movementData;
        }

        /// IF: Spline Middleware || OpenSpline Ending
        public static (List<float3>, List<quaternion>) FindPHCmotion(Transform pT0, 
                                                                      Transform pT1, 
                                                                      quaternion v1  )
        {
            float4 q1 = FindEndPointRotations(v1, Mathf.PI / 8f);
            pT1.rotation = H.Float4ToQuat(q1);

            (List<float3>, List<quaternion>) movementData = CalcPHcurveMotion(
                                        pT0.position, H.QuatToFloat4(pT0.rotation), 
                                         pT1.position, q1);
            return movementData;
        }

        /// IF: ClosedSpline Ending
        public static (List<float3>, List<quaternion>) FindPHCmotion(Transform pT0, Transform pT1)
        {
            (List<float3>, List<quaternion>) movementData = CalcPHcurveMotion(
                                        pT0.position, H.QuatToFloat4(pT0.rotation),
                                         pT1.position, H.QuatToFloat4(pT1.rotation));
            return movementData;
        }

        //##### Private Methods ##############################################################################
        private static float4 FindEndPointRotations(quaternion vec, float t)
        {
            float4 F01 = H.QuatToFloat4(m.normalizesafe(vec)); // ERFrame x axis (tangent)

            float4 oq1 = H.one - H.Mult(F01, H.iif);
            float4 oq2 = F01 + H.iif;
            float io1 = m.sqrt(H.QuatLength(oq1));
            float io2 = m.sqrt(H.QuatLength(oq2));

            float4 nq1 = oq1 / io1;
            float4 nq2 = oq2 / io2;

            return GetQuatOnSphere(nq1, nq2, t);
        }

        private static (float4, float4) FindEndPointRotations(quaternion v0, quaternion v1)
        {
            //two frames of p0 & p1:
            float4 F01 = H.QuatToFloat4(m.normalizesafe(v0));
            //quaternion F02h = m.normalizesafe(new quaternion(-F01h.value.y, F01h.value.x, 0f, 0f));
            //quaternion F03h = H.Mult(F01h, F02h);

            float4 F11 = H.QuatToFloat4(m.normalizesafe(v1));
            //quaternion F12 = m.normalizesafe(new quaternion(-F11.value.y, F11.value.x, 0f, 0f));
            //quaternion F13 = H.Mult(F11, F12);

            float4 one = new float4(0f, 0f, 0f, 1f);

            float4 oq1 = one - H.Mult(F01, H.iif);
            float4 oq2 = F01 + H.iif;

            float io1 = m.sqrt(H.QuatLength(oq1));
            float io2 = m.sqrt(H.QuatLength(oq2));

            float4 oq3 = one - H.Mult(F11, H.iif);
            float4 oq4 = F11 + H.iif;

            float io3 = m.sqrt(H.QuatLength(oq3));
            float io4 = m.sqrt(H.QuatLength(oq4));

            float4 nq1 = oq1 / io1;
            float4 nq2 = oq2 / io2;
            float4 nq3 = oq3 / io3;
            float4 nq4 = oq4 / io4;

            float t1 = Mathf.PI / 12f;
            float t2 = -1f*Mathf.PI / 8f;
            float4 q0 = GetQuatOnSphere(nq1, nq2, t1);
            float4 q1 = GetQuatOnSphere(nq3, nq4, t2);

            return (q0, q1);
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
            float4 cc = 1.5f * (m.pow(lmd0, 2f) * H.StarDub(q0)
                      + m.pow(lmd1, 2f) * H.StarDub(q1)
                      + (lmd0 * lmd1 * H.StarOpr(q0, q1)) / 3f
                      - 5f * (p1 - p0));

            // Isdalintas Y skaiciavimas:
            float4 bc = 0.25f * H.StarDub(bb) - cc;
            float nbc = m.length(bc); // norm of bc
            float4 virsus = H.iif + bc / nbc;
            float4 Y = m.sqrt(nbc) * virsus / m.length(virsus);

            float phi = -Mathf.PI / 2.0f;
            float4 qphi = new float4(m.sin(phi), 0f, 0f, m.cos(phi));
            //float4 qphi = H.Round(new float4(m.sin(phi), 0f, 0f, m.cos(phi))); !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return -0.5f * bb + H.Mult(Y, qphi);
        }

        private static (List<float3>, List<quaternion>) CalcPHcurveMotion (
                float3 point0, float4 q0, float3 point1, float4 q1)
        {
            float4 p0 = new float4(point0, 0f);
            float4 p1 = new float4(point1, 0f);
            float lmd0 = m.sqrt(H.QuatLength(p0 - p1));
            //float lmd0 = lmd1 = 1.2f;
            float lmd1 = lmd0;

            //Debug.Log("Turi buti teigiamas: " + (q0.x * q1.x + q0.y * q1.y + q0.z * q1.z + q0.w * q1.w));

            //float4 A0 = lmd0 * q0;
            //float4 A1 = FindMidRotationPolynomial(p0, p1, q0, q1, lmd0, lmd1);
            //float4 A2 = lmd1 * q1;

            float4[] rotationPolynomial = new float4[]
            {
                lmd0 * q0,
                FindMidRotationPolynomial(p0, p1, q0, q1, lmd0, lmd1),
                lmd1 * q1
            };
            float4[] bezPoints = FindBezierControlPoints(rotationPolynomial);
            float4[] intBezPoints = IntegrateBezierControlPoints(p0, bezPoints);

            // Calculate PH curve in Bezier form
            int rez = BootStrap.Settings.pathResolution;
            float time = 0f;
            float timeStep = 1f / rez;

            // List<float3> pathHod = new List<float3>();
            List<float3> pathPoints = new List<float3>();
            List<quaternion> pathRotations = new List<quaternion>();

            for (int i = 0; i <= rez; i++)
            {
                //float timeDelta = time - 1f;
                float timeDelta = 1f - time;

                // float3 hodPoint = FindHodographPoint(time, timeDelta, bezPoints); //hodograph point (hod(t))
                quaternion rotation = FindRotation(time, timeDelta, rotationPolynomial); //rotations (aa in maple, in text A(t))
                float3 curvePoint = FindCurvePoint(time, timeDelta, intBezPoints); // PH curve points (pp(t))

                pathPoints.Add(curvePoint);
                pathRotations.Add(rotation);
                time += timeStep;
            }
            return (pathPoints, pathRotations);
        }

        private static quaternion FindRotation(float time, float timeDelta, float4[] rotPolym)
        {   // rotPolym = [A0, A1, A2]
            return H.Float4ToQuat(rotPolym[0] * m.pow(timeDelta, 2f)
                                  + rotPolym[1] * (2f * timeDelta * time)
                                  + rotPolym[2] * m.pow(time, 2f));
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
    }
}
