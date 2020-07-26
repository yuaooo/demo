using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class UIPauseMenu : MonoBehaviour {

		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIPauseMenu instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			thisObj.SetActive(false);
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		public void OnResumeButton(){
			UIMainControl.ResumeGame();
		}
		public void OnRestartButton(){
			GameControl.RestartScene();
		}
		public void OnOptionButton(){
			
		}
		public void OnMenuButton(){
			GameControl.LoadMainMenu();
		}
		
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			Cursor.visible=true;
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			Cursor.visible=false;
			UIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
		}
	}

}