using UnityEngine;
using UnityEditor;

public abstract class SaveDataViewerEditorBase<TViewer, TData> : Editor
    where TViewer : SaveDataViewer<TData>
    where TData : class, new()
{
    protected TViewer viewer;
    protected SerializedObject viewerObject;
    protected abstract string Label { get; }

    protected virtual void OnEnable()
    {
        viewer = target as TViewer;
        viewerObject = new SerializedObject(viewer);
        Load();
    }

    protected abstract void Load();
    protected abstract void Save();

    public override void OnInspectorGUI()
    {
        viewerObject.Update();

        EditorGUILayout.LabelField($"── {Label} ──", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(viewerObject.FindProperty("data"), true);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("File Save");
        if (GUILayout.Button("SAVE")) Save();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("File Load");
        if (GUILayout.Button("LOAD")) { Load(); viewerObject.Update(); }
        EditorGUILayout.EndHorizontal();

        viewerObject.ApplyModifiedProperties();
    }
}