using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	[SelectionBase]
	[RequireComponent (typeof (Rigidbody))]
	[RequireComponent (typeof (CapsuleCollider))]
	public class Unit : MonoBehaviour {
		
		public delegate void DestroyCallback();		//for spawner to keep track of active ai unit count
		private List<DestroyCallback> destroyCallbackList=new List<DestroyCallback>();
		public void SetDestroyCallback(DestroyCallback callback){ destroyCallbackList.Add(callback); }
		

		[HideInInspector] public Transform thisT;
		[HideInInspector] public GameObject thisObj;
		
		[HideInInspector] public int prefabID=0;
		[HideInInspector] public int instanceID=0;
		public Sprite icon;
		public string unitName="Unit";
		public string desp="";
		
		protected bool hostileUnit=false;
		protected bool isPlayer=false;
		
		
		
		[Header("Basic Stats")]
		public int level=1;
		
		public float hitPointFull=10;
		public float hitPoint=10;
		public bool startHitPointAtFull=true;	//not used by AI
		
		public float hpRegenRate=0;
		public float hpRegenStagger=5;
		private float hpRegenStaggerCounter=0;
		
		
		//energy is not used by AI
		public float energyFull=10;
		public float energy=10;
		public float energyRate=0.5f;
		public bool startEnergyAtFull=true;
		
		public int armorType=0;
		
		public bool anchorDown=false;
		
		public float moveSpeed=5;
		public float brakeRange=2;
		public float rotateSpeed=90;	//in degree per second
		
		
		[HideInInspector] public Unit target;
		
		
		[Header("Range Attack Setting")]
		public bool enableRangeAttack=false;
		
		public GameObject shootObject;
		public List<Transform> shootPointList=new List<Transform>();
		public float shootPointDelay=0;
		
		public Transform turretObj;
		public bool smoothTurretRotation=true;
		public float turretTrackingSpeed=90;		//in degree per second
		
		public float range=30;
		public float cooldown=5f;
		protected float currentCD=0.25f;
		public AttackStats attackStats;
		
		
		[Header("Contact Attack Stats")]
		public bool enableContactAttack=false;
		public float contactCooldown=1f;
		[HideInInspector] public float contactCurrentCD=.0f;
		public AttackStats contactAttackStats;
		
		
		
		[Header("Destroyed Setting ")]
		public int valueCredits=0;
		public int valueScore=0;
		public int valueHitPoint=0;
		public int valueEnergy=0;
		public int valueExp=0;		//experience
		public int valuePerkC=0;	//perkCurrency
		
		
		public float destroyCamShake=0;
		
		public GameObject destroyedEffectObj;
		public bool autoDestroyDObj=true;
		public float dObjActiveDuration=2;
		
		public bool useDropManager=true;
		public GameObject dropObject;
		public float dropChance=0.5f;
		
		public Unit spawnUponDestroy;
		public int spawnUponDestroyCount=2;
		
		
		protected bool destroyed=false;
		public bool IsDestroyed(){ return destroyed; }
		
		
		protected Collider thisCollider;
		public Collider GetCollider(){ return thisCollider; }
		
		
		public UnitAnimation uAnimation;
		public void SetUnitAnimation(UnitAnimation uAnim){ uAnimation=uAnim; }
		protected bool moved=false;	//to indicate if the unit has moved in the frame (move animation is called in LateUpdate())
		
		
		//attack range
		public virtual float GetRange(){ return range; }
		
		
		public virtual void Awake() {
			thisT=transform;
			thisObj=gameObject;
			
			//initiate the base stats
			if(startHitPointAtFull) hitPoint=hitPointFull;
			
			if(startEnergyAtFull) energy=energyFull;
			else energy=Mathf.Clamp(energy, 0, energyFull);
			
			currentCD=0;
			
			//setup the physics
			thisCollider=thisObj.GetComponent<Collider>();
			if(anchorDown){
				thisObj.GetComponent<Rigidbody>().constraints=RigidbodyConstraints.FreezeAll;
				thisObj.GetComponent<Rigidbody>().isKinematic=true;
				moveSpeed=0;
				rotateSpeed=0;
			}
			else{
				Rigidbody rBody=thisObj.GetComponent<Rigidbody>();
				if(rBody!=null) rBody.constraints=RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
			}
			
			//initiate the attack-stats
			attackStats.Init();
			contactAttackStats.Init();
			
			if(shootObject!=null) ObjectPoolManager.New(shootObject);
			if(destroyedEffectObj!=null) ObjectPoolManager.New(destroyedEffectObj);
			
			//TDSUtility.DestroyColliderRecursively(thisT);
		}
		
		public virtual void Start(){
			//assign a unique ID to the unit instance
			instanceID=GameControl.GetUnitInstanceID();
			
			//add to UnitTracker if this is a hostile unit
			if(hostileUnit) UnitTracker.AddUnit(this);
			
			TDS.NewUnit(this);
		}
		
		//called by unit spawner to override the hitpoint
		public void OverrideHitPoint(float value, _OverrideMode mode){
			if(mode==_OverrideMode.Replace) hitPointFull=value;
			else if(mode==_OverrideMode.Addition) hitPointFull+=value;
			else if(mode==_OverrideMode.Multiply) hitPointFull*=value;
			
			hitPoint=hitPointFull;
		}
		
		
		public virtual void Update(){
			//regenerate energy
			if(energy<GetFullEnergy() && GetEnergyRegen()>0){
				energy=Mathf.Clamp(energy+GetEnergyRegen()*Time.deltaTime, 0, GetFullEnergy());
			}
			
			//regenerate hit point
			hpRegenStaggerCounter-=Time.deltaTime;
			if(hpRegenStaggerCounter<=0 && GetHitPointRegen()>0){
				hitPoint=Mathf.Min(GetFullHitPoint(), hitPoint+GetHitPointRegen()*Time.deltaTime);
			}
		}
		
		
		//function call when the unit instance gain/lost hitpoint from external source
		public virtual float GainHitPoint(float value){
			float limit=GetFullHitPoint()-hitPoint;
			hitPoint+=Mathf.Min(value, limit);
			
			//if the unit lost hitpoint instead of gaining it
			if(value<0){
				hpRegenStaggerCounter=hpRegenStagger;
				
				//if this is the player, call the damage event (inform UI)
				if(thisObj.layer==TDS.GetLayerPlayer()) TDS.PlayerDamaged(value);
			}
			
			//if hitpoint reach 0, destroy the unit
			if(hitPoint<=0) Destroyed();
			
			return limit;
		}
		//function call when the unit instance gain/lost energy 
		public virtual float GainEnergy(float value){
			float limit=GetFullEnergy()-energy;
			energy+=Mathf.Min(value, limit);
			return limit;
		}
		
		
		public virtual float GetSpeedMultiplier(){ return activeEffect.speedMul; }
		public bool IsStunned(){ return activeEffect.stun; }
		public bool IsInvincible(){ return activeEffect.invincible; }
		
		
		//applies the effect of an attack
		public void ApplyAttack(AttackInstance attInstance){
			//if unit is invincible, do nothing
			if(IsInvincible()){
				Debug.Log("Immuned");
				Vector3 osPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(thisT.position+osPos, "Immuned");
				return;
			}
			
			AttackStats aStats=attInstance.aStats;
			
			//check if the attack is an aoe and modify the damage value is dimishingAOE is enabled
			float damage=Random.Range(aStats.damageMin, aStats.damageMax);
			if(attInstance.isAOE && aStats.diminishingAOE){
				damage*=Mathf.Clamp(1-attInstance.aoeDistance/aStats.aoeRadius, 0, 1);
			}
			
			
			//check for cirtical and modify the damage based on critical multiplier
			bool critical=Random.value<aStats.critChance;
			if(critical) damage*=aStats.critMultiplier;
			
			//modify the damage based on damage and armor type
			damage*=DamageTable.GetModifier(armorType, aStats.damageType);
			
			//if damage is valid, modify the hit point
			if(damage>0){
				//show the overlay (for UI)
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				if(!critical) new TextOverlay(thisT.position+offsetPos, damage.ToString("f0"));
				else new TextOverlay(thisT.position+offsetPos, damage.ToString("f0"), new Color(1f, 0.9f, 0.9f, 1f), 1.5f);
				
				//if the unit is player, fire event to inform UI
				if(thisObj.layer==TDS.GetLayerPlayer()) TDS.PlayerDamaged(damage);
				
				//register the stagger
				hpRegenStaggerCounter=hpRegenStagger;
				
				hitPoint-=damage;	//damage the hitpoint
				if(hitPoint<=0) Destroyed(attInstance.GetSrcPlayer());
				
				if(hitPoint>0 && uAnimation!=null) uAnimation.Hit();
			}
			
			//apply effect if there's any
			//if(aStats.effect!=null) ApplyEffect(aStats.effect);
			if(aStats.effectIdx>=0) ApplyEffect(EffectDB.CloneItem(aStats.effectIdx));
		}
		
		//for apply damage directly, not in used
		public void ApplyDamage(float damage){
			//show the overlay (for UI)
			Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
			new TextOverlay(thisT.position+offsetPos, damage.ToString("f0"));
			
			//if the unit is player, fire event to inform UI
			if(thisObj.layer==TDS.GetLayerPlayer()) TDS.PlayerDamaged(damage);
			
			//register the stagger
			hpRegenStaggerCounter=hpRegenStagger;
			
			hitPoint-=damage;	//damage the hitpoint
			if(hitPoint<=0) Destroyed();
			
			if(hitPoint>0 && uAnimation!=null) uAnimation.Hit();
		}
		
		
		
		
		//for when the unit is destroy by game mechanic
		public void Destroyed(UnitPlayer player=null){	
			if(destroyed) return;
			
			//if(player==null) Debug.LogWarning("unit destroyed by not source player has been detected", null);
			
			destroyed=true;
			
			//spawn a unit if spawnUponDestroy is assigned
			if(spawnUponDestroy!=null){
				//loop as many as required
				for(int i=0; i<spawnUponDestroyCount; i++){
					//instantiate the unit
					//GameObject unitObj=(GameObject)Instantiate(spawnUponDestroy.gameObject, thisT.position, thisT.rotation);
					GameObject unitObj=ObjectPoolManager.Spawn(spawnUponDestroy.gameObject, thisT.position, thisT.rotation);
					unitObj.layer=thisObj.layer;
					
					//pass on the wave info
					Unit unitInstance=unitObj.GetComponent<Unit>();
					if(waveID>=0){
						unitInstance.SetWaveID(spawner, waveID);
						spawner.AddUnitToWave(unitInstance);
					}
					else if(spawner!=null) spawner.AddUnit(unitInstance);
				}
			}
			
			//check if the unit is going to drop any collectible
			if(useDropManager){	//if useDropManager is enabled, inform CollectibleDropManager
				CollectibleDropManager.UnitDestroyed(thisT.position);
			}
			else{
				//check chance and spawn the item
				if(dropObject!=null && Random.value<dropChance)
					ObjectPoolManager.Spawn(dropObject.gameObject, thisT.position, Quaternion.identity);
			}
			
			
			if(player!=null){
				//applies the bonus value for when the unit is destroyed.
				//if(valueCredits>0) GameControl.GainCredits(valueCredits);
				if(valueScore>0) GameControl.GainScore(valueScore);
				
				if(valuePerkC>0) player.GainPerkCurrency(valuePerkC);
				if(valueExp>0) player.GainExp(valueExp);
				if(valueHitPoint>0) player.GainHitPoint(valueHitPoint);
				if(valueEnergy>0) player.GainEnergy(valueEnergy);
			}
			
			
			//shake camera
			TDS.CameraShake(destroyCamShake);
			
			float delay=uAnimation!=null ? uAnimation.Destroyed() : 0;
			
			ClearUnit(true, delay);
		}
		
		//called when unit is destroyed, also called by other component to take unit out of the game
		public void ClearUnit(bool showDestroyEffect=true, float delay=0){	StartCoroutine(_ClearUnit(showDestroyEffect, delay)); }
		public IEnumerator _ClearUnit(bool showDestroyEffect=true, float delay=0){
			destroyed=true;
			
			//if the unit is spawned from a spawner for a parcicular wave, inform the spawner that a unit in the wave is cleared
			if(spawner!=null && waveID>=0){
				spawner.UnitCleared(waveID);
			}
			
			//remove the unit from UnitTracker and inform GameControl (to check for objective)
			if(hostileUnit){
				UnitTracker.RemoveUnit(this);
				GameControl.UnitDestroyed(this);
			}
			
			//clear all the effects on the unit
			for(int i=0; i<effectList.Count; i++) effectList[i].expired=true;
			
			//call all the destroy callback, if there's any
			for(int i=0; i<destroyCallbackList.Count; i++) destroyCallbackList[i]();
			
			//spawn the destroy effect
			if(showDestroyEffect) DestroyedEffect(thisT.position+new Vector3(0, 0.1f, 0));
			
			if(delay>0) yield return new WaitForSeconds(delay);
			
			//if(destroyParent) Destroy(thisT.parent.gameObject);
			
			Destroy(thisObj);
			
			yield return null;
		}
		
		//[HideInInspector] public bool destroyParent=false;
		
		//spawn the destroy effect
		void DestroyedEffect(Vector3 pos){
			if(destroyedEffectObj==null) return;
			//~ GameObject obj=(GameObject)MonoBehaviour.Instantiate(destroyedEffectObj, pos, thisT.rotation);
			//~ if(autoDestroyDObj) MonoBehaviour.Destroy(obj, dObjActiveDuration);
			
			if(!autoDestroyDObj) ObjectPoolManager.Spawn(destroyedEffectObj, pos, thisT.rotation);
			else ObjectPoolManager.Spawn(destroyedEffectObj, pos, thisT.rotation, dObjActiveDuration);
		}
		
		
		//for unit which spawned for a particular wave in wave based spawning
		//to keep track if the wave has been cleared
		[HideInInspector] public int waveID=-1;
		[HideInInspector] public UnitSpawner spawner;
		public void SetWaveID(UnitSpawner sp, int id){
			spawner=sp;
			waveID=id;
		}
		
		
		
		
		//the lsit of active effects on the unit
		private List<Effect> effectList=new List<Effect>();
		
		//function call to add new effect to the unit
		public virtual bool ApplyEffect(Effect effect){
			if(effect==null || !effect.Applicable()) return false;
			
			//if the effect missed
			if(Random.value>effect.hitChance) return false;
			
			//add the effect to the list and update the active effect
			effectList.Add(effect);
			UpdateActiveEffect();
			
			//run the coroutine for the effect
			if(!effCoroutine) StartCoroutine(EffectRoutine());
			
			return true;
		}
		
		private bool effCoroutine=false;	//make sure we dont run 2 instance of EffectRoutine
		//to keep track of all the effect
		IEnumerator EffectRoutine(){
			effCoroutine=true;
			
			//loop forever
			while(true){
				//gain hit point or energy if applicable
				if(activeEffect.restoreHitPoint!=0) GainHitPoint(activeEffect.restoreHitPoint * Time.deltaTime);
				if(activeEffect.restoreEnergy!=0) GainEnergy(activeEffect.restoreEnergy * Time.deltaTime);
				
				bool updateEffect=false;
				for(int i=0; i<effectList.Count; i++){
					effectList[i].duration-=Time.deltaTime;
					//if an effect duration has run out
					if(effectList[i].duration<=0){
						effectList.RemoveAt(i);
						i-=1;
						updateEffect=true;	//set updateEffect to true so UpdateActiveEffect() will be called later
					}
				}
				if(updateEffect) UpdateActiveEffect();
				
				yield return null;
			}
		}
		
		//this is the master effect combining all the stats from all active effect
		//the stats is recalculated in UpdateActiveEffect everytime an effect is added or removed
		public Effect activeEffect=new Effect();
		
		//refresh the stats in activeEffect
		public void UpdateActiveEffect(){
			activeEffect=new Effect();
			for(int i=0; i<effectList.Count; i++){
				activeEffect.restoreHitPoint+=effectList[i].restoreHitPoint;
				activeEffect.restoreEnergy+=effectList[i].restoreEnergy;
				
				activeEffect.invincible|=effectList[i].invincible;
				activeEffect.stun|=effectList[i].stun;
				activeEffect.speedMul*=effectList[i].speedMul;
				
				activeEffect.damageMul*=effectList[i].damageMul;
				
				activeEffect.critChanceMul*=effectList[i].critChanceMul;
				activeEffect.critMultiplierMul*=effectList[i].critMultiplierMul;
			}
		}
		
		//modify the attackStats to active effect
		protected AttackStats ModifyAttackStatsToExistingEffect(AttackStats aStats){
			aStats.damageMin*=activeEffect.damageMul;
			aStats.damageMax*=activeEffect.damageMul;
			
			aStats.critChance*=activeEffect.critChanceMul;
			aStats.critMultiplier*=activeEffect.critMultiplierMul;
			
			return aStats;
		}
		
		
		
		public virtual float GetFullHitPoint(){ return hitPointFull; }
		public virtual float GetFullEnergy(){ return energyFull; }
		public virtual float GetHitPointRegen(){ return hpRegenRate; }
		public virtual float GetEnergyRegen(){ return energyRate; }
		
		public virtual UnitPlayer GetUnitPlayer(){ return null; }
		
	}

	
}