using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	[System.Serializable]
	public class CollectibleSpawnInfo{
		public Collectible item;
		public float chance=0.5f;
		public float cooldown=10;
		[HideInInspector] public float currentCD=0;
	}
	
	
	public class CollectibleSpawner : MonoBehaviour {
		
		public bool spawnUponStart=true;
		private bool spawnStarted=false;
		
		
		void Awake() {
			//if no spawn area has been assigned, create one
			if(spawnArea==null) spawnArea=gameObject.AddComponent<TDSArea>();
		}
		
		void Start(){
			for(int i=0; i<spawnItemList.Count; i++){
				if(spawnItemList[i].item==null){
					spawnItemList.RemoveAt(i);
					i-=1;
					continue;
				}
				
				//Debug.Log(i+"   "+spawnItemList[i].item);
				ObjectPoolManager.New(spawnItemList[i].item.gameObject, 1);
			}
			
			//if spawnUponStart is enabled, start spawning
			if(spawnUponStart) StartSpawn();
		}
		
		public void StartSpawn(){
			if(spawnStarted) return;
			spawnStarted=true;
			
			StartCoroutine(SpawnRoutine());
		}
		
		
		
		
		[HideInInspector] public TDSArea spawnArea;
		
		public float startDelay=5;
		public float spawnCD=10;
		public int maxItemCount=1;
		private List<Collectible> existingItemList=new List<Collectible>();
		
		public float spawnChance=0.2f;
		public float failModifier=0.1f;
		private int failCount=0;
		public List<CollectibleSpawnInfo> spawnItemList=new List<CollectibleSpawnInfo>();
		
		
		IEnumerator SpawnRoutine(){
			yield return new WaitForSeconds(startDelay);
			
			//keep on looping
			while(true){
				Collectible newObj=SpawnItem(spawnArea.GetPosition());
				if(newObj!=null){
					newObj.SetTriggerCallback(this.ItemTriggeredCallback);	//set callback function for the collectible (which clear the item in existingItemList)
					existingItemList.Add(newObj);	//add the new item to existingItemList
				}
				
				//wait for spawnCD before attempting next spawn
				yield return new WaitForSeconds(spawnCD);
			}
		}
		
		void Update(){
			//iterate the cooldown of each spawn item
			for(int i=0; i<spawnItemList.Count; i++) spawnItemList[i].currentCD-=Time.deltaTime;
		}
		
		
		
		public Collectible SpawnItem(Vector3 pos){
			if(existingItemList.Count>=maxItemCount) return null;
			
			float chance=spawnChance+(failCount*failModifier);
			
			//check the chance, if this doesnt pass, dont spawn
			if(Random.value>chance){
				failCount+=1;
				return null;
			}
			
			failCount=0;
			
			//a list of potential item available for spawn
			List<int> potentialList=new List<int>();
			
			//loop through all item, add the available item to potential list
			for(int i=0; i<spawnItemList.Count; i++){
				if(spawnItemList[i].item==null) continue;
				if(spawnItemList[i].currentCD>0) continue;
				if(Random.value>spawnItemList[i].chance) continue;
				
				potentialList.Add(i);
			}
			
			//if there's item available to spawn
			if(potentialList.Count>0){
				//select an random item from the list and spawn it
				int rand=Random.Range(0, potentialList.Count);
				int ID=potentialList[rand];
				GameObject obj=ObjectPoolManager.Spawn(spawnItemList[ID].item.gameObject, pos, Quaternion.identity);
				//GameObject obj=(GameObject)Instantiate(spawnItemList[ID].item.gameObject, pos, Quaternion.identity);
				
				//refresh the item cooldown
				spawnItemList[ID].currentCD=spawnItemList[ID].cooldown;
				
				return obj.GetComponent<Collectible>();
			}
			
			return null;
		}
		
		
		//callback function for when an item is triggered
		public void ItemTriggeredCallback(Collectible obj){
			existingItemList.Remove(obj);
		}
		
		
		void OnDrawGizmos() {
			if(gameObject.GetComponent<CollectibleDropManager>()) return;
			
			Gizmos.DrawIcon(transform.position, "SpawnDrop.png", true);
			
			if(spawnArea!=null) spawnArea.gizmoColor=new Color(1, 1f, 0f, 1);
		}
		
	}

}