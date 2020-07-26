using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public class Weapon : MonoBehaviour{
		[HideInInspector] public int ID=0;
		public Sprite icon;
		public string weaponName="Weapon";
		public string desp="";
		
		[Space(10)]
		public GameObject shootObject;
		public List<Transform> shootPointList=new List<Transform>();
		public float shootPointDelay=0.05f;
		
		[Space(10)]
		public bool continousFire=true;
		
		[Header("Base Stats")]
		public float range=20;
		public float cooldown=0.15f;
		[HideInInspector] public float currentCD=0.25f;
		//public float GetCurrentCD(){ return currentCD; }
		
		public int clipSize=30;
		public int currentClip=30;
		
		public int ammoCap=300;
		public int ammo=300;
		
		public float reloadDuration=2;
		[HideInInspector] public float currentReload=0;
		//public float GetCurrentReload(){ return currentReload; }
		
		public float recoilMagnitude=.2f;
		[HideInInspector] public float recoil=1;
		
		public float recoilCamShake=0;
		
		public int spread=0;
		public float spreadAngle=15;
		
		public AttackStats aStats=new AttackStats();
		public AttackStats CloneAttackStats(){ return aStats.Clone(); }
		public AttackStats GetRuntimeAttackStats(){ return ModifyAttackStatsToPerk(aStats.Clone()); }
		
		[Header("Audio")]
		public AudioClip shootSFX;
		public AudioClip reloadSFX;
		
		
		[HideInInspector] public bool temporary=false; 		//used for weapon that is collected via collectible, cannot switch weapon and discard when out of ammo
		
		
		void Awake(){
			//make sure none of the element in shootPointList is null
			for(int i=0; i<shootPointList.Count; i++){
				if(shootPointList[i]==null){
					shootPointList.RemoveAt(i);	i-=1;
				}
			}
			
			//if no shoot point has been assigned, use the current transform
			if(shootPointList.Count==0) shootPointList.Add(transform);
			
			if(shootObject!=null) InitShootObject();
			
			aStats.Init();	//initiate the attack stats
			
			//create the ability
			ability=AbilityDB.CloneItem(abilityID);
			if(ability!=null) ability.Init();
			
			if(shootObject!=null) ObjectPoolManager.New(shootObject, 3);
			else Debug.LogWarning("shoot object for weapon unassigned", this);
			
			Reset();
		}
		//init certain parameter based on shoot object
		public void InitShootObject(){
			ShootObject so=shootObject.GetComponent<ShootObject>();
			requireAiming=(so.type==_SOType.Homing || so.type==_SOType.Point);
			randCursorForRecoil=so.type==_SOType.Point;
		}
		
		//stop reloading when the weapon is disabled (when it's deselect, the weapon gameObject will be deactivated)
		void OnDisable(){ reloading=false; }
		
		//reset the weapon, called when the weapon is first used or first collected
		public void Reset(){
			currentClip=GetClipSize();
			ammo=GetAmmoCap();
			currentCD=0;
		}
		
		//function call to fire the weapon
		public void Fire(){
			currentCD=GetCoolDown();
			recoil+=GetRecoilMagnitude()*2;
			
			AudioManager.PlaySound(shootSFX);
			
			//for weapon with finite clip
			if(currentClip>0){
				currentClip-=1;
				if(currentClip<=0){	//if out of ammo
					//if this is a temporary weapon, remove the weapon
					if(temporary){
						GameControl.GetPlayer().RemoveWeapon();
						return;
					}
					
					//if auto reload is enabled, reload
					if(GameControl.EnableAutoReload()) Reload();
				}
			}
		}
		
		
		private bool reloading=false;
		
		public bool Reload(){ 
			if(ammo==0) return false;					//if out of ammo, dont continue
			if(reloading) return false;						//if reloading is in process, dont continue
			if(currentClip==GetClipSize()) return false;	//if clip is full, dont continue
			
			StartCoroutine(ReloadRoutine());			//start reloading coroutine
			TDS.Reloading();									//fire the reload event
			
			AudioManager.PlaySound(reloadSFX);		//play the sound
			
			return true;
		}
		IEnumerator ReloadRoutine(){
			reloading=true;	//to mark the we are reloading
			currentReload=0;	//time counter for reloading
			
			while(currentReload<GetReloadDuration()){
				currentReload+=Time.deltaTime;
				yield return null;
			}
			
			//refill the weapon clip and reduce the ammo count accordingly
			currentClip=ammo==-1 ? GetClipSize() : Mathf.Min(GetClipSize(), ammo);
			if(ammo>0) ammo=Mathf.Max(ammo-GetClipSize(), 0);
			
			reloading=false;
		}
		
		public bool Reloading(){ return reloading; }
		public bool OnCoolDown(){ return currentCD>0 ? true : false ; }
		public bool OutOfAmmo(){ return currentClip==0 ? true: false ; }
		
		
		//set ammo to full
		public void FullAmmo(){ 
			ammo=GetAmmoCap();
		}
		//gain a set amount of ammo
		public int GainAmmo(int value){
			int limit=GetAmmoCap()-ammo;
			ammo+=(int)Mathf.Min(value, limit);
			return limit;
		}
		
		
		void Update(){
			currentCD-=Time.deltaTime;				//reduce the cooldown (weapon can only be fired when currentCD<0)
			recoil=recoil*(1-Time.deltaTime*3);	//reduce the recoil
			
			if(ability!=null) ability.currentCD-=Time.deltaTime;	//reduce the ability cooldown
		}
		
		
		
		public int abilityID=-1;		//the ability ID, used in editor and correspoding to the ID of the abilities in DB
		[HideInInspector] public Ability ability;	//the actual ability, only assigned in runtime
		
		//fire alternate mode
		public void FireAlt(){
			//if there's no ability, return
			if(ability==null || abilityID<0) return;
			
			//check if the ability is ready to be activated
			string status=ability.IsReady();
			if(status!=""){
				//if cannot fire, fire event explaining why (for UI)
				TDS.FireAltFail(status);
				return;
			}
			
			//launch the ability
			AbilityManager.LaunchAbility(ability);
		}
		
		
		private bool requireAiming=false;	//set to true if the shoot-object used require aiming (missile or point)
		public bool RequireAiming(){ return requireAiming; }
		
		public bool randCursorForRecoil=false;
		public bool RandCursorForRecoil(){ return randCursorForRecoil; }
		
		
		
		
		
		
		private PlayerPerk perk;
		public void SetPlayerPerk(PlayerPerk pPerk){ perk=pPerk; }
		
		public float GetRange(){ return range*(1+(perk!=null ? perk.GetWeaponRangeMul(ID) : 0)); }
		public float GetCoolDown(){ return cooldown*(1+(perk!=null ? perk.GetWeaponCDMul(ID) : 0)); }
		public int GetClipSize(){ return (int)(clipSize*(1+(perk!=null ? perk.GetWeaponClipSizeMul(ID) : 0))); }
		public int GetAmmoCap(){ return (int)(ammoCap*(1+(perk!=null ? perk.GetWeaponAmmoCapMul(ID) : 0))); }
		public float GetReloadDuration(){ return reloadDuration*(1+(perk!=null ? perk.GetWeaponReloadDurMul(ID) : 0)); }
		public float GetRecoilMagnitude(){ return recoilMagnitude*(1+(perk!=null ? perk.GetWeaponRecoilMagMul(ID) : 0)); }
		
		public AttackStats ModifyAttackStatsToPerk(AttackStats aStats){	//
			if(perk==null) return aStats;
			
			aStats.damageMin*=(1+perk.GetWeaponDamageMul(ID));
			aStats.damageMax*=(1+perk.GetWeaponDamageMul(ID));
			
			aStats.critChance*=(1+perk.GetWeaponCritMul(ID));
			aStats.critMultiplier*=(1+perk.GetWeaponCritMulMul(ID));
			
			aStats.aoeRadius*=(1+perk.GetWeaponAOEMul(ID));
			
			return aStats;
		}
		
		public void ChangeEffect(int newID, int newIdx){ 	//for perk
			aStats.effectID=newID;
			aStats.effectIdx=newIdx;
		}
		
		public void ChangeAbility(int newID){
			ability=AbilityDB.CloneItem(abilityID);
		}
		
	}
	

}