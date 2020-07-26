//class containing information about a particular attack
//each attack on a particular unit will contain an attack instance (even ability)

using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[System.Serializable]
	public class AttackInstance{
		public Unit srcUnit;
		
		public bool isAOE=false;		//marked if the attack is AOE
		public float aoeDistance=0;	//the distance from the origin of the aoe attack to the target
		
		public AttackStats aStats;	//contain all the stats
		
		//create a new attackInstance, given the AttackStats (the set of stats for the attack unit or ability)
		public AttackInstance(Unit src=null, AttackStats aSt=null){
			srcUnit=src;
			aStats=aSt;
		}
		
		public AttackInstance Clone(){
			AttackInstance aInstance=new AttackInstance();
			
			aInstance.srcUnit=srcUnit;
			
			aInstance.isAOE=isAOE;
			aInstance.aoeDistance=aoeDistance;
			aInstance.aStats=aStats.Clone();
			
			return aInstance;
		}
		
		public UnitPlayer GetSrcPlayer(){
			return srcUnit!=null ? srcUnit.GetUnitPlayer() : null ;
		}
	}
	
}
