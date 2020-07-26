using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_RespawnPoint : Trigger {
		
		[Space(5)]
		[Tooltip("Check to have player unit save any progress made in leveling and perks")]
		public bool triggerSave=false;
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nIt set the player respawn point to the trigger's position";
		}
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				if(triggerSave) collider.gameObject.GetComponent<UnitPlayer>().Save();
				GameControl.SetRespawnPoint(transform.position);
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(0f, 1f, 1f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}