using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	[RequireComponent (typeof (UnitPlayer))]
	public class PlayerProgression : MonoBehaviour {
		
		[Space(10)]
		public bool enableLeveling=true;
		
		[Space(10)]
		public bool loadProgress=false;
		public bool saveProgress=false;
		public bool resetOnStart=true;
		
		[Space(10)]
		public int level=0;
		public int GetLevel(){ return level; }
		public int GetLevelCap(){ return stats.levelCap; }
		
		public int exp=0;
		public int GetCurrentExp(){ return exp; }
		
		
		[Space(10)]
		public bool loadStatsFromDB=true;
		public LevelProgressionStats stats;
		
		
		
		public int testVariable=0;
		
		
		
		[HideInInspector] public UnitPlayer player;
		public void SetPlayer(UnitPlayer unit){
			player=unit;
			Init();
		}
		
		[HideInInspector] public bool init=false;
		public void Init(){
			if(init) return;
			init=false;
			
			if(!enableLeveling) return;
			
			if(loadStatsFromDB) ProgressionStatsDB.CopyStats(this);
		}
		
		IEnumerator Start(){
			//if(player==null) yield break;
			yield return null;
			
			if(player.loadProgress) Load();
			
			if(resetOnStart) _Reset();
		}
		
		
		//public static void Reset(){ instance._Reset(); }
		public void _Reset(){
			level=0;
			exp=0;
			
			GainExp();
		}
		
		
		
		public void Save(){
			Debug.Log("save _Player_Progress");
			PlayerPrefs.SetInt("p"+player.playerID+"_progress", 1);	//to indicate save exist when loading
			PlayerPrefs.SetInt("p"+player.playerID+"_lvl", level);
			PlayerPrefs.SetInt("p"+player.playerID+"_exp", exp);
		}
		public void Load(){
			if(PlayerPrefs.HasKey("p"+player.playerID+"_progress")){
				level=PlayerPrefs.GetInt("p"+player.playerID+"_lvl", 1);
				exp=PlayerPrefs.GetInt("p"+player.playerID+"_exp", 0);
			}
		}
		public void DeleteSave(int playerID=0){
			PlayerPrefs.DeleteKey("p"+playerID+"_progress");
			PlayerPrefs.DeleteKey("p"+playerID+"_lvl");
			PlayerPrefs.DeleteKey("p"+playerID+"_exp");
		}
		
		
		
		//public static void GainExp(int gain=0){ if(instance!=null) instance._GainExp(gain); }
		public void GainExp(int gain=0){
			if(!enableLeveling) return;
			
			if(stats.expThresholdList.Count<level) return;
			
			exp+=gain;
			if(exp>=stats.expThresholdList[level] && level<stats.levelCap) _LevelUp();
			
			if(level==stats.levelCap) exp=stats.expThresholdList[stats.expThresholdList.Count-1];
			
			if(player.SaveUponChange()) Save();
		}
		
		
		//public static void LevelUp(){ instance._LevelUp(); }
		public void _LevelUp(){
			PlayerPerk perk=player.GetPlayerPerk();
			if(perk!=null){
				for(int i=0; i<stats.perkUnlockingAtLevelList.Count; i++){
					if(level+1==stats.perkUnlockingAtLevelList[i].level){
						for(int n=0; n<stats.perkUnlockingAtLevelList[i].perkIDList.Count; n++)
							perk.PurchasePerk(stats.perkUnlockingAtLevelList[i].perkIDList[n], false);
						
						break;
					}
				}
			}
			
			level+=1;
			
			if(level>1){
				player.GainHitPoint(stats.hitPointGain);
				player.GainEnergy(stats.energyGain);
				
				player.GainPerkCurrency(stats.perkCurrencyGain);
				
				TDS.PlayerLevelUp(player);
				TDS.OnGameMessage("level Up!");
			}
			
			GainExp();	//call the function to make sure if we under level
		}
		
		
		//public static int GetExpToNextLevel(){ return instance._GetExpToNextLevel(); }
		public int GetExpToNextLevel(){
			return stats.expThresholdList[level]-exp;
		}
		//public static int GetCurrentLevelExp(){ return instance._GetCurrentLevelExp(); }
		public int GetCurrentLvllExp(){
			return exp-stats.expThresholdList[Mathf.Max(0, level-1)];
		}
		public int GetCurrentLvllUpExp(){
			return stats.expThresholdList[level];
		}
		public int GetPrevLvllUpExp(){
			return stats.expThresholdList[Mathf.Max(0, level-1)];
		}
		//public static float GetCurrentLevelProgress(){ return instance._GetCurrentLevelProgress(); }
		public float GetCurrentLevelProgress(){
			float denominator=(float)(GetCurrentLvllUpExp()-GetPrevLvllUpExp());
			return denominator==0 ? 0 : (float)(exp-GetPrevLvllUpExp())/denominator;
		}
		
		
		/*
		void OnGUI(){
			if(stats.expThresholdList.Count<level) return;
			
			GUI.Label(new Rect(50, 220, 200, 25), "Level: "+level);
			GUI.Label(new Rect(50, 240, 200, 25), "Exp: "+exp+"/"+GetCurrentLvllUpExp());
			//GUI.Label(new Rect(50, 260, 200, 25), "Test: "+GetCurrentLevelExp());
		}
		*/
		
		
		
		public float GetHitPointGain(){ return (level-1)*stats.hitPointGain; }
		public float GetHitPointRegenGain(){ return (level-1)*stats.hitPointRegenGain; }
		public float GetEnergyGain(){ return (level-1)*stats.energyGain; }
		public float GetEnergyRegenGain(){ return (level-1)*stats.energyRegenGain; }
		public float GetSpeedMulGain(){ return (level-1)*stats.speedMulGain; }
		public float GetDamageMulGain(){ return (level-1)*stats.dmgMulGain; }
		public float GetCritChanceMulGain(){ return (level-1)*stats.critChanceMulGain; }
		public float GetCritMultiplierMulGain(){ return (level-1)*stats.critMultiplierMulGain; }
		
		
		
		
		
		public static List<int> GenerateExpTH(bool sumR=true, float m=1.5f, float c=10f, int lvlCap=50){
			List<int> thList=new List<int>{ 0 };
			
			int sum=0;
			for(int i=0; i<lvlCap-1; i++){
				if(sumR) sum+=(int)Mathf.Round(m*(float)i+c);
				else sum=(int)Mathf.Round(m*(float)i+c);
				
				thList.Add(sum);
			}
			
			return thList;
		}
		
		
	}
	

}