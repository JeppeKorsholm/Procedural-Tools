using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OutputNode : BaseNode {
    private string result = "";
    private int buttonAmount;
    private List<List<BaseInputNode>> inputNodes = new List<List<BaseInputNode>>();
    private Rect[] inputNodeRects;
    private float[] hSliderVal;
    private float[] minVal;
    private float[] maxVal;
    private string[] variableNames = new string[] {"strength", "offsetX", "offsetY", "octaves","lacunarity","persistence","weight"};
    private bool extendedOptions = false;
    private bool dampening;
    List<Rect> things;
    Rect[] things2;

    public float Length;
    bool _isLengthDecimal;
    bool _isLengthMinus;
    public OutputNode()
    {
        buttonAmount = variableNames.Length;
        windowTitle = "Output Node";
        hasInput = true;
        inputNodeRects = new Rect[buttonAmount];
        hSliderVal = new float[buttonAmount];
        minVal = new float[buttonAmount];
        maxVal = new float[buttonAmount];
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            inputNodes.Add(new List<BaseInputNode>());
            minVal[i] = 0;
            maxVal[i] = 10;
        }
    }
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        string input1Title = "None";
        if (inputNodes.Count > 0)
        {
            //input1Title = inputNode.GetResult();
        }
        extendedOptions = GUILayout.Toggle(extendedOptions,"Extended Options");
        dampening = GUILayout.Toggle(dampening, "Dampening");
        /*string lengthText = GUILayout.TextField((_isLengthMinus ? "" : "") + Length.ToString() + (_isLengthDecimal ? "." : ""));
        _isLengthDecimal = lengthText.EndsWith(".");
        _isLengthMinus = lengthText[0].Equals('-');

        float newLength;
        if (float.TryParse(lengthText, out newLength))
        {
            Length = newLength;
        }*/

        if (extendedOptions)
        {
            windowRect = new Rect(windowRect.x, windowRect.y, 200, 500);
            for (int i = 0; i < inputNodeRects.Length; i++)
            {
                GUILayout.Label(variableNames[i]);
                if (e.type == EventType.Repaint)
                {
                    inputNodeRects[i] = GUILayoutUtility.GetLastRect();
                }
                GUILayout.BeginHorizontal();
                float.TryParse(GUILayout.TextField(minVal[i].ToString(), GUILayout.Width(40)), out minVal[i]);
                //GUILayout.Space(100);
                hSliderVal[i] = GUILayout.HorizontalSlider(hSliderVal[i], minVal[i], maxVal[i], GUILayout.Width(100));
                float.TryParse(GUILayout.TextField(maxVal[i].ToString(), GUILayout.Width(40)), out maxVal[i]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(45);
                float.TryParse(GUILayout.TextField(hSliderVal[i].ToString(), GUILayout.Width(100)), out hSliderVal[i]);
                GUILayout.EndHorizontal();
                if (hSliderVal[i] < minVal[i]) hSliderVal[i] = minVal[i];
                else if (hSliderVal[i] > maxVal[i]) hSliderVal[i] = maxVal[i];
            }
        }
        else
        {
            windowRect = new Rect(windowRect.x, windowRect.y, 200, 400);
            for (int i = 0; i < inputNodeRects.Length; i++)
            {
                GUILayout.Label(variableNames[i]);
                if (e.type == EventType.Repaint)
                {
                    inputNodeRects[i] = GUILayoutUtility.GetLastRect();
                }
                GUILayout.BeginHorizontal();
                hSliderVal[i] = GUILayout.HorizontalSlider(hSliderVal[i], minVal[i], maxVal[i], GUILayout.Width(100));
                float.TryParse(GUILayout.TextField(hSliderVal[i].ToString(), GUILayout.Width(50)), out hSliderVal[i]);
                GUILayout.EndHorizontal();
                if (hSliderVal[i] < minVal[i]) hSliderVal[i] = minVal[i];
                else if (hSliderVal[i] > maxVal[i]) hSliderVal[i] = maxVal[i];
            }
        }
        GUILayout.Label("Result : " + result);
        outputRect = GUILayoutUtility.GetLastRect();
    }
    public override void DrawCurves()
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodes.Count > 0)
            {
                foreach (BaseInputNode node in inputNodes[i])
                {
                    Rect rect = windowRect;
                    rect.x += inputNodeRects[i].x;
                    rect.y += inputNodeRects[i].y + inputNodeRects[i].height / 2;
                    rect.width = 1;
                    rect.height = 1;
                    NodeEditor.DrawNodeCurve(node.outputRect, rect);
                }
            }
        }
    }
    public override void NodeDeleted(BaseNode node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            foreach (BaseNode n in inputNodes[i])
            {
                if (node.Equals(n))
                {
                    inputNodes[i].Remove((BaseInputNode)node);
                    break;
                }
            }
        }
    }
    /*public override void NodeDeleted(int node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            /*foreach (int n in inputNodes[i])
            {
                if (node.Equals(n))
                {
                    inputNodes[i].Remove(n);
                    break;
                }
            }
        }
    }*/
    public override BaseInputNode ClickedOnInput(Vector2 pos)
    {
        BaseInputNode retVal = null;
        selectedConnection = null;
        pos.x -= windowRect.x;
        pos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(pos) && inputNodes.Count > 0)
            {
                /*GenericMenu menu = new GenericMenu();
                for(int i = 0; i < inputNodes.Count; i++)
                {
                    menu.AddItem(new GUIContent(""+ i), false, RemoveConnection, i);
                }
                menu.ShowAsContext();
                Debug.Log("calling method");
                retVal = selectedConnection;
                inputNodes.Remove(retVal);*/
                retVal = inputNodes[i][0];
                inputNodes[i] = new List<BaseInputNode>();
            }
        }
        return retVal;
    }
    BaseInputNode selectedConnection = null;
    public void RemoveConnection(object index)
    {
        int obj = (int)index;
        selectedConnection = inputNodes[0][obj];
    }
    public override void SetInput(BaseInputNode input, Vector2 clickPos)
    {
        clickPos.x -= windowRect.x;
        clickPos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(clickPos))
            {
                inputNodes[i].Add(input);
            }
        }
    }
}
