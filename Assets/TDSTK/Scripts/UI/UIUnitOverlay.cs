using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class UIUnitOverlay : MonoBehaviour {
		
		public Unit unit;
		
		private Vector2 fullSize;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private Image image;
		
		void Awake() {
			thisObj=gameObject;
			rectT=gameObject.GetComponent<RectTransform>();
			image=gameObject.GetComponent<Image>();
			
			fullSize=rectT.sizeDelta;
		}
		
		// Update is called once per frame
		void LateUpdate () {
			if(unit==null){
				if(thisObj.activeInHierarchy) thisObj.SetActive(false);
				return;
			}
			
			if(unit.hitPoint>=unit.hitPointFull){
				if(image.enabled) image.enabled=false;
			}
			else{
				if(!image.enabled) image.enabled=true;
			}
			
			
			if(!thisObj.activeInHierarchy) return;
			
			Vector3 screenPos=Camera.main.WorldToScreenPoint(unit.thisT.position+new Vector3(0, 2, 0));
			screenPos.z=0;
			rectT.localPosition=screenPos*UIMainControl.GetScaleFactor(); 
			
			rectT.sizeDelta=new Vector2((unit.hitPoint/unit.hitPointFull)*fullSize.x, fullSize.y);
		}
		
		
	}
	
}