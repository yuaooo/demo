using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(PlayerProgression))]
	[CanEditMultipleObjects]
	public class PlayerProgressionEditor : TDSEditorInspector {
	
		private static PlayerProgression instance;
		void Awake(){
			instance = (PlayerProgression)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			serializedObject.Update();
			
				cont=new GUIContent("Enable Leveling:", "Check to enable leveling system\nOtherwise player will not level up");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("enableLeveling"), cont);
				
				cont=new GUIContent("Use Global Stats:", "Check to use the progression stats from global db.\nWhen unchecked, local progression stats on this component will be used instead\nYou can edit the stats on this component in Progression Stats Editor Window while selecting this game-object.");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("loadStatsFromDB"), cont);
			
			EditorGUILayout.Space();
			
				EditorGUILayout.HelpBox("Editing PlayerProgression's stats using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
				if(GUILayout.Button("Progression Stats Editor Window")){
					ProgressStatsEditorWindow.Init();
				}
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}