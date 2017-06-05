using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class NoiseNode : BaseInputNode
{
    [SerializeField]
    private NoiseType noiseType;
    public enum NoiseType
    {
        WhiteNoise,
        Perlin,
        DomainWarp
    }
    public ComputeTextureCreator myComputeTextureCreator;
    private string result = "";
    private int buttonAmount;
    [SerializeField]
    private List<List<BaseInputNode>> inputNodes = new List<List<BaseInputNode>>();
    private Rect[] inputNodeRects;
    [SerializeField]
    private float[] hSliderVal;
    [SerializeField]
    private float[] minVal;
    [SerializeField]
    private float[] maxVal;
    private string[] variableNames = new string[] { "strength", "offsetX", "offsetY", "octaves", "lacunarity", "persistence", "weight" };
    [SerializeField]
    public bool extendedOptions;
    [SerializeField]
    public bool advancedOptions;
    public bool dampening;
    public bool things;

    public float Length;
    bool _isLengthDecimal;
    bool _isLengthMinus;
    
    public NoiseNode()
    {
        //EditorUtility.SetDirty(this);
        windowTitle = "Noise Node";
        hasInput = true;
        buttonAmount = variableNames.Length + 1;
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
        EditorGUI.BeginChangeCheck();
        noiseType = (NoiseType)EditorGUILayout.EnumPopup("Noise Type : ", noiseType);
        if (inputNodes.Count > 0)
        {
            //input1Title = inputNode.GetResult();
        }
        advancedOptions = GUILayout.Toggle(advancedOptions, "AdvancedOptions");
        if (advancedOptions)
        {
            extendedOptions = GUILayout.Toggle(extendedOptions, "Extended Options");
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
                windowRect = new Rect(windowRect.x, windowRect.y, 200, 550);
                for (int i = 0; i < variableNames.Length; i++)
                {
                    GUILayout.Label(variableNames[i]);
                    if (e.type == EventType.Repaint)
                    {
                        inputNodeRects[i] = GUILayoutUtility.GetLastRect();
                    }
                    GUILayout.BeginHorizontal();
                    minVal[i] = (EditorGUILayout.FloatField(minVal[i], GUILayout.Width(40)));
                    //GUILayout.Space(100);
                    hSliderVal[i] = GUILayout.HorizontalSlider(hSliderVal[i], minVal[i], maxVal[i], GUILayout.Width(100));
                    maxVal[i] = (EditorGUILayout.FloatField(maxVal[i], GUILayout.Width(40)));
                    //float.TryParse(GUILayout.TextField(maxVal[i].ToString(), GUILayout.Width(40)), out maxVal[i]);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    hSliderVal[i] = (EditorGUILayout.FloatField(hSliderVal[i], GUILayout.Width(100)));
                    GUILayout.EndHorizontal();
                    if (hSliderVal[i] < minVal[i]) hSliderVal[i] = minVal[i];
                    else if (hSliderVal[i] > maxVal[i]) hSliderVal[i] = maxVal[i];
                }
            }
            else
            {
                windowRect = new Rect(windowRect.x, windowRect.y, 200, 400);
                //Debug.Log(variableNames.Length);
                //Debug.Log(inputNodeRects.Length);
                for (int i = 0; i < variableNames.Length; i++)
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
        }
        else windowRect = new Rect(windowRect.x, windowRect.y, 200, 120);
        GUILayout.Label("Result : " + result);
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            inputNodeRects[inputNodeRects.Length - 1] = outputRect;
            outputRect = new Rect(windowRect.x + windowRect.width / 1.1f, windowRect.y + outputRect.y, 1, 1);
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
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
            if (inputNodeRects[i].Contains(pos) && inputNodes[i].Count > 0)
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
    public List<int> TheMethod(int iteration)
    {
        Debug.Log(windowTitle + " " +iteration);
        if (HasInputs())
        {
            foreach (List<BaseInputNode> node in inputNodes)
            {
                foreach (BaseInputNode n in node)
                {
                    return ((NoiseNode)n).TheMethod(iteration + 1);
                }
            }
        }
        return new List<int>();
    }
    bool HasInputs()
    {
        foreach(List<BaseInputNode> n in inputNodes)
        {
            if(n.Count > 0)
            {
                return true;
            }
        }
        return false;
    }
}
