using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	//tracker that track all hostile unit in game
	public class UnitTracker{
		private static List<Unit> allUnitList=new List<Unit>();
		public static List<Unit> GetAllUnitList(){ return allUnitList; }
		
		public static int GetUnitCount(){ return allUnitList.Count; }
		
		//function call to scan for all active unit in game
		//not in used atm
		public static void ScanForUnit(){
			allUnitList=new List<Unit>();
			Unit[] list=GameObject.FindObjectsOfType(typeof(UnitAI)) as Unit[];
			
			for(int i=0; i<list.Length; i++){
				if(list[i].thisObj.layer==TDS.GetLayerPlayer()) continue;
				allUnitList.Add(list[i]);
			}
		}
		
		//add unit to be tracked (called by every hostile unit upon activation)
		public static void AddUnit(Unit unit){
			if(allUnitList.Contains(unit)) return;
			allUnitList.Add(unit);
		}
		//remove unit from tracker (call by every hostile unit upon deactivation)
		public static void RemoveUnit(Unit unit){
			allUnitList.Remove(unit);
		}
		
		//clear the list
		public static void Clear(){ allUnitList=new List<Unit>(); }
	}
	
	
	public class UnitSpawnerTracker{
		private static List<UnitSpawner> allSpawnerList=new List<UnitSpawner>();
		public static List<UnitSpawner> GetAllSpawnerList(){ return allSpawnerList; }
		
		public static int GetSpawnerCount(){ return allSpawnerList.Count; }
		
		//function call to scan for all unit spawner in game
		//not in used atm
		public static void ScanForSpawner(){
			allSpawnerList=new List<UnitSpawner>();
			UnitSpawner[] list=GameObject.FindObjectsOfType(typeof(UnitSpawner)) as UnitSpawner[];
			
			for(int i=0; i<list.Length; i++) allSpawnerList.Add(list[i]);
		}
		
		//add spawner to be tracked (called by every hostile unit upon activation)
		public static void AddSpawner(UnitSpawner spawner){
			if(allSpawnerList.Contains(spawner)) return;
			allSpawnerList.Add(spawner);
		}
		//remove spawner from tracker (call by every spawner upon cleared/destroyed)
		public static void RemoveSpawner(UnitSpawner spawner){
			allSpawnerList.Remove(spawner);
		}
		
		//clear the list
		public static void Clear(){ allSpawnerList=new List<UnitSpawner>(); }
	}
	
}
