using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _SOType{
		Simple,
		Homing,
		Beam,
		Point,
	}

	public class ShootObject : MonoBehaviour {
		
		//a class given to all fired shootobject, contain the aimed target information 
		public class AimInfo{
			public Vector3 hitPoint;
			public Transform targetT;
			public Collider collider;
			public Unit unit;
			
			public AimInfo(Vector3 point){
				hitPoint=point;
			}
			public AimInfo(Unit tgt){			//for a specific target (used by AI unit)
				unit=tgt;
				hitPoint=unit.thisT.position;
				targetT=unit.thisT;
				collider=unit.GetCollider();
			}
			public AimInfo(RaycastHit hit){	//from the raycast from the cursor used to aim at the target
				hitPoint=hit.point;
				targetT=hit.transform;
				collider=hit.collider;
				unit=targetT!=null ? hit.transform.GetComponent<Unit>() : null;
			}
		}
		
		
		public _SOType type=_SOType.Simple;
		
		public enum _State{Idle, Shot, Hit}
		[HideInInspector] public _State state=_State.Idle;
		
		[HideInInspector] public Transform thisT;
		[HideInInspector] public GameObject thisObj;
		
		[HideInInspector] private float srcRange=50;  //effective range for projectile and beam, value is retrived from shooter
		
		[Header("Projectile")]
		[Tooltip("The travelling speed of the shoot object")]
		public float speed=15; 
		public float delayBeforeDestroy=0;
		private float travelledDistance=0;
		
		[Header("Homing")]
		public float trackingDuration=1f;
		public float spread=1f;
		private Unit targetUnit;
		private Vector3 targetPos;
		private Vector3 initialPos;
		private Vector3 dummyPos;
		private float initialDist;
		private float curveMod=0;
		private float timeSinceFire=0;
		
		[Header("Beam")]
		public LineRenderer line;
		public float beamDuration=0.2f;
		public bool continousEffect=true;
		private float lastHitCD=0;
		//not in used
		//[HideInInspector] private bool attachedToShootPoint=true; 
		//public void DontAttachToShootPoint(){ attachedToShootPoint=false; }	//for chain, called before Shoot() is called
		
		//[Tooltip("Check if the shoot object penetrate any target it hits and carry on\nOnly applies for simple and beam")]
		public bool shootThrough=false;
		
		[Header("Common Setting")]
		//[Tooltip("The size of the shoot object hit box. The higher the value, the easier it's to hit something")]
		public float hitRadius=0.25f;
		
		public float impactCamShake=0;
		
		public GameObject shootEffect;
		public bool destroyShootEffect=false;
		public float destroyShootDuration=1;
		
		public GameObject hitEffect;
		public bool destroyHitEffect=false;
		public float destroyHitDuration=1;
		
		
		//[HideInInspector] public Unit srcUnit;
		[HideInInspector] public AttackInstance attInstance;
		
		private float destroyTime=5;
		private float shootTime=0;
		
		//private Collider srcCollider;
		
		private int srcLayer;
		private LayerMask mask;
		
		//private TrailRenderer[] trailList=new TrailRenderer[0];
		
		void Awake(){
			Init();
			
			//trailList = thisObj.GetComponentsInChildren<TrailRenderer>(true);
		}
		
		void OnEnable(){
			//for(int i=0; i<trailList.Length; i++) trailList[i].Clear();
		}
		
		
		
		
		
		
		
		//initiate the shootobject
		private bool init=false;
		public void Init() {
			if(init) return;
			init=true;
			
			thisT=transform;
			thisObj=gameObject;
			thisObj.layer=TDS.GetLayerShootObject();
			
			if(type==_SOType.Simple || type==_SOType.Homing){
				//hitRadius=thisObj.GetComponent<SphereCollider>().radius;
				//destroyTime=100f/speed;
				InitProjectile();
			}
			else if(type==_SOType.Beam){
				speed=9999;
			}
			
			if(shootEffect!=null) ObjectPoolManager.New(shootEffect, 1);
			if(hitEffect!=null) ObjectPoolManager.New(hitEffect, 1);
		}
		
		
		private Collider thisCollider;
		public Collider GetCollider(){ return thisCollider; }
		//initiate the projectile
		public void InitProjectile(){
			//make sure the shootobject's own collider is correctly setup
			SphereCollider collider=gameObject.GetComponent<SphereCollider>();
			if(collider==null) collider=thisObj.AddComponent<SphereCollider>();
			collider.radius=hitRadius;
			collider.isTrigger=true;
			thisCollider=collider;
			
			//make sure the shootobject's rigidbody is correctly setup
			Rigidbody rb=gameObject.GetComponent<Rigidbody>();
			if(rb==null) rb=thisObj.AddComponent<Rigidbody>();
			rb.useGravity=false;
			rb.constraints=RigidbodyConstraints.FreezeAll;
		}
		//external function call asking the shootobject to ignore a specific collider
		public void IgnoreCollider(Collider collider){
			if(thisCollider!=null && collider.enabled){
				thisCollider.enabled=true;
				Physics.IgnoreCollision(collider, thisCollider, true);
			}
		}
		
		
		//function call to fire the object
		public void Shoot(int srcL, float srcR, Transform shootPoint, AttackInstance aInstance=null, AimInfo aimInfo=null){
			Init();
			
			thisObj.SetActive(true);
			
			//cached all the passed information lcoally
			attInstance=aInstance;
			srcLayer=srcL;	//the layer of the shooting unit (so we know it's from player or AI)
			srcRange=srcR;	//the range of the shooting unit (so we know when to stop)
			
			state=_State.Shot;
			shootTime=Time.time;	//to track how long the shoot object is has been
			travelledDistance=0;	//to track how far the shoot object is has been fired
			
			//if there's any hideObject, set it to true (it's probably set to false when the shoot-object last hit something)
			if(hideObject!=null) hideObject.SetActive(true);
			
			//instantiate the shoot effect
			ShootEffect(thisT.position, thisT.rotation);
			
			if(type==_SOType.Simple || type==_SOType.Homing){
				//if(aInstance!=null && thisCollider.enabled){
					//~ Physics.IgnoreCollision(aInstance.srcUnit.GetCollider(), thisObj.GetComponent<Collider>(), true);
					//Debug.Log("collision avoidance with shooter unresolved");
					//Physics.IgnoreCollision(srcCollider, thisCollider, true);
				//}
				
				// for homing shootobject, the shootobject needs to be aiming at some position, or a specific unit
				if(type==_SOType.Homing){
					if(aimInfo.targetT!=null) targetUnit=aimInfo.unit;
					
					targetPos=aimInfo.hitPoint+new Vector3(Random.value-0.5f, 0, Random.value-0.5f)*2*spread;
					initialPos=shootPoint.position;
					initialDist=Vector3.Distance(targetPos, initialPos);
					
					curveMod=Random.Range(0, 2f);
					dummyPos=thisT.TransformPoint(Vector3.forward*speed*5);
					dummyPos=(targetPos+dummyPos)/2;
				}
			}
			else if(type==_SOType.Beam){
				//if(attachedToShootPoint) transform.parent=shootPoint;
				thisT.parent=shootPoint;
				ObjectPoolManager.Unspawn(thisObj, beamDuration-.01f);
			}
			else if(type==_SOType.Point){
				StartCoroutine(PointRoutine(aimInfo));
			}
			
			//update the layermask used to do the hit detection in accordance to rules set in GameControl
			UpdateSphereCastMask(!GameControl.SOHitFriendly(), !GameControl.SOHitShootObject(), !GameControl.SOHitCollectible());
		}
		void UpdateSphereCastMask(bool ignoreFriendly, bool ignoreSO, bool ignoreCollectible){
			mask=~(1<<TDS.GetLayerTrigger());
			if(ignoreFriendly) 	mask&=~(1<<srcLayer);
			if(ignoreSO) 			mask&=~(1<<TDS.GetLayerShootObject());
			if(ignoreCollectible) 	mask&=~(1<<TDS.GetLayerCollectible());
		}
		
		
		
		
		
		// Update is called once per frame
		void Update () {
			//if the shootobject is not in the shot state, dont continue
			if(type!=_SOType.Beam && state!=_State.Shot) return;
			
			float travelDistance=GetRangePerFrame();	//get the distance the shootobject travelled in this frame
			
			//do a sphere-cast to limit the travel distance in this frame so we dont over-shoot anything the shootobject might hit
			RaycastHit hit;
			if(Physics.SphereCast(thisT.position, hitRadius, thisT.forward, out hit, travelDistance, mask)){
				travelDistance=Vector3.Distance(thisT.position, hit.point);
			}
			
			
			if(type==_SOType.Simple){
				travelledDistance+=travelDistance;
				thisT.Translate(Vector3.forward * travelDistance);
				if(Time.time-shootTime>destroyTime) Hit();
				if(travelledDistance>srcRange) OnTriggerEnter(null);
			}
			if(type==_SOType.Homing){
				if(targetUnit!=null){
					targetPos=targetUnit.thisT.position;
					
					Vector3 thisTempPos=thisT.position;
					thisTempPos.y=targetPos.y;
					Quaternion tgtRot=Quaternion.LookRotation(targetPos-thisTempPos);
					
					//if the target is too far off angle
					if(Mathf.Abs(thisT.rotation.eulerAngles.y-tgtRot.eulerAngles.y)>65){
						targetPos=thisT.TransformPoint(0, 0, speed);
						targetPos.y=targetUnit.thisT.position.y;
						targetUnit=null;
					}
					else if(Time.time-shootTime>trackingDuration){
						targetUnit=null;
						targetPos+=new Vector3(Random.value-0.5f, 0, Random.value-0.5f)*2*spread/2;
					}
				}
				
				float currentDist=Vector3.Distance(thisT.position, targetPos);
				
				float timeMultipler=(speed/initialDist)*0.125f;
				curveMod+=Time.deltaTime*timeMultipler;
				timeSinceFire+=Time.deltaTime*timeMultipler;
				
				dummyPos=Vector3.Lerp(dummyPos, targetPos, timeSinceFire);
				
				Quaternion wantedRot=Quaternion.LookRotation(dummyPos-thisT.position);
				thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*speed*(1.5f+curveMod));
				
				thisT.Translate(Vector3.forward * travelDistance);
				
				if(currentDist<hitRadius) OnTriggerEnter(null);
			}
			else if(type==_SOType.Beam){
				if(shootThrough) travelDistance=GetRangePerFrame();
				
				if(line!=null){
					line.SetPosition(0, new Vector3(0, 0, 0));
					line.SetPosition(1, new Vector3(0, 0, travelDistance));
				}
				
				if(!shootThrough){
					if(hit.collider!=null){
						if(Time.time-lastHitCD>0.2f){
							if(state==_State.Shot) HitEffect(hit.point);
							OnTriggerEnter(hit.collider);
							lastHitCD=Time.time;
						}
						if(continousEffect) state=_State.Shot;
					}
				}
				else{
					if(Time.time-lastHitCD>0.1f){
						lastHitCD=Time.time;
						RaycastHit[] hitList=Physics.SphereCastAll(thisT.position, hitRadius, thisT.forward, travelDistance);
						
						for(int i=0; i<hitList.Length; i++){
							if(state==_State.Shot) HitEffect(hitList[i].point);
							state=_State.Shot;
							if(hitList[i].collider!=null) OnTriggerEnter(hitList[i].collider);
						}
						
						if(continousEffect) state=_State.Shot;
					}
				}
			}
		}
		
		
		//calculate how far the shootobject might travel in the next frame
		private float GetRangePerFrame(){
			if(type==_SOType.Simple || type==_SOType.Homing) return Time.deltaTime * speed;
			else if(type==_SOType.Beam) return srcRange;
			return 0;
		}
		
		//for 'point' shootobject, which hits the target poins immediately (after a frame)
		IEnumerator PointRoutine(AimInfo aimInfo){
			yield return null;
			
			thisT.position=aimInfo.hitPoint;	//move the shootobject to hitpoint
			
			OnTriggerEnter(aimInfo.collider);
		}
		
		
		//called when shootobject hit something
		void OnTriggerEnter(Collider col){
			if(state==_State.Hit) return;	//if the shootobject has hit something before this, return
			
			//if the shootobject hits another shootobject from friendly unit
			if(!GameControl.SOHitFriendly() && col!=null){
				if(srcLayer==col.gameObject.layer) return;
			}
			
			//register the state of the shootobject as hit
			state=_State.Hit;
			
			TDS.CameraShake(impactCamShake);
			
			
			//when hit a shootObject
			if(col!=null && col.gameObject.layer==TDS.GetLayerShootObject()){
				//if this is a beam shootobject, inform the other shootobject that it has been hit (beam doesnt use a collider)
				if(type==_SOType.Beam) col.gameObject.GetComponent<ShootObject>().Hit();
			}
			
			
			//when hit a collectible, destroy the collectible
			if(col!=null && col.gameObject.layer==TDS.GetLayerCollectible()){
				ObjectPoolManager.Unspawn(col.gameObject);
				return;
			}
			
			
			//if there's a attack instance (means the shootobject has a valid attacking stats and all)
			if(attInstance!=null){
				float aoeRadius=attInstance.aStats.aoeRadius;
				
				//for area of effect hit
				if(aoeRadius>0){
					Unit unitInstance=null;
					
					//get all the potental target in range
					Collider[] cols=Physics.OverlapSphere(thisT.position, aoeRadius);
					for(int i=0; i<cols.Length; i++){
						//if the collider in question is the collider the shootobject hit in the first place, apply the full attack instance
						if(cols[i]==col){
							unitInstance=col.gameObject.GetComponent<Unit>();
							if(unitInstance!=null) unitInstance.ApplyAttack(attInstance.Clone());
							continue;
						}
						
						//no friendly fire, then skip if the target is a friendly unit
						if(!GameControl.SOHitFriendly()){
							if(cols[i].gameObject.layer==srcLayer) continue;
						}
						
						unitInstance=cols[i].gameObject.GetComponent<Unit>();
						
						//create a new attack instance and mark it as aoe attack, so diminishing aoe can be applied if enabled
						AttackInstance aInstance=attInstance.Clone();
						aInstance.isAOE=true;
						aInstance.aoeDistance=Vector3.Distance(thisT.position, cols[i].transform.position);
						
						//apply the attack
						if(unitInstance!=null) unitInstance.ApplyAttack(aInstance);
					}
				}
				else{
					if(col!=null){
						//get the unit and apply the attack
						Unit unitInstance=col.gameObject.GetComponent<Unit>();
						if(unitInstance!=null) unitInstance.ApplyAttack(attInstance);
					}
				}
				
				if(col!=null){
					//apply impact force to the hit object
					Vector3 impactDir=Quaternion.Euler(0, thisT.eulerAngles.y, 0)*Vector3.forward;
					TDSPhysics.ApplyAttackForce(thisT.position, impactDir, col.gameObject, attInstance.aStats);
				}
			}
			
			//add collider to the condition so the shootobject wont split if the shootObject didnt hit anything (it could have just reach the range limit)
			if(splitUponHit && col!=null) Split(col);	
			
			Hit();
		}
		
		
		//also get called (on the projectile instance) when a projectile being hit by a beam
		public void Hit(){
			if(type==_SOType.Simple || type==_SOType.Homing){
				HitEffect(thisT.position);
				if(!shootThrough) ProjectileHitDelay();
				else state=_State.Shot;
				
				if(Time.time-shootTime>destroyTime || travelledDistance>srcRange) ProjectileHitDelay();
			}
			else if(type==_SOType.Point){
				HitEffect(thisT.position+new Vector3(0, 0.2f, 0));
				ObjectPoolManager.Unspawn(thisObj);
			}
		}
		
		
		
		[Tooltip("The object to deactivate upon hit (so it's not visible while the trail-renderer/particle-effect clear up)")]
		public GameObject hideObject;
		[Tooltip("The delay in second before the game object is destroyed upon hit. This is to enabled the trail-renderer/particle-effect to clear naturally")]
		public float projectileDestroyDelay=0;
		void ProjectileHitDelay(){
			if(projectileDestroyDelay<=0){//Destroy(thisObj);
				ObjectPoolManager.Unspawn(thisObj);
				return;
			}
			
			state=_State.Hit;
			
			if(thisCollider!=null) thisCollider.enabled=false;
			
			if(hideObject!=null) hideObject.SetActive(false);
			ObjectPoolManager.Unspawn(thisObj, projectileDestroyDelay);
		}
		
		
		//function call to spawn the hit effect
		void HitEffect(Vector3 pos){
			if(hitEffect==null) return;
			
			if(!destroyHitEffect) ObjectPoolManager.Spawn(hitEffect, pos, Quaternion.identity);
			else ObjectPoolManager.Spawn(hitEffect, pos, Quaternion.identity, destroyHitDuration);
		}
		//function call to spawn the shoot effect
		void ShootEffect(Vector3 pos, Quaternion rot){
			if(shootEffect==null) return;
			
			if(!destroyShootEffect) ObjectPoolManager.Spawn(shootEffect, pos, rot);
			else ObjectPoolManager.Spawn(shootEffect, pos, rot, destroyShootDuration);
		}
		
		
		
		
		public enum _SplitMode{Aim, AOE,}
		[Header("Projectile Split Upon Hit")]
		public bool splitUponHit=false;
		public int splitCount=8;
		public int splitRange=15;
		public _SplitMode splitMode=_SplitMode.Aim;
		[HideInInspector] public bool splitRecursively=false;
		
		//function call to spawn another series of shootobject after the shootobject hits something, creating a split upon hit effect
		void Split(Collider col){
			if(type!=_SOType.Simple) return;
			
			if(splitMode==_SplitMode.Aim) SplitAim(col);
			else if(splitMode==_SplitMode.AOE) SplitAOE();
		}
		//aim split, the split shootobject will try to aim at unit in range
		void SplitAim(Collider col){	
			//first all all target in range
			LayerMask mask=1<<TDS.GetLayerAIUnit();
			Collider[] cols=Physics.OverlapSphere(thisT.position, splitRange, mask);
			
			if(cols.Length==0){
				Debug.Log("no target in range");
				return;
			}
			
			//make sure we dont aim at the collider we hit in th first place again
			List<Collider> colList=cols.ToList();
			for(int i=0; i<colList.Count; i++){
				if(colList[i]==col){ colList.RemoveAt(i); i-=1; }
			}
			
			//randomly reduce the possible target so we dont exceed the split count
			List<Transform> targetList=new List<Transform>();
			for(int i=0; i<Mathf.Min(colList.Count, splitCount); i++){ 
				int rand=Random.Range(0, colList.Count);
				targetList.Add(colList[i].transform);
				colList.RemoveAt(rand);
			}
			
			//not aim at each target and fire another shootobject at them
			for(int i=0; i<targetList.Count; i++){
				Quaternion shootRot=Quaternion.LookRotation(targetList[i].position-thisT.position);
				FireSplitSO(shootRot);
			}
		}
		void SplitAOE(){
			for(int i=0; i<splitCount; i++){
				Quaternion shootRot=Quaternion.Euler(0, i*(360f/splitCount), 0);
				FireSplitSO(shootRot);
			}
		}
		//fire the split shootobject
		void FireSplitSO(Quaternion shootRot){
			GameObject soObj=ObjectPoolManager.Spawn(thisObj, thisT.position, shootRot);
			
			ShootObject soInstance=soObj.GetComponent<ShootObject>();
			soInstance.DisableCollider(0.1f);
			
			soInstance.srcRange=splitRange;	//limite the range of split shootobject accordingly
			
			if(!splitRecursively) soInstance.splitCount=0;
			else soInstance.splitCount=(int)Mathf.Floor(splitCount/2);
			
			soInstance.Shoot(srcLayer, srcRange, thisT, attInstance);
		}
		
		
		//disable the collider for a brief moment
		//used for when the shootobject is first fired so it wont hit immediately
		public void DisableCollider(float duration=0.05f){
			StartCoroutine(_DisableCollider(duration));
		}
		IEnumerator _DisableCollider(float duration){
			thisObj.GetComponent<Collider>().enabled=false;
			yield return new WaitForSeconds(duration);
			thisObj.GetComponent<Collider>().enabled=true;
		}
		
		
		
		
		//for chain shoot object, not in used aim
		/*
		[HideInInspector] public int chainCount=4;
		private bool chainFired=false;
		public void Chain(Vector3 startPos){		//not in used
			if(chainFired || chainCount==0) return;
			
			chainFired=true;
			
			LayerMask mask=1<<TDS.GetLayerAIUnit();
			Collider[] cols=Physics.OverlapSphere(startPos, 20, mask);
			
			if(cols.Length==0){
				Debug.Log("no target in range");
				return;
			}
			
			//~ List<Collider> colList=cols.ToList();
			//~ List<Transform> targetList=new List<Transform>();
			
			float nearest=Mathf.Infinity;
			int nearestID=0;
			
			for(int i=0; i<cols.Length; i++){ 
				float dist=Vector3.Distance(cols[i].transform.position, thisT.position);
				if(dist<nearest){
					nearestID=i;
					nearest=dist;
				}
			}
			
			Quaternion shootRot=Quaternion.LookRotation(cols[nearestID].transform.position-startPos);
			GameObject soObj=(GameObject)Instantiate(gameObject, startPos, shootRot);
			
			ShootObject soInstance=soObj.GetComponent<ShootObject>();
			soInstance.DisableCollider(0.1f);
			soInstance.chainCount=chainCount-1;
			//soInstance.DontAttachToShootPoint();
			
			soInstance.Shoot(srcLayer, srcRange, thisT, attInstance);
		}
		*/
		
	}

}