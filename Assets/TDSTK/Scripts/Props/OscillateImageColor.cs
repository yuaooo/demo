//for oscillating an UI image between 2 colors
//used for PlayerHitOverlay in UI

using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class OscillateImageColor : MonoBehaviour {
	
	public float freq=5;
	
	public float minR=0.5f;
	public float maxR=1f;
	
	public float minG=0.5f;
	public float maxG=1f;
	
	public float minB=0.5f;
	public float maxB=1f;
	
	
	private Image sprite;
	
	void Awake(){
		sprite=gameObject.GetComponent<Image>();
	}
	
	
	// Update is called once per frame
	void Update () {
		float multiplier=Mathf.Abs(Mathf.Sin(freq*Time.time*Mathf.PI));
		
		float r=minR+multiplier*(maxR-minR);
		float g=minG+multiplier*(maxG-minG);
		float b=minB+multiplier*(maxB-minB);
		
		sprite.color=new Color(r, g, b, sprite.color.a);
	}
}
