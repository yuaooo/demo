//for LineRenderer on beam shoot-object
//reduce the width of the line over a fixed duration

using UnityEngine;
using System.Collections;


public class FadeOutBeam : MonoBehaviour {

	public float fadeDuration=.5f;
	public float startingWidth=.5f;
	
	void OnEnable(){
		LineRenderer line=gameObject.GetComponent<LineRenderer>();
		if(line!=null) StartCoroutine(Fade(line));
	}
	
	IEnumerator Fade(LineRenderer line){
		float durationModifier=1f/fadeDuration;
		
		float duration=0;
		while(duration<1){
			
			float width=Mathf.Lerp(startingWidth, 0, duration);
			line.SetWidth(width, width);
			
			duration+=Time.deltaTime*durationModifier;
			yield return null;
		}
		
		line.SetWidth(0, 0);
	}
	
}
