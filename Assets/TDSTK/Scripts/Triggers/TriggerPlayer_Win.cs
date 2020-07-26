using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_Win : Trigger {
		
		[Space(5)]
		[Tooltip("Check to have player unit save any progress made in leveling and perks")]
		public bool triggerSave=false;
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nThe level is considered cleared when player hit this trigger";
		}
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				GameControl.GameOver(true);
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(0f, 1f, 0.5f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}