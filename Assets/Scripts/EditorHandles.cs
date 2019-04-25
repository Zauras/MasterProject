using System;
using System.Collections;
using Master;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HandlesTarget))]
public class EditorHandles : Editor
{
    private HandlesTarget targetScript;
    private MotionData _motionData;

    private void OnEnable()
    {
        targetScript = (HandlesTarget)target;
        _motionData = targetScript.gameObject.GetComponent<MotionData>();
    }

    private void OnSceneGUI()
    {
        SetNodePoints();
        Handles.color = Color.magenta;
        
        if (_motionData.isClosedSpline)
        {
            for (int i = 0; i < targetScript.nodePoints.Length; i++)
            {
                Handles.DrawLine(
                    targetScript.nodePoints[i],
                    targetScript.nodePoints[(int) Mathf.Repeat(i+1, targetScript.nodePoints.Length)] 
                );
            }
        }
        else
        {
            for (int i = 0; i < targetScript.nodePoints.Length-1; i++)
            {
                Handles.DrawLine(targetScript.nodePoints[i], targetScript.nodePoints[i+1]);
            }
        }
    }

    private void SetNodePoints()
    {
        bool updateChildrenCount = false;
        int count = 0;
        
        foreach (Transform child in targetScript.gameObject.transform)
        {
            if (child.CompareTag("ControlPoint"))
            {
                if (targetScript.nodePoints.Length <= count) if (!updateChildrenCount) updateChildrenCount = true;
                // else if everything is alright
                else targetScript.nodePoints[count] = child.position;
                ++count;
            }
        }
        // Recreate array and recalculate if spline length changed
        if (updateChildrenCount || targetScript.nodePoints.Length > count)
        {
            targetScript.nodePoints = new float3[count];
            SetNodePoints();
        }
    }
}
