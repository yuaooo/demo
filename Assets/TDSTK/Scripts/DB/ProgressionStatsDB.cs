using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class ProgressionStatsDB : MonoBehaviour {
		
		//~ public int levelCap=50;
		
		//~ public float hitPointGain=5;
		//~ public float hitPointRegenGain=0;
		//~ public float energyGain=1;
		//~ public float energyRegenGain=0;
		//~ public float speedMulGain=0;
		//~ public float dmgMulGain=0.1f;
		//~ public float critChanceMulGain=0;
		//~ public float critMultiplierMulGain=0;
		
		//~ public int perkCurrencyGain=1;
		
		//~ public bool sumRecursively=true;
		//~ public float expTHM=1.5f;
		//~ public int expTHC=10;
		//~ public List<int> expThresholdList=new List<int>();
		
		//~ public List<PerkUnlockingAtLevel> perkUnlockingAtLevelList=new List<PerkUnlockingAtLevel>();
		
		
		public LevelProgressionStats stats;
		
		
		private static ProgressionStatsDB db;	//for runtime
		
		private static bool initiated=false;
		public static void Init(){
			if(initiated) return;
			initiated=true;
			
			db=LoadDB();
		}
		public static void CopyStats(PlayerProgression progress){	//only used in runtime to load db data to runtime PlayerProgression
			Init();
			
			//~ progress.levelCap=db.levelCap;
			
			//~ progress.perkCurrencyGain=db.perkCurrencyGain;
			
			//~ progress.hitPointGain=db.hitPointGain;
			//~ progress.hitPointRegenGain=db.hitPointRegenGain;
			//~ progress.energyGain=db.energyGain;
			//~ progress.energyRegenGain=db.energyRegenGain;
			//~ progress.speedMulGain=db.speedMulGain;
			//~ progress.dmgMulGain=db.dmgMulGain;
			//~ progress.critChanceMulGain=db.critChanceMulGain;
			//~ progress.critMultiplierMulGain=db.critMultiplierMulGain;
			
			//~ progress.expThresholdList=new List<int>( db.expThresholdList );
			//~ progress.perkUnlockingAtLevelList=new List<PerkUnlockingAtLevel>( db.perkUnlockingAtLevelList );
			
			progress.stats=db.stats.Clone();
		}
		
		
		
		
		public static ProgressionStatsDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Progression", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<ProgressionStatsDB>();
		}
		
		
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<ProgressionStatsDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_Progression.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
