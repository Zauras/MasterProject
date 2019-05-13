using System;
using System.Collections;
using Master;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HandlesTarget), true)]
public class EditorHandles : Editor
{
    private HandlesTarget _targetScript;
    private MotionData _motionData;
    private byte _controlPointCountStep;

    private void OnEnable()
    {
        _targetScript = (HandlesTarget)target;
        _targetScript.nodePoints = new float3[_targetScript.gameObject.transform.childCount];
        _motionData = _targetScript.gameObject.GetComponent<MotionData>();

        if (_targetScript.gameObject.GetComponent<PHmotionMarker>())
            _controlPointCountStep = 2;
        else _controlPointCountStep = 3;

        SceneView.duringSceneGui += EventHandlerOnSceneGUI;
    }

    private void EventHandlerOnSceneGUI(SceneView sceneview)
    {
        //if (Application.isPlaying) return;
        if (!_targetScript) return;

        Transform targetT = _targetScript.transform;
        string filterTag = "ControlPoint";
        
        if (_motionData.isClosedSpline)
        {
            int length = targetT.childCount;
            for (int i = 0; i < length; i++)
                DrawLines(filterTag, 
                    targetT.GetChild(i), 
                    targetT.GetChild((int) Mathf.Repeat(i + 1, length)), 
                    targetT.GetChild((int) Mathf.Repeat(i + _controlPointCountStep, length)));
        }
        else
        {
            int length = targetT.childCount-1;
            
            for (int i = 0; i < targetT.childCount-1; i++)
                DrawLines(filterTag,
                    targetT.GetChild(i),
                    targetT.GetChild(i + 1),
                    (i < targetT.childCount-_controlPointCountStep) 
                        ? targetT.GetChild(i + _controlPointCountStep) : null
                    );
        }
    }
    
    private void DrawLines(string filterTag, Transform from, Transform to, Transform filteredTo)
    {
        if (from.CompareTag(filterTag) && filteredTo)
        {
            Handles.color = Color.magenta;
            Handles.DrawLine(from.position, filteredTo.position);
        }
        Handles.color = Color.yellow;
        Handles.DrawLine(from.position, to.position);
    }

    private void SetNodePoints(string filterTag)
    {
        int actualCount = GetTargetedObjectsCount(_targetScript.transform, filterTag);
        if (_targetScript.nodePoints != null || _targetScript.nodePoints.Length != actualCount)
            _targetScript.nodePoints = new float3[actualCount];

        int count = 0;
            foreach (Transform child in _targetScript.gameObject.transform)
                if (child.CompareTag(filterTag))
                    _targetScript.nodePoints[count++] = child.position;
    }

    private int GetTargetedObjectsCount(Transform transform, string filterTag)
    {
        int count = 0;
        foreach (Transform child in _targetScript.gameObject.transform)
            if (child.CompareTag(filterTag))
                count++;
        return count;
    }
    
}
