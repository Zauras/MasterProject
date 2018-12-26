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
        struct PathChunks
        {
            public readonly int Length;
            public ComponentArray<PathMarker> paths;
            public ComponentArray<LineRenderer> lrs;
        }
        [Inject] private PathChunks _pathChunks;

        struct VectorChunks
        {
            public readonly int Length;
            public ComponentArray<VectorPointMarker> vectors;
            public ComponentArray<LineRenderer> lrs;
        }
        [Inject] private VectorChunks _vectorChunks;

        protected override void OnStartRunning()
        {
            UpdateInjectedComponentGroups();
           // Debug.Log(_pathChunks.lrs[0]);
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

        protected override void OnUpdate() { }
    }
}
