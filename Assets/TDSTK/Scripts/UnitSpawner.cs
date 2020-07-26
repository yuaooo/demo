using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _SpawnMode{
		WaveBased,
		FreeForm,
	}
	
	public enum _OverrideMode{ Replace, Addition, Multiply }
	public enum _SpawnLimitType{ Count, Timed, None }	//for free formed spawning
	
	public class UnitSpawner : MonoBehaviour {
		
		public _SpawnMode spawnMode;
		public bool spawnUponStart=true;
		private bool spawnStarted=false;
		
		public float startDelay=1;
		
		//public TDSArea spawnArea;
		public List<TDSArea> spawnAreaList=new List<TDSArea>(); //first element is used as the default 
		public bool randomRotation=true;
		
		public bool anchorToPoint=false;
		public float anchorRadius=3;
		
		
		
		[Header("Procedural Generation Setting")]	//used in freeform and endless wave
		public List<Unit> spawnUnitList=new List<Unit>();
		
		public bool overrideHitPoint=false;
		public _OverrideMode overrideHPMode=_OverrideMode.Multiply;
		public float startingHitPoint=25;
		public float hitPointIncrement=5;
		public float hitPointTimeStep=30;
		public float maxHitPoint = 3;
		private float spawnHP=0;
		
		
		
		[Header("WaveBased Setting")]
		private int currentWaveIDX=-1;
		
		public bool endlessWave=false;
		public List<Wave> waveList=new List<Wave>();
		public float delayBetweenWave=3;
		
		
		
		[Header("Wave Endless Setting")]
		public int maxSubWaveCount=3;
		public int unitCount=8;
		public int unitCountInc=4;
		private Wave waveE=null;	//the current wave used in endless mode
		
		public int startingCredit=10;
		public int creditIncrement=10;
		
		public int startingScore=10;
		public int scoreIncrement=10;
		
		
		
		[Header("FreeForm Setting")]
		public float spawnCD=1.5f;
		
		public int activeLimit=10;
		public int limitSpawnCount=20;
		public int limitSpawnTime=20;
		public _SpawnLimitType limitType=_SpawnLimitType.Timed;
		
		
		
		[Header("Stats Tracking")]
		private int activeCount=0;	//the total unit currently active in the game
		private int spawnCount=0;	//the total unit that has been spawned
		
		private int killCount=0;		//the total unit that has been destroyed
		
		
		
		
		private Transform thisT;
		void Awake(){
			thisT=transform;
			
			//assign each of the wave in waveList with an ID
			for(int i=0; i<waveList.Count; i++) waveList[i].waveID=i;
			
			//make sure none of the element in spawnAreaList and spawnUnitList is null
			for(int i=0; i<spawnAreaList.Count; i++){
				if(spawnAreaList[i]==null){
					spawnAreaList.RemoveAt(i); i-=1;
				}
			}
			for(int i=0; i<spawnUnitList.Count; i++){
				if(spawnUnitList[i]==null){
					spawnUnitList.RemoveAt(i); i-=1;
				}
			}
			
			//if no spawn area has been assigned, create one
			if(spawnAreaList.Count==0){
				spawnAreaList.Add(gameObject.AddComponent<TDSArea>());
			}
			
			//warn user if no unit has been assigned
			if(spawnMode==_SpawnMode.FreeForm || (spawnMode==_SpawnMode.WaveBased && endlessWave)){
				if(spawnUnitList.Count==0) Debug.LogWarning("No unit has been specified for unit spawner", thisT);
			}
		}
		
		
		void Start(){
			InitObjectPool();
			
			//add this spawner to tracker
			UnitSpawnerTracker.AddSpawner(this);
			
			if(overrideHitPoint) spawnHP=startingHitPoint;
			
			//if spawnUponStart is enabled, start spawning
			if(spawnUponStart) StartSpawn();
		}
		
		
		void OnDisable(){
			//if the spawner is destroyed, consider it cleared
			Cleared();
		}
		
		
		public void InitObjectPool(){
			if(spawnMode==_SpawnMode.FreeForm || (spawnMode==_SpawnMode.WaveBased && endlessWave)){
				for(int i=0; i<spawnUnitList.Count; i++){
					ObjectPoolManager.New(spawnUnitList[i].gameObject, 5);
				}
			}
			else{
				for(int i=0; i<waveList.Count; i++){
					for(int n=0; n<waveList[i].subWaveList.Count; n++){
						if(waveList[i].subWaveList[n].unitPrefab!=null)
							ObjectPoolManager.New(waveList[i].subWaveList[n].unitPrefab.gameObject, 5);
						else
							Debug.LogWarning("unit prefab for wave-"+i+", subwave-"+n+" is unspecified", this);
					}
				}
			}
		}
		
		
		public void StartSpawn(){
			if(!gameObject.activeInHierarchy) return;
			
			if(spawnStarted) return;	//prevent duplicate spawning routine
			spawnStarted=true;
			
			if(spawnMode==_SpawnMode.WaveBased){
				if(!endlessWave) SpawnWaveFromList(startDelay);
				else SpawnGeneratedWave(startDelay);
			}
			else if(spawnMode==_SpawnMode.FreeForm){
				StartCoroutine(SpawnFreeForm());
			}
		}
		
		
		//spawn the next wave in wavelist
		void SpawnWaveFromList(float delay=0){
			//if we have past the final wave, stop and clear the spawner
			if(currentWaveIDX+1>=waveList.Count){
				Debug.Log(gameObject.name+" - all waves cleared");
				Cleared();
				return;
			}
			StartCoroutine(SpawnWave(waveList[currentWaveIDX+=1], delay));
		}
		//spawn a generated wave, for endless mode only
		void SpawnGeneratedWave(float delay=0){
			waveE=GenerateWave(currentWaveIDX+=1);
			StartCoroutine(SpawnWave(waveE, delay));
		}
		IEnumerator SpawnWave(Wave wave, float delay){
			yield return new WaitForSeconds(delay);
			
			Debug.Log(gameObject.name+" - start spawning wave "+currentWaveIDX);
			
			if(currentWaveIDX>0) spawnHP+=hitPointIncrement;
			
			if(wave.subWaveList.Count==0) Debug.LogWarning("Trying to spawn an empty wave", thisT);
			
			wave.subWaveSpawned=0;
			
			//start coroutine for each subwave
			for(int i=0; i<wave.subWaveList.Count; i++) StartCoroutine(SpawnSubWave(wave, i));
			
			//wait until all of the wubwave finish spawning
			while(wave.subWaveSpawned<wave.subWaveList.Count) yield return null;
			
			wave.spawned=true;
			Debug.Log(gameObject.name+" - wave "+currentWaveIDX+" spawned complete");
		}
		//function call to spawn a subwave
		IEnumerator SpawnSubWave(Wave wave, int subWaveIdx){
			SubWave subWave=wave.subWaveList[subWaveIdx];
			TDSArea sArea=subWave.spawnArea!=null ? subWave.spawnArea : spawnAreaList[0];	//use the default spawn area if nothing has been assigned for the subwave
			
			//wait for start delay
			yield return new WaitForSeconds(subWave.startDelay);
			
			if(subWave.unitPrefab!=null){
				for(int i=0; i<subWave.count; i++){
					//wait for the spawn cooldown
					if(i>0) yield return new WaitForSeconds(subWave.interval);
					
					Quaternion rot=!randomRotation ? sArea.GetRotation() : Quaternion.Euler(0, Random.Range(0, 360), 0);
					
					UnitAI unitInstance=SpawnUnit(subWave.unitPrefab.gameObject, sArea.GetPosition(), rot, subWave.unitPrefab.gameObject.name+"_"+spawnCount);
					unitInstance.SetWaveID(this, wave.waveID);	//assign the unit with the waveID so they know they belong to a wave (unit with valid waveID will call UnitCleared() callback)
					
					wave.activeUnitCount+=1;
				}
			}
			
			//increase the subWaveSpawned counter
			wave.subWaveSpawned+=1;
			yield return null;
		}
		
		//called from unit to add the new SpawnUponDestroy unit to the parent unit's wave
		public void AddUnitToWave(Unit unitInstance){
			waveList[unitInstance.waveID].activeUnitCount+=1;
			AddUnit(unitInstance);
		}
		
		//callback when a unit belong to a particular wave is destroyed, check if the wave is cleared
		public void UnitCleared(int waveID){
			bool waveCleared=false;
			
			//check if the wave has completed its spawning and have all active units destroyed
			if(!endlessWave){
				waveList[waveID].activeUnitCount-=1;
				//if the wave has done spawning and there's no active unit
				if(waveList[waveID].spawned && waveList[waveID].activeUnitCount==0){
					waveCleared=true;
					waveList[waveID].Completed();
					SpawnWaveFromList(delayBetweenWave); //start spawning the next wave
				}
			}
			else{
				waveE.activeUnitCount-=1;
				//if the wave has done spawning and there's no active unit
				if(waveE.spawned && waveE.activeUnitCount==0){
					waveCleared=true;
					waveE.Completed();
					SpawnGeneratedWave(delayBetweenWave);	//start spawning the next wave
				}
			}
			
			//if the wave is cleared
			if(waveCleared){
				Debug.Log(gameObject.name+" - wave "+(currentWaveIDX-1)+" cleared");
				Debug.Log("");
			}
		}
		
		
		
		
		
		//free form spawn mode
		IEnumerator SpawnFreeForm(){
			yield return new WaitForSeconds(startDelay);
			
			//if limited by time, start the timer
			if(limitType==_SpawnLimitType.Timed) StartCoroutine(SpawnLimitTimerRoutine());
			
			if(overrideHitPoint) StartCoroutine(OverridingHitPointRoutine());
			
			//keep on looping
			while(true){
				if(maxHitPoint == spawnHP)
                {
					StopCoroutine(OverridingHitPointRoutine());
					maxHitPoint = 0;
                }

				//if spawnUnitList is empty, do nothing
				while(spawnUnitList.Count==0) yield return null;
				
				//if activeCount has reached the limit, hold on for spawning
				while(activeCount==activeLimit) yield return null;
				
				//if using timer and time is up, break from the loop and stop the spawning
				if(limitType==_SpawnLimitType.Timed && freeformTimeOut) break;
				
				//randomly choose a spawn area and position as well as a rotation
				int rand=Random.Range(0, spawnAreaList.Count);
				Vector3 pos=spawnAreaList[rand].GetPosition();
				Quaternion rot=!randomRotation ? spawnAreaList[rand].GetRotation() : Quaternion.Euler(0, Random.Range(0, 360), 0);
				
				//choose the unit prefab to spawn
				int randU=Random.Range(0, spawnUnitList.Count);
				SpawnUnit(spawnUnitList[randU].gameObject, pos, rot, spawnUnitList[randU].gameObject.name+"_"+spawnCount);
				
				//if spawnCount has reached the specified limit, break from the loop and stop the spawning
				if(limitType==_SpawnLimitType.Count && spawnCount==limitSpawnCount) break;
				
				//wait for spawnCD before attempting next spawn
				yield return new WaitForSeconds(spawnCD);
			}
			
			//wait until all active unit is cleared before proceed
			while(activeCount>0) yield return null;
			
			//spawn completed and all spawned unit is cleared
			Debug.Log(gameObject.name+" (UnitSpawner) is cleared");
			Cleared();
		}
		
		
		
		//spawn timer, spawning will stop when time run out, only used for free-form mode
		private bool freeformTimeOut=false;
		IEnumerator SpawnLimitTimerRoutine(){
			yield return new WaitForSeconds(limitSpawnTime);
			freeformTimeOut=true;
		}
		
		//Gradually incrase the overridingHP, only used in freeform mode
		IEnumerator OverridingHitPointRoutine(){
			while(true){
				yield return new WaitForSeconds(hitPointTimeStep);
				spawnHP+=hitPointIncrement;
			}
		}
		
		
		
		
		//function to check if the spawner has spawned everything
		public bool IsSpawnCompleted(){
			if(spawnMode==_SpawnMode.WaveBased){
				if(endlessWave) return false;	//the spawn will never complete in endless mode
				
				//check for everywave, the spawn is considered completed if all wave has been spawned
				bool allSpawned=true;
				for(int i=0; i<waveList.Count; i++){
					if(!waveList[i].spawned){
						allSpawned=false;
						break;
					}
				}
				return allSpawned;
			}
			else if(spawnMode==_SpawnMode.FreeForm){
				//the spawn is considered completed if spawn count has exceeded spawn limit or spawner timer has ran out
				if(limitType==_SpawnLimitType.Count && spawnCount>=limitSpawnCount) return true;
				else if(limitType==_SpawnLimitType.Timed && freeformTimeOut) return true;
			}
			
			//none of the criteria has been fullfiled, return false;
			return false;
		}
		
		
		
		//spawn the an unit instance of given the prefab
		private UnitAI SpawnUnit(GameObject prefab, Vector3 spawnPos, Quaternion rot, string name=""){
			//instantiate the unit, assign the layer and name
			//~ GameObject unitObj=(GameObject)Instantiate(prefab, spawnPos, rot);
			GameObject unitObj=ObjectPoolManager.Spawn(prefab, spawnPos, rot);
			unitObj.layer=TDS.GetLayerAIUnit();
			unitObj.name=name;
			
			//get the UnitAI instance and assign target
			UnitAI unitInstance=unitObj.GetComponent<UnitAI>();
			unitInstance.target=GameControl.GetPlayer();
			//if(anchorToPoint) unitInstance.SetAnchorPoint(transform, anchorRadius);	//not in use atm
			
			//override the unit default hitpoint if overrideHitPoint is enabled
			if(overrideHitPoint) unitInstance.OverrideHitPoint(spawnHP, overrideHPMode);
			
			AddUnit(unitInstance);	//track unit
			
			return unitInstance;
		}
		
		
		//track unit
		public void AddUnit(Unit unitInstance){
			//set destroy callback for the unit
			unitInstance.SetDestroyCallback(this.UnitDestroy);
			spawnCount+=1;
			activeCount+=1;
		}
		//untrack unit
		public void UnitDestroy(){
			activeCount-=1;
			killCount+=1;
		}
		
		
		public void Cleared(){
			//remove the spawner from the tracker
			UnitSpawnerTracker.RemoveSpawner(this);
			//inform the GameControl that the spawner has been cleared (which would in term check for objective)
			GameControl.UnitSpawnerCleared(this);
		}
		
		
		
		//called to generate a spawn wave using the procedural generation parameter
		Wave GenerateWave(int waveIDX){
			Wave wave=new Wave();
			
			wave.waveID=waveIDX;
			wave.spawnArea=spawnAreaList[Random.Range(0, spawnAreaList.Count)];
			
			wave.creditGain=startingCredit+creditIncrement*waveIDX;
			wave.scoreGain=startingScore+scoreIncrement*waveIDX;
			
			int subWaveCount=Random.Range(1, waveIDX);
			subWaveCount=Mathf.Clamp(subWaveCount, 1, maxSubWaveCount);
			subWaveCount=Mathf.Clamp(subWaveCount, 1, spawnUnitList.Count);
			
			List<int> countList=new List<int>();
			for(int i=0; i<subWaveCount; i++) countList.Add(1);
			
			int totalUnitCount=unitCount+unitCountInc*waveIDX-subWaveCount;
			if(subWaveCount<=0) totalUnitCount=0;
			
			int count=0;
			while(totalUnitCount>0){
				int rand=Random.Range(1, totalUnitCount);
				countList[count]+=rand;
				totalUnitCount-=rand;
				if((count+=1)>=subWaveCount) count=0;
			}
			
			List<Unit> unitList=new List<Unit>( spawnUnitList );
			
			wave.subWaveList=new List<SubWave>();
			for(int i=0; i<subWaveCount; i++){
				SubWave subWave=new SubWave();
				subWave.count=countList[i];
				
				int rand=Random.Range(0, unitList.Count);
				subWave.unitPrefab=unitList[rand];
				unitList.RemoveAt(rand);
				
				subWave.startDelay=0.5f;
				subWave.interval=Random.Range(1f, 2f);
				wave.subWaveList.Add(subWave);
			}
			
			return wave;
		}
		
		
		
		
		void OnDrawGizmos() {
			Gizmos.DrawIcon(transform.position, "SpawnUnit.png", true);
			
			for(int i=0; i<spawnAreaList.Count; i++){
				if(spawnAreaList[i]==null) continue;
				spawnAreaList[i].gizmoColor=new Color(1, 0, 0.5f, 1);
			}
		}
		
	}
	
	

}