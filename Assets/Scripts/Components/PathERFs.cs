using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.UI;

namespace Master
{
    public class PathERFs : MonoBehaviour
    {
       // public GameObject[] holders;
        public EulerRodriguesFrame[] ERframes;
        private static Material lineMaterial;

        void Start()
        {
            CreateLineMaterial();
        }

        void OnDrawGizmos()
        {
            float3[] positions = GetComponent<MotionData>().positions;

            try
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    //X axis
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(positions[i], ERframes[i].vecX);
                    if (i != 0) Gizmos.DrawLine(ERframes[i - 1].vecX, ERframes[i].vecX);
                    //Y axis
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(positions[i], ERframes[i].vecY);
                    if (i != 0) Gizmos.DrawLine(ERframes[i - 1].vecY, ERframes[i].vecY);
                    //Z axis
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(positions[i], ERframes[i].vecZ);
                    if (i != 0) Gizmos.DrawLine(ERframes[i - 1].vecZ, ERframes[i].vecZ);
                }
            }
            catch (NullReferenceException) {};
        }

        // Will be called after all regular rendering is done
        public void OnRenderObject()
        {
            float3[] positions = GetComponent<MotionData>().positions;
            // Apply the line material
            

            //GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            //GL.MultMatrix(transform.localToWorldMatrix);

            // Draw lines
            GL.Begin(GL.LINES);
            for (int i = 0; i < positions.Length; i++)
            {
                GL.Color(Color.red);
                GL.Vertex3(positions[i].x, positions[i].y, positions[i].z);
                GL.Vertex3(ERframes[i].vecY.x, ERframes[i].vecY.y, ERframes[i].vecY.z);
                if (i != 0) {
                    GL.Vertex3(ERframes[i].vecY.x, ERframes[i].vecY.y, ERframes[i].vecY.z);
                    GL.Vertex3(ERframes[i-1].vecY.x, ERframes[i-1].vecY.y, ERframes[i-1].vecY.z);
                }
                GL.Color(Color.yellow);
                GL.Vertex3(positions[i].x, positions[i].y, positions[i].z);
                GL.Vertex3(ERframes[i].vecZ.x, ERframes[i].vecZ.y, ERframes[i].vecZ.z);
                if (i != 0)
                {
                    GL.Vertex3(ERframes[i].vecZ.x, ERframes[i].vecZ.y, ERframes[i].vecZ.z);
                    GL.Vertex3(ERframes[i - 1].vecZ.x, ERframes[i - 1].vecZ.y, ERframes[i - 1].vecZ.z);
                }
            }
            GL.End();
            //GL.PopMatrix();
        }

        private static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                // lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                //  lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                //  lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                //  lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.SetPass(0);
            }
        }

    }
}
