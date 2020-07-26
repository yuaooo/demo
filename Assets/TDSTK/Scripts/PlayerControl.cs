//the main component that control the general game logic

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class PlayerControl : MonoBehaviour {

		//the active player unit in the game
		private UnitPlayer player;
		public static UnitPlayer GetPlayer(){ return instance==null ? null : instance.player ; }
		
		
		//how many time player can respawn
		public int playerLife=0;
		public static int GetPlayerLife(){ return instance.playerLife; }
		public static void GainLife(){ instance.playerLife+=1; }
		
		
		//credit is not in used atm
		[HideInInspector] public int credits=0;
		public static int GetCredits(){ return instance.credits; }
		public static void GainCredits(int value){ instance.credits+=value; }
		public static void SpendCredits(int value){ instance.credits=Mathf.Max(0, instance.credits-value); }
		
		
		[HideInInspector] public int score=0;
		public static int GetScore(){ return instance.score; }
		public static void GainScore(int value){ 
			instance.score+=value;
			GameControl.GainScore(value);
			//if(instance.objective!=null) instance.objective.GainScore();
		}
		
		
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
				GameControl.GameOver(false);
				return;
			}
			
			respawning=true;	//set respawning flag to true to prevent duplicate spawn
			
			//create a duplicate of current player unit so the selected weapon and ability is retained
			GameObject obj=(GameObject)Instantiate(player.gameObject, player.thisT.position, player.thisT.rotation);
			player=obj.GetComponent<UnitPlayer>();
			
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
		}
		
		
		
		
		private static PlayerControl instance;
		public static PlayerControl GetInstance(){ return instance; }
		
		void Awake() {
			instance=this;
			
			//get the unit in game
			player = (UnitPlayer)FindObjectOfType(typeof(UnitPlayer));
		}
		
		
		//start of the game
		void Start(){
			//set respawn point
			if(startPoint!=null) SetRespawnPoint(startPoint.position);
			else SetRespawnPoint(player.thisT.position);
		}
		
	}

}