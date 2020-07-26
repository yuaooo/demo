using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{

	[CustomEditor(typeof(CollectibleSpawner))]
	[CanEditMultipleObjects]
	public class CollectibleSpawnerEditor : TDSEditorInspector {
	
		private static CollectibleSpawner instance;
		void Awake(){ 
			instance = (CollectibleSpawner)target;
			LoadDB();
		}
		
		private CollectibleDropManager dropManager;
		void OnEnable(){
			dropManager=instance.gameObject.GetComponent<CollectibleDropManager>();
		}
		
		public override void OnInspectorGUI(){
			if(dropManager!=null) return;
			
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "CollectibleSpawner");
			
			EditorGUILayout.Space();
			
			serializedObject.Update();
			DrawBasicConfigure();
			serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("Editing of spawn item using Inspector is not recommended\nUse the editor window instead", MessageType.Info);
			
			if(GUILayout.Button("Open Editor Window")) CollectibleSpawnerEditorWindow.Init(instance.gameObject);
			
			//EditorGUILayout.Space();
			
			//DrawConfigure();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		
		void DrawBasicConfigure(){
			cont=new GUIContent("Spawn Upon Start:", "Check to have the spawner start spawning as soon as the game start");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("spawnUponStart"), cont);
			
			cont=new GUIContent("Start Delay:", "Delay (in second) before the spawning start");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("startDelay"), cont);
			
			
			EditorGUILayout.Space();
			
			if(!serializedObject.isEditingMultipleObjects){
				EditorGUILayout.BeginHorizontal();
				
				cont=new GUIContent("Spawn Area:", "The area where the units will be spawned in. Require a TDSArea component");
				instance.spawnArea=(TDSArea)EditorGUILayout.ObjectField(cont, instance.spawnArea, typeof(TDSArea), true);
				
				if(instance.spawnArea==null){
					if(GUILayout.Button("Add")){
						TDSArea existingArea=instance.gameObject.GetComponent<TDSArea>();
						if(existingArea!=null) instance.spawnArea=existingArea;
						else instance.spawnArea=instance.gameObject.AddComponent<TDSArea>();
					}
				}
				
				EditorGUILayout.EndHorizontal();
			}
			else{
				cont=new GUIContent("Spawn Area:", "The area where the units will be spawned in. Require a TDSArea component");
				EditorGUILayout.LabelField(cont, new GUIContent("Cannot edit multiple instance"));
			}
			
			
			EditorGUILayout.Space();
			
			cont=new GUIContent("Spawn Cooldown:", "The cooldown between each spawn attempt");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("spawnCD"), cont);
			
			cont=new GUIContent("Max Item Count:", "The maximum amount of active item in the game allowed by this spawner at any given item");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("maxItemCount"), cont);
			
			cont=new GUIContent("Spawn Chance:", "The chance to successfully spawn an item during each cooldown cycle. Takes value from 0-1 with 0.3 being 30% to successfully spawn an item");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("spawnChance"), cont);
			
			cont=new GUIContent("Fail Modifier:", "A modifier to the spawn chance should a spawn attempt fail (to prevent the attempt fail too many time in a row). ie if modifier is set as 0.1, each fail attempt will increase the spawn chance by 10%");
			EditorGUILayout.PropertyField(serializedObject.FindProperty ("failModifier"), cont);
			
			//ConfigureSpawnItemList();
		}
		
		
		
		
		
		//not in used
		void ConfigureSpawnItemList(){
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(new GUIContent("Spawn Item List:", ""), GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
				if(GUILayout.Button("+", GUILayout.Width(40), GUILayout.MaxHeight(14))){ 
					instance.spawnItemList.Add(new CollectibleSpawnInfo());
				}
				if(GUILayout.Button("-", GUILayout.Width(40), GUILayout.MaxHeight(14))){ 
					instance.spawnItemList.RemoveAt(instance.spawnItemList.Count-1);
				}
			EditorGUILayout.EndHorizontal();
			
			//~ EditorGUILayout.BeginHorizontal();
				//~ SpaceH(20);
				//~ EditorGUILayout.LabelField("Item", GUILayout.Width(130));
				//~ EditorGUILayout.LabelField("chance", GUILayout.Width(50));
				//~ EditorGUILayout.LabelField("cd:", GUILayout.Width(30));
			//~ EditorGUILayout.EndHorizontal();
				
			for(int i=0; i<instance.spawnItemList.Count; i++){
				CollectibleSpawnInfo spInfo=instance.spawnItemList[i];
				
				EditorGUILayout.BeginHorizontal();
					//~ EditorGUILayout.LabelField("  -", GUILayout.MaxWidth(25));
					
					SpaceH(10);
					
					int itemIdx=spInfo.item!=null ? TDSEditor.GetCollectibleIndex(spInfo.item.ID) : 0 ;
					EditorGUILayout.LabelField("-", GUILayout.MaxWidth(10));
					itemIdx=EditorGUILayout.Popup(itemIdx, collectibleLabel, GUILayout.Width(130)); 
					if(itemIdx==0) spInfo.item=null;
					else if(itemIdx>0) spInfo.item=collectibleDB.collectibleList[itemIdx-1];
				
					spInfo.chance=EditorGUILayout.FloatField(spInfo.chance, GUILayout.Width(30));
				//SpaceH(10);
					spInfo.cooldown=EditorGUILayout.FloatField(spInfo.cooldown, GUILayout.Width(30));
					//~ spInfo.currentCD=EditorGUILayout.FloatField(spInfo.currentCD, GUILayout.Width(30));
				
				EditorGUILayout.EndHorizontal();
			}
		}
		
		
	}

}