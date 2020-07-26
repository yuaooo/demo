using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_Objective : Trigger {
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nIt's to be act as one of the criteria for a the objective tracker component";
		}
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				for(int i=0; i<triggerCallbackList.Count; i++){
					if(triggerCallbackList[i]!=null) triggerCallbackList[i](this);
				}
				
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(0f, 1f, 0.5f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}