using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OutputNode : BaseInputNode {
    private string result = "";
    private int buttonAmount;
    public BaseInputNode inputNodes;
    private Rect[] inputNodeRects;
    private float[] minVal;
    private float[] maxVal;
    private string[] variableNames = new string[] {"Output"};
    public float[][] output;
    private bool realTimeCalculations;
    public float Length;
    bool _isLengthDecimal;
    bool _isLengthMinus;
    public bool somethingChanged;
    private bool calculateOutput;
    private bool applyToTerrain;
    private bool realTimeTerrain;
    public OutputNode()
    {
        buttonAmount = variableNames.Length;
        windowTitle = "Output Node";
        realTimeCalculations = false;
        somethingChanged = false;
        calculateOutput = false;
        inputNodeRects = new Rect[buttonAmount];
        minVal = new float[buttonAmount];
        maxVal = new float[buttonAmount];
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            minVal[i] = 0;
            maxVal[i] = 10;
        }
    }
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        EditorGUI.BeginChangeCheck();
        realTimeCalculations = GUILayout.Toggle(realTimeCalculations, "Calculate in real time");
        calculateOutput = GUILayout.Button("Calculate Output");
        realTimeTerrain = GUILayout.Toggle(realTimeTerrain, "Apply automatically");
        applyToTerrain = GUILayout.Button("Apply To Terrain");
        if (somethingChanged)
        {
            if (calculateOutput || realTimeCalculations)
            {
                GiveOutput();
            }
        }
        
        if((applyToTerrain || realTimeTerrain) && !somethingChanged)
        {
            ApplyToTerrain();
        }
        GUILayout.Label("Result : " + result);
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            inputNodeRects[0] = outputRect;
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
    }
    public override void DrawCurves()
    {
        if (inputNodes != null)
        {
            Rect rect = windowRect;
            rect.x += inputNodeRects[0].x;
            rect.y += inputNodeRects[0].y + inputNodeRects[0].height / 2;
            rect.width = 1;
            rect.height = 1;
            NodeEditor.DrawNodeCurve(inputNodes.outputRect, rect);
        }
        
    }
    public override void NodeDeleted(BaseNode node)
    {
        if (node.Equals(inputNodes))
        {
            inputNodes = null;
        }
    }
    public override BaseInputNode ClickedOnInput(Vector2 pos)
    {
        BaseInputNode retVal = null;
        selectedConnection = null;
        pos.x -= windowRect.x;
        pos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(pos))
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
                retVal = inputNodes;
                inputNodes = null;
                somethingChanged = true;
            }
        }
        return retVal;
    }
    BaseInputNode selectedConnection = null;
    public void RemoveConnection(object index)
    {
        int obj = (int)index;
        //selectedConnection = inputNodes[0][obj];
    }
    public override void SetInput(BaseInputNode input, Vector2 clickPos)
    {
        clickPos.x -= windowRect.x;
        clickPos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(clickPos))
            {
                inputNodes = input;
                if (!input.outputNodes.Contains(this))
                    input.outputNodes.Add(this);
                somethingChanged = true;
            }
        }
        
    }
    public override float[][] GiveOutput()
    {
        somethingChanged = false;
        if(inputNodes != null)
        {
            output = inputNodes.GiveOutput();
        }
        if (output != null)
        {
            //EditorComputeTextureCreator.textureCreator.output2 = output[0];
        }
        return output;
    }
    public override void iHaveBeenRecalculated()
    {
        somethingChanged = true;
    }
    public void ApplyToTerrain()
    {
        if (output == null)
            GiveOutput();
        if (output != null)
            EditorComputeTextureCreator.textureCreator.ApplyToTerrain(output);
    }
}
