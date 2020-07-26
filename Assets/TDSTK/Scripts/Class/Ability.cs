using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _AbilityType{
		AOE,
		AOESelf,
		//Ray,
		All,
		Self,
		Shoot,
		Movement,
		Custom,
	}
	
	public enum _MoveType{ Blink, Dash, Teleport }
	
	[System.Serializable]
	public class Ability : Item{
		public string desp;
		
		public _AbilityType type;
		
		//public float aoeRadius=3;	//aoeradius on aStats is used instead
		
		//used for movement only
		public _MoveType moveType;
		public float duration=0.2f;
		//end use for movement
		
		public float cost;
		
		public float cooldown=0.15f;
		[HideInInspector] public float currentCD=0.0f;
		
		public AttackStats aStats=new AttackStats();
		
		public float range=20;
		public GameObject shootObject;
		public Vector3 shootPosOffset=new Vector3(0, 1, 0);
		
		public GameObject launchObj;
		public bool autoDestroyLaunchObj=true;
		public float launchObjActiveDuration=2;
		
		public AudioClip launchSFX;
		
		//called at the when an ability is first added to player
		public void Init(){ 
			aStats.Init();
			
			currentCD=0;
				
			if(type==_AbilityType.Shoot && shootObject==null){
				Debug.LogWarning("Ability ("+name+") - is shoot type but no shoot object has been assigned", null);
			}
		}
		
		
		//check if the ability is ready to launch
		public string IsReady(){
			if(GameControl.GetPlayer().energy<GetCost()) return "Insufficient Energy";
			if(currentCD>0) return "Ability on Cooldown";
			return "";
		}
		
		
		//launch the ability, at the position given
		public void Activate(Vector3 pos=default(Vector3), bool useCostNCD=true){
			if(useCostNCD){
				currentCD=GetCooldown();							//set the cooldown
				GameControl.GetPlayer().energy-=GetCost();	//deduct player energy by the ability cost
			}
			
			AudioManager.PlaySound(launchSFX);
			
			//instantiate the launch object, if there's any
			if(launchObj!=null){
				GameObject obj=(GameObject)MonoBehaviour.Instantiate(launchObj, pos, Quaternion.identity);
				if(autoDestroyLaunchObj) MonoBehaviour.Destroy(obj, launchObjActiveDuration);
			}
			
			//for aoe ability
			if(type==_AbilityType.AOE || type==_AbilityType.AOESelf){
				//get all the collider in range
				Collider[] cols=Physics.OverlapSphere(pos, GetAOERadius());
				for(int i=0; i<cols.Length; i++){
					Unit unitInstance=cols[i].gameObject.GetComponent<Unit>();
					
					//only continue if the collider is a unit and is not player
					if(unitInstance!=null && unitInstance!=GameControl.GetPlayer()){
						//create an AttackInstance and mark it as AOE attack
						AttackInstance aInstance=new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
						aInstance.isAOE=true;
						aInstance.aoeDistance=Vector3.Distance(pos, cols[i].transform.position);
						//apply the AttackInstance
						unitInstance.ApplyAttack(aInstance);
					}
				}
				
				//apply explosion force
				TDSPhysics.ApplyExplosionForce(pos, aStats);
			}
			
			//for ability that affects all hostile unit in game
			else if(type==_AbilityType.All){
				//get all hostile unit for unit tracker
				List<Unit> unitList=new List<Unit>( UnitTracker.GetAllUnitList() );
				//loop through all unit, create an AttackInstance and apply the attack
				for(int i=0; i<unitList.Count; i++){
					AttackInstance aInstance=new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
					unitList[i].ApplyAttack(aInstance);
				}
			}
			
			//for ability that meant to be cast on player unit
			else if(type==_AbilityType.Self){
				//apply the attack on player 
				AttackInstance aInstance=new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
				GameControl.GetPlayer().ApplyAttack(aInstance);
			}
			
			//for ability that fires a shoot object
			else if(type==_AbilityType.Shoot){
				//get the position and rotation to fire the shoot object from
				Transform srcT=GetShootObjectSrcTransform();
				Vector3 shootPos=srcT.TransformPoint(shootPosOffset);
				pos.y=shootPos.y;
				Quaternion shootRot=Quaternion.LookRotation(pos-shootPos);
				
				//create the AttackInstance
				AttackInstance aInstance=new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
				
				//Instantiate the shoot-object and fires it
				GameObject soObj=(GameObject)MonoBehaviour.Instantiate(shootObject, shootPos, shootRot);
				ShootObject soInstance=soObj.GetComponent<ShootObject>();
				soInstance.Shoot(GameControl.GetPlayer().thisObj.layer, GetRange(), srcT, aInstance);
			}
			
			else if(type==_AbilityType.Movement){
				if(moveType==_MoveType.Blink){
					GameControl.GetPlayer().thisT.position+=GameControl.GetPlayer().thisT.TransformVector(Vector3.forward*range);
				}
				else if(moveType==_MoveType.Dash){
					//GameControl.GetPlayer().thisT.position+=GameControl.GetPlayer().thisT.TransformPoint(Vector3.z)*range;
					GameControl.GetPlayer().Dash(range, duration);
				}
				else if(moveType==_MoveType.Teleport){
					Transform playerT=GameControl.GetPlayer().thisT;
					Vector3 tgtPos=new Vector3(pos.x, playerT.position.y, pos.z);
					
					if(Vector3.Distance(playerT.position, tgtPos)>range){
						tgtPos=playerT.position+(tgtPos-playerT.position).normalized*range;
					}
					
					playerT.position=tgtPos;
				}
			}
		}
		
		
		//to get the player turret object (for shooting ability) 
		public Transform GetShootObjectSrcTransform(){
			return GameControl.GetPlayer().turretObj!=null ? GameControl.GetPlayer().turretObj : GameControl.GetPlayer().thisT;
		}
		
		
		public Ability Clone(){
			Ability ab=new Ability();
			
			ab.ID=ID;
			ab.name=name;
			ab.icon=icon;
			ab.desp=desp;
			
			ab.type=type;
			
			ab.cost=cost;
			ab.cooldown=cooldown;
			ab.currentCD=currentCD;
			
			ab.aStats=aStats.Clone();
			
			ab.range=range;
			ab.shootObject=shootObject;
			ab.shootPosOffset=shootPosOffset;
			
			ab.moveType=moveType;
			ab.duration=duration;
			
			ab.launchObj=launchObj;
			ab.autoDestroyLaunchObj=autoDestroyLaunchObj;
			ab.launchObjActiveDuration=launchObjActiveDuration;
			
			return ab;
		}
		
		
		
		
		private PlayerPerk perk;
		public void SetPlayerPerk(PlayerPerk pPerk){ perk=pPerk; }
		
		
		public float GetCost(){ return cost*(1+(perk!=null ? perk.GetAbilityCostMul(ID) : 0)); }
		public float GetCooldown(){ return cooldown*(1+(perk!=null ? perk.GetAbilityCooldownMul(ID) : 0)); }
		public float GetRange(){ return range*(1+(perk!=null ? perk.GetAbilityRangeMul(ID) : 0)); }
		public float GetAOERadius(){ return aStats.aoeRadius*(1+(perk!=null ? perk.GetAbilityAOEMul(ID) : 0)); }
		
		public AttackStats GetRuntimeAttackStats(){
			if(perk==null) return aStats.Clone();
			
			aStats.damageMin*=(1+perk.GetAbilityDamageMul(ID));
			aStats.damageMax*=(1+perk.GetAbilityDamageMul(ID));
			
			aStats.critChance*=(1+perk.GetAbilityCritMul(ID));
			aStats.critMultiplier*=(1+perk.GetAbilityCritMulMul(ID));
			
			aStats.aoeRadius*=(1+perk.GetAbilityAOEMul(ID));
			
			return aStats;
		}
		
		
		public void ChangeEffect(int newID, int newIdx){ 	//for perk
			aStats.effectID=newID;
			aStats.effectIdx=newIdx;
		}
		
		
	}
	
}
