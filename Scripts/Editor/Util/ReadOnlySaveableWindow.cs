using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Medallyon
{
    [CustomEditor(typeof(Saveable))]
    public class ReadOnlySaveableWindow : Editor
    {
        public override void OnInspectorGUI()
        {
            Saveable saveableScript = (Saveable)target;
            EditorGUILayout.LabelField("ID", saveableScript.ID);

            if (GUILayout.Button("Refresh ID"))
                saveableScript.RefreshID();
        }
    }
}
