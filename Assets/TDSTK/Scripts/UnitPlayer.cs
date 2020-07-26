using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _MovementMode{Rigid, FreeForm}
	public enum _TurretAimMode{ScreenSpace, Raycast}
	
	public class UnitPlayer : Unit {
		
		public override UnitPlayer GetUnitPlayer(){ return this; } 
		
		private Camera cam;
		private Transform camT;
		
		private Transform turretObjParent;
		
		[Header("Weapon Info")]
		public Transform weaponMountPoint;
		
		public bool enableAllWeapons=true;
		public List<Weapon> weaponList=new List<Weapon>();
		public int weaponID=0;
		[HideInInspector] public bool weaponInitiated=false;	//for respawning unit
		
		
		[Header("Abilities")]
		public bool enableAbility=true;
		public bool enableAllAbilities=true;
		public List<int> abilityIDList=new List<int>();
		//[HideInInspector] public List<Ability> abilityList=new List<Ability>();
		
		public bool enableTurretRotate=true;
		public _TurretAimMode turretAimMode=_TurretAimMode.ScreenSpace;
		
		public bool faceTravelDirection=true;
		public bool enabledMovementX=true;
		public bool enabledMovementZ=true;
		
		public _MovementMode movementMode=_MovementMode.FreeForm;
		public float acceleration=3;
		public float decceleration=1;
		public float boostEnergyRate=0.2f;
		public float boostMultiplier=2f;
		public float activeBrakingRate=1;
		private Vector3 velocity;
		private float momentum;
		
		public bool useLimit=false;
		public bool showGizmo=true;
		public float minPosX=-Mathf.Infinity;
		public float minPosZ=-Mathf.Infinity;
		public float maxPosX=Mathf.Infinity;
		public float maxPosZ=Mathf.Infinity;
		
		
		
		// Use this for initialization
		public override void Awake() {
			base.Awake();
			
			thisObj.layer=TDS.GetLayerPlayer();
			
			isPlayer=true;
			
			SetDestroyCallback(this.PlayerDestroyCallback);
			
			if(enableAllWeapons) weaponList=new List<Weapon>( WeaponDB.Load() );
			
			if(weaponMountPoint==null) weaponMountPoint=thisT;
			
			if(turretObj!=null) turretObjParent=turretObj.parent;
			else{
				turretObj=thisT;
				faceTravelDirection=false;
			}
			
			hitPointBase=hitPointFull;
			energyBase=energyFull;
			
			progress=thisObj.GetComponent<PlayerProgression>();
			if(progress!=null) progress.SetPlayer(this);
			
			perk=thisObj.GetComponent<PlayerPerk>();
			if(perk!=null) perk.SetPlayer(this);
			
			Load();
		}
		
		public override void Start(){
			cam=Camera.main;
			camT=cam.transform;
			//camPivot=cam.transform.parent;
			
			if(enableAbility && GameControl.EnableAbility()) 
				AbilityManager.SetupAbility(abilityIDList, enableAllAbilities);
			
			if(perk!=null){
				List<Ability> abList=AbilityManager.GetAbilityList();
				for(int i=0; i<abList.Count; i++) abList[i].SetPlayerPerk(perk);
			}
			
			Init();
		}
		
		
		private bool init=false;
		void Init(){
			if(init) return;
			init=true;
			
			//AbilityManager.SetupAbility(abilityList);
			
			if(!weaponInitiated){
				weaponInitiated=true;
				for(int i=0; i<weaponList.Count; i++){
					GameObject obj=MountWeapon((GameObject)Instantiate(weaponList[i].gameObject));
					weaponList[i]=obj.GetComponent<Weapon>();
					weaponList[i].Reset();
					if(i>0) obj.SetActive(false);
					
					if(perk!=null) weaponList[i].SetPlayerPerk(perk);
				}
			
				if(weaponList.Count==0){
					GameObject obj=MountWeapon(null, "defaultObject");
					weaponList.Add(obj.AddComponent<Weapon>());
					weaponList[0].aStats.damageMax=2;
					weaponList[0].Reset();
					
					obj=MountWeapon(GameObject.CreatePrimitive(PrimitiveType.Sphere), "defaultShootObject");
					obj.AddComponent<ShootObject>();
					obj.SetActive(false);
					obj.transform.localScale=new Vector3(.2f, .2f, .2f);
					
					GameObject soObj=(GameObject)Instantiate(Resources.Load("Prefab_TDSTK/DefaultShootObject", typeof(GameObject)));
					soObj.SetActive(false);
					
					weaponList[0].shootObject=soObj;
					weaponList[0].InitShootObject();
				}
			}
			
			TDS.SwitchWeapon(weaponList[weaponID]);
		}
		
		
		private GameObject MountWeapon(GameObject obj, string objName=""){
			if(obj==null) obj=new GameObject();
			obj.transform.parent=weaponMountPoint;
			obj.transform.localPosition=Vector3.zero;
			obj.transform.localRotation=Quaternion.identity;
			if(objName!="") obj.name=objName;
			
			return obj;
		}
		
		private int tempWeapReturnID=0;	//a cache to store the default weaponID when using temporary weapon
		public void AddWeapon(Weapon prefab, bool replaceWeapon=false, bool temporary=false, float temporaryTimer=30){
			GameObject obj=MountWeapon((GameObject)Instantiate(prefab.gameObject));
			
			//replace weapon and temporary are mutually exclusive
			if(replaceWeapon){
				Destroy(weaponList[weaponID].gameObject);
				weaponList[weaponID]=obj.GetComponent<Weapon>();
				
				TDS.NewWeapon(weaponList[weaponID], weaponID);
			}
			else if(temporary){
				tempWeapReturnID=weaponID;
				weaponList[weaponID].gameObject.SetActive(false);
				
				if(weaponList[weaponID].temporary) RemoveWeapon();
				
				weaponID=weaponList.Count;
				weaponList.Add(obj.GetComponent<Weapon>());
				
				weaponList[weaponList.Count-1].temporary=true;
				if(temporaryTimer>0) StartCoroutine(TemporaryWeaponTimer(weaponList[weaponList.Count-1], temporaryTimer));
			}
			else{
				weaponID=weaponList.Count;
				weaponList.Add(obj.GetComponent<Weapon>());
				TDS.NewWeapon(weaponList[weaponID]);
			}
			
			weaponList[weaponID].Reset();
			
			TDS.SwitchWeapon(weaponList[weaponID]);
		}
		public void RemoveWeapon(){	//used to remove temporary weapon only
			if(weaponList.Count<=1) return;
			
			if(weaponList[weaponID]!=null) Destroy(weaponList[weaponID].gameObject);
			
			weaponList.RemoveAt(weaponID);
			weaponID=tempWeapReturnID;
			
			weaponList[weaponID].gameObject.SetActive(true);
			TDS.SwitchWeapon(weaponList[weaponID]);
		}
		
		IEnumerator TemporaryWeaponTimer(Weapon weapon, float time){
			Effect effect=new Effect();
			effect.ID=999;
			effect.duration=time;
			effect.icon=weapon.icon;
			ApplyEffect(effect);
			
			while(effect.duration>0 && weapon!=null) yield return null;
			
			effect.duration=-1;
			if(weapon!=null) RemoveWeapon();
		}
		
		
		
		
		public void SwitchWeapon(int newID){
			weaponList[weaponID].gameObject.SetActive(false);
			weaponList[newID].gameObject.SetActive(true);
			
			if(weaponList[newID].currentClip==0 && GameControl.EnableAutoReload()){
				weaponList[newID].Reload();
			}
			
			TDS.SwitchWeapon(weaponList[newID]);
			
			weaponID=newID;
		}
		
		
		
		//***********************************************************************
		//Input command
		
		//switch to next/prev weapon in the list
		public void ScrollWeapon(int scrollDir){
			if(destroyed) return;
			if(weaponList[weaponID].temporary) return;
			
			int newID=weaponID+scrollDir;
			if(newID>=weaponList.Count) newID=0;
			else if(newID<0) newID=weaponList.Count-1;
				
			if(newID!=weaponID) SwitchWeapon(newID);
		}
		
		//turret facing
		public void AimTurretMouse(Vector3 mousePos){
			if(destroyed || IsStunned()) return;
			
			if(!enableTurretRotate || turretObj==null) return;
			
			if(turretAimMode==_TurretAimMode.ScreenSpace){
				//get camera direction and mouse direction with repect to the player position on screen
				Vector3 camV=Quaternion.Euler(0, camT.eulerAngles.y, 0) * Vector3.forward;
				Vector3 dir=(mousePos-Camera.main.WorldToScreenPoint(thisT.position)).normalized;
				dir=new Vector3(dir.x, 0, dir.y);
				
				float angleOffset=camT.eulerAngles.y;	//get the camera y-axis angle 
				float sign=dir.x>0 ? 1 : -1;					//get the angle direction
				
				Vector3 dirM=Quaternion.Euler(0, angleOffset, 0) * dir;	//rotate the dir for the camera angle, dir has to be vector3 in order to work
				
				Quaternion wantedRot=Quaternion.Euler(0, sign*Vector3.Angle(camV, dirM)+angleOffset, 0);
				if(!smoothTurretRotation) turretObj.rotation=wantedRot;
				else turretObj.rotation=Quaternion.Slerp(turretObj.rotation, wantedRot, Time.deltaTime*15);
			}
			else if(turretAimMode==_TurretAimMode.Raycast){
				LayerMask mask=1<<TDS.GetLayerTerrain();
				Ray ray = Camera.main.ScreenPointToRay(mousePos);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
					Vector3 point=new Vector3(hit.point.x, thisT.position.y, hit.point.z);
					
					Quaternion wantedRot=Quaternion.LookRotation(point - thisT.position);
					if(!smoothTurretRotation) turretObj.rotation=wantedRot;
					else turretObj.rotation=Quaternion.Slerp(turretObj.rotation, wantedRot, Time.deltaTime*15);
				}
			}
		}
		//for DPad (touch input)
		public void AimTurretDPad(Vector2 direction){
			if(destroyed || IsStunned()) return;
			
			direction=direction.normalized*100;
			
			Vector2 screenPos=Camera.main.WorldToScreenPoint(thisT.position);
			float x=screenPos.x+direction.x;
			float y=screenPos.y+direction.y;
			
			AimTurretMouse(new Vector2(x, y));
		}
		
		//fire main weapon
		public void FireWeapon(){
			if(destroyed || IsStunned()) return;
			if(!thisObj.activeInHierarchy) return;
			
			int fireState=CanFire();
			if(fireState==0){
				if(weaponList[weaponID].RequireAiming()){
					Vector2 cursorPos=Input.mousePosition;
					
					if(weaponList[weaponID].RandCursorForRecoil()){
						float recoil=GameControl.GetPlayer().GetRecoil()*4;
						cursorPos+=new Vector2(Random.value-0.5f, Random.value-0.5f)*recoil;
					}
					
					Ray ray = Camera.main.ScreenPointToRay(cursorPos);
					RaycastHit hit;
					//LayerMask mask=1<<TDS.GetLayerTerrain();
					//Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
					Physics.Raycast(ray, out hit, Mathf.Infinity);
					
					ShootObject.AimInfo aimInfo=new ShootObject.AimInfo(hit);
					
					StartCoroutine(ShootRoutine(aimInfo));
				}
				else StartCoroutine(ShootRoutine());
				
				weaponList[weaponID].Fire();
			}
			else{
				string text="";
				if(fireState==1) text="attack disabled";
				if(fireState==2) text="weapon's on cooldown";
				if(fireState==3) text="weapon's out of ammo";
				if(fireState==4) text="weapon's reloading";
				TDS.FireFail(text);
				
				if(GetCurrentClip()==0 && GameControl.EnableAutoReload()) weaponList[weaponID].Reload();
			}
		}
		//alt fire, could fire weapon alt-mode to launch selected ability
		public void FireAbility(){
			if(destroyed || IsStunned()) return;
			if(GameControl.EnableAltFire()){
				weaponList[weaponID].FireAlt();
			}
			if(!GameControl.EnableAltFire() && GameControl.EnableAbility()){
				AbilityManager.LaunchAbility();
			}
		}
		//launch ability
		public void FireAbilityAlt(){
			if(destroyed || IsStunned()) return;
			if(GameControl.EnableAltFire() && GameControl.EnableAbility())
				AbilityManager.LaunchAbility();
		}
		
		public void Reload(){
			if(destroyed || IsStunned()) return;
			weaponList[weaponID].Reload();
		}
		
		//move
		public void Move(Vector2 direction, bool boosting=false){
			if(destroyed || IsStunned()) return;

      direction=direction.normalized;
      // Debug.Log(direction);
      Vector3 dirV=Quaternion.Euler(0, camT.eulerAngles.y, 0) * Vector3.forward; //forward direction with respect to the cam, so just yaxis-rotation * (0, 0, 1)
			Vector3 dirH=Quaternion.Euler(0, 90, 0) * dirV;											//right direction, so rotate it further by 90 degree
			dirV=dirV*direction.y*(enabledMovementZ ? 1 : 0);
			dirH=dirH*direction.x*(enabledMovementX ? 1 : 0);
			Vector3 dirHV=(dirH+dirV).normalized;
			
			float boost=1;
			if(energy>0 && boosting){
				boost=boostMultiplier;
				energy-=Time.deltaTime*energyFull*boostEnergyRate;
			}
			
			if(movementMode==_MovementMode.FreeForm){
				velocity+=dirHV*0.025f*boost*(acceleration-velocity.magnitude);
			}
			else {
        thisT.Translate(dirHV * moveSpeed * Time.deltaTime * GetTotalSpeedMultiplier() * boost, Space.World);
        velocity = dirHV;
				moved=true;
			}
			
			//take the angle difference between the travel direction and +z direction to get the rotation
			if(faceTravelDirection && rotateSpeed>0 && turretObj!=thisT){
				if(turretObj!=null) turretObj.parent=null;
				
				float sign=dirHV.x>0 ? 1 : -1;	
				Quaternion wantedRot=Quaternion.Euler(0, sign*Vector3.Angle(Vector3.forward, dirHV), 0);
				thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*rotateSpeed);
				
				if(turretObj!=null && turretObjParent!=null) turretObj.parent=turretObjParent;
			}
		}
		
		public void Brake(){
			if(movementMode!=_MovementMode.FreeForm) return;
			velocity=Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime*activeBrakingRate*5);
		}
		
		//***********************************************************************
		
		
		public override void Update () {
			if(useLimit){
				float x=Mathf.Clamp(thisT.position.x, minPosX, maxPosX);
				float z=Mathf.Clamp(thisT.position.z, minPosZ, maxPosZ);
				thisT.position=new Vector3(x, thisT.position.y, z);
			}
			
			if(!GameControl.IsGamePlaying()) return;
			if(destroyed || IsStunned() || IsDashing()) return;
			
			base.Update();
			
			if(movementMode==_MovementMode.FreeForm){
				float brakeBoost=Input.GetKey(KeyCode.Space) ? 2 : 1 ;
				thisT.Translate(velocity * moveSpeed * Time.deltaTime * GetTotalSpeedMultiplier(), Space.World);
				velocity*=(1-Time.deltaTime*decceleration*velocity.magnitude*brakeBoost);
			}
		}
		
		
		//for animation
		void LateUpdate(){
			if(movementMode==_MovementMode.FreeForm) AnimationMove(velocity);
			else if(movementMode==_MovementMode.Rigid) AnimationMove(moved ? velocity : Vector3.zero);
			moved=false;
		}
		
		void AnimationMove(Vector3 dir){
			if(uAnimation==null) return;
			dir=Quaternion.Euler(0, -turretObj.rotation.eulerAngles.y, 0) * dir;
			uAnimation.Move(dir.z, dir.x);
		}
		
		
		
		//for camera dynamic zoom
		public float GetVelocity(){
			if(movementMode==_MovementMode.FreeForm) return velocity.magnitude*GetTotalSpeedMultiplier();
			return (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) ? moveSpeed*GetTotalSpeedMultiplier()*.15f : 0; 
		}
		
		
		
		//modify the attackStats to active effect
		public Effect levelModifier=new Effect();
		protected AttackStats ModifyAttackStatsToLevel(AttackStats aStats){
			if(progress==null) return aStats;
			
			float dmgMul=GetDamageMultiplier();
			aStats.damageMin*=dmgMul;
			aStats.damageMax*=dmgMul;
			
			aStats.critChance*=GetCritChanceMultiplier();
			aStats.critMultiplier*=GetCritMulMultiplier();
			
			return aStats;
		}
		
		
		
		IEnumerator ShootRoutine(ShootObject.AimInfo aimInfo=null){
			if(uAnimation!=null) uAnimation.AttackRange();
			
			AttackStats aStats=ModifyAttackStatsToLevel(weaponList[weaponID].GetRuntimeAttackStats());
			aStats=ModifyAttackStatsToExistingEffect(aStats);
			//aStats=ModifyAttackStatsToExistingEffect(weaponList[weaponID].GetRuntimeAttackStats());
			AttackInstance aInstance=new AttackInstance(this, aStats);
			
			int weapID=weaponID;	//to prevent weapon switch and state change while delay and firing multiple so
			
			int spread=weaponList[weapID].spread;
			if(spread>1){
				aInstance.aStats.damageMin/=spread;
				aInstance.aStats.damageMax/=spread;
			}
			
			float startAngle=spread>1 ? -weaponList[weapID].spreadAngle/2f : 0 ;
			float angleDelta=spread>1 ? weaponList[weapID].spreadAngle/(spread-1) : 0 ;
			
			List<Collider> soColliderList=new List<Collider>();	//colliders of all the so fired, used to tell each so to ignore each other
			
			for(int i=0; i<weaponList[weapID].shootPointList.Count; i++){
				Transform shootPoint=weaponList[weapID].shootPointList[i];
				
				float recoilSign=(Random.value<recoilSignTH ? -1 : 1);
				recoilSignTH=Mathf.Clamp(recoilSignTH+(recoilSign>0 ? 0.25f : -0.25f), 0, 1);
				float recoilValue=recoilSign*Random.Range(0.1f, 1f)*GetRecoil();
				Quaternion baseShootRot=shootPoint.rotation*Quaternion.Euler(0, recoilValue, 0);
				
				for(int m=0; m<Mathf.Max(1, spread); m++){
					Vector3 shootPos=shootPoint.position;
					if(spread>1) shootPos=shootPoint.TransformPoint(new Vector3(0, 0, Random.Range(-1.5f, 1.5f)));
					Quaternion shootRot=baseShootRot*Quaternion.Euler(0, startAngle+(m*angleDelta), 0);
					
					//GameObject soObj=(GameObject)Instantiate(weaponList[weapID].shootObject, shootPos, shootRot);
					GameObject soObj=ObjectPoolManager.Spawn(weaponList[weapID].shootObject, shootPos, shootRot);
					ShootObject soInstance=soObj.GetComponent<ShootObject>();
					
					soInstance.IgnoreCollider(GetCollider());
					for(int n=0; n<soColliderList.Count; n++) soInstance.IgnoreCollider(soColliderList[n]);
					if(soInstance.GetCollider()!=null) soColliderList.Add(soInstance.GetCollider());
					
					soInstance.Shoot(thisObj.layer, GetRange(), shootPoint, aInstance.Clone(), aimInfo);
					//soInstance.Shoot(thisObj.layer, GetRange(), shootPoint, aInstance.Clone(), hit);
				}
				
				TDS.CameraShake(weaponList[weapID].recoilCamShake);
				
				if(weaponList[weapID].shootPointDelay>0) yield return new WaitForSeconds(weaponList[weapID].shootPointDelay);
				
				if(weapID>=weaponList.Count) break;
			}
			
		}
		
		
		
		
		
		private bool disableFire=false;	//for whatever reason some external component need to stop player from firing
		public void DisableFire(){ disableFire=true; }
		public void EnableFire(){ disableFire=false; }
		
		public int CanFire(){
			if(disableFire) return 1;
			if(weaponList[weaponID].OnCoolDown()) return 2;
			if(weaponList[weaponID].OutOfAmmo()) return 3;
			if(weaponList[weaponID].Reloading()) return 4;
			return 0;
		}
		
		public bool ContinousFire(){ return GameControl.EnableContinousFire() & weaponList[weaponID].continousFire; }
		
		public override float GetRange(){ return weaponList[weaponID].GetRange(); }
		
		private float recoilSignTH=0.5f;
		public float GetRecoil(){ return weaponList[weaponID].GetRecoilMagnitude(); }
		
		public bool Reloading(){ return weaponList[weaponID].Reloading(); }
		public float GetReloadDuration(){ return weaponList[weaponID].GetReloadDuration(); }
		public float GetCurrentReload(){ return weaponList[weaponID].currentReload; }
		public int GetCurrentClip(){ return weaponList[weaponID].currentClip; }
		public int GetAmmo(){ return weaponList[weaponID].ammo; }
		
		public int GetCurrentWeaponIndex(){ return weaponID; }
		public Ability GetWeaponAbility(){ return weaponList[weaponID].ability; }
		
		
		
		public void GainAmmo(int idx, int value){
			if(idx>=weaponList.Count){
				Debug.LogWarning("collectible ammo info not matched");
				return;
			}
			
			if(idx<0){
				if(value==-1){
					for(int i=0; i<weaponList.Count; i++){
						weaponList[i].FullAmmo();
					}
				}
				else{
					for(int i=0; i<weaponList.Count; i++){
						weaponList[i].GainAmmo(value);
					}
				}
				
				return;
			}
			
			for(int i=0; i<weaponList.Count; i++){
				if(weaponList[i].ID==idx){
					if(value<0) weaponList[i].FullAmmo();
					if(value>0) weaponList[i].GainAmmo(value);
				}
			}
		
		}
		
		public override bool ApplyEffect(Effect effect){
			if(!base.ApplyEffect(effect)) return false;
			TDS.GainEffect(effect);	//for UIBuffIcons
			return true;
		}
		
		
		public void PlayerDestroyCallback(){
			if(weaponList[weaponID].temporary) RemoveWeapon();
			GameControl.PlayerDestroyed();
		}
		
		
		
		
		
		[HideInInspector] public int playerID=0;	//for saving
		public bool loadProgress=false;
		public bool saveProgress=false;
		
		public bool saveUponChange=false;
		public bool SaveUponChange(){ return saveProgress & saveUponChange; }
		
		public void Save(){
			if(!saveProgress) return;
			if(progress!=null) progress.Save();
			if(perk!=null) perk.Save();
		}
		public void Load(){
			if(!loadProgress) return;
			if(progress!=null) progress.Load();
			if(perk!=null) perk.Load();
		}
		public void DeleteSave(){
			if(progress==null) progress=gameObject.GetComponent<PlayerProgression>();
			if(perk==null) perk=gameObject.GetComponent<PlayerPerk>();
			
			if(progress!=null) progress.DeleteSave();
			if(perk!=null) perk.DeleteSave();
		}
		
		
		
		
		[HideInInspector] public PlayerProgression progress;
		public PlayerProgression GetPlayerProgression(){ return progress; }
		[HideInInspector] public PlayerPerk perk;
		public PlayerPerk GetPlayerPerk(){ return perk; }
		
		public int GetLevel(){ return progress!=null ? progress.GetLevel() : level ; }
		public int GetPerkCurrency(){ return perk!=null ? perk.GetPerkCurrency() : 0 ; }
		public int GetPerkPoint(){ return perk!=null ? perk.GetPerkPoint() : 0 ; }
		public List<Perk> GetPerkList(){ return perk!=null ? perk.GetPerkList() : new List<Perk>() ; }
		
		private float hitPointBase=0;
		private float energyBase=0;
		public float GetBaseHitPoint(){ return hitPointBase; }
		public float GetBaseEnergy(){ return energyBase; }
		
		//void OnGUI(){
		//	GUI.Label(new Rect(50, 300, 200, 30), ""+GetFullHitPoint()+"    "+GetPerkHitPoint());
		//	GUI.Label(new Rect(50, 320, 200, 30), ""+(1+GetLevelDamageMul()+GetPerkDamageMul()));
		//}
		
		public override float GetFullHitPoint(){ return hitPointBase+GetLevelHitPoint()+GetPerkHitPoint(); }
		public override float GetFullEnergy(){ return energyBase+GetLevelEnergy()+GetPerkEnergy(); }
		public override float GetHitPointRegen(){ return hpRegenRate+GetLevelHitPointRegen()+GetPerkHitPointRegen(); }
		public override float GetEnergyRegen(){ return energyRate+GetLevelEnergyRegen()+GetPerkEnergyRegen(); }
		
		public float GetDamageMultiplier(){ return 1+GetLevelDamageMul()+GetPerkDamageMul(); }
		public float GetCritChanceMultiplier(){ return 1+GetLevelCritMul()+GetPerkCritMul(); }
		public float GetCritMulMultiplier(){ return 1+GetLevelCritMulMul()+GetPerkCritMulMul(); }
		public override float GetSpeedMultiplier(){ return 1+GetLevelSpeedMul()+GetPerkSpeedMul(); }
		
		
		public float GetEffSpeedMultiplier(){ return activeEffect.speedMul; }
		public float GetTotalSpeedMultiplier(){ return GetEffSpeedMultiplier()*GetSpeedMultiplier(); }
		
		
		
		public float GetLevelHitPoint(){ return progress!=null ? progress.GetHitPointGain() : 0 ; }
		public float GetLevelEnergy(){ return progress!=null ? progress.GetEnergyGain() : 0 ; }
		public float GetLevelHitPointRegen(){ return progress!=null ? progress.GetHitPointRegenGain() : 0 ; }
		public float GetLevelEnergyRegen(){ return progress!=null ? progress.GetEnergyRegenGain() : 0 ; }
		
		public float GetLevelDamageMul(){ return progress!=null ? progress.GetDamageMulGain() : 0 ; }
		public float GetLevelCritMul(){ return progress!=null ? progress.GetCritChanceMulGain() : 0 ; }
		public float GetLevelCritMulMul(){ return progress!=null ? progress.GetCritMultiplierMulGain() : 0 ; }
		public float GetLevelSpeedMul(){ return progress!=null ? progress.GetSpeedMulGain() : 0 ; }
		
		
		
		public float GetPerkHitPoint(){ return perk!=null ? perk.GetBonusHitPoint() : 0 ; }
		public float GetPerkEnergy(){ return perk!=null ? perk.GetBonusEnergy() : 0 ; }
		public float GetPerkHitPointRegen(){ return perk!=null ? perk.GetBonusHitPointRegen() : 0 ; }
		public float GetPerkEnergyRegen(){ return perk!=null ? perk.GetBonusEnergyRegen() : 0 ; }
		
		public float GetPerkSpeedMul(){ return perk!=null ? perk.GetMoveSpeedMul() : 0 ; }
		
		public float GetPerkDamageMul(){ return perk!=null ? perk.GetDamageMul() : 0 ; }
		public float GetPerkCritMul(){ return perk!=null ? perk.GetCritMul() : 0 ; }
		public float GetPerkCritMulMul(){ return perk!=null ? perk.GetCirtMulMul() : 0 ; }
		
		
		//for perk that modify the weapon attack effect
		public void ChangeAllWeaponEffect(int effectID){
			int effectIndex=EffectDB.GetEffectIndex(effectID);
			for(int i=0; i<weaponList.Count; i++) weaponList[i].ChangeEffect(effectID, effectIndex);
		}
		public void ChangeWeaponEffect(int weaponID, int effectID){
			int effectIndex=EffectDB.GetEffectIndex(effectID);
			for(int i=0; i<weaponList.Count; i++){
				if(weaponList[i].ID==weaponID){
					weaponList[i].ChangeEffect(effectID, effectIndex);
					break;
				}
			}
		}
		
		//for perk that modify the weapon ability
		public void ChangeAllWeaponAbility(int abilityID){
			for(int i=0; i<weaponList.Count; i++) weaponList[i].ChangeAbility(abilityID);
		}
		public void ChangeWeaponAbility(int weaponID, int abilityID){
			for(int i=0; i<weaponList.Count; i++){
				if(weaponList[i].ID==weaponID){
					weaponList[i].ChangeAbility(abilityID);
					break;
				}
			}
		}
		
		//for perk that modify the ability attack effect
		public void ChangeAllAbilityEffect(int effectID){
			int effectIndex=EffectDB.GetEffectIndex(effectID);
			List<Ability> abList=AbilityManager.GetAbilityList();
			for(int i=0; i<abList.Count; i++) abList[i].ChangeEffect(effectID, effectIndex);
		}
		public void ChangeAbilityEffect(int abilityID, int effectID){
			int effectIndex=EffectDB.GetEffectIndex(effectID);
			List<Ability> abList=AbilityManager.GetAbilityList();
			for(int i=0; i<abList.Count; i++){
				if(abList[i].ID==abilityID) abList[i].ChangeEffect(effectID, effectIndex);
			}
		}
		
		
		
		private bool dashing=false;
		public bool IsDashing(){ return dashing; }
		public void Dash(float range, float dur){ StartCoroutine(_Dash(range, dur)); }
		public IEnumerator _Dash(float range, float dur){
			dashing=true;
			Vector3 startPos=thisT.position;
			Vector3 tgtPos=thisT.TransformPoint(Vector3.forward*range);
			float step=1f/dur;
			float duration=0;
			while(duration<1){
				thisT.position=Vector3.Lerp(startPos, tgtPos, duration);
				duration+=Time.deltaTime*step;
				yield return null;
			}
			thisT.position=tgtPos;
			dashing=false;
		}
		
		
		
		public float GetScoreMultiplier(){
			return perk!=null ? 1+perk.GetScoreGainMul() : 1;
		}
		
		public void GainPerkCurrency(int value){
			if(perk!=null && value>0) perk.GainCurrency(value);
		}
		
		public void GainExp(int val){
			if(progress==null) return;
			float multiplier=1+(perk!=null ? perk.GetExpGainMul() : 0);
			progress.GainExp((int)(val*multiplier));
		}
		
		public override float GainHitPoint(float value){
			float multiplier=1+(perk!=null ? perk.GetHitPointGainMul() : 0);
			return base.GainHitPoint(value*multiplier);
		}
		public override float GainEnergy(float value){
			float multiplier=1+(perk!=null ? perk.GetEnergyGainMul() : 0);
			return base.GainEnergy(value*multiplier);
		}
		
		
		
		
		void OnDrawGizmos(){
			if(useLimit && showGizmo){
				Vector3 p1=new Vector3(minPosX, transform.position.y, maxPosZ);
				Vector3 p2=new Vector3(maxPosX, transform.position.y, maxPosZ);
				Vector3 p3=new Vector3(maxPosX, transform.position.y, minPosZ);
				Vector3 p4=new Vector3(minPosX, transform.position.y, minPosZ);
				
				Gizmos.color=Color.blue;
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p3);
				Gizmos.DrawLine(p3, p4);
				Gizmos.DrawLine(p4, p1);
			}
		}
		
	}
	

}
