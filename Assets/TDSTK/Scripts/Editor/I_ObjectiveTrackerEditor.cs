using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(ObjectiveTracker))]
	[CanEditMultipleObjects]
	public class ObjectiveTrackerEditor : TDSEditorInspector {
	
		private static ObjectiveTracker instance;
		void Awake(){ 
			instance = (ObjectiveTracker)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "ObjectiveTracker");
			
			EditorGUILayout.Space();
			
			//EditorGUILayout.LabelField("Level Objective:", headerStyle);
			
			DrawObjective(instance);
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		/*
		void OnEnable(){
			int mainObjCount=0;
			ObjectiveTracker[] objectives = FindObjectsOfType(typeof(ObjectiveTracker)) as ObjectiveTracker[];
			foreach (ObjectiveTracker objective in objectives){
				if(objective.mainObjective) mainObjCount+=1;
			}
			
			if(mainObjCount>1) Debug.LogWarning("There are more than one main objectives in the scene", null);
		}
		*/
		
		
		private bool showUnitList=false;
		private bool showSpawnerList=false;
		private bool showTriggerList=false;
		private bool showCollectibleList=false;
		void DrawObjective(ObjectiveTracker objective){
			if(objective==null) return;
			
			
			EditorGUILayout.Space();
			
			
			//cont=new GUIContent("Main Objective:", "Check if this is the main objective of the level. Completing this will end the level");
			//objective.mainObjective=EditorGUILayout.Toggle(cont, objective.mainObjective);
			
			cont=new GUIContent("Wait For Timer:", "Enable to wait for the timer to run out before trigger a level complete state");
			objective.waitForTimer=EditorGUILayout.Toggle(cont, objective.waitForTimer);
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("EnableTargetScore:", "Check if the objective is to gain a certain amount of score");
			objective.enableScoring=EditorGUILayout.Toggle(cont, objective.enableScoring);
			
			cont=new GUIContent("Target Score:", "The target score the player needs to gain to complete the objective");
			if(objective.enableScoring) objective.targetScore=EditorGUILayout.FloatField(cont, objective.targetScore);
			else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Clear All Hostile:", "Check if all active hostile unit in the level has to be destroyed before the objective is completed");
			objective.clearAllHostile=EditorGUILayout.Toggle(cont, objective.clearAllHostile);
			
			if(!objective.clearAllHostile){
				cont=new GUIContent("Unit List:", "Unit in the scene that need to be destroyed to complete the objective");
				showUnitList = EditorGUILayout.Foldout(showUnitList, cont);
				if(showUnitList){
					int count=objective.unitList.Count;
					
					EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(15));
					count=EditorGUILayout.IntField("Count", count);
					EditorGUILayout.EndHorizontal();
					
					while(count>objective.unitList.Count) objective.unitList.Add(null);
					while(count<objective.unitList.Count) objective.unitList.RemoveAt(objective.unitList.Count-1);
					
					for(int i=0; i<objective.unitList.Count; i++){
						EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(20));
						objective.unitList[i]=(Unit)EditorGUILayout.ObjectField("Element "+i, objective.unitList[i], typeof(Unit), true);
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			
			
			EditorGUILayout.Space();
			
			
			//public List<Collectible> collectibleList=new List<Collectible>();	//collectible that exist in the scene
			cont=new GUIContent("Collectible List:", "Collectible item in the scene that need to be collected to complete the objective");
			showCollectibleList = EditorGUILayout.Foldout(showCollectibleList, cont);
			if(showCollectibleList){
				int count=objective.collectibleList.Count;
				
				EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(15));
				count=EditorGUILayout.IntField("Count", count);
				EditorGUILayout.EndHorizontal();
				
				while(count>objective.collectibleList.Count) objective.collectibleList.Add(null);
				while(count<objective.collectibleList.Count) objective.collectibleList.RemoveAt(objective.collectibleList.Count-1);
				
				for(int i=0; i<objective.collectibleList.Count; i++){
					EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(20));
					objective.collectibleList[i]=(Collectible)EditorGUILayout.ObjectField("Element "+i, objective.collectibleList[i], typeof(Collectible), true);
					EditorGUILayout.EndHorizontal();
				}
			}
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Clear All Spawner:", "Check if all active unit spawner in the level has to be destroyed/cleared before the objective is completed");
			objective.clearAllSpawner=EditorGUILayout.Toggle(cont, objective.clearAllSpawner);
			
			if(!objective.clearAllSpawner){
				cont=new GUIContent("Unit Spawner List:", "Unit Spawner in the scene that need to be cleared to complete the objective");
				showSpawnerList = EditorGUILayout.Foldout(showSpawnerList, cont);
				if(showSpawnerList){
					int count=objective.spawnerList.Count;
					
					EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(15));
					count=EditorGUILayout.IntField("Count", count);
					EditorGUILayout.EndHorizontal();
					
					while(count>objective.spawnerList.Count) objective.spawnerList.Add(null);
					while(count<objective.spawnerList.Count) objective.spawnerList.RemoveAt(objective.spawnerList.Count-1);
					
					for(int i=0; i<objective.spawnerList.Count; i++){
						EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(20));
						objective.spawnerList[i]=(UnitSpawner)EditorGUILayout.ObjectField("Element "+i, objective.spawnerList[i], typeof(UnitSpawner), true);
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Objective Trigger List:", "");
				showTriggerList = EditorGUILayout.Foldout(showTriggerList, cont);
				if(showTriggerList){
					int count=objective.triggerList.Count;
					
					EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(15));
					count=EditorGUILayout.IntField("Count", count);
					EditorGUILayout.EndHorizontal();
					
					while(count>objective.triggerList.Count) objective.triggerList.Add(null);
					while(count<objective.triggerList.Count) objective.triggerList.RemoveAt(objective.triggerList.Count-1);
					
					for(int i=0; i<objective.triggerList.Count; i++){
						EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField("", GUILayout.MaxWidth(20));
						objective.triggerList[i]=(Trigger)EditorGUILayout.ObjectField("Element "+i, objective.triggerList[i], typeof(Trigger), true);
						EditorGUILayout.EndHorizontal();
					}
				}
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Objective Targets:", "Specific numbers of any particular unit that needs to be destroyed before the level is cleared");
			EditorGUILayout.LabelField(cont);
			
			if(objective.prefabList.Count!=objective.prefabCountList.Count){
				//if(objective.targetUnitList.Count>objective.targetUnitCountList.Count)
				while(objective.prefabList.Count>objective.prefabCountList.Count) 
					objective.prefabCountList.Add(1);
				while(objective.prefabList.Count<objective.prefabCountList.Count) 
					objective.prefabCountList.RemoveAt(objective.prefabCountList.Count-1);
			}
			
			
			for(int i=0; i<objective.prefabList.Count+1; i++){
				if(i==objective.prefabList.Count && objective.prefabList.Count==unitAIDB.unitList.Count) continue;
				
				int unitIdx=i<objective.prefabList.Count ? TDSEditor.GetUnitAIIndex(objective.prefabList[i].prefabID) : 0 ;
				
				EditorGUILayout.BeginHorizontal();
				
					EditorGUILayout.LabelField("  -  ", GUILayout.MaxWidth(20));//GUILayout.MaxWidth(115));
					unitIdx=EditorGUILayout.Popup(unitIdx, unitAILabel); 
					
					if(i<objective.prefabList.Count){
						objective.prefabCountList[i]=EditorGUILayout.IntField(objective.prefabCountList[i], GUILayout.MaxWidth(40));
						objective.prefabCountList[i]=Mathf.Max(1, objective.prefabCountList[i]);
					}
					else
						EditorGUILayout.LabelField("    ", GUILayout.MaxWidth(40));
				
				EditorGUILayout.EndHorizontal();
				
				if(i<objective.prefabList.Count){
					if(unitIdx==0) objective.prefabList.RemoveAt(i);
					else if(unitIdx>0 && !objective.prefabList.Contains(unitAIDB.unitList[unitIdx-1]))
						objective.prefabList[i]=unitAIDB.unitList[unitIdx-1];
				}
				else{
					if(unitIdx>0 && !objective.prefabList.Contains(unitAIDB.unitList[unitIdx-1]))
						objective.prefabList.Add(unitAIDB.unitList[unitIdx-1]);
				}
			}
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Objective Collectibles:", "Specific numbers of any particular collectible that needs to be collectible before the level is cleared");
			EditorGUILayout.LabelField(cont);
			
			if(objective.colPrefabList.Count!=objective.colPrefabCountList.Count){
				while(objective.colPrefabList.Count>objective.colPrefabCountList.Count) 
					objective.colPrefabCountList.Add(1);
				while(objective.colPrefabList.Count<objective.colPrefabCountList.Count) 
					objective.colPrefabCountList.RemoveAt(objective.colPrefabCountList.Count-1);
			}
			
			
			for(int i=0; i<objective.colPrefabList.Count+1; i++){
				//if(i==objective.colPrefabList.Count && objective.prefabList.Count==unitAIDB.unitList.Count) continue;
				
				int itemIdx=i<objective.colPrefabList.Count ? TDSEditor.GetCollectibleIndex(objective.colPrefabList[i].ID) : 0 ;
				
				EditorGUILayout.BeginHorizontal();
				
					EditorGUILayout.LabelField("  -  ", GUILayout.MaxWidth(20));//GUILayout.MaxWidth(115));
					itemIdx=EditorGUILayout.Popup(itemIdx, collectibleLabel); 
					
					if(i<objective.colPrefabList.Count){
						objective.colPrefabCountList[i]=EditorGUILayout.IntField(objective.colPrefabCountList[i], GUILayout.MaxWidth(40));
						objective.colPrefabCountList[i]=Mathf.Max(1, objective.colPrefabCountList[i]);
					}
					else
						EditorGUILayout.LabelField("    ", GUILayout.MaxWidth(40));
				
				EditorGUILayout.EndHorizontal();
				
				if(i<objective.colPrefabList.Count){
					if(itemIdx==0) objective.colPrefabList.RemoveAt(i);
					else if(itemIdx>0 && !objective.colPrefabList.Contains(collectibleDB.collectibleList[itemIdx-1]))
						objective.colPrefabList[i]=collectibleDB.collectibleList[itemIdx-1];
				}
				else{
					if(itemIdx>0 && !objective.colPrefabList.Contains(collectibleDB.collectibleList[itemIdx-1]))
						objective.colPrefabList.Add(collectibleDB.collectibleList[itemIdx-1]);
				}
			}
			
			//~ public List<Collectible> colPrefabList=new List<Collectible>();	//the collectible that need to be collect
			//~ public List<int> colPrefabCountList=new List<int>();		//the count required, editable in editor
			
		}
		
		
	}

}