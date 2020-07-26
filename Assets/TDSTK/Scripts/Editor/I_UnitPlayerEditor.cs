using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(UnitPlayer))]
	[CanEditMultipleObjects]
	public class UnitPlayerEditor : TDSEditorInspector {
	
		private static UnitPlayer instance;
		void Awake(){
			instance = (UnitPlayer)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "Player");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Load Progress:", "Check to enable loading of progress made in leveling and perk unlocking from previous save");
				instance.loadProgress=EditorGUILayout.Toggle(cont, instance.loadProgress);
				cont=new GUIContent("Save Progress:", "Check to enable saving of progress made in leveling and perk unlocking");
				instance.saveProgress=EditorGUILayout.Toggle(cont, instance.saveProgress);
				
				cont=new GUIContent("Save Upon Change:", "Check to enable instant save upon any changes made\nOtherwise saving is only done player hits a save trigger");
				if(instance.saveProgress) instance.saveUponChange=EditorGUILayout.Toggle(cont, instance.saveUponChange);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			EditorGUILayout.Space();
			
				EditorGUILayout.BeginHorizontal();
					bool saveExisted=PlayerPrefs.HasKey("p"+instance.playerID+"_progress") | PlayerPrefs.HasKey("p"+instance.playerID+"_perk");
					EditorGUILayout.LabelField("Save Existed: "+(saveExisted ? "Yes" : "No"));
					if(saveExisted && GUILayout.Button("Delete Save")) instance.DeleteSave();
				EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			
				EditorGUILayout.HelpBox("Editing the rest of UnitPlayer component using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
				if(GUILayout.Button("Unit Editor Window")){
					UnitPlayerEditorWindow.Init();
				}
				
				if(TDSEditor.IsPrefab(instance.gameObject)){
					if(!TDSEditor.ExistInDB(instance)){
						EditorGUILayout.Space();
						
						EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible by other editor.", MessageType.Warning);
						GUI.color=new Color(1f, 0.7f, .2f, 1f);
						if(GUILayout.Button("Add Prefab to Database")){
							UnitPlayerEditorWindow.Init();
							UnitPlayerEditorWindow.NewItem(instance);
							UnitPlayerEditorWindow.Init();	//call again to select the instance in editor window
						}
						GUI.color=Color.white;
					}
				}
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			
			serializedObject.ApplyModifiedProperties();
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}