//a controller component to a ColletibleSpawner in game
//this is used exclusively to spawn collectible when a unit is killed

using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[RequireComponent (typeof (CollectibleSpawner))]
	public class CollectibleDropManager : MonoBehaviour {

		private static CollectibleDropManager instance;
		public static CollectibleDropManager GetInstance(){ return instance; }
		
		[HideInInspector] public CollectibleSpawner spawner;
		
		void Awake() {
			instance=this;
			
			spawner=gameObject.GetComponent<CollectibleSpawner>();
			spawner.spawnUponStart=false;	//disable the spawner spawnUponStart so that it only spawned when called
		}
		
		
		//called from Unit with useDropManager enabled.
		public static void UnitDestroyed(Vector3 pos){ 
			if(instance==null) return;
			if(instance.spawner==null) return;
			
			//if the spawner exist, spawned a collectible item at the unit position
			//this doesnt guarantee a successful spawn, it will depend on the spawn chance of the spawner and what not
			instance.spawner.SpawnItem(pos);
		}
		
	}

}