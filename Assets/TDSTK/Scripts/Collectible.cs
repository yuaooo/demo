using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _CollectType{Self, AOEHostile, AllHostile, Ability}
	
	[RequireComponent (typeof (SphereCollider))]
	public class Collectible : MonoBehaviour {
		
		[HideInInspector] public int ID=-1;
		public Sprite icon;
		public string collectibleName="Collectible";
		public string desp="";
		
		public _CollectType type;
		
		[Header("Hostile")]
		public float aoeRange=0;
		
		public AttackStats aStats;
		
		
		[Header("Self")]
		public int life=0;
		
		public float hitPoint=0;
		public float energy=0;
		
		public int credit=0;
		public int score=0;
		
		public int ammo=0;		//-1 to fully refilled
		public int ammoID=-1; 	//-1 for all weapons, else for weapon index
		
		public int exp=0;
		public int perkCurrency=0;
		
		public int effectID=-1;
		[HideInInspector] private int effectIdx=-1;	//effect's index to effectlist in DB during runtime
		//[HideInInspector] public Effect effect;
		
		public bool gainWeapon=false;
		public bool randomWeapon=true;
		public bool enableAllWeapon=true;
		public List<Weapon> weaponList=new List<Weapon>();
		public bool discardWhenOutOfAmmo=true;
		public bool replaceCurrentWeapon=false;
		
		public enum _WeaponType{ Add, Replacement, Temporary };
		public _WeaponType weaponType=_WeaponType.Replacement;
		public float tempWeapDuration=10;
		
		
		public int abilityID=0;
		
		
		[Header("Common")]
		
		public GameObject triggerEffectObj;
		public bool autoDestroyEffectObj=true;
		public float effectObjActiveDuration=2;
		
		public AudioClip triggerSFX;
		
		
		public bool selfDestruct=false;
		public float selfDestructDuration=5;
		
		public bool blinkBeforeDestroy;
		public float blinkDuration;
		public GameObject blinkObj;
		public Transform GetBlinkObjT(){ return blinkObj==null ? null : blinkObj.transform; }
		
		
		
		
		void Awake(){
			gameObject.GetComponent<Collider>().isTrigger=true;
			gameObject.layer=TDS.GetLayerCollectible();
			
			if(type==_CollectType.Self){
				//initiate the weapon list to contain all weapon if the condition is checked
				if(gainWeapon && randomWeapon && enableAllWeapon) weaponList=new List<Weapon>( WeaponDB.Load() );
				
				//make sure none of the element in weaponList is null
				for(int i=0; i<weaponList.Count; i++){
					if(weaponList[i]==null){ weaponList.RemoveAt(i); i-=1; }
				}
			}
			
			//effect=EffectDB.CloneItem(effectID);
			effectIdx=EffectDB.GetEffectIndex(effectID);
			
			if(triggerEffectObj!=null) ObjectPoolManager.New(triggerEffectObj, 1);
		}
		
		
		//this is called whenever the object is activated
		void OnEnable(){
			//if self destruct is enabled...
			if(selfDestruct){
				//Destroy(gameObject, selfDestructDuration);
				ObjectPoolManager.Unspawn(gameObject, selfDestructDuration);
				
				//if blinkBeforeDestroy is enabled, call the blink coroutine 
				if(blinkBeforeDestroy && blinkObj!=null) StartCoroutine(Blink());
			}
		}
		//blink the designated object (activate/deactivate it) before this collectible is destroyed
		IEnumerator Blink(){
			float delay=Mathf.Max(0, selfDestructDuration-blinkDuration);
			yield return new WaitForSeconds(delay);
			
			while(true){
				blinkObj.SetActive(false);
				yield return new WaitForSeconds(0.15f);
				blinkObj.SetActive(true);
				yield return new WaitForSeconds(0.35f);
			}
		}
		
		
		void OnDisable(){
			//call the callback function if there's any
			for(int i=0; i<triggerCallbackList.Count; i++){
				if(triggerCallbackList[i]!=null) triggerCallbackList[i](this);
			}
		}
		
		
		//when any collider trigger the collider of this collectible
		void OnTriggerEnter(Collider col){
			//only carry on if the trigger object is player
			if(col.gameObject.layer!=TDS.GetLayerPlayer()) return;
			
			//check the effect type and apply them accordingly
			if(type==_CollectType.Self){	//apply effect to player unit
				ApplyEffectSelf(col.gameObject.GetComponent<UnitPlayer>());
			}
			else if(type==_CollectType.AOEHostile){	//apply effect to all surrounding hostile
				ApplyEffectAOE(col);
			}
			else if(type==_CollectType.AllHostile){		//apply effect to all hostile
				ApplyEffectAll();
			}
			else if(type==_CollectType.Ability){
				AbilityManager.TriggerAbility(abilityID);
			}
			
			GameControl.ColletibleCollected(this);
			
			//play the sound and show spawn the trigger effect object at current position
			AudioManager.PlaySound(triggerSFX);
			TriggeredEffect(transform.position+new Vector3(0, 0.1f, 0));
			
			//Destroy(gameObject);
			ObjectPoolManager.Unspawn(gameObject);
		}
		
		
		//apply effect to all active hostile unit 
		void ApplyEffectAll(){
			//get all active hostile unit from UnitTracker
			List<Unit> unitList=UnitTracker.GetAllUnitList();
				
			for(int i=0; i<unitList.Count; i++){
				//clone the attack stats so that the original value wont get modified when making further calculation
				AttackInstance aInstance=new AttackInstance();
				aInstance.aStats=aStats.Clone();
				unitList[i].ApplyAttack(aInstance);
			}
		}
		
		
		//apply effect to all hostile unit surrounding the player
		void ApplyEffectAOE(Collider playerCollider){
			float aoeRadius=aStats.aoeRadius;
			
			if(aoeRadius>0){
				Collider[] cols=Physics.OverlapSphere(transform.position, aoeRadius);	//get all the collider in range
				for(int i=0; i<cols.Length; i++){
					if(cols[i]==playerCollider) continue;
					
					//clone the attack stats so that the original value wont get modified when making further calculation
					AttackInstance aInstance=new AttackInstance(null, aStats);
					aInstance.isAOE=true;
					aInstance.aoeDistance=Vector3.Distance(transform.position, cols[i].transform.position);
					
					Unit unitInstance=cols[i].gameObject.GetComponent<Unit>();
					if(unitInstance!=null) unitInstance.ApplyAttack(aInstance);
				}
			}
			
			//since this is aoe effect, explosion force applies
			TDSPhysics.ApplyExplosionForce(transform.position, aStats, true);
		}
		
		
		//apply the effect to player
		void ApplyEffectSelf(UnitPlayer player){
			
			//gain life
			if(life>0) GameControl.GainLife();
			
			//gain hit-point
			if(hitPoint>0){
				float hitPointGained=player.GainHitPoint(hitPoint);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+"+hitPointGained.ToString("f0"), new Color(0.3f, 1f, 0.3f, 1));
			}
			
			//gain energy
			if(energy>0){
				float energyGained=player.GainEnergy(energy);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+"+energyGained.ToString("f0"), new Color(.3f, .3f, 1f, 1));
			}
			
			//not in used
			if(credit>0){
				GameControl.GainCredits(credit);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+$"+credit.ToString("f0"), new Color(.5f, .75f, 1, 1));
			}
			
			//gain score
			if(score>0){
				GameControl.GainScore(score);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+"+score.ToString("f0"), new Color(.1f, 1f, 1, 1));
			}
			
			//gain ammo
			if(ammo!=0){
				player.GainAmmo(ammoID, ammo);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+ammo");
			}
			
			//gain exp
			if(exp!=0){
				player.GainExp(exp);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+exp", new Color(1f, 1f, 1, 1));
			}
			
			//gain perk currency
			if(perkCurrency!=0){
				player.GainPerkCurrency(perkCurrency);
				
				Vector3 offsetPos=new Vector3(0, Random.value+0.5f, 0);
				new TextOverlay(transform.position+offsetPos, "+perk points", new Color(1f, 1f, 1, 1));
			}
			
			
			//effects
			if(effectIdx>=0) player.ApplyEffect(EffectDB.CloneItem(effectIdx));
			//if(effect!=null && effect.duration>0) player.ApplyEffect(effect);
			
			//gain weapon
			if(gainWeapon && weaponList.Count>0){
				int playerWeaponID=player.weaponList[player.weaponID].ID;
				int rand=randomWeapon ? Random.Range(0, weaponList.Count) : 0 ;
				if(randomWeapon && weaponList.Count>1){
					int count=0;
					while(weaponList[rand].ID==playerWeaponID){
						rand=Random.Range(0, weaponList.Count);
						count+=1;
						if(count>50) break;
					}
				}
				
				bool replaceWeapon=weaponType==_WeaponType.Replacement;
				bool temporaryWeapon=weaponType==_WeaponType.Temporary;
				player.AddWeapon(weaponList[rand], replaceWeapon, temporaryWeapon, tempWeapDuration);
			}
		}
		
		
		//spawn the trigger effect
		void TriggeredEffect(Vector3 pos){
			if(triggerEffectObj==null) return;
			
			if(!autoDestroyEffectObj) ObjectPoolManager.Spawn(triggerEffectObj, pos, Quaternion.identity);
			else ObjectPoolManager.Spawn(triggerEffectObj, pos, Quaternion.identity, effectObjActiveDuration);
		}
		
		
		//the callback functions when the collectible is triggered
		public delegate void TriggerCallback(Collectible clt);
		private List<TriggerCallback> triggerCallbackList=new List<TriggerCallback>();
		public void SetTriggerCallback(TriggerCallback callback){ triggerCallbackList.Add(callback); }
		
	}

}