using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;

using TDSTK;

namespace TDSTK{

	public class CameraControl : MonoBehaviour {
		
		[HideInInspector] public Transform thisT;
		[HideInInspector] public GameObject thisObj;
		
		private static CameraControl instance;
		
		private Camera cam;
		private Transform camT;
		public static Camera GetMainCamera(){ return instance.cam; }
		
		[HideInInspector] public BlurOptimized blurEffect;
		
		public float trackSpeed;
		
		public Vector3 posOffset;
		
		public bool enableLimit=false;
		public float minPosX=-30;
		public float minPosZ=-30;
		public float maxPosX=30;
		public float maxPosZ=30;
		
		public bool enableDynamicZoom=true;
		public float zoomNormalizeFactor=3;
		public float zoomSpeed=2;
		public float defaultZoom;
		public float defaultZoomOrtho;
		
		void Awake() {
			instance=this;
			thisT=transform;
			thisObj=gameObject;
			
			cam=thisObj.GetComponentInChildren<Camera>();
			camT=cam.transform;
			
			defaultZoom=camT.localPosition.z;
			defaultZoomOrtho=cam.orthographicSize;
			
			blurEffect=thisObj.GetComponentInChildren<BlurOptimized>();
			if(blurEffect!=null) blurEffect.enabled=false;
		}
		
		void Start(){
			if(trackSpeed>0 && GameControl.GetPlayer()!=null)
				thisT.position=GameControl.GetPlayer().thisT.position+posOffset;
		}
		
		void OnEnable(){
			TDS.onCameraShakeE += CameraShake;
		}
		void OnDisable(){
			TDS.onCameraShakeE -= CameraShake;
		}
		
		// Update is called once per frame
		void Update () {
			Shake();
			
			float wantedZoom=defaultZoom;
			float wantedZoomOrtho=cam.orthographicSize;
			
			UnitPlayer player=GameControl.GetPlayer();
			if(player!=null){
				Vector3 targetPos=player.thisT.position+posOffset;
				
				//if enabled limit is enabled, clamp the position so it wont go out of bound
				if(enableLimit){
					targetPos.x=Mathf.Clamp(targetPos.x, minPosX, maxPosX);
					targetPos.z=Mathf.Clamp(targetPos.z, minPosZ, maxPosZ);
				}
				
				//lerp to target's position
				thisT.position=Vector3.Lerp(thisT.position, targetPos, Time.deltaTime * trackSpeed);
				
				//adjust wanted zoom level based on player speed
				wantedZoom=defaultZoom*(1+(player.GetVelocity()/zoomNormalizeFactor));
				wantedZoomOrtho=defaultZoomOrtho*(1+(player.GetVelocity()/zoomNormalizeFactor));
			}
			
			//if dynamicZoom is enabled, adjust the zoom acording to wanted zoom level
			if(enableDynamicZoom){
				camT.localPosition=new Vector3(0, 0, Mathf.Lerp(camT.localPosition.z, wantedZoom, Time.deltaTime*zoomSpeed));
				cam.orthographicSize=Mathf.Lerp(cam.orthographicSize, wantedZoomOrtho, Time.deltaTime*zoomSpeed);
			}
		}
		
		
		//called by the UI during game paused or game over to turn the bluring effect on/off
		public static void TurnBlurOn(){
			if(instance==null || instance.blurEffect==null) return;
			instance.StartCoroutine(instance.FadeBlurRoutine(instance.blurEffect, 0, 2));
		}
		public static void TurnBlurOff(){
			if(instance==null || instance.blurEffect==null) return;
			instance.StartCoroutine(instance.FadeBlurRoutine(instance.blurEffect, 2, 0));
		}
		
		public static void FadeBlur(BlurOptimized blurEff, float startValue=0, float targetValue=0){
			if(blurEff==null && instance==null) return;
			instance.StartCoroutine(instance.FadeBlurRoutine(blurEff, startValue, targetValue));
		}
		//change the blur component blur size from startValue to targetValue over 0.25 second
		IEnumerator FadeBlurRoutine(BlurOptimized blurEff, float startValue=0, float targetValue=0){
			blurEff.enabled=true;
			
			float duration=0;
			while(duration<1){
				float value=Mathf.Lerp(startValue, targetValue, duration);
				blurEff.blurSize=value;
				duration+=Time.unscaledDeltaTime*4f;	//multiply by 4 so it only take 1/4 of a second
				yield return null;
			}
			blurEff.blurSize=targetValue;
			
			if(targetValue==0) blurEff.enabled=false;
			if(targetValue==1) blurEff.enabled=true;
		}
		
		
		
		//camera shake
		public float shakeMultiplier=0.5f;		//check the inspector
		private float camShakeMagnitude=0;	//current active shake magnitude
		
		//delagates of TDS.onCameraShakeE
		public void CameraShake(float magnitude=1){
			if(magnitude==0) return;
			instance.camShakeMagnitude=magnitude*0.5f;
		}
		//called from Update()
		public void Shake(){
			if(Time.timeScale==0) return;	//dont execute if the game is paused
			
			//randomize the camera transform x and y local position, create a shaking effect
			float x=2*(Random.value-0.5f)*camShakeMagnitude*shakeMultiplier;
			float y=2*(Random.value-0.5f)*camShakeMagnitude*shakeMultiplier;
			camT.localPosition=new Vector3(x, y, camT.localPosition.z);
			
			//reduce the shake magnitude overtime
			camShakeMagnitude*=(1-Time.deltaTime*5);
		}
		
		
		
		
		
		public bool showGizmo=true;
		void OnDrawGizmos(){
			if(enableLimit && showGizmo){
				Vector3 p1=new Vector3(minPosX, transform.position.y, maxPosZ);
				Vector3 p2=new Vector3(maxPosX, transform.position.y, maxPosZ);
				Vector3 p3=new Vector3(maxPosX, transform.position.y, minPosZ);
				Vector3 p4=new Vector3(minPosX, transform.position.y, minPosZ);
				
				Gizmos.color=new Color(0f, 1f, 1f, 1f);
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p3);
				Gizmos.DrawLine(p3, p4);
				Gizmos.DrawLine(p4, p1);
			}
		}
		
	}

}