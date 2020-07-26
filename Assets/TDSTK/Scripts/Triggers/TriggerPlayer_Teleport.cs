using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_Teleport : Trigger {
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nHitting this trigger will teleport player to a specific location";
		}
		
		[Space(5)]
		[Tooltip("The transform which mark the position where the player should be teleport to")]
		public Transform targetTransform;
		
		
		public override bool UseAltTriggerEffectObj(){ return true; }
		
		
		public override void OnTriggerEnter(Collider collider){
			if(targetTransform==null) return;
			
			UnitPlayer player=collider.gameObject.GetComponent<UnitPlayer>();
			
			if(player!=null){
				//for effect, check parent class
				if(!spawnEffectAtOrigin) effPos=player.transform.position;
				targetEffPos=targetTransform.position;
				
				player.thisT.position=targetTransform.position;
				
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			if(targetTransform!=null){
				Gizmos.color=new Color(0.25f, 1f, 0.25f, 1f);
				Gizmos.DrawLine(transform.position, targetTransform.position);
			}
			
			Gizmos.color=new Color(0f, 1f, 0.5f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}