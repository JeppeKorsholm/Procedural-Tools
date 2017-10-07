﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaveNode : BaseInputNode {

    public enum WaveType
    {
        Sinusoid,
        Sawtooth,
        Triangle,
        Square
    }
    public WaveType type;

    private List<ListWrapper> inputNodes = new List<ListWrapper>();
    [SerializeField]
    private Rect[] inputNodeRects;
    private float[][] output;
    private Vector2 Phase = new Vector2(0,0);
    private float minScale;
    private float maxScale;

    private float frequency;
    // Use this for initialization

    public WaveNode()
    {
        windowTitle = "WaveNode";
        resolution = new Vector2(1024, 1024);
        inputNodeRects = new Rect[1];
        minScale = -1;
        maxScale = 1;
        frequency = 10;
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        windowRect = new Rect(windowRect.x, windowRect.y, 200, 250);
        EditorGUI.BeginChangeCheck();

        type = (WaveType)EditorGUILayout.EnumPopup("Wave Type : ", type);
        resolution = EditorGUILayout.Vector2Field("Resolution", resolution);
        numberScale = EditorGUILayout.Vector2Field("Number Scale", numberScale);

        GUILayout.BeginHorizontal();
        minScale = (EditorGUILayout.FloatField(minScale, GUILayout.Width(40)));
        EditorGUILayout.MinMaxSlider(ref numberScale.x, ref numberScale.y, minScale, maxScale, GUILayout.Width(100));
        maxScale = (EditorGUILayout.FloatField(maxScale, GUILayout.Width(40)));
        GUILayout.EndHorizontal();
        GUILayout.Label("Frequency");
        frequency = EditorGUILayout.FloatField(frequency);
        Phase = EditorGUILayout.Vector2Field("Phase :", Phase);
        if (EditorGUI.EndChangeCheck())
        {
            outputIsCalculated = false;
            iHaveBeenRecalculated();
            EditorUtility.SetDirty(this);
        }

    }

    public override float[][] GiveOutput()
    {
        if (outputIsCalculated)
        {
            return output;
        }
        else
        {
            switch (type)
            {
                case WaveType.Sinusoid:
                    output = GenerateSinusoid();
                    output = ReScaleArray(new Vector2(-1, 1), numberScale, output);
                    break;
                case WaveType.Sawtooth:
                    
                    break;

                case WaveType.Square:
                    output = GenerateSquare();
                    break;
                case WaveType.Triangle:

                    break;
            }
            outputIsCalculated = true;
            return output;
        }
    }

    public float[][] GenerateSinusoid()
    {
        float[][] temp = new float[(int)resolution.x][];
        for (int x = 0; x < (int)resolution.x; x++)
        {
            temp[x] = new float[(int)resolution.y];
            for (int y = 0; y < (int)resolution.y; y++)
            {
                temp[x][y] = (Mathf.Sin((x/resolution.x + Phase.x)*frequency*Mathf.PI) + Mathf.Cos((y / resolution.y + Phase.y) * frequency* Mathf.PI))/2;
            }
        }
        return temp;
    }
    public float[][] GenerateSquare()
    {
        float[][] temp = new float[(int)resolution.x][];

        float minMaxDiff = numberScale.y - numberScale.x;
        for (int x = 0; x < (int)resolution.x; x++)
        {
            temp[x] = new float[(int)resolution.y];
            for (int y = 0; y < (int)resolution.y; y++)
            {
                temp[x][y] = (((int)(((x / resolution.x + Phase.x) * frequency*2)%2) + (int)(((y / resolution.y + Phase.y) * frequency*2)%2)) / 2)*minMaxDiff + numberScale.x;
            }
        }
        return temp;

    }
    public override void DrawCurves()
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodes.Count > 0)
            {
                foreach (BaseNode node in inputNodes[i].nodes)
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
            foreach (BaseNode n in inputNodes[i].nodes)
            {
                if (node.Equals(n))
                {
                    inputNodes[i].nodes.Remove((BaseInputNode)node);
                    outputIsCalculated = false;
                    iHaveBeenRecalculated();
                    break;
                }
            }
        }
        foreach (BaseNode n in outputNodes)
        {
            if (node.Equals(n))
            {
                outputNodes.Remove(n);
                break;
            }
        }
        AssetDatabase.SaveAssets();
    }
    public override BaseInputNode ClickedOnInput(Vector2 pos)
    {
        BaseInputNode retVal = null;
        selectedConnection = null;
        pos.x -= windowRect.x;
        pos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(pos) && inputNodes[i].nodes.Count > 0)
            {
                retVal = (BaseInputNode)inputNodes[i].nodes[0];
                inputNodes[i].nodes = new List<BaseNode>();
                outputIsCalculated = false;
                iHaveBeenRecalculated();
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
                //inputNodes[i].Add(input);
                if (!input.IsInInfiniteLoop(this))
                {
                    inputNodes[i].nodes.Add(input);
                    if (!input.outputNodes.Contains(this))
                        input.outputNodes.Add(this);
                    outputIsCalculated = false;
                    iHaveBeenRecalculated();
                }
                else { EditorUtility.DisplayDialog("Infinite Loop Error", "An infinite loop would be created by making that connection", "Okay"); }
            }
        }
        AssetDatabase.SaveAssets();
    }
    public override bool IsInInfiniteLoop(BaseNode node)
    {
        bool isInInfiniteLoop = false;
        if (hasInput)
        {
            foreach (ListWrapper list in inputNodes)
            {
                for (int i = 0; i < list.nodes.Count; i++)
                {
                    if (list.nodes[i].Equals(node))
                    {
                        isInInfiniteLoop = true;
                        Debug.Log("IS IN INFINITE LOOP 1");
                        return isInInfiniteLoop;
                    }
                    else
                    {
                        isInInfiniteLoop = list.nodes[i].IsInInfiniteLoop(node);
                        if (isInInfiniteLoop)
                        {
                            Debug.Log("IS IN INFINITE LOOP 2");
                            return isInInfiniteLoop;
                        }
                    }
                }
            }
            return isInInfiniteLoop;
        }
        else
            return isInInfiniteLoop;
    }
    public override bool HasInputs()
    {
        foreach (ListWrapper n in inputNodes)
        {
            if (n.nodes.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

   
}
