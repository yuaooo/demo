using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class UIGameOver : MonoBehaviour {

		public Text lbTitle;
		
		public GameObject butContinueObj;
		
		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIGameOver instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			thisObj.SetActive(false);
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		public void OnContinueButton(){
			GameControl.LoadNextScene();
		}
		public void OnRestartButton(){
			GameControl.RestartScene();
		}
		public void OnMenuButton(){
			GameControl.LoadMainMenu();
		}
		
		
		public static void Show(bool won){ instance._Show(won); }
		public void _Show(bool won){
			Cursor.visible=true;
			
			if(won){
				lbTitle.text="Level Cleared!";
				butContinueObj.SetActive(true);
			}
			else{
				lbTitle.text="Level Lost";
				butContinueObj.SetActive(UIMainControl.ShowContinueButtonWhenLost());
			}
			
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
		}
	}

}