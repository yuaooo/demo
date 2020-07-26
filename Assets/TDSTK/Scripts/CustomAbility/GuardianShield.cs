using UnityEngine;
using System.Collections;

using TDSTK;

public class GuardianShield : MonoBehaviour {

	public float duration=2;
	
	// Use this for initialization
	IEnumerator Start () {
		transform.parent=GameControl.GetPlayer().turretObj;
		transform.localPosition=Vector3.zero;
		transform.localRotation=Quaternion.identity;
		
		while(duration>0){
			if(!GameControl.IsGamePaused()) duration-=Time.deltaTime;
			yield return null;
		}
		
		Destroy(gameObject);
	}
	
}
