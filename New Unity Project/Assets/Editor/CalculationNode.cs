using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class CalculationNode : BaseInputNode {
    public enum CalcType
    {
        Adition,
        Subtraction,
        Division,
        Multiplication,
        Clamp,
        SingleNumber
    }
    public enum InputType
    {
        Node,
        Number
    }
    [SerializeField]
    CalcType calcType = new CalcType();
    CalcType pastCalcType = new CalcType();
    private float[][] output;

    public Vector2 pastRes;

    string result = "";
    public string[] variableNames = new string[] {"Var 0","Var 1"};
    [SerializeField]
    List<BaseInputNode> inputNodes = new List<BaseInputNode>();
    float[] inputNumbers = new float[] { 0, 0, 0 };
    private Rect[] inputNodeRects;
    public float[] viewArray11;
    public float[] viewArray21;
    public float[] viewArray31;

    bool calculateOutput;
    
    public CalculationNode()
    {
        windowTitle = "Calculation Node";
        inputNodeRects = new Rect[variableNames.Length];
        pastCalcType = calcType;
        for(int i = 0; i < inputNodeRects.Length; i++)
        {
            inputNodes.Add(null);
        }
        
    }
    public float[][] CalculateOutput()
    {
        output = new float[0][];
        if (HasInputs())
        {
            switch (calcType)
            {
                case CalcType.Adition:
                    output = inputNodes[0] + inputNodes[1];
                    break;
                case CalcType.Subtraction:
                    output = inputNodes[0] - inputNodes[1];
                    break;
                case CalcType.Division:
                    output = inputNodes[0] / inputNodes[1];
                    break;
                case CalcType.Multiplication:
                    output = inputNodes[0] * inputNodes[1];
                    break;
            }
            if (output.Length > 0)
            {
                viewArray11 = output[0];
                viewArray21 = output[1];
                viewArray31 = output[2];
            }
            outputIsCalculated = true;
        }
        
        return output;
    }
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        EditorGUI.BeginChangeCheck();
        calcType = (CalcType)EditorGUILayout.EnumPopup("Calculation Type : ", calcType);
        resolution = EditorGUILayout.Vector2Field("Resolution", resolution);


        for (int i = 0; i < variableNames.Length; i++)
        {
            GUILayout.Label(variableNames[i]);
            if (e.type == EventType.Repaint)
            {
                inputNodeRects[i] = GUILayoutUtility.GetLastRect();
            }
        }

        windowRect = new Rect(windowRect.x, windowRect.y, 200, 150);
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            outputRect = new Rect(windowRect.x + windowRect.width / 1.1f, windowRect.y + outputRect.y, 1, 1);

        }
        if (EditorGUI.EndChangeCheck())
        {
            outputIsCalculated = false;
            iHaveBeenRecalculated();
            EditorUtility.SetDirty(this);
        }
        calculateOutput = GUILayout.Button("Calculate Output");
        if(!outputIsCalculated && calculateOutput)
        {
            output =  CalculateOutput();
        }
    }
    public override void DrawCurves()
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodes[i] != null)
            {
                Rect rect = windowRect;
                rect.x += inputNodeRects[i].x;
                rect.y += inputNodeRects[i].y + inputNodeRects[i].height / 2;
                rect.width = 1;
                rect.height = 1;
                NodeEditor.DrawNodeCurve(inputNodes[i].outputRect, rect);
            }
        }
    }
    public override void NodeDeleted(BaseNode node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            
            if (node.Equals(inputNodes[i]))
            {
                inputNodes[i] = null;
                variableNames[i] = "Var " + i;
                outputIsCalculated = false;
                break;
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
            if (inputNodeRects[i].Contains(pos) && inputNodes[i] != null)
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
                retVal = (BaseInputNode)inputNodes[i];
                inputNodes[i] = null;
                variableNames[i] = "Var " + i;
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
                    inputNodes[i] = input;
                    variableNames[i] = input.windowTitle;
                    if (!input.outputNodes.Contains(this))
                        input.outputNodes.Add(this);
                    output = CalculateOutput();
                    if (output.Length > 0)
                    {
                        viewArray11 = output[0];
                        viewArray21 = output[1];
                        viewArray31 = output[2];
                    }
                    outputIsCalculated = false;
                    iHaveBeenRecalculated();
                }
                else { EditorUtility.DisplayDialog("Infinite Loop Error", "An infinite loop would be created by making that connection", "Okay"); }
            }
        }
        AssetDatabase.SaveAssets();
    }
    public override bool HasInputs()
    {
        for (int i = 0; i < inputNodes.Count; i++)
        {
            if (inputNodes[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    public override bool IsInInfiniteLoop(BaseNode node)
    {
        bool isInInfiniteLoop = false;
        if (hasInput)
        {
            foreach (BaseInputNode list in inputNodes)
            {
                if (list != null)
                {
                    if (list.Equals(node))
                    {
                        isInInfiniteLoop = true;
                        Debug.Log("IS IN INFINITE LOOP 1");
                        return isInInfiniteLoop;
                    }
                    else
                    {
                        isInInfiniteLoop = list.IsInInfiniteLoop(node);
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
        if (HasInputs())
        {
            if (!outputIsCalculated)
            {
                output = CalculateOutput();
            }
            return output;
        }
        else
            foreach (BaseInputNode n in inputNodes)
            {
                if (n != null)
                {
                    return n.GiveOutput();
                }
            }
        return null;
    }
    void createArray(out float[][] array, int width, int depth, float val)
    {
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
}
