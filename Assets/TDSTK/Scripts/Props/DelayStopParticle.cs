//to stop a particle system from playing/emiting after a certain time
//used in various timed effect 

using UnityEngine;
using System.Collections;

public class DelayStopParticle : MonoBehaviour {

	private ParticleSystem pSystem;
	
	public float delay=0.5f;
	
	// Use this for initialization
	void Awake () {
		pSystem=gameObject.GetComponent<ParticleSystem>();
	}
	
	void OnEnable(){
		StartCoroutine(DisableRoutine());
	}
	
	IEnumerator DisableRoutine(){
		yield return new WaitForSeconds(delay);
		
		#if UNITY_5_3_OR_NEWER
			var emission = pSystem.emission;
			var enabled = pSystem.emission.enabled;
			enabled=false;
			emission.enabled=enabled;
		#else
			pSystem.enableEmission=false;
		#endif
	}
	
}
