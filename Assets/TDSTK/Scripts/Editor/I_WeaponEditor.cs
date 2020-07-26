using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(Weapon))]
	[CanEditMultipleObjects]
	public class WeaponEditor : TDSEditorInspector {
	
		private static Weapon instance;
		void Awake(){
			instance = (Weapon)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "Weapon");
			
			EditorGUILayout.Space();
			
			
			EditorGUILayout.HelpBox("Editing Weapon component using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
			if(GUILayout.Button("Weapon Editor Window")){
				WeaponEditorWindow.Init();
			}
			
			
			if(TDSEditor.IsPrefab(instance.gameObject)){
				if(!TDSEditor.ExistInDB(instance)){
					EditorGUILayout.Space();
					
					EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible by other editor.", MessageType.Warning);
					GUI.color=new Color(1f, 0.7f, .2f, 1f);
					if(GUILayout.Button("Add Prefab to Database")){
						WeaponEditorWindow.Init();
						WeaponEditorWindow.NewItem(instance);
						WeaponEditorWindow.Init();	//call again to select the instance in editor window
					}
					GUI.color=Color.white;
				}
			}
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}