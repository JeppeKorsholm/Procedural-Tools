using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
public class ListWrapper
{
    public List<BaseNode> nodes = new List<BaseNode>();
}
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
    //private List<List<BaseInputNode>> inputNodes = new List<List<BaseInputNode>>();
    private List<ListWrapper> inputNodes = new List<ListWrapper>();
    [SerializeField]
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
    
    private float[][] output;

    public float[][] testArray;
    public float[] viewArray1;
    public float[] viewArray2;
    public float[] viewArray3;
    public NoiseNode()
    {
        
        //EditorUtility.SetDirty(this);
        windowTitle = "Noise Node";
        buttonAmount = variableNames.Length + 1;
        inputNodeRects = new Rect[buttonAmount];
        hSliderVal = new float[buttonAmount];
        minVal = new float[buttonAmount];
        maxVal = new float[buttonAmount];
        resolution = new Vector2(256, 256);
        createArray(out output, (int)resolution.x, (int)resolution.y, 2,true);

        createArray(out testArray, 64, 64, 1, false);

        testArray = CombineArrays(testArray, output, true);
        viewArray1 = testArray[0];
        viewArray2 = testArray[1];
        viewArray3 = testArray[2];

        output = testArray;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            //inputNodes.Add(new List<BaseInputNode>());
            inputNodes.Add(new ListWrapper());
            minVal[i] = 0;
            maxVal[i] = 10;
        }
    }
    public Vector2 pastRes;
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        EditorGUI.BeginChangeCheck();
        
        noiseType = (NoiseType)EditorGUILayout.EnumPopup("Noise Type : ", noiseType);
        resolution = EditorGUILayout.Vector2Field("Resolution", resolution);
        if(resolution.x != pastRes.x || resolution.y != pastRes.y)
            createArray(out output, (int)resolution.x, (int)resolution.y, 1, false);
        pastRes = resolution;
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
                windowRect = new Rect(windowRect.x, windowRect.y, 200, 600);
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
                windowRect = new Rect(windowRect.x, windowRect.y, 200, 430);
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
        else windowRect = new Rect(windowRect.x, windowRect.y, 200, 150);
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
                /*foreach (BaseInputNode node in inputNodes[i])
                {
                    Rect rect = windowRect;
                    rect.x += inputNodeRects[i].x;
                    rect.y += inputNodeRects[i].y + inputNodeRects[i].height / 2;
                    rect.width = 1;
                    rect.height = 1;
                    NodeEditor.DrawNodeCurve(node.outputRect, rect);
                }*/

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
    /*public override void NodeDeleted(BaseNode node)
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
    }*/
    /*public override void NodeDeleted(int node)
    {
        if (index > node) index--;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            //foreach (int n in inputNodes[i])
            for (int j = inputNodes[i].Count - 1; j >= 0; j--) // in inputNodes[i])
            {
                int n = inputNodes[i][j];
                if (node.Equals(n))
                {
                    inputNodes[i].Remove(n);
                }
                if(n > node)
                {
                    inputNodes[i][j]--;
                }
            }
        }
        AssetDatabase.SaveAssets();
    }*/
    public override void NodeDeleted(BaseNode node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            foreach (BaseNode n in inputNodes[i].nodes)
            {
                if (node.Equals(n))
                {
                    inputNodes[i].nodes.Remove((BaseInputNode)node);
                    break;
                }
            }
        }
        foreach(BaseNode n in outputNodes)
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
                /*GenericMenu menu = new GenericMenu();
                for(int i = 0; i < inputNodes.Count; i++)
                {
                    menu.AddItem(new GUIContent(""+ i), false, RemoveConnection, i);
                }
                menu.ShowAsContext();
                Debug.Log("calling method");
                retVal = selectedConnection;
                inputNodes.Remove(retVal);*/

                /*
                retVal = inputNodes[i][0];
                inputNodes[i] = new List<BaseInputNode>();*/
                retVal = (BaseInputNode) inputNodes[i].nodes[0];
                inputNodes[i].nodes = new List<BaseNode>();
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
                    if(!input.outputNodes.Contains(this))
                        input.outputNodes.Add(this);
                }
                else { EditorUtility.DisplayDialog("Infinite Loop Error", "An infinite loop would be created by making that connection", "Okay"); }
            }
        }
        AssetDatabase.SaveAssets();
    }
    public List<int> TheMethod(int iteration)
    {
        Debug.Log(windowTitle + " " +iteration);
        if (HasInputs())
        {
            /*foreach (List<BaseInputNode> node in inputNodes)
            {
                foreach (BaseInputNode n in node)
                {
                    return ((NoiseNode)n).TheMethod(iteration + 1);
                }
            }*/
        }
        return new List<int>();
    }
    bool HasInputs()
    {
        foreach(ListWrapper n in inputNodes)
        {
            if(n.nodes.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public override bool IsInInfiniteLoop(BaseNode node)
    {
        bool isInInfiniteLoop = false;
        if (hasInput)
        {
            foreach(ListWrapper list in inputNodes)
            {
                for(int i = 0; i < list.nodes.Count; i++)
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

    public override float[][] GiveOutput()
    {
        viewArray1 = output[0];
        viewArray2 = output[1];
        viewArray3 = output[2];
        return output;
    }
    public override float[] GiveOutput(Vector2 res)
    {
        return base.GiveOutput(res);
    }
    void createArray(out float[][] array, int width, int depth)
    {
        Debug.Log("creating array");
        float[][] temp = new float[width][];
        for(int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
        }
        array = temp;
    }
    void createArray(out float[][] array, int width, int depth, float val)
    {
        Debug.Log("creating array with val " + val);
        float[][] temp = new float[width][];
        for (int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
            for (int y = 0; y < depth; y++)
            {
                temp[x][y] = val;
            }
        }
        array = temp;
    }
    void createArray(out float[][] array, int width, int depth, Vector2 vals)
    {
        Debug.Log("creating array with val " + vals);
        float[][] temp = new float[width][];
        for (int x = 0; x < width-1; x+=2)
        {
            temp[x] = new float[depth];
            temp[x+1] = new float[depth];
            for (int y = 0; y < depth-1; y+=2)
            {
                temp[x][y] = vals.x;
                temp[x+1][y] = vals.y;
                temp[x][y+1] = vals.y;
                temp[x + 1][y+1] = vals.x;
            }
        }
        array = temp;
    }
    void createArray(out float[][] array, int width, int depth, float val, bool consecutiveNumbers)
    {
        Debug.Log("creating array with val " + val);
        float[][] temp = new float[width][];
        for (int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
            for (int y = 0; y < depth; y++)
            {
                if (consecutiveNumbers)
                    temp[x][y] = y + val;
                else
                    temp[x][y] = y + x * val;
            }
        }
        array = temp;
    }
}
