using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowEditorNodeSaver : ScriptableObject {

    public List<BaseNode> nodes;
    public int assetAmount;

    void OnEnable()
    {
        //nodes = new List<BaseNode>();
    }

}
