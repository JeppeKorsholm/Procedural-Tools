using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNode : ScriptableObject {
    public Rect windowRect;
    public Rect outputRect;
    public bool hasInput = false;
    public string windowTitle = "";
    public virtual void DrawWindow()
    {
        windowTitle = EditorGUILayout.TextField("Title", windowTitle);
    }
    public abstract void DrawCurves();
    public virtual void SetInput(BaseInputNode input, Vector2 pos)
    {

    }
    public virtual void NodeDeleted(BaseNode node)
    { }
    public virtual BaseInputNode ClickedOnInput(Vector2 pos)
    {
        return null;
    }
}
