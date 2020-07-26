//for muzzle effect on the machine gun
//turn the renderer on and off for a fliker effect

using UnityEngine;
using System.Collections;

public class GunShotMuzzle : MonoBehaviour {

	private LineRenderer rend;
	public float blinkDuration=0.02f;
	
	// Use this for initialization
	void Awake() {
		rend=gameObject.GetComponent<LineRenderer>();
	}
	
	void OnEnable(){
		if(rend!=null) StartCoroutine(DisableRenderer());
	}
	
	IEnumerator DisableRenderer(){
		while(true){
			yield return new WaitForSeconds(0.025f);
			rend.enabled=!rend.enabled;
		}
	}
	
	
}
