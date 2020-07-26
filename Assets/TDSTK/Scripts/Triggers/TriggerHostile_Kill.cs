using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerHostile_Kill : Trigger {

		public override string GetEditorDescription(){ 
			return "This is a trigger for hostile unit\nAny hostile unit hitting this trigger will be destroyed immediately";
		}
		
		[Space(10)]
		[Tooltip("check if the unit should show their respective destroy effect")]
		public bool showDestroyEffect=true;
		
		//public int creditGain=1;
		
		[Tooltip("Score bonus for player if a unit enter this trigger")]
		public int scoreGain=1;
		
		
		
		public override void OnTriggerEnter(Collider collider){
			UnitAI unit=collider.gameObject.GetComponent<UnitAI>();
			if(unit==null){
				Debug.Log("no unit, return");
				return;
			}
			
			unit.ClearUnit(showDestroyEffect);
			//GameControl.GainCredits(creditGain);
			GameControl.GainScore(scoreGain);
			
			Triggered();
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(1f, 0.5f, 0.5f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}
