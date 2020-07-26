//component to specify level objective (in edit mode) and keep track of them (during runtime)

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class ObjectiveTracker : MonoBehaviour {

		public string objectiveName="Objective";
		
		public bool waitForTimer=true;		//wait for timer (if enabled) to run out before consider the objective complete
		
		public bool enableScoring=false;
		public float targetScore=0;
		private bool scored=false;
		
		public bool clearAllHostile=true;
		public List<Unit> unitList=new List<Unit>();	//unit that exist in the scene
		
		public List<Unit> prefabList=new List<Unit>();		//unit that yet to be in the scene, waiting for spawner
		public List<int> prefabCountList=new List<int>();		//the count required, editable in editor
		[HideInInspector] public List<int> prefabKillCountList=new List<int>();
		
		
		public bool clearAllSpawner=true;
		public List<UnitSpawner> spawnerList=new List<UnitSpawner>();	//all unit spawner in the scene
		
		
		public List<Trigger> triggerList=new List<Trigger>();	//the objective triggeres that need to be activated
		
		
		//public bool clearAllCol=false;
		public List<Collectible> collectibleList=new List<Collectible>();	//collectible that exist in the scene
		
		public List<Collectible> colPrefabList=new List<Collectible>();	//the collectible that need to be collect
		public List<int> colPrefabCountList=new List<int>();		//the count required, editable in editor
		[HideInInspector] public List<int> colPrefabCollectedCountList=new List<int>();
		
		
		private bool isComplete=false;
		public bool IsComplete(){ return isComplete; }
		
		
		//function call to check all the objective has been completed
		public void CheckObjectiveComplete(){
			bool cleared=true;	
			//objective status set to true by default, it will be set to false if any of the objective condition is not full-filled
			//cleared flag will then be use when calling GameControl.GameOver, indicate if player win/lose the level
			
			//if require to wait for timer but time is not up yet, dont proceed
			if(waitForTimer && !GameControl.TimesUp()) return;
			
			//if scoring criteria not full filled, set cleared to false
			if(enableScoring && !scored) cleared=false;
			
			
			if(colPrefabList.Count>0){
				for(int i=0; i<colPrefabList.Count; i++){
					if(colPrefabCountList[i]>colPrefabCollectedCountList[i]){
						cleared=false;
						break;
					}
				}
			}
			
			//if(clearAllCol){	//if clear all collectible is required and not all collectible is collected, set clear to false
			//	if(GetAllCollectibleCount>0) cleared=false;
			//}
			if(collectibleList.Count>0) cleared=false;		//if not all require collectible required has been collectible
			
			
			//if either of the prefab kill count is not adequate, set cleared to false
			if(prefabList.Count>0){
				for(int i=0; i<prefabList.Count; i++){
					if(prefabCountList[i]>prefabKillCountList[i]){
						cleared=false;
						break;
					}
				}
			}
			
			if(clearAllHostile){	//if clear all hostile is required and not all unit is destroyed, set clear to false
				if(UnitTracker.GetUnitCount()>0) cleared=false;
			}
			else{	//if not all the target unit has been destroyed, set clear to false
				if(unitList.Count>0) cleared=false;
			}
			
			if(clearAllSpawner){	//if clear all spawner is required and not all spawner is cleared/destroyed, set clear to false
				if(UnitSpawnerTracker.GetSpawnerCount()>0) cleared=false;
			}
			else{	//if not all the spawner has been cleared/destroyed, set clear to false
				if(spawnerList.Count>0) cleared=false;
			}
			
			//if time is up, consider game is complete and call GameOver
			if(GameControl.TimesUp()){
				isComplete=true;
				GameControl.GameOver(cleared);
			}
			//else if we dont need to wait for timer and the objective is cleared, call GameOver
			else if(!waitForTimer){
				if(cleared){
					isComplete=true;
					GameControl.GameOver(cleared);
				}
			}
			
		}
		
		
		void Start(){
			//make sure the none of the element in unitList is null
			for(int i=0; i<unitList.Count; i++){
				if(unitList[i]==null){
					unitList.RemoveAt(i);	i-=1;
				}
			}
			
			//make sure the none of the element in spawnerList is null
			for(int i=0; i<spawnerList.Count; i++){
				if(spawnerList[i]==null){
					spawnerList.RemoveAt(i);	i-=1;
				}
			}
			
			//get all unit spawner in current level
			if(clearAllSpawner){
				spawnerList=UnitSpawnerTracker.GetAllSpawnerList();
			}
			
			for(int i=0; i<prefabList.Count; i++) prefabKillCountList.Add(0);
			
			for(int i=0; i<triggerList.Count; i++) triggerList[i].SetTriggerCallback(this.Triggered);
			
			//GameControl.SetObjective(this);
		}
		
		
		//called from GameControl whnenever player score points
		public void GainScore(){
			if(enableScoring && !scored){
				if(GameControl.GetScore()>=targetScore){
					scored=true;
					CheckObjectiveComplete();
				}
			}
		}
		
		
		
		//called from GameControl when a unit is destroyed
		public void UnitDestroyed(Unit unit){
			if(unit==null) return;
			
			//remove the destroyed unit from unitList
			unitList.Remove(unit);
			
			//if the unit's prefab is in prefabList, increase the corresponding kill count
			for(int i=0; i<prefabList.Count; i++){
				if(unit.prefabID==prefabList[i].prefabID){
					prefabKillCountList[i]+=1;
					break;
				}
			}
			
			//if unitList is cleared, check if objective is complete
			if(unitList.Count==0) CheckObjectiveComplete();
		}
		
		
		//called from GameControl when a unit spawner has done spawning (or is destroyed)
		public void SpawnerCleared(UnitSpawner spawner){
			if(spawner==null) return;
			
			//remove the cleared unit spawner from spawnerList
			spawnerList.Remove(spawner);
			
			//if spawnerList is cleared, check if objective is complete
			if(spawnerList.Count==0) CheckObjectiveComplete();
		}
		
		
		//called from trigger objective
		public void Triggered(Trigger trigger){
			if(trigger==null) return;
			
			//remove the trigger from triggerList
			if(triggerList.Contains(trigger)) triggerList.Remove(trigger);
			
			//if trigger list is cleared, check if objective is complete
			if(triggerList.Count==0) CheckObjectiveComplete();
		}
		
		
		//called from collectible OnTriggerEnter
		public void ColletibleCollected(Collectible item){
			//remove the collected item from specific collectible item list
			collectibleList.Remove(item);
			
			//if the item's prefab is in prefabList, increase the corresponding collected count
			for(int i=0; i<colPrefabList.Count; i++){
				if(item.ID==colPrefabList[i].ID){
					colPrefabCollectedCountList[i]+=1;
					break;
				}
			}
		}
	}
	
}
