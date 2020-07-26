using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class UnitAnimation : MonoBehaviour {
		
		[Tooltip("The Animator component on the animated model")]
		public Animator animator;
		
		[Space(5)]
		public AnimationClip clipIdle;
		
		[Space(5)]
		public AnimationClip clipForward;
		public AnimationClip clipBackward;
		public AnimationClip clipStrafeLeft;
		public AnimationClip clipStrafeRight;
		
		[Space(5)]
		public AnimationClip clipAttackRange;
		public AnimationClip clipAttackMelee;
		public AnimationClip clipHit;
		public AnimationClip clipDestroyed;
		
		//public float moveSpeedMultiplier=1;
		
		//public float attackDelayRange=0;
		//public float attackDelayMelee=0;
		
		private UnitPlayer unitPlayer;
		private UnitAI unitAI;
		
		void Awake() {
			Unit unit=gameObject.GetComponent<Unit>();
			unit.SetUnitAnimation(this);
			
			AnimatorOverrideController overrideController = new AnimatorOverrideController();
			overrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
			
			overrideController["TDSDummyIdle"] = clipIdle;
			
			overrideController["TDSDummyMoveForward"] = clipForward;
			overrideController["TDSDummyMoveBackward"] = clipBackward;
			overrideController["TDSDummyMoveStrafeLeft"] = clipStrafeLeft;
			overrideController["TDSDummyMoveStrafeRight"] = clipStrafeRight;
			
			overrideController["TDSDummyAttackRange"] = clipAttackRange;
			overrideController["TDSDummyAttackMelee"] = clipAttackMelee;
			overrideController["TDSDummyHit"] = clipHit;
			overrideController["TDSDummyDestroy"] = clipDestroyed;
			
			animator.runtimeAnimatorController = overrideController;
			
			//AnimatorStateInfo stateInfo=animator.GetCurrentAnimatorStateInfo(0);
			//stateInfo.speed=unit.moveSpeed*moveSpeedMultiplier;
		}
		
		
		
		
		public void Move(float speedZ, float speedX){
			animator.SetFloat("SpeedZ", speedZ);
			animator.SetFloat("SpeedX", speedX);
		}
		
		
		public void AttackRange(){
			animator.SetTrigger("AttackRange");
		}
		public void AttackMelee(){
			animator.SetTrigger("AttackMelee");
		}
		
		
		public void Hit(){
			animator.SetTrigger("Hit");
		}
		public float Destroyed(){
			animator.SetTrigger("Destroyed");
			return clipDestroyed!=null ? clipDestroyed.length : 0;
		}
		
	}

}