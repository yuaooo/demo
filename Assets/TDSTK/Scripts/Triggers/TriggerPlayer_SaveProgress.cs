using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_SaveProgress : Trigger {
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nThe progress made on PlayerProgression and PlayerPerk is saved when player hit this trigger";
		}
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				collider.gameObject.GetComponent<UnitPlayer>().Save();
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(0f, 1f, 0.5f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}