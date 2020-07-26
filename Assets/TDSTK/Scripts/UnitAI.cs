using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _Behaviour{ 
		StandGuard,			//stand guard, only chase when aggro-ed, break pursuit when target is out of range
		Aggressive,				//look for target all the time
		Aggressive_Trigger,	//StandGuard until aggro-ed, once aggro-ed switch to Aggressive
		Passive,					//do nothing, for unit that do periodic firing
	}
	
	public class UnitAI : Unit {
		
		[Header("AI Setting")]
		
		public UnityEngine.AI.NavMeshAgent agent;
		public bool destroyUponPlayerContact=false;
		//[HideInInspector] 
		public _Behaviour behaviour=_Behaviour.Aggressive;
		public float aggroRange=20;
		private bool guardTriggered=false;	//for _Behaviour.StandGuard, true when unit is aggro-ed
		
		public bool stopOccasionally=false;
		public float stopRate=0.5f;
		public float stopDuration=1.5f;
		public float stopCooldown=3;
		private bool stopping=false;
		private float stopCD=0;
		
		public bool evadeOccasionally=false;
		public float evadeRate=0.5f;
		public float evadeDuration=1.5f;
		public float evadeCooldown=3;
		private bool evading=false;
		private float evadeCD=0;
		
		
		
		public bool shootPeriodically=false;
		public bool alwaysShootTowardsTarget=false;
		
		public bool randFirstAttackDelay=true;
		public float firstAttackDelay=0;
		
		//not in used atm
		[HideInInspector] public bool anchorToPoint=false;		//check to use anchor, only works with _Behaviour.StandGuard
		[HideInInspector] public Transform anchorT;				//point which the unit will stay at, when it's not aggro-ed
		[HideInInspector] public float anchorRadius;
		
		[HideInInspector] public Vector3 anchorPoint;		//the position of anchorT, cahced as soon as anchorT is set
		[HideInInspector] public Vector3 currentAnchor;	//the current random generate position based on anchorPoint and radius
		
		public void SetAnchorPoint(Transform ancT, float radius){
			anchorToPoint=true;
			anchorT=ancT;
			anchorPoint=ancT.position;
			anchorRadius=Random.Range(radius, radius);
			
			GenerateNewAnchor();
		}
		void GenerateNewAnchor(){ currentAnchor=TDSUtility.GetRandPosFromPoint(anchorPoint, anchorRadius); }
		
		
		
		
		public override void Awake() {
			base.Awake();
			
			//initiation
			thisObj.layer=TDS.GetLayerAIUnit();
			hostileUnit=true;
			
			//setup the cooldown
			if(!randFirstAttackDelay) currentCD=firstAttackDelay;
			else currentCD=Random.Range(0, firstAttackDelay);
			
			//make sure the setting for range attack is correct
			if(enableRangeAttack){
				if(shootPointList.Count==0) shootPointList.Add(thisT);	//make sure there a valid shoot point
				if(shootObject==null){
					Debug.LogWarning("shoot object unassigned for range attack unit, attack disabled", thisT);
					enableRangeAttack=false;
				}
			}
			
			//if using navmeshagent
			agent=thisObj.GetComponent<UnityEngine.AI.NavMeshAgent>();
			if(agent!=null){
				agent.stoppingDistance=brakeRange;
				agent.speed=moveSpeed;
			}
			
			evadeCD=evadeCooldown;
			stopCD=stopCooldown;
		}
		
		public override void Start() {
			base.Start();
		}
		
		
		public override void Update(){
			if(GameControl.IsGameOver() || destroyed || IsStunned()) return;
			
			//get player as target
			target=GameControl.GetPlayer();
			if(target!=null && !target.thisObj.activeInHierarchy) target=null;
			
			//reduce the attack cooldown
			currentCD-=Time.deltaTime;
			contactCurrentCD-=Time.deltaTime;
			
			//base.Update();
			
			//if shootPeriodically, then shoot
			if(shootPeriodically) ShootTarget();
			
			//if there's not target, reset the turret to origin
			if(target==null){
				ResetTurret();
				return;
			}
			
			//calculate the target dist, this is for later use
			float targetDist=Vector3.Distance(thisT.position, target.thisT.position);
			
			//now depend on the unit behaviour, act accordingly
			if(behaviour==_Behaviour.Aggressive){
				EngageHostile(targetDist);	//engage the target
			}
			else if(behaviour==_Behaviour.Aggressive_Trigger){
				//if target is close enough to trigger a respond, set behaviour to aggressive
				if(Vector3.Distance(thisT.position, target.thisT.position)<=aggroRange) behaviour=_Behaviour.Aggressive;
			}
			else if(behaviour==_Behaviour.StandGuard){
				if(guardTriggered){	//if the unit is aggro'ed
					EngageHostile(targetDist);	//engage the target
					
					//this line is irrelevant atm since anchor to point is not in used
					//if(anchorToPoint) dist=Vector3.Distance(anchorPoint, target.thisT.position);
					
					//if target is out of range already, set aggro status to false
					if(targetDist>aggroRange*2){
						guardTriggered=false;
						//if(anchorToPoint) GenerateNewAnchor();
					}
				}
				else{
					//if(anchorToPoint) MoveToPoint(currentAnchor, 0.5f);
					
					//if target is close enough to aggor the unit, set guardTriggered to true so the target can be engaged in next frame
					if(targetDist<=aggroRange) guardTriggered=true;
				}
			}
		}
		
		
		void LateUpdate(){
			//this is for move animation, if unit has moved in the frame, move will be set to true
			if(uAnimation!=null) uAnimation.Move(moved ? 1 : 0, 0);
			moved=false;	//clear moved flag the value wont be true in the next frame
		}
		
		
		//function call to engage the target, targetDist is used to determined what to do
		void EngageHostile(float targetDist){
			AimAtTarget(targetDist);	//aim at the target
			ShootTarget(targetDist);	//and fire!
			
			//stop and evade is a special maneuver to add variation to unit movement
			//reduce the cooldown on the special maneuver
			stopCD-=Time.deltaTime;
			evadeCD-=Time.deltaTime;
			
			//if possible, perform the special maneuver (if they are enabled, not being performed currently, and not on CD)
			if(stopOccasionally && !stopping && stopCD<0) StartCoroutine(StopRoutine());
			if(evadeOccasionally && !evading && evadeCD<0) StartCoroutine(EvadeRoutine());
			
			//if the unit is performaing either of the special maneuver, stop the function here, we dont need to move towards the target anymore
			if(stopping || evading) return;
			
			ChaseTarget();	//move towards the target
		}
		
		//coroutine called when a stop meneuver is started
		//the unit simply stopped for a brief moment
		IEnumerator StopRoutine(){
			//randomize the duration
			float duration=Random.Range(stopDuration*0.8f, stopDuration*1.2f);
			stopCD=stopCooldown+duration;	//set the CD on top of the duration
			
			if(Random.value>stopRate) yield break;	//sometimes, skip the routine so they dont get performed too routinely
			
			stopping=true;	//set the stopping flag to true and then false for the duration of the meneuver
			yield return new WaitForSeconds(duration);
			stopping=false;
		}
		//coroutine called when a stop meneuver is started
		//the unit simply rotate rapidly to left/right for a brief moment
		IEnumerator EvadeRoutine(){
			//randomize the duration
			float duration=Random.Range(evadeDuration*0.8f, evadeDuration*1.2f);
			evadeCD=evadeCooldown+duration;	//set the CD on top of the duration
			
			if(Random.value>evadeRate) yield break;	//sometimes, skip the routine so they dont get performed too routinely
			
			//randomize a position left/right to the unit and move towards it
			float x=Random.Range(0.5f, 1f)*(Random.value<0.5f ? -1 : 1);
			float z=Random.Range(.5f, .1f);
			Vector3 dummyPos=thisT.TransformPoint(new Vector3(x, 0, z).normalized*moveSpeed*duration*2);
			
			evading=true;	//set the evading flag to true and then false for the duration of the meneuver
			while(duration>0){
				//move towards the position & reduce the duration. once the duration is <0, the loop will be break
				MoveToPoint(dummyPos, 2);
				duration-=Time.deltaTime;
				yield return null;
			}
			evading=false;
		}
		
		
		//function call to move towards a particular point
		private bool inRange=false; //used to create a schmitt trigger effect for move threshold
		void MoveToPoint(Vector3 targetPoint, float brakeTH=0, float speedMultiplier=1){
			if(moveSpeed<=0) return;	//no point moving is move speed is zero
			
			//calculate the rotation towards the target
			Quaternion wantedRot=Quaternion.LookRotation(targetPoint-thisT.position);
			
			//if the target is out of range of the stop threshold, rotate and move towards it
			//brakeTH's value is modify with inRange flag, so that state wont flip in and out (in range ine one frame, out of range the next)
			if(Vector3.Distance(thisT.position, targetPoint)>brakeTH+(inRange ? moveSpeed : 0)){
				if(inRange) inRange=false;	//set in range to false
				
				//rotate towards the wantedRot
				thisT.rotation=Quaternion.Lerp(thisT.rotation, wantedRot, GetRotateSpeed(thisT.rotation, wantedRot, rotateSpeed * speedMultiplier));
				
				//modify the effective move speed based on the direction the unit wanted to go (it will only go full speed if it's facing the right durection)
				float modifier=(180-Mathf.Min(180, Quaternion.Angle(wantedRot, thisT.rotation)+45))/180f;
				//move towards the target point
				thisT.Translate(Vector3.forward * Time.deltaTime * moveSpeed * modifier * GetSpeedMultiplier() * speedMultiplier);
				
				moved=true;	//for animation, clear in each LateUpdate() call
			}
			//if target is in range of the stop threshold, just rotate towards it
			else{
				if(!inRange) inRange=true;	//set in range to true
				
				//rotate towards the targets, if turretObj is null, rotate all the way, else, just rotate the less than 90 degree, let turretObj do the rest
				if(turretObj==null || Quaternion.Angle(wantedRot, thisT.rotation)>90){
					thisT.rotation=Quaternion.Lerp(thisT.rotation, wantedRot, GetRotateSpeed(thisT.rotation, wantedRot, rotateSpeed));
				}
			}
		}
		
		//function call to aim turret object towards the target
		void AimAtTarget(float targetDist){
			if(turretObj==null) return;	//if there's no turret object, dont continue
			
			//if there's no target or target is out of effective firing range, rotate back to origin
			if(target==null || targetDist>GetRange()*1.25f){
				ResetTurret();
				return;
			}
			
			Vector3 tgtPos=target.thisT.position;
			Vector3 turretPos=new Vector3(turretObj.position.x, tgtPos.y, turretObj.position.z);
			Quaternion wantedRot=Quaternion.LookRotation(tgtPos-turretPos);
			
			if(!smoothTurretRotation) turretObj.rotation=wantedRot;
			else turretObj.rotation=Quaternion.Lerp(turretObj.rotation, wantedRot, GetRotateSpeed(turretObj.rotation, wantedRot, turretTrackingSpeed));
		}
		//function call to rotate the turret aiming back to origin
		void ResetTurret(){
			if(turretObj==null) return;
			if(!smoothTurretRotation) turretObj.rotation=Quaternion.identity;
			else turretObj.localRotation=Quaternion.Lerp(turretObj.localRotation, Quaternion.identity, Time.deltaTime*turretTrackingSpeed);
		}
		
		//function call to move towards the target
		void ChaseTarget(){
			if(agent!=null){	//if the unit is using navmeshagent, set the target's position as the destination
				agent.speed=moveSpeed * GetSpeedMultiplier();
				agent.SetDestination(target.thisT.position);
			}
			else{	//otherwise just call the function in this script to move towards the target
				MoveToPoint(target.thisT.position, brakeRange);
			}
		}
		
		
		private Vector3 targetLastPos;	//a temporary cached for target position when shooting
		//function call to fire at the target, default target distance is set to 0 so the unit will always fire, it's only used when ShootTarget is called from EngageHostile
		void ShootTarget(float targetDist=0){
			if(enableRangeAttack && currentCD<=0) StartCoroutine(_ShootTarget(targetDist));
		}
		IEnumerator _ShootTarget(float targetDist){
			if(targetDist>GetRange()) yield break;	//if target is out of range, dont continue
			
			currentCD=cooldown;	//set the cooldown
			
			if(uAnimation!=null) uAnimation.AttackRange();	//play the attack animation
			
			List<Collider> soColliderList=new List<Collider>();	//colliders of all the shoot-objs fired, used to tell each so to ignore each other
			
			//loop through the shoot-points, fire an shoot-object for each of them
			for(int i=0; i<shootPointList.Count; i++){
				//create a new attak instance for the attack
				AttackInstance attInstance=new AttackInstance(this, ModifyAttackStatsToExistingEffect(attackStats.Clone()));
				
				//record the target position, in case the target get destroyed before all shoot point has fired
				if(target!=null) targetLastPos=target.thisT.position;
				
				//create an aimInfo instance for the shootObject
				ShootObject.AimInfo aimInfo=target!=null ? new ShootObject.AimInfo(target) : new ShootObject.AimInfo(targetLastPos);
				
				//get the shoot rotation
				Quaternion shootRot=shootPointList[i].rotation;
				if(alwaysShootTowardsTarget && target!=null) shootRot=Quaternion.LookRotation(target.thisT.position-thisT.position);
				
				//spawn the shootobject
				GameObject soObj=ObjectPoolManager.Spawn(shootObject, shootPointList[i].position, shootRot);
				
				ShootObject soInstance=soObj.GetComponent<ShootObject>();
				
				//inform the shootobject to ignore the certain collider
				soInstance.IgnoreCollider(GetCollider());
				for(int n=0; n<soColliderList.Count; n++) soInstance.IgnoreCollider(soColliderList[n]);
				if(soInstance.GetCollider()!=null) soColliderList.Add(soInstance.GetCollider());
				
				//fire the shootobject
				soInstance.Shoot(thisObj.layer, GetRange(), shootPointList[i], attInstance, aimInfo);
				
				//delay a bit before the next shoot point, if a delay has been specified
				if(shootPointDelay>0) yield return new WaitForSeconds(shootPointDelay);
			}
			
			yield return null;
		}
		
		
		
		
		//get rotation speed for Quaternion.Lerp, modified to give a uniform speed regardless of the difference in angle
		private float GetRotateSpeed(Quaternion srcRot, Quaternion wantedRot, float rotSpd){	//normalized according to angle to be used in a lerp function
			float angle=Quaternion.Angle(srcRot, wantedRot);
			return angle==0 ? 0 : Time.deltaTime*rotSpd*GetSpeedMultiplier()/(Quaternion.Angle(srcRot, wantedRot));
		}
		
		
		//when the unit collide with something, for collision with player
		void OnCollisionEnter(Collision collision){
			if(collision.gameObject.layer!=TDS.GetLayerPlayer()) return;
			
			OnPlayerContact();
			if(destroyUponPlayerContact) ClearUnit();	//if destroyUponPlayerContact is enabled, clear the unit
		}
		//when the unit enter some trigger
		void OnTriggerEnter(Collider col){
			if(col.gameObject.layer!=TDS.GetLayerPlayer()) return;
			
			OnPlayerContact();
			if(destroyUponPlayerContact) ClearUnit();	//if destroyUponPlayerContact is enabled, clear the unit
		}
		
		//function call when the unit collide with a player unit, for contact(melee) attack
		void OnPlayerContact(){
			if(!enableContactAttack) return;
			
			if(contactCurrentCD>0) return;		//if attack is still on cd
			
			if(uAnimation!=null) uAnimation.AttackMelee();	//play attack animation
			
			contactCurrentCD=contactCooldown;	//set the cooldown
			
			//create an attack instance and attack the player unit
			AttackInstance attInstance=new AttackInstance(this, ModifyAttackStatsToExistingEffect(contactAttackStats.Clone()));
			GameControl.GetPlayer().ApplyAttack(attInstance);
		}
		
		
		
	}
	
	
	
}