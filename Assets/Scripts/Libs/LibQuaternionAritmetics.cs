﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Master
{
    using m = Unity.Mathematics.math;


    public struct float4Matrix
    {
        public float4Matrix(float4 r0, float4 r1, float4 r2, float4 r3) : this() {
            this.r0 = r0; this.r1 = r1; this.r2 = r2; this.r3 = r3;
        }
        public float4 r0, r1, r2, r3;
    }

    public sealed class LibQuaternionAritmetics
    {
        public static LibQuaternionAritmetics H;

        public static float4 one = new float4(0f, 0f, 0f, 1f);

        public static quaternion ii = new quaternion(1f, 0f, 0f, 0f); // x, y, z, w
        public static quaternion jj = new quaternion(0f, 1f, 0f, 0f);
        public static quaternion kk = new quaternion(0f, 0f, 1f, 0f);

        public static float4 iif = new float4(1f, 0f, 0f, 0f); // x, y, z, w
        public static float4 jjf = new float4(0f, 1f, 0f, 0f);
        public static float4 kkf = new float4(0f, 0f, 1f, 0f);

        // like in Maple
        public static float4 iifm = new float4(0f, 1f, 0f, 0f); // w, x, y, z
        public static float4 jjfm = new float4(0f, 0f, 1f, 0f);
        public static float4 kkfm = new float4(0f, 0f, 0f, 1f);

        public static float4Matrix MulPQ(float4 p)
        {
            return new float4Matrix(
                new float4(p.x, -p.y, -p.z, -p.w),
                new float4(p.y, p.x, -p.w, p.z),
                new float4(p.z, p.w, p.x, -p.y),
                new float4(p.w, -p.z, p.y, p.x)
            );
        }


        public static float4Matrix MulQP(float4 p)
        {
            return new float4Matrix(
                new float4(p.x, -p.y, -p.z, -p.w),
                new float4(p.y, p.x, p.w, -p.z),
                new float4(p.z, -p.w, p.x, p.y),
                new float4(p.w, p.z, -p.y, p.x)
            );
        }

        public static float4Matrix SubMatrix(float4Matrix m1, float4Matrix m2)
        {
            return new float4Matrix(
                Round(new float4(m1.r0.x - m2.r0.x, m1.r0.y - m2.r0.y, m1.r0.z - m2.r0.z, m1.r0.w - m2.r0.w)),
                Round(new float4(m1.r1.x - m2.r1.x, m1.r1.y - m2.r1.y, m1.r1.z - m2.r1.z, m1.r1.w - m2.r1.w)),
                Round(new float4(m1.r2.x - m2.r2.x, m1.r2.y - m2.r2.y, m1.r2.z - m2.r2.z, m1.r2.w - m2.r2.w)),
                Round(new float4(m1.r3.x - m2.r3.x, m1.r3.y - m2.r3.y, m1.r3.z - m2.r3.z, m1.r3.w - m2.r3.w))
            );
        }

        public static (float4, float4, float4) Bezier(float t, float4[] cp, float4[] icp, float4[] rotPolym)
        {
            float delta = -1 * (t - 1);
            // rotPolym = [A0, A1, A2]
            float4 rotation = math.pow(delta, 2f) * rotPolym[0] 
                                + 2f * delta * t * rotPolym[1] 
                                + math.pow(t, 2f) * rotPolym[2];

            float bern40 = m.pow(delta, 4f);
            float bern41 = 4f * t * m.pow(delta, 3f);
            float bern42 = 6f * m.pow(t, 2f) * m.pow(delta, 2f);
            float bern43 = 4f * m.pow(t, 3f) * delta;
            float bern44 = m.pow(t, 4f);

            float bern50 = m.pow(delta, 5f);
            float bern51 = 5f * t * m.pow(delta, 4f);
            float bern52 = 10f * m.pow(t, 2f) * m.pow(delta, 3f);
            float bern53 = 10f * m.pow(t, 3f) * m.pow(delta, 2f);
            float bern54 = 5f * m.pow(t, 4f) * delta;
            float bern55 = m.pow(t, 5f);

            // Hodografas?
            float4 pointHod = cp[0] * bern40 +
                            cp[1] * bern41 +
                            cp[2] * bern42 +
                            cp[3] * bern43 +
                            cp[4] * bern42;
            // PH Kreive
            float4 pointPH = icp[0] * bern50 +
                            icp[1] * bern51 +
                            icp[2] * bern52 +
                            icp[3] * bern53 +
                            icp[4] * bern54 +
                            icp[5] * bern55;

            return (pointPH, pointHod, rotation);
        }

        public static float4 Round(float4 h)
        {
            float x, y, z, w;
            float max = 0.00001f, min = -0.00001f;
            if (h.x < max && h.x > min) x = 0f; else x = h.x;
            if (h.y < max && h.y > min) y = 0f; else y = h.y;
            if (h.z < max && h.z > min) z = 0f; else z = h.z;
            if (h.w < max && h.w > min) w = 0f; else w = h.w;
            return new float4(x, y, z, w);

            // return new float4(
            //     (float) Math.Round(h.x, 7),
            //     (float) Math.Round(h.y, 7),
            //    (float) Math.Round(h.z, 7),
            //    (float) Math.Round(h.w, 7));

           // return h;
        }

        public static bool isZero (float4 f)
        {
            if (f.x == 0f && f.y==0f && f.z==0f && f.w==0f) return true;
            return false;
        }

        public static float4 GetDelta(float4 iPoint, float4 jPoint)
        {
            return iPoint - jPoint;
        }

        public static float4 StarOpr(float4 a, float4 b)
        {
            return (Mult(Mult(a, iif), Conj(b)) + Mult(Mult(b, iif), Conj(a))) / 2f;
        }

        public static float4 StarDub(float4 a)
        {
            return StarOpr(a, a);
        }

        /*
        public static quaternion Unit(quaternion a)
        {
            // make it norm of one
            return a;
        }

        public static float4 Unit(float4 a)
        {
            // make it norm of one
            return a;
        }
        */

        public static float4 Mult (float4 a, float4 b) {
			float w = (b.w*a.w - b.x*a.x - b.y*a.y - b.z*a.z);
			float x = (b.w*a.x + b.x*a.w - b.y*a.z + b.z*a.y);
			float y = (b.w*a.y + b.x*a.z + b.y*a.w - b.z*a.x);
			float z = (b.w*a.z - b.x*a.y + b.y*a.x + b.z*a.w);
			return new float4 (x, y, z, w);
		}

        public static float4 Mult(float3 a, float4 b)
        {
            float w = (0f - b.x * a.x - b.y * a.y - b.z * a.z);
            float x = (b.w * a.x - b.y * a.z + b.z * a.y);
            float y = (b.w * a.y + b.x * a.z - b.z * a.x);
            float z = (b.w * a.z - b.x * a.y + b.y * a.x);
            return new float4(x, y, z, w);
        }

        public static float4 Mult(float3 a, float3 b)
        {
            float w = (0f - b.x * a.x - b.y * a.y - b.z * a.z);
            float x = (0f - b.y * a.z + b.z * a.y);
            float y = ( b.x * a.z + 0 - b.z * a.x);
            float z = (0f - b.x * a.y + b.y * a.x);
            return new float4(x, y, z, w);
        }

        public static quaternion Mult(quaternion a, quaternion b)
        {
            float w = (b.value.w * a.value.w - b.value.x * a.value.x - b.value.y * a.value.y - b.value.z * a.value.z);
            float x = (b.value.w * a.value.x + b.value.x * a.value.w - b.value.y * a.value.z + b.value.z * a.value.y);
            float y = (b.value.w * a.value.y + b.value.x * a.value.z + b.value.y * a.value.w - b.value.z * a.value.x);
            float z = (b.value.w * a.value.z - b.value.x * a.value.y + b.value.y * a.value.x + b.value.z * a.value.w);
            return new quaternion(x, y, z, w);
        }

        public static float4 Div (float4 a, float4 b) { // a/b laikom, kad a = w, x, y, z; Daryti w pradzioje ar gale ???
			float daliklis = (b.w*b.w + b.x*b.x + b.y*b.y + b.z*b.z);

			float w = (b.w*a.w + b.x*a.x + b.y*a.y + b.z*a.z) / daliklis;
			float x = (b.w*a.x - b.x*a.w - b.y*a.z + b.z*a.y) / daliklis;
			float y = (b.w*a.y + b.x*a.z - b.y*a.w - b.z*a.x) / daliklis;
			float z = (b.w*a.z - b.x*a.y + b.y*a.x - b.z*a.w) / daliklis;
			return new float4 (x, y, z, w);
		}
        public static quaternion Div(quaternion a, quaternion b)
        { // a/b laikom, kad a = w, x, y, z; Daryti w pradzioje ar gale ???
            float daliklis = (b.value.w * b.value.w + b.value.x * b.value.x + b.value.y * b.value.y + b.value.z * b.value.z);

            float w = (b.value.w * a.value.w + b.value.x * a.value.x + b.value.y * a.value.y + b.value.z * a.value.z) / daliklis;
            float x = (b.value.w * a.value.x - b.value.x * a.value.w - b.value.y * a.value.z + b.value.z * a.value.y) / daliklis;
            float y = (b.value.w * a.value.y + b.value.x * a.value.z - b.value.y * a.value.w - b.value.z * a.value.x) / daliklis;
            float z = (b.value.w * a.value.z - b.value.x * a.value.y + b.value.y * a.value.x - b.value.z * a.value.w) / daliklis;
            return new quaternion(x, y, z, w);
        }

        public static float4 Conj(float4 quaternion) {
			float xNeg = quaternion.x * (-1.0f);
			float yNeg = quaternion.y * (-1.0f);
			float zNeg = quaternion.z * (-1.0f);
			return new float4 (xNeg, yNeg, zNeg, quaternion.w);
		}

        public static quaternion Conj(quaternion quaternion)
        {
            float xNeg = quaternion.value.x * (-1.0f);
            float yNeg = quaternion.value.y * (-1.0f);
            float zNeg = quaternion.value.z * (-1.0f);
            return new quaternion(xNeg, yNeg, zNeg, quaternion.value.w);
        }

        public static quaternion DivQuatFromConst(quaternion q, float c)
        {
            return new quaternion(q.value.x / c, q.value.y / c, q.value.z / c, q.value.w / c);
        }


        public static float4 Abs(float4 quaternion)
        {
			float xx = math.sqrt (quaternion.x * quaternion.x);
			float yy = math.sqrt (quaternion.y * quaternion.y);
			float zz = math.sqrt (quaternion.z * quaternion.z);
			float ww = math.sqrt (quaternion.w * quaternion.w);
			return new float4 (xx, yy, zz, ww);
		}

        public static quaternion Abs(quaternion quaternion)
        {
            float xx = math.sqrt(quaternion.value.x * quaternion.value.x);
            float yy = math.sqrt(quaternion.value.y * quaternion.value.y);
            float zz = math.sqrt(quaternion.value.z * quaternion.value.z);
            float ww = math.sqrt(quaternion.value.w * quaternion.value.w);
            return new quaternion(xx, yy, zz, ww);
        }

        public static float4 Rise_2Deg (Vector4 quat) { // arba kitaip Norm()
			float4 risedQuat = new float4(quat.x*quat.x, quat.y*quat.y, quat.z*quat.z, quat.w*quat.w);
			return risedQuat;
		}

		public static float4 Invers(float4 q) { // Quaternion Invers
			float n = QuatLength(q);
			return new float4(-1.0f*q.x/n, -1.0f*q.y/n, -1.0f*q.z/n, q.w/n);
		}

        public static float3 Invers(float3 q)
        { // Quaternion Invers
            float n = PureQuatLength(q);
            return new float3(-1.0f * q.x / n, -1.0f * q.y / n, -1.0f * q.z / n);
        }

        public static float QuatLength(float4 q)
        { // Quaternion Length
            return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
        }

        public static float PureQuatLength(float3 q)
        { // Quaternion Length
            return q.x * q.x + q.y * q.y + q.z * q.z;
        }

        public static float QuatLength(quaternion q)
        { // Quaternion Length
            return q.value.x * q.value.x + q.value.y * q.value.y + q.value.z * q.value.z + q.value.w * q.value.w;
        }

        public static float4 Norm(float4 quaternion) {
			float4 normalizedQuat = Div(quaternion, Abs(quaternion));
			return normalizedQuat;
		}

        public static quaternion Norm(quaternion quaternion)
        {
            // quaternion normalizedQuat = Div(quaternion, Abs(quaternion));
            return math.normalizesafe(quaternion);
        }

        public static quaternion Rotation(quaternion quaternionT, float angle ) {
			//if (angle > 720) { } Apsauga, kad angle == [0,720];
			Vector4 quat = QuatToFloat4(quaternionT);

			Vector4 normQuat = Norm(quat);
			float magnitude = quat.sqrMagnitude;
			Vector3 unitQuat = new Vector4 (quat.x / magnitude,
											quat.y / magnitude, 
											quat.z / magnitude);
			Vector3 unitQuatRot = unitQuat * Mathf.Sin(angle/2.0f);
			Vector4 rotationQuat = new Vector4 (unitQuatRot.x, unitQuatRot.y, unitQuatRot.z, Mathf.Cos(angle/2.0f)); // x,y,z,w
		
			Vector4 rotetedQuat = Mult(Mult(rotationQuat, quat), Invers(quat));
			return Float4ToQuat(rotetedQuat);;
		}

		public static float4 QuatToFloat4(quaternion q) {
			return new float4(q.value.x, q.value.y, q.value.z, q.value.w);
		}

		public static quaternion Float4ToQuat(float4 vec4) {
			return new quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
		}

        public static float3 Float4ToFloat3(float4 vec4){
            return new float3(vec4.x, vec4.y, vec4.z);
        }

        public static float4 Float3ToFloat4(float3 vec3)
        {
            return new float4(vec3.x, vec3.y, vec3.z, 0f);
        }

        public static float3 QuatToFloat3(quaternion q)
        {
            return new float3(q.value.x, q.value.y, q.value.z);
        }

        public static float4 TransformToQWYZ(float4 vec4)
        {
            return new float4(vec4.w, vec4.x, vec4.y, vec4.z);
        }

        public static bool isEqual(Vector3 v3, float3 f3)
        {
            return (v3.x == f3.x && v3.y == f3.y && v3.z == f3.z);
        }

        public static bool isEqual(Quaternion Q, quaternion q)
        {
            return (Q.x == (int) (q.value.x * 10f) / 10f
                 && Q.y == (int) (q.value.y * 10f) / 10f
                 && Q.z == (int) (q.value.z * 10f) / 10f
                 && Q.w == (int) (q.value.w * 10f) / 10f);
        }

        public static bool isEqual(quaternion q0, quaternion q)
        {
            return ((int)(q0.value.x * 10f) / 10f == (int)(q.value.x * 10f) / 10f
                 && (int)(q0.value.x * 10f) / 10f == (int)(q.value.y * 10f) / 10f
                 && (int)(q0.value.x * 10f) / 10f == (int)(q.value.z * 10f) / 10f
                 && (int)(q0.value.x * 10f) / 10f == (int)(q.value.w * 10f) / 10f);
        }




    }

}
