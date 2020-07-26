using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class UICursor : MonoBehaviour {

		public RectTransform cursorT;
		
		public Text lbAmmo;
		public GameObject defaultCursorObj;
		public GameObject reloadingCursorObj;
		public Image imgReloading;
		
		private RectTransform rectDefaultCursor;
		private Vector2 cursorDefaultSize;
		
		void Awake(){
			Cursor.visible=false;
			
			rectDefaultCursor=defaultCursorObj.GetComponent<RectTransform>();
			cursorDefaultSize=rectDefaultCursor.sizeDelta;
			
			StartCoroutine(Loop());
		}
		
		private int dotCounter=0;
		IEnumerator Loop(){
			while(true){
				dotCounter+=1;
				if(dotCounter>3) dotCounter=0;
				yield return new WaitForSeconds(0.25f);
			}
		}
			
		void Update () {
			UnitPlayer player=GameControl.GetPlayer();
			if(player==null) return;
			
			cursorT.localPosition=Input.mousePosition*UIMainControl.GetScaleFactor();
			
			rectDefaultCursor.sizeDelta=cursorDefaultSize+new Vector2(player.GetRecoil()*2, player.GetRecoil()*2);
			
			if(player.Reloading()){
				ShowReloading();
				imgReloading.fillAmount=player.GetCurrentReload()/player.GetReloadDuration();
			}
			else{
				HideReloading();
			}
		}
		
		private bool reloading=false;
		void ShowReloading(){
			if(reloading) return;
			reloading=true;
			defaultCursorObj.SetActive(false);
			reloadingCursorObj.SetActive(true);
		}
		void HideReloading(){
			if(!reloading) return;
			reloading=false;
			defaultCursorObj.SetActive(true);
			reloadingCursorObj.SetActive(false);
		}
	}

	
}