//class containing all the stats about an attack

using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[System.Serializable]
	public class AttackStats{
		[Header("Attributes")]
		public int damageType=0;
		public float damageMin=0;
		public float damageMax=0;
		
		public float aoeRadius=0;
		//public bool dimishingAOE=true;
		public bool diminishingAOE=true;
		
		public float critChance=0;
		public float critMultiplier=2;
		
		[Header("Physics")]
		public float impactForce=0;
		
		public float explosionRadius=0;
		public float explosionForce=0;
		
		
		[Header("Effects")]
		public int effectID=-1;
		//[HideInInspector] 
		public int effectIdx=-1;	//effect'sindex to effectlist in DB during runtime
		//[HideInInspector] public Effect effect=null;
		
		
		public void Init(){
			//clone the effect so the original in DB wont get modified
			//effect=EffectDB.CloneItem(effectID);
			effectIdx=EffectDB.GetEffectIndex(effectID);
		}
		
		
		//not in used
		public void ModifyValueToLevelMultiplier(int attackerLevel, int targetLevel){
			int disparity=attackerLevel-targetLevel;
			int sign=disparity>0 ? 1 : -1;
			
			float baseDmgModifier=0.05f;
			float baseCritModifier=0.1f;
			
			float multiplierDmg=1f;
			float multiplierCrit=1f;
			
			for(int i=0; i<Mathf.Abs(disparity); i++) multiplierDmg*=1+(sign*baseDmgModifier);
			for(int i=0; i<Mathf.Abs(disparity); i++) multiplierCrit*=1+(sign*baseCritModifier);
			
			damageMin*=multiplierDmg;
			damageMax*=multiplierDmg;
			critChance*=multiplierCrit;
		}
		
		
		public AttackStats Clone(){
			AttackStats stats=new AttackStats();
			
			stats.damageMin=damageMin;
			stats.damageMax=damageMax;
			
			stats.aoeRadius=aoeRadius;
			//stats.dimishingAOE=dimishingAOE;
			stats.diminishingAOE=diminishingAOE;
			
			stats.critChance=critChance;
			stats.critMultiplier=critMultiplier;
			
			stats.impactForce=impactForce;
			stats.explosionRadius=explosionRadius;
			stats.explosionForce=explosionForce;
			
			stats.effectID=effectID;
			stats.effectIdx=effectIdx;
			//stats.effect=effect!=null ? effect.Clone() : null;
			
			return stats;
		}
	}
	
}
