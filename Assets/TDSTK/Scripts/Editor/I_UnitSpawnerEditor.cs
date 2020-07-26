using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(UnitSpawner))]
	[CanEditMultipleObjects]
	public class SpawnerEditor : TDSEditorInspector {
	
		private static UnitSpawner instance;
		void Awake(){ 
			instance = (UnitSpawner)target;
			LoadDB();
			InitLabel();
		}
		
		private static string[] spawnTypeLabel;
		private static string[] spawnTypeTooltip;
		
		private static bool initiated=false;
		private static void InitLabel(){
			if(initiated) return;
			initiated=true;
			
			int enumLength = Enum.GetValues(typeof(_SpawnMode)).Length;
			spawnTypeLabel=new string[enumLength];
			spawnTypeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				spawnTypeLabel[n]=((_SpawnMode)n).ToString();
				
				if((_SpawnMode)n==_SpawnMode.WaveBased) spawnTypeTooltip[n]="";
				if((_SpawnMode)n==_SpawnMode.FreeForm) 	spawnTypeTooltip[n]="";
			}
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			serializedObject.Update();
			
			Undo.RecordObject (instance, "UnitSpawner");
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Spawn Upon Start:", "Check to have the spawner start spawning as soon as the game start");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("spawnUponStart"), cont);
			
			cont=new GUIContent("Start Delay:", "Delay (in second) before the spawning start");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("startDelay"), cont);
			
			EditorGUILayout.Space();
			
			cont=new GUIContent("Random Rotation:", "Check to have the unit spawned facing random rotation, otherwise the rotation of the spawn area will be used");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("randomRotation"), cont);
			
			
			EditorGUILayout.Space();
			
			
			if(!serializedObject.isEditingMultipleObjects){
				if(instance.spawnAreaList.Count==0) instance.spawnAreaList.Add(null);
				
				EditorGUILayout.BeginHorizontal();
				
				TDSArea area=instance.spawnAreaList[0];
				cont=new GUIContent("Spawn Area:", "The area where the units will be spawned in. Require a TDSArea component");
				area=(TDSArea)EditorGUILayout.ObjectField(cont, area, typeof(TDSArea), true);
				instance.spawnAreaList[0]=area;
				
				TDSArea existingArea=instance.gameObject.GetComponent<TDSArea>();
				if(instance.spawnAreaList.Count==0 || instance.spawnAreaList[0]==null){
					if(GUILayout.Button("Add")){
						if(instance.spawnAreaList.Count==0){
							if(existingArea!=null) instance.spawnAreaList.Add(existingArea);
							else instance.spawnAreaList.Add(instance.gameObject.AddComponent<TDSArea>());
						}
						else if(instance.spawnAreaList[0]==null){
							if(existingArea!=null) instance.spawnAreaList[0]=existingArea;
							else instance.spawnAreaList[0]=instance.gameObject.AddComponent<TDSArea>();
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			else{
				cont=new GUIContent("Spawn Area:", "The area where the units will be spawned in. Require a TDSArea component");
				EditorGUILayout.LabelField(cont, new GUIContent("Cannot edit multiple instance"));
			}
			
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("Editing of spawner's spawn information using Inspector is not recommended\nUse the editor window instead", MessageType.Info);
			
			if(GUILayout.Button("Open Editor Window")) UnitSpawnerEditorWindow.Init(instance.gameObject);
			
			//~ EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}