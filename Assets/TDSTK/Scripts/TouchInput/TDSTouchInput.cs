using UnityEngine;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class TDSTouchInput : MonoBehaviour {
		
		public UnitPlayer player;
		
		public TDSJoystick joystickMove;
		public TDSJoystick joystickAim;
		
		public GameObject fireButton;
		public GameObject weaponAbilityTabButton;
		
		public GameObject levelPerkButton;
		
		private static TDSTouchInput instance;
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			rectT.localPosition=Vector3.zero;
		}
		
		void Start(){
			player=GameControl.GetPlayer();
			
			joystickAim.transform.parent.gameObject.SetActive(player.enableTurretRotate);
			fireButton.SetActive(!player.enableTurretRotate);
			
			if(!UIMainControl.EnableItemSelectTab()) weaponAbilityTabButton.SetActive(false);
			if(!UILevelPerkMenu.Enabled()) levelPerkButton.SetActive(false);
			Debug.Log("StartStart  "+UILevelPerkMenu.Enabled());
		}
		
		//Update is called once per frame
		void Update () {
			if(player==null){
				player=GameControl.GetPlayer();
				if(player==null) return;
			}
			
			if(player.IsDestroyed()) return;
			
			if(joystickMove.GetMagnitude()>0) player.Move(joystickMove.GetValue());
			if(joystickAim.GetMagnitude()>0){
				player.AimTurretDPad(joystickAim.GetValue());
				player.FireWeapon();
			}
		}
		
		public void OnFireButton(){
			if(player!=null) player.FireWeapon();
		}
		public void OnReloadButton(){
			if(player!=null) player.Reload();
		}
		
		public void OnAbilityButton(){
			if(player!=null) player.FireAbility();
		}
		public void OnAbilityButtonAlt(){
			if(player!=null) player.FireAbilityAlt();
		}
		
		public void OnWeaponAbilityTab(){
			if(!UIWeaponAbilityTab.IsOn()){
				UIWeaponAbilityTab.TurnTabOn();
				_Hide();
			}
			else UIWeaponAbilityTab.TurnTabOff();
		}
		
		
		public void OnPrevWeapon(){
			if(player!=null) player.ScrollWeapon(-1);
		}
		public void OnNextWeapon(){
			if(player!=null) player.ScrollWeapon(1);
		}
		
		
		public void OnLevelPerkButton(){
			UIMainControl.ToggleLevelPerkMenu();
		}
		
		
		public void OnMenuButton(){
			UIMainControl.TogglePause();
		}
		
		
		public static void Hide(){ if(instance!=null) instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
			//thisObj.SetActive(false);
		}
		public static void Show(){ if(instance!=null) instance._Show(); }
		public void _Show(){
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
			//thisObj.SetActive(true);
		}
	}

}