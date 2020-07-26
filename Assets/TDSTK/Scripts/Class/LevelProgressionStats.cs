using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	//unlocking perk at specific level
	[System.Serializable]
	public class PerkUnlockingAtLevel{
		public int level=1;
		public List<int> perkIDList=new List<int>();
		
		public PerkUnlockingAtLevel(int lvl=1){ level=lvl; }
		
		public PerkUnlockingAtLevel Clone(){
			PerkUnlockingAtLevel newItem=new PerkUnlockingAtLevel(level);
			newItem.perkIDList=new List<int>( perkIDList );
			return newItem;
		}
	}
	
	
	[System.Serializable]
	public class LevelProgressionStats{
		public int levelCap=50;
		
		//follwoing gain are per level
		public float hitPointGain=5;
		public float hitPointRegenGain=0;
		public float energyGain=1;
		public float energyRegenGain=0;
		public float speedMulGain=0;
		public float dmgMulGain=0.1f;
		public float critChanceMulGain=0;
		public float critMultiplierMulGain=0;
		
		public int perkCurrencyGain=1;
		//end per level gain
		
		public bool sumRecursively=true;	//these are for exp generation
		public float expTHM=1.5f;
		public int expTHC=10;
		
		public List<int> expThresholdList=new List<int>();
		
		public List<PerkUnlockingAtLevel> perkUnlockingAtLevelList=new List<PerkUnlockingAtLevel>();
		
		
		public void RearrangePerkUnlockingList(){
			List<PerkUnlockingAtLevel> newList=new List<PerkUnlockingAtLevel>();
			
			while(perkUnlockingAtLevelList.Count>0){
				float lowest=Mathf.Infinity;
				int lowestIdx=0;
				for(int n=0; n<perkUnlockingAtLevelList.Count; n++){
					if(perkUnlockingAtLevelList[n].level<lowest){
						lowest=perkUnlockingAtLevelList[n].level;
						lowestIdx=n;
					}
				}
				newList.Add(perkUnlockingAtLevelList[lowestIdx]);
				perkUnlockingAtLevelList.RemoveAt(lowestIdx);
			}
			perkUnlockingAtLevelList=newList;
			
			//Debug.Log("start");
			//for(int n=0; n<perkUnlockingAtLevelList.Count; n++) Debug.Log(perkUnlockingAtLevelList[n].level);
			//Debug.Log("end");
		}
		
		
		public LevelProgressionStats Clone(){
			LevelProgressionStats newStats=new LevelProgressionStats();
			
			newStats.levelCap=levelCap;
			
			newStats.hitPointGain=hitPointGain;
			newStats.hitPointRegenGain=hitPointRegenGain;
			newStats.energyGain=energyGain;
			newStats.energyRegenGain=energyRegenGain;
			newStats.speedMulGain=speedMulGain;
			newStats.dmgMulGain=dmgMulGain;
			newStats.critChanceMulGain=critChanceMulGain;
			newStats.critMultiplierMulGain=critMultiplierMulGain;
			
			newStats.perkCurrencyGain=perkCurrencyGain;
			
			newStats.expThresholdList=new List<int>( expThresholdList );
			
			for(int i=0; i<perkUnlockingAtLevelList.Count; i++){
				newStats.perkUnlockingAtLevelList.Add(perkUnlockingAtLevelList[i].Clone());
			}
			
			return newStats;
		}
	}
	
}
