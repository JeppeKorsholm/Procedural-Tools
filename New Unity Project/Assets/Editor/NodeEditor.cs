using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class NodeEditor : EditorWindow {

    private List<BaseNode> nodes = new List<BaseNode>();
    private Vector2 mousePos;
    private BaseNode selectedNode;
    private bool makeTransitionMode = false;

    [MenuItem("Window/Node Editor")]
    static void ShowEditor()
    {
        NodeEditor editor = EditorWindow.GetWindow<NodeEditor>();
    }
    void OnGUI()
    {
        Event e = Event.current;
        mousePos = e.mousePosition;
        if (e.button == 1 && !makeTransitionMode)
        {
            if (e.type == EventType.MouseDown)
            {
                bool clickedOnWindow = false;
                BaseNode tempSelectedNode = null;

                foreach (BaseNode node in nodes)
                {
                    if (node.windowRect.Contains(mousePos))
                    {
                        tempSelectedNode = node;
                        clickedOnWindow = true;
                        break;
                    }
                }
                if (!clickedOnWindow)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add Noise Node"), false, ContextCallback, "noiseNode");
                    menu.AddItem(new GUIContent("Add Output Node"), false, ContextCallback, "outputNode");
                    menu.ShowAsContext();
                    e.Use();
                }
                else
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, "makeTransition");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");
                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && makeTransitionMode)
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow && !tempSelectedNode.Equals(selectedNode))
            {
                tempSelectedNode.SetInput((BaseInputNode)selectedNode, mousePos);
                makeTransitionMode = false;
                selectedNode = null;
            }
            if (!clickedOnWindow)
            {
                makeTransitionMode = false;
                selectedNode = null;
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && !makeTransitionMode)
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow)
            {
                BaseInputNode nodeToChange = tempSelectedNode.ClickedOnInput(mousePos);
                if(nodeToChange != null)
                {
                    makeTransitionMode = true;
                    selectedNode = nodeToChange;
                }
            }
        }
        if(makeTransitionMode && selectedNode != null)
        {
            Rect mouseRect = new Rect(mousePos.x, mousePos.y, 10, 10);
            DrawNodeCurve(selectedNode.windowRect, mouseRect);
            Repaint();
        }
        foreach(BaseNode n in nodes)
        {
            n.DrawCurves();
        }
        BeginWindows();
        for(int i = 0; i < nodes.Count; i++)
        {
            nodes[i].windowRect = GUI.Window(i, nodes[i].windowRect, DrawNodeWindow, nodes[i].windowTitle);
        }
        EndWindows();
    }

    void DrawNodeWindow(int id)
    {
        nodes[id].DrawWindow();
        GUI.DragWindow();
    }

    void ContextCallback(object obj)
    {
        string clb = obj.ToString();
        if (clb.Equals("noiseNode"))
        {
            NoiseNode noiseNode = new NoiseNode();
            noiseNode.windowRect = new Rect(mousePos, new Vector2(200, 150));
            nodes.Add(noiseNode);
        }
        else if (clb.Equals("outputNode"))
        {
            OutputNode noiseNode = new OutputNode();
            noiseNode.windowRect = new Rect(mousePos, new Vector2(200, 200));
            nodes.Add(noiseNode);
        }
        else if (clb.Equals("makeTransition"))
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow)
            {
                selectedNode = tempSelectedNode;
                makeTransitionMode = true;
            }
        }
        else if (clb.Equals("deleteNode"))
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow)
            {
                nodes.Remove(tempSelectedNode);
                foreach (BaseNode node in nodes)
                {
                    node.NodeDeleted(tempSelectedNode);
                }
            }
        }
    }
    public static void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for(int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        }
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
}
