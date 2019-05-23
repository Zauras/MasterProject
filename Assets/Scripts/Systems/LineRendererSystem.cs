using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;

namespace Master
{
    [UpdateBefore(typeof(MasterEngineSystem))]
    public class LineRendererSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            //UpdateInjectedComponentGroups();

           // Debug.Log(_pathChunks.lrs[0]);
           Entities.ForEach((MotionData motion, LineRenderer lineRenderer) =>
           {
               SetupLineRenderer(lineRenderer,
                   BootStrap.Settings.lineWide,
                   BootStrap.Settings.lineColor,
                   BootStrap.Settings.lineRendererMaterial);
           });

           Entities.ForEach((VectorPointMarker vectorMarker, LineRenderer lineRenderer) =>
           {
               SetupLineRenderer(lineRenderer,
                   BootStrap.Settings.lineWide / 2f,
                   Color.red, 
                   BootStrap.Settings.lineRendererMaterial);
           });
           
           Entities.ForEach((RphMotionMarker marker, Transform parentTransform) =>
           {
               LineRenderer movementPhLr = parentTransform.GetChild(0).GetComponent<LineRenderer>();
               LineRenderer rotationPhLr = parentTransform.GetChild(1).GetComponent<LineRenderer>();;
               
               SetupLineRenderer(movementPhLr,
                   BootStrap.Settings.lineWide,
                   BootStrap.Settings.lineColor,
                   BootStrap.Settings.lineRendererMaterial);
               
               SetupLineRenderer(rotationPhLr,
                   BootStrap.Settings.lineWide,
                   BootStrap.Settings.lineColor,
                   BootStrap.Settings.lineRendererMaterial);
           });

            /*
            for (int i = 0; i < _pathChunks.Length; i++)
            {
                SetupLineRenderer(_pathChunks.lrs[i],
                BootStrap.Settings.lineWide,
                BootStrap.Settings.lineColor,
                BootStrap.Settings.lineRendererMaterial);
            }

            for (int i = 0; i < _vectorChunks.Length; i++)
            {
                SetupLineRenderer(_vectorChunks.lrs[i],
                BootStrap.Settings.vectorLineWide,
                BootStrap.Settings.vectorLineColor,
                BootStrap.Settings.lineRendererMaterial);
            }
            */
        }

        private static void SetupLineRenderer(LineRenderer lineRenderer, float widness, Color color, Material material)
        {
            lineRenderer.material = new Material(material);
            // A simple 2 color gradient with a fixed alpha of 1.0f.
            float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            lineRenderer.colorGradient = gradient;

            lineRenderer.widthMultiplier = widness;
            //lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        }

        public static void SetPolygonPoints(LineRenderer lineRenderer, List<float3> pointList)
        {
            lineRenderer.positionCount = pointList.Count;
            for (int i = 0; i < pointList.Count; i++)
            {
                lineRenderer.SetPosition(i, pointList[i]);
            }
        }

        public static void SetPolygonPoints(LineRenderer lineRenderer, float3[] pointList)
        {
            lineRenderer.positionCount = pointList.Length;
            for (int i = 0; i < pointList.Length; i++)
            {
                lineRenderer.SetPosition(i, pointList[i]);
            }
        }

        public static void SetLinePoints(LineRenderer lineRenderer, float3 pBeg, float3 pEnd)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, pBeg);
            lineRenderer.SetPosition(1, pEnd);
        }

        protected override void OnUpdate() { }
    }
}
