using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UILevelPerkMenu : MonoBehaviour {
		
		public enum _PerkTabType{LevelList, RepeatableList, Item, None}
		
		
		[Tooltip("Check if this menu is the only menu in the scene (being used as inter-level upgrade screen for instance)")]
		public bool LevelPerkMenuOnly=false;
		
		
		[Space(10)]
		[Tooltip("Check to enable the stats panel")]
		public bool enableStatsTab=true;
		public UIStatsTab uiStatsTab;
		
		
		[Space(10)]
		[Tooltip("The perk panel type:\n\nLevelList - A simple non-interactable list showing the perk to be unlocked at specific level\n\nRepeatableList - a list type perk menu designed for attribute style perk\n\nItem -  A full perk menu, with option to configure custom tech or skill-tree\n\nNone - Don't how any perk menu")]
		public _PerkTabType perkTabType=_PerkTabType.Item;
		
		[Space(10)]
		public UIPerkTabListDisplay uiLPerkTabDisplay;
		public UIPerkTabList uiPerkTabList;
		public UIPerkTab uiPerkTab;
		
		[Space(10)]
		public UIButton butClose;
		
		
		
		public static bool Enabled(){
			return instance.enableStatsTab | instance.perkTabType!=_PerkTabType.None;
		}
		
		
		
		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UILevelPerkMenu instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			if(!Enabled()){
				thisObj.SetActive(false);
				return;
			}
			
			butClose.Init();
			butClose.SetCallback(null, null, this.OnCloseButton, null);
			
			uiStatsTab.gameObject.SetActive(enableStatsTab);
			
			uiLPerkTabDisplay.gameObject.SetActive(perkTabType==_PerkTabType.LevelList);
			uiPerkTabList.gameObject.SetActive(perkTabType==_PerkTabType.RepeatableList);
			uiPerkTab.gameObject.SetActive(perkTabType==_PerkTabType.Item);
		}
		
		void Start(){
			if(enableStatsTab) uiStatsTab.Init();
			if(perkTabType==_PerkTabType.LevelList) uiLPerkTabDisplay.Init();
			if(perkTabType==_PerkTabType.RepeatableList) uiPerkTabList.Init();
			if(perkTabType==_PerkTabType.Item) uiPerkTab.Init();
			
			if(!LevelPerkMenuOnly){
				thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
				thisObj.SetActive(false);
				canvasGroup.alpha=0;
			}
			else{
				butClose.rootObj.SetActive(false);
				StartCoroutine(DelayShow());
			}
		}
		
		IEnumerator DelayShow(){
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			Show();
		}
		
		
		
		
		public static void OnPlayerRespawn(UnitPlayer player){ instance._OnPlayerRespawn(player); }
		public void _OnPlayerRespawn(UnitPlayer player){
			//UnitPlayer player=GameControl.GetPlayer();
			
			//uiStatsTab.SetPlayer(player);
			
			uiLPerkTabDisplay.SetPlayer(player);
			uiPerkTabList.SetPlayer(player);
			uiPerkTab.SetPlayer(player);
		}
		
		
		
		void Update(){
			if(LevelPerkMenuOnly) return;
			if(IsOn() && Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.C)) OnCloseButton();
		}
		
		public void OnCloseButton(GameObject butObj=null, int pointerID=-1){
			UIMainControl.CloseLevelPerkMenu();
		}
		
		
		
		private bool isOn=false;
		public static bool IsOn(){ return instance==null ? false : instance.isOn; }
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			if(enableStatsTab) uiStatsTab.Show();
			
			if(perkTabType==_PerkTabType.LevelList) uiLPerkTabDisplay.Show();
			else if(perkTabType==_PerkTabType.RepeatableList) uiPerkTabList.Show();
			else if(perkTabType==_PerkTabType.Item) uiPerkTab.Show();
			
			isOn=true;
			if(!LevelPerkMenuOnly) butClose.SetActive(true);
			
			Cursor.visible=true;
			
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			
			butClose.SetActive(false);
			Cursor.visible=false;
			
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return StartCoroutine(UIMainControl.WaitForRealSeconds(0.25f));
			isOn=false;
			
			thisObj.SetActive(false);
		}
		
	}

}