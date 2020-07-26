using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {
	
	public enum _PerkType{
		//~ ModifyHitPoint,
		//~ ModifyEnergy,
		//~ ModifyMovement,
		//~ ModifyGain,
		
		ModifyGeneralStats,
		
		AddWeapon,
		ModifyWeapon,
		
		AddAbility, 		
		ModifyAbility,
		
		Custom,
	}
	
	
	[System.Serializable]
	public class Perk : Item{
		public string desp;
		
		public _PerkType type;
		public bool repeatable=false;
		public int limit=0;	//for repeatable perk, how many time it can be purchased, <=0 means unlimited
		
		public int purchased=0;	//0 is not purchased, 1 is purchased, >1 is for repeatable perk specify how many time they have been purchased
		public bool Purchased(){ return purchased>0; }
		
		public int cost=1;
		public int minLevel=1;								//min level to reach before becoming available (check GameControl.levelID)
		public int minPerkPoint=0;						//min perk point 
		public List<int> prereq=new List<int>();	//prerequisite perk before becoming available, element is removed as the perk is unlocked in runtime
		
		public UnitPlayer player;
		public void SetPlayer(UnitPlayer unit){ player=unit; }
		
		
		public string IsAvailable(){
			if(player==null) return "Error: perk has no designated player unit";
			
			if(purchased>0 && !repeatable) return "Purchased";
			if(repeatable && limit>0 && purchased>=limit) return "Limit reached";
			if(player.GetLevel()<minLevel) return "Require level - "+minLevel;
			if(player.GetPerkCurrency()<cost) return "Insufficient currency. Require "+cost;
			if(player.GetPerkPoint()<minPerkPoint) return "Insufficient perk point. Require "+minPerkPoint;
			if(prereq.Count>0){
				string text="Require: ";
				bool first=true;
				List<Perk> perkList=player.GetPerkList();
				for(int i=0; i<prereq.Count; i++){
					for(int n=0; n<perkList.Count; n++){
						if(perkList[n].ID==prereq[i]){
							text+=((!first) ? ", " : "")+perkList[n].name;
							first=false;
							break;
						}
					}
				}
				return text;
			}
			return "";
		}
		
		
		public string Purchase(PlayerPerk playerPerk=null, bool usePerkCurrency=true){
			if(purchased>0 && !repeatable) return "Error trying to re-purchase non-repeatable perk";
			
			if(repeatable && purchased>=limit) return "Limit reached";
			
			if(usePerkCurrency && playerPerk!=null){
				if(playerPerk.GetPerkCurrency()<cost) return "Insufficient perk currency";
				playerPerk.SpendCurrency(cost);
			}
			
			purchased+=1;
			
			return "";
		}
		
		
		
		
		//ModifyHitPoint,
		public float hitPoint=0;
		public float hitPointCap=0;
		public float hitPointRegen=0;
		
		//ModifyEnergy
		public float energy=0;
		public float energyCap=0;
		public float energyRegen=0;
		
		//ModifyMovement
		public float moveSpeedMul=0;
		
		//ModifyAttack
		public float dmgMul=0;
		public float critChanceMul=0;
		public float CritMultiplierMul=0;
		
		//ModifyGain
		public float expGainMul=0;		//experience
		public float creditGainMul=0;
		public float scoreGainMul=0;
		public float hitPointGainMul=0;
		public float energyGainMul=0;
	
		//AddWeapon,
		public int newWeaponID=-1;
		public bool replaceExisting=false;
		public int replaceWeaponID=-1;
		
		//ModifyWeapon	all these are multiplier
		public bool appliedToAllWeapon=false;
		public List<int> weaponIDList=new List<int>();
		
		public float weapDmg=0;
		public float weapCrit=0;
		public float weapCritMul=0;
		public float weapAOE=0;
		public float weapRange=0;
		public float weapCooldown=0;
		public float weapClipSize=0;
		public float weapAmmoCap=0;
		public float weapReloadDuration=0;
		public float weapRecoilMagnitude=0;
		
		public int weapEffectID=-1;
		public int weapAbilityID=-1;
		
		
		//AddAbility, 
		public int newAbilityID=-1;
		public int replaceAbilityID=-1;
		
		//ModifyAbility,
		public bool appliedToAllAbility=true;
		public List<int> abilityIDList=new List<int>();
		
		public float abCost;
		public float abCooldown;
		public float abRange=0;
		
		public float abDmg=0;
		public float abCrit=0;
		public float abCritMul=0;
		public float abAOE=0;
		
		public int abEffectID=-1;
		
		
		//Custom
		public GameObject customObject;
		
		
		
		public Perk Clone(){
			Perk perk=new Perk();
			
			perk.ID=ID;
			perk.icon=icon;
			perk.name=name;
			perk.desp=desp;
			
			perk.type=type;
			perk.repeatable=repeatable;
			perk.limit=limit;
			
			perk.purchased=purchased;
			
			perk.cost=cost;
			perk.minLevel=minLevel;
			perk.minPerkPoint=minPerkPoint;
			perk.prereq=new List<int>( prereq );
			
			
			//generic multiplier
			perk.hitPoint=hitPoint;
			perk.hitPointCap=hitPointCap;
			perk.hitPointRegen=hitPointRegen;
			
			perk.energy=energy;
			perk.energyCap=energyCap;
			perk.energyRegen=energyRegen;
			
			perk.moveSpeedMul=moveSpeedMul;
			
			perk.dmgMul=dmgMul;
			perk.critChanceMul=critChanceMul;
			perk.CritMultiplierMul=CritMultiplierMul;
			
			perk.expGainMul=expGainMul;
			perk.creditGainMul=creditGainMul;
			perk.scoreGainMul=scoreGainMul;
			perk.hitPointGainMul=hitPointGainMul;
			perk.energyGainMul=energyGainMul;
			
			//add new weapon
			perk.newWeaponID=newWeaponID;
			perk.replaceExisting=replaceExisting;
			perk.replaceWeaponID=replaceWeaponID;
			
			//add new ability
			perk.newAbilityID=newAbilityID;
			perk.replaceAbilityID=replaceAbilityID;
		
			
			//modify weapon
			perk.appliedToAllWeapon=appliedToAllWeapon;
			perk.weaponIDList=new List<int>( weaponIDList );
			
			perk.weapDmg=weapDmg;
			perk.weapCrit=weapCrit;
			perk.weapCritMul=weapCritMul;
			perk.weapAOE=weapAOE;
			perk.weapRange=weapRange;
			perk.weapCooldown=weapCooldown;
			perk.weapClipSize=weapClipSize;
			perk.weapAmmoCap=weapAmmoCap;
			perk.weapReloadDuration=weapReloadDuration;
			perk.weapRecoilMagnitude=weapRecoilMagnitude;
			perk.weapEffectID=weapEffectID;
			perk.weapAbilityID=weapAbilityID;
			
			
			//modify ability
			perk.appliedToAllAbility=appliedToAllAbility;
			perk.abilityIDList=new List<int>( abilityIDList );
			
			perk.abCost=abCost;
			perk.abCooldown=abCooldown;
			perk.abRange=abRange;
			
			perk.abDmg=abDmg;
			perk.abCrit=abCrit;
			perk.abCritMul=abCritMul;
			perk.abAOE=abAOE;
			perk.abEffectID=abEffectID;
			
			
			//custom
			perk.customObject=customObject;
			
			return perk;
		}
		
	}
	
}