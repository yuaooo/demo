//for random item in the game that needs to follow the player

using UnityEngine;
using System.Collections;

using TDSTK;

public class FollowPlayer : MonoBehaviour {

	private Transform thisT;
	
	void Awake(){
		thisT=transform;
	}
	
	// Update is called once per frame
	void Update () {
		UnitPlayer player=GameControl.GetPlayer();
		if(player!=null) thisT.position=player.thisT.position;
	}
	
}
