using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class TriggerPlayer_CollectibleSpawner : Trigger {
		
		public override string GetEditorDescription(){
			return "This is a trigger for player unit\nThe spawner listed will start spawning when player hit this trigger";
		}
		
		
		[Space(10)]
		[Tooltip("The CollectibleSpawner to start spawning when this trigger is triggered")]
		public List<CollectibleSpawner> spawnerList=new List<CollectibleSpawner>();
		
		public override void OnTriggerEnter(Collider collider){
			if(collider.gameObject.GetComponent<UnitPlayer>()!=null){
				for(int i=0; i<spawnerList.Count; i++){
					spawnerList[i].StartSpawn();
				}
				
				Triggered();
			}
		}
		
		
		void OnDrawGizmosSelected(){
			Gizmos.color=new Color(0f, 1f, 1f, 1f);
			for(int i=0; i<spawnerList.Count; i++){
				Gizmos.DrawLine(spawnerList[i].transform.position, transform.position);
			}
		}
		
	}

}