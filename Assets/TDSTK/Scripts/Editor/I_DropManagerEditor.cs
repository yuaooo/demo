using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{

	[CustomEditor(typeof(CollectibleDropManager))]
	public class CollectibleDropManagerEditor : TDSEditorInspector {
	
		private static CollectibleDropManager instance;
		void Awake(){ 
			instance = (CollectibleDropManager)target;
			LoadDB();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "DropManager");
			
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("Editing of DropManager using Inspector is not recommended\nUse the editor window instead", MessageType.Info);
			
			if(GUILayout.Button("Open Editor Window")) CollectibleSpawnerEditorWindow.Init();
			
			//EditorGUILayout.Space();
			
			//DrawConfigure();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}