using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_Damage : Trigger {
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nHitting this trigger will damage player";
		}
		
		[Space(10)]
		[Tooltip("damage inflicted to player when this trigger is triggered")]
		public int damageMin=1;
		public int damageMax=1;
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				AttackStats aStats=new AttackStats();
				aStats.damageMin=damageMin;
				aStats.damageMax=damageMax;
				AttackInstance aInstance=new AttackInstance(null, aStats);
				
				collider.gameObject.GetComponent<UnitPlayer>().ApplyAttack(aInstance);
				//collider.gameObject.GetComponent<UnitPlayer>().GainHitPoint(-damage);
				
				Triggered();
			}
		}
		
		
		protected override void OnDrawGizmos(){
			Gizmos.color=Color.red;
			base.OnDrawGizmos();
		}
		
	}

}