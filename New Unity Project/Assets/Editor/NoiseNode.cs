using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NoiseNode : BaseInputNode
{
    private NoiseType noiseType;
    public enum NoiseType
    {
        WhiteNoise,
        Perlin,
        DomainWarp
    }
    public NoiseNode()
    {
        windowTitle = "Noise Node";
        hasInput = true;
    }
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        noiseType = (NoiseType)EditorGUILayout.EnumPopup("Noise Type : ", noiseType);
        GUILayout.Label("Result : ");
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            outputRect = new Rect(windowRect.x + windowRect.width/1.1f, windowRect.y + outputRect.y, 1, 1);
        }
        //outputRect = new Rect(windowRect.x + windowRect.width/1.1f, windowRect.y + windowRect.height/1.1f, 1, 1);
    }
}
