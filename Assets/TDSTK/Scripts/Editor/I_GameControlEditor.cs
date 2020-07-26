using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(GameControl))]
	public class GameControlEditor : TDSEditorInspector {
	
		private static GameControl instance;
		void Awake(){ 
			instance = (GameControl)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "GameControl");
			
			
			EditorGUILayout.Space();
			
			
			EditorGUILayout.LabelField("Level Setting", headerStyle);
			
			cont=new GUIContent("Enable Timer:", "Check if the level has a limited duration.\nWhen enabled with other objective, the player needs to clear other objective bfore the timer runs out in order to 'win' the level.\nOtherwise, the player simply need to survive for the set amount of time to 'win' the level.");
			instance.enableTimer=EditorGUILayout.Toggle(cont, instance.enableTimer);
			
			cont=new GUIContent("Game Duration:", "The duration of the game");
			if(instance.enableTimer) instance.timerDuration=EditorGUILayout.FloatField(cont, instance.timerDuration);
			else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
			
			cont=new GUIContent("Player Life:", "How many time player can respawn in the level");
			instance.playerLife=EditorGUILayout.IntField(cont, instance.playerLife);
			
			cont=new GUIContent("Initial Spawn Point:", "The first/default respawn point of the player.\nif left unassigned, player starting position will be used instead");
			if(instance.playerLife>0){
				instance.startPoint=(Transform)EditorGUILayout.ObjectField(cont, instance.startPoint, typeof(Transform), true);
			}
			else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Objective:", "The objective of this level");
			if(instance.objective!=null)
				instance.objective=(ObjectiveTracker)EditorGUILayout.ObjectField(cont, instance.objective, typeof(ObjectiveTracker), true);
			else{
				EditorGUILayout.BeginHorizontal();
				instance.objective=(ObjectiveTracker)EditorGUILayout.ObjectField(cont, instance.objective, typeof(ObjectiveTracker), true);
				if(GUILayout.Button("Add")){
					GameObject obj=new GameObject();
					obj.name="ObjectiveTracker";
					obj.transform.parent=instance.transform;
					obj.transform.position=Vector3.zero;
					instance.objective=obj.AddComponent<ObjectiveTracker>();
				}
				EditorGUILayout.EndHorizontal();
			}
				
				
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Control Setting", headerStyle);
			
			cont=new GUIContent("Ability:", "Check to enable casting of player ability using middle mouse click. If alt-fire is disabled. ability is cast using right mouse click instead");
			instance.enableAbility=EditorGUILayout.Toggle(cont, instance.enableAbility);
			
			cont=new GUIContent("Alt-Fire:", "Check to enable alt firing (ability) for weapon when using right mouse click. Otherwise right-click would be use for ability (if enable)");
			instance.enableAltFire=EditorGUILayout.Toggle(cont, instance.enableAltFire);
			
			cont=new GUIContent("Continous Fire:", "Check to enable continous firing on weapon when holding down fire button.\n\nRequire weapon that support continous-fire");
			instance.enableContinousFire=EditorGUILayout.Toggle(cont, instance.enableContinousFire);
			
			cont=new GUIContent("Auto Reload:", "Check to enable auto-reload when weapon is out of ammo");
			instance.enableAutoReload=EditorGUILayout.Toggle(cont, instance.enableAutoReload);
			
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("ShootObject Hits:", headerStyle);
			
			cont=new GUIContent("Friendly:", "Check if there's friendly fire");
			instance.friendly=EditorGUILayout.Toggle(cont, instance.friendly);//, GUILayout.MaxWidth(20));
			
			cont=new GUIContent("ShootObject:", "Check if shoot-object can hit and cancel out each other");
			instance.shootObject=EditorGUILayout.Toggle(cont, instance.shootObject);//, GUILayout.MaxWidth(20));
			
			cont=new GUIContent("Collectible:", "Check if shoot-object can hit and destroy collectible");
			instance.collectible=EditorGUILayout.Toggle(cont, instance.collectible);//, GUILayout.MaxWidth(20));
			
			
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Scene Control:", headerStyle);
			
			cont=new GUIContent("MainMenu Name:", "Scene's name of the main menu to be loaded when return to menu on UI is called");
			instance.mainMenu=EditorGUILayout.TextField(cont, instance.mainMenu);
			cont=new GUIContent("NextScene Name:", "Scene's name to be loaded when this level is completed");
			instance.nextScene=EditorGUILayout.TextField(cont, instance.nextScene);
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		/*
		private bool showUnitList=false;
		private bool showSpawnerList=false;
		void DrawObjective(ObjectiveTracker objective){
			if(objective==null) return;
			
			cont=new GUIContent("EnableSurviveTimer:", "Check if the objective is to survive for a set duration");
			objective.enableSurviveTimer=EditorGUILayout.Toggle(cont, objective.enableSurviveTimer);
			
			cont=new GUIContent("Survive Duration:", "The duration in second the player need to survive to complete the objective");
			if(objective.enableSurviveTimer) objective.survivalDuration=EditorGUILayout.FloatField(cont, objective.survivalDuration);
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
			
			
		}
		*/
		
		
	}

}