using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_ActivateUnit : Trigger {
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nThe unit listed in unit list will become active (by enabling the UnitAI component)";
		}
		
		[Space(10)]
		[Tooltip("The unit to activate when this trigger is triggered")]
		public List<UnitAI> unitList=new List<UnitAI>();
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				for(int i=0; i<unitList.Count; i++){
					if(unitList[i]==null) continue;
					unitList[i].enabled=true;
				}
				
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(1f, 0.5f, 0.0f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}