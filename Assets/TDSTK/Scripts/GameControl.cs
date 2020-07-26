//the main component that control the general game logic

using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public enum _GameState{
		Playing,		//when game is playing
		Paused,		//when game is paused
		GameOver,	//when game is over
	}

	[RequireComponent(typeof (DamageTable))]
	public class GameControl : MonoBehaviour {
		
		private static GameControl instance;
		public static GameControl GetInstance(){ return instance; }
		
		private _GameState gameState=_GameState.Playing;
		
		
		public bool enableTimer=false;
		public float timerDuration=0;
		private bool timesUp=false;
		[HideInInspector] public float remainingDuration=0;
		
		public static bool EnableTimer(){ return instance!=null ? instance.enableTimer : false; }	//called to check if timer is enabled
		public static bool TimesUp(){ return instance!=null ? instance.timesUp : false; }			//called to check if time is up
		public static float GetRemainingDuration(){ return instance!=null ? instance.remainingDuration : 0; }	//get remaining time
		
		//credit is not in used atm
		[HideInInspector] public int credits=0;
		public static int GetCredits(){ return instance.credits; }
		public static void GainCredits(int value){ instance.credits+=value; }
		public static void SpendCredits(int value){ instance.credits=Mathf.Max(0, instance.credits-value); }
		
		[HideInInspector] public int score=0;
		public static int GetScore(){ return instance.score; }
		public static void GainScore(int value){ 
			instance.score+=(int)Mathf.Round(value*GetPlayer().GetScoreMultiplier());
			if(instance.objective!=null) instance.objective.GainScore();
		}
		
		public static void ColletibleCollected(Collectible item){ if(instance!=null && instance.objective!=null) instance.objective.ColletibleCollected(item); }
		
		
		//the active player unit in the game
		private UnitPlayer player;
		public static UnitPlayer GetPlayer(){ return instance==null ? null : instance.player ; }
		public static void SetPlayer(UnitPlayer newPlayer){ if(instance!=null) instance.player=newPlayer; }
		
		
		public bool enableAbility=false;
		public static bool EnableAbility(){ return instance.enableAbility; }
		
		public bool enableAltFire=false;
		public static bool EnableAltFire(){ return instance.enableAltFire; }
		
		public bool enableContinousFire=true;
		public static bool EnableContinousFire(){ return instance.enableContinousFire; }
		
		public bool enableAutoReload=true;
		public static bool EnableAutoReload(){ return instance.enableAutoReload; }
		
		
		[Header("ShootObject hits")]
		public bool friendly=false;
		public bool shootObject=true;
		public bool collectible=false;
		public static bool SOHitFriendly(){ return instance.friendly; }
		public static bool SOHitShootObject(){ return instance.shootObject; }
		public static bool SOHitCollectible(){ return instance.collectible; }
		
		
		[Header("Level Objective")]
		public ObjectiveTracker objective;
		//public static void SetObjective(ObjectiveTracker objInstance){ instance.objective=objInstance; }
		//public static void ObjectiveComplete(ObjectiveTracker objectiveInstance){
		//	if(objectiveInstance.mainObjective) GameOver(true);
		//}
		
		
		
		//call to inform objective that a unitspawner has been cleared
		public static void UnitSpawnerCleared(UnitSpawner spawner){ if(instance!=null) instance._UnitSpawnerCleared(spawner); }
		public void _UnitSpawnerCleared(UnitSpawner spawner){
			if(objective!=null) objective.SpawnerCleared(spawner);
		}
		//call to inform objective that a unit has been destroyed
		public static void UnitDestroyed(Unit unit){ if(instance!=null) instance._UnitDestroyed(unit); }
		public void _UnitDestroyed(Unit unit){
			if(objective!=null) objective.UnitDestroyed(unit);
		}
		
		
		
		//how many time player can respawn
		public int playerLife=0;
		public static int GetPlayerLife(){ return instance.playerLife; }
		public static void GainLife(){ instance.playerLife+=1; }
		
		
		public Transform startPoint;	//the designated player start point, used as the first respawn point
		private Vector3 respawnPoint;
		public static void SetRespawnPoint(Vector3 pos){
			instance.respawnPoint=pos;
		}
		
		
		private bool respawning=false;
		public static void PlayerDestroyed(){ instance._PlayerDestroyed(); }
		public void _PlayerDestroyed(){
			if(respawning) return;
			
			//player unit is destroyed, check playerLife and respawn player if need be
			
			playerLife-=1;
			
			if(playerLife<=0){	//playerLife's used up, game over
				GameOver(false);
				return;
			}
			
			respawning=true;	//set respawning flag to true to prevent duplicate spawn
			
			//create a duplicate of current player unit so the selected weapon and ability is retained
			GameObject obj=(GameObject)Instantiate(player.gameObject, player.thisT.position, player.thisT.rotation);
			player=obj.GetComponent<UnitPlayer>();
			player.hitPoint=player.GetFullHitPoint();
			player.Start();
			
			//set the new player unit to false to give it a little delay before showing it again
			obj.SetActive(false);
			
			//call the coroutine which will do the delay and reactivate the new unit
			StartCoroutine(ActivateRepawnPlayer());	
		}
		IEnumerator ActivateRepawnPlayer(){
			//delay for 1 second
			yield return new WaitForSeconds(1);
			
			//after the delay, set the new player unit to the respawn point and activate it
			player.thisT.position=respawnPoint;
			player.thisObj.SetActive(true);
			
			respawning=false;	//clear the respawning flag
			
			TDS.PlayerRespawned();
		}
		
		
		//a unique ID for each new active unit in game, each time a value is retrieved to be given to a new unit, the number is increased
		private int unitInstanceID=-1;
		public static int GetUnitInstanceID(){ return instance==null ? -1 : instance.unitInstanceID+=1; }
		
		
		
		public string mainMenu="MainScene";
		public string nextScene="";
		public static void LoadMainMenu(){ LoadScene("MainScene"); }
		public static void LoadNextScene(){ LoadScene(instance.nextScene); }
		public static void RestartScene(){ 
			#if UNITY_5_3_OR_NEWER
				LoadScene(SceneManager.GetActiveScene().name);
			#else
				LoadScene(Application.loadedLevelName);
			#endif
		}
		public static void LoadScene(string sceneName){
			if(sceneName=="") return;
			Time.timeScale=1;
			
			#if UNITY_5_3_OR_NEWER
				SceneManager.LoadScene(sceneName);
			#else
				Application.LoadLevel(sceneName);
			#endif
		}
		
		
		void Awake() {
			instance=this;
			
			//QualitySettings.vSyncCount=1;
			//Application.targetFrameRate=60;
			
			//get the unit in game
			player = (UnitPlayer)FindObjectOfType(typeof(UnitPlayer));
			
			//setup the collision rules
			Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerShootObject(), !shootObject);
			Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerCollectible(), !collectible);
			
			Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerTerrain(), true);
			Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerTrigger(), true);
			
			//clear all the spawner and tracker sicne it's a new game
			UnitTracker.Clear();
			UnitSpawnerTracker.Clear();
			
			//this is not required, each individual unit and spawner will register itself to the tracker
			//UnitTracker.ScanForUnit();
			//UnitSpawnerTracker.ScanForSpawner();
		}
		
		//start of the game
		void Start(){
			//if timer is enabled, start the count down
			if(enableTimer) StartCoroutine(TimerCountDown());
			
			//set respawn point
			if(startPoint!=null) SetRespawnPoint(startPoint.position);
			else SetRespawnPoint(player.thisT.position);
		}
		
		//the coroutine for counting down timer
		IEnumerator TimerCountDown(){
			remainingDuration=timerDuration;
			//keep looping at every frame while the count down is not complete
			while(remainingDuration>0){
				while(!IsGamePlaying()) yield return null;	//if game is not playing, hold the count
				remainingDuration-=Time.deltaTime;			//reduce the remaining duration by how much time escalated since last frame
				yield return null;
			}
			timesUp=true;
			objective.CheckObjectiveComplete();	//check objective
		}
		
		
		private bool gameOver=false;	//a local flag to prevent gameover routine being run twice
		public static void GameOver(bool won){	//static function to end the game
			if(!instance.gameObject.activeInHierarchy) return;
			instance.StartCoroutine(instance._GameOver(won));
		}
		public IEnumerator _GameOver(bool won){
			if(gameOver) yield break;	//stop the coroutine if it has already been called
			gameOver=true;
			
			Debug.Log("game over - "+(won ? "win" : "lost"));
			
			//delay 1.5 second before issuing a global gameover event
			yield return new WaitForSeconds(1.5f);
			gameState=_GameState.GameOver;
			TDS.GameOver(won);
		}
		
		
		public static bool IsGamePlaying(){ return instance==null ? true : (instance.gameState==_GameState.Playing ? true : false ); }
		public static bool IsGamePaused(){ return instance==null ? true : (instance.gameState==_GameState.Paused ? true : false ); }
		public static bool IsGameOver(){ return instance==null ? true : (instance.gameState==_GameState.GameOver ? true : false ); }
		
		public static void PauseGame(){ instance.gameState=_GameState.Paused;	}
		public static void ResumeGame(){ instance.gameState=_GameState.Playing;	}
		
	}
	

}