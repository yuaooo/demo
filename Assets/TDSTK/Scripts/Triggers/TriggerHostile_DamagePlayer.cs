using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerHostile_DamagePlayer : Trigger {

		public override string GetEditorDescription(){
			return "This is a trigger for hostile unit\nHostile unit hitting this trigger will damage player hit point";
		}
		
		[Space(10)]
		[Tooltip("The amount of hit point player will lost if a unit enter this trigger")]
		public int hitPointLost=1;
		
		//[Tooltip("The amount of credit player will lost if a unit enter this trigger")]
		//public int creditLost=1;
		
		[Tooltip("Score penalty for player if a unit enter this trigger")]
		public int scoreLost=1;
		
		[Tooltip("If the unit enter this trigger should be destroyed")]
		public bool destroyUnit=true;
		
		
		public override void OnTriggerEnter(Collider collider){
			UnitAI unit=collider.gameObject.GetComponent<UnitAI>();
			if(unit==null){
				Debug.Log("no unit, return");
				return;
			}
			
			if(destroyUnit) unit.ClearUnit();
			
			GameControl.GetPlayer().GainHitPoint(-hitPointLost);
			//GameControl.GainCredits(-creditLost);
			GameControl.GainScore(-scoreLost);
			
			Triggered();
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=new Color(1f, 0.5f, 0.5f, 1f);
			base.OnDrawGizmos();
		}
		
	}

}
