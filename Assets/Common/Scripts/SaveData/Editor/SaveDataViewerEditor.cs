using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(SaveDataViewer))]
public class SaveDataViewerEditor : Editor
{
	SaveDataViewer viewer;
	SerializedObject viewerObject;
	string path;

	void OnEnable ()
	{
		path = Application.persistentDataPath + "/bin00.root";
		viewer = target as SaveDataViewer;
		viewerObject = new SerializedObject (viewer);
			
		SaveData.Load (path, ref viewer.info);
	}
	
	public override void OnInspectorGUI ()
	{
		viewerObject.Update ();
		
		EditorGUILayout.PropertyField (viewerObject.FindProperty ("info"), true);
		
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("File Save");
		if (GUILayout.Button ("SAVE")) {
			SaveData.Save (path, viewer.info);
		}
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("File Load");
		if (GUILayout.Button ("LOAD")) {
			SaveData.Load (path, ref viewer.info);
		}
		EditorGUILayout.EndHorizontal ();
		
		viewerObject.ApplyModifiedProperties ();
	}
}
