using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;

using TDSTK;


namespace TDSTK_UI{

	public class UIMainControl : MonoBehaviour {

		private static UIMainControl instance;
		
		[Tooltip("Check to enable mouse and keyboard input")]
		public bool enableMouseNKeyInput=true; 
		
		[Space(10)]
		[Tooltip("Check to show hit-point overlay on top of each unit")]
		public bool enableHPOverlay=true;
		public static bool EnableHPOverlay(){ return instance.enableHPOverlay; }
		
		[Tooltip("Check to show damage overlay of each attack when hitting a target")]
		public bool enableTextOverlay=true;
		public static bool EnableTextOverlay(){ return instance.enableTextOverlay; }
		
		[Space(10)]
		[Tooltip("Check to have the HUD list all the ability available to the player, if there's any")]
		public bool listAllAbility=true;
		public static bool ListAllAbility(){ return instance.listAllAbility; }
		
		[Space(10)]
		[Tooltip("Check to enable the weapon and ability select tab, which can be bought up by holding TAB key")]
		public bool enableItemSelectTab=true;
		public static bool EnableItemSelectTab(){ return instance.enableItemSelectTab; }
		
		[Space(10)]
		[Tooltip("Check to show continue button to the next level on game-over menu even when the level is lost")]
		public bool showContinueButtonWhenLost=false;
		public static bool ShowContinueButtonWhenLost(){ return instance.showContinueButtonWhenLost; }
		
		[Space(10)]
		[Tooltip("The blur image effect component on the main ui camera (optional)")]
		public BlurOptimized uiBlurEffect;
		
		//~ [Tooltip("The CanvasScaler component of the main canvas. Required to have the overlay appear in the right screen position")]
		//~ public CanvasScaler scaler;
		//~ public static float GetScaleFactor(){ 
			//~ if(instance.scaler==null) return 1;
			//~ return (float)instance.scaler.referenceResolution.x/(float)Screen.width;
		//~ }
		
		[Space(10)]
		[Tooltip("Check to disable auto scale up of UIElement when the screen resolution exceed reference resolution specified in CanvasScaler/nRecommended to have this set to false when building for mobile")]
		public bool limitScale=true;
		
		[Tooltip("The CanvasScaler components of all the canvas. Required to have the floating UI elements appear in the right screen position")]
		public List<CanvasScaler> scalerList=new List<CanvasScaler>();
		public static float GetScaleFactor(){ 
			if(instance.scalerList.Count==0) return 1;
			
			if(instance.scalerList[0].uiScaleMode==CanvasScaler.ScaleMode.ConstantPixelSize) 
				return 1f/instance.scalerList[0].scaleFactor;
			if(instance.scalerList[0].uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize) 
				return (float)instance.scalerList[0].referenceResolution.x/(float)Screen.width;
			
			return 1;
		}
		
		
		
		private float scrollCD=0;	//for switching weapon using mouse scroll
		
		
		void Awake(){
			instance=this;
		}
		
		void Start(){
			if(limitScale){
				for(int i=0; i<scalerList.Count; i++){
					if(Screen.width>=scalerList[i].referenceResolution.x) instance.scalerList[i].uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
					else instance.scalerList[i].uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
				}
			}
		}
		
		void OnEnable() {
			TDS.onGameOverE += OnGameOver;
			TDS.onGameMessageE += OnGameMessage;
			
			TDS.onPlayerRespawnedE += OnPlayerRespawn;
		}
		void OnDisable() {
			TDS.onGameOverE -= OnGameOver;
			TDS.onGameMessageE -= OnGameMessage;
			
			TDS.onPlayerRespawnedE -= OnPlayerRespawn;
		}
		
		
		void OnPlayerRespawn(){
			UILevelPerkMenu.OnPlayerRespawn(GameControl.GetPlayer());
		}
		
		
		public static void OnGameMessage(string msg){ instance._OnGameMessage(msg); }
		public void _OnGameMessage(string msg){ UIMessage.Display(msg); }
		
		
		//called when the game is over
		public void OnGameOver(bool won){
			StartCoroutine(GameOverDelay(won));
		}
		IEnumerator GameOverDelay(bool won){
			yield return StartCoroutine(WaitForRealSeconds(.1f));
			CameraControl.FadeBlur(uiBlurEffect, 0, 2);
			CameraControl.TurnBlurOn();
			UIGameOver.Show(won);
			
			TDSTouchInput.Hide();
		}
		
		
		void Update(){
			if(!enableMouseNKeyInput) return;
			
			if(Input.GetKeyDown(KeyCode.Escape) && !GameControl.IsGameOver()){
				_TogglePause();
			}
			//if(Input.GetKeyDown(KeyCode.C) && !GameControl.IsGameOver()) ToggleLevelPerkMenu();
			
			/*
			if(Input.GetKeyDown(KeyCode.Q)){
				Debug.Log("Paused for screen shot. Fire from UIMainControl");
				if(Time.timeScale==1) Time.timeScale=0;
				else Time.timeScale=1;
			}
			*/
			
			if(GameControl.IsGamePlaying()){
				UnitPlayer player=GameControl.GetPlayer();
				if(player!=null && !player.IsDestroyed() && Input.touchCount==0){
					//Debug.Log("Fire!!  "+Time.time);
					
					//movement
					if(Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
						player.Move(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), Input.GetKey(KeyCode.LeftShift));
					
					//brake
					if(Input.GetKey(KeyCode.Space)) player.Brake();
					
					//switch weapon
					if(Input.GetAxisRaw("Mouse ScrollWheel")!=0 && scrollCD<=0){
						player.ScrollWeapon(Input.GetAxis("Mouse ScrollWheel")>0 ? 1 : -1);
						scrollCD=0.15f;
					}
					scrollCD-=Time.deltaTime;
					
					//turret facing
					player.AimTurretMouse(Input.mousePosition);
					//AimTurretDPad(Input.mousePosition-new Vector3(Screen.width/2, Screen.height/2));
					
					//fire
					bool continousFire=player.ContinousFire() & Input.GetMouseButton(0);
					if(Input.GetMouseButtonDown(0) || continousFire) player.FireWeapon();
					
					//alt fire, could fire weapon alt-mode to launch selected ability
					if(Input.GetMouseButtonDown(1)) player.FireAbility();
					
					//launch ability
					if(Input.GetMouseButtonDown(2)) player.FireAbilityAlt();
					
					//reload
					if(Input.GetKeyDown(KeyCode.R)) player.Reload();
					
					//bring up the chracter & perk menu
					if(UILevelPerkMenu.Enabled() && Input.GetKeyDown(KeyCode.C)) ToggleLevelPerkMenu();
				}
			}
			
		}
		
		
		
		public static void ToggleLevelPerkMenu(){ instance._ToggleLevelPerkMenu(); }
		public void _ToggleLevelPerkMenu(){
			if(GameControl.IsGamePlaying()) ShowLevelPerkMenu();
			else if(GameControl.IsGamePaused()) CloseLevelPerkMenu();
		}
		
		public static void ShowLevelPerkMenu(){ instance._ShowLevelPerkMenu(); }
		public void _ShowLevelPerkMenu(){
			CameraControl.FadeBlur(uiBlurEffect, 0, 2);
			CameraControl.TurnBlurOn();
			GameControl.PauseGame();	//Telling GameControl to paause the game
			UILevelPerkMenu.Show();
			
			TDSTouchInput.Hide();
			
			Time.timeScale=0;
		}
		public static void CloseLevelPerkMenu(){ instance.StartCoroutine(instance._CloseLevelPerkMenu()); }
		public IEnumerator _CloseLevelPerkMenu(){
			CameraControl.FadeBlur(uiBlurEffect, 2, 0);
			CameraControl.TurnBlurOff();
			UILevelPerkMenu.Hide();
			
			TDSTouchInput.Show();
			
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
			
			Time.timeScale=1;
			GameControl.ResumeGame();	//Telling GameControl to resume the game
		}
		
		
		
		public static void TogglePause(){ instance._TogglePause(); }
		public void _TogglePause(){
			if(GameControl.IsGamePlaying()) PauseGame();
			else if(GameControl.IsGamePaused()) ResumeGame();
		}
		
		
		public static void PauseGame(){ instance._PauseGame(); }
		public void _PauseGame(){
			CameraControl.FadeBlur(uiBlurEffect, 0, 2);
			CameraControl.TurnBlurOn();
			GameControl.PauseGame();	//Telling GameControl to paause the game
			UIPauseMenu.Show();
			
			TDSTouchInput.Hide();
			
			Time.timeScale=0;
		}
		public static void ResumeGame(){ instance.StartCoroutine(instance._ResumeGame()); }
		IEnumerator _ResumeGame(){
			CameraControl.FadeBlur(uiBlurEffect, 2, 0);
			CameraControl.TurnBlurOff();
			GameControl.ResumeGame();	//Telling GameControl to resume the game
			UIPauseMenu.Hide();
			
			TDSTouchInput.Show();
			
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
			Time.timeScale=1;
		}
		
		
		
		public static IEnumerator WaitForRealSeconds(float time){
			float start = Time.realtimeSinceStartup;
			while(Time.realtimeSinceStartup < start + time) yield return null;
		}
		
		
		
		public static void FadeOut(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			instance.StartCoroutine(instance._FadeOut(canvasGroup, 1f/duration, obj));
		}
		IEnumerator _FadeOut(CanvasGroup canvasGroup, float timeMul, GameObject obj){
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(1f, 0f, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=0f;
			
			if(obj!=null) obj.SetActive(false);
		}
		public static void FadeIn(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			instance.StartCoroutine(instance._FadeIn(canvasGroup, 1f/duration, obj)); 
		}
		IEnumerator _FadeIn(CanvasGroup canvasGroup, float timeMul, GameObject obj){
			if(obj!=null) obj.SetActive(true);
			
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(0f, 1f, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=1f;
		}
		
		
	}

}