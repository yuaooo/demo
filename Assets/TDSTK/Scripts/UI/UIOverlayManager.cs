using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIOverlayManager : MonoBehaviour {
		
		public List<Unit> unitList=new List<Unit>();
		public List<UIUnitOverlay> unitOverlayList=new List<UIUnitOverlay>();
		
		private static UIOverlayManager instance;
		public static UIOverlayManager GetInstance(){ return instance; }
		
		// Use this for initialization
		void Awake() {
			instance=this;
			
			for(int i=0; i<20; i++){
				if(i>0){
					GameObject newObj=UI.Clone(unitOverlayList[0].gameObject);
					unitOverlayList.Add(newObj.GetComponent<UIUnitOverlay>());
				}
				unitOverlayList[i].gameObject.SetActive(false);
			}
		}
		
		
		void OnEnable(){
			TDS.onNewUnitE += NewUnit;
		}
		void OnDisable(){
			TDS.onNewUnitE -= NewUnit;
		}
		
		
		public static void NewUnit(Unit unit){ instance._NewUnit(unit); }
		public void _NewUnit(Unit unit){
			if(!UIMainControl.EnableHPOverlay()) return;
			
			for(int i=0; i<unitOverlayList.Count; i++){
				if(unitOverlayList[i].unit!=null) continue;
				
				unitOverlayList[i].unit=unit;
				unitOverlayList[i].gameObject.SetActive(true);
				
				return;
			}
			
			GameObject newObj=UI.Clone(unitOverlayList[0].gameObject);
			unitOverlayList.Add(newObj.GetComponent<UIUnitOverlay>());
			
			unitOverlayList[unitOverlayList.Count-1].unit=unit;
			unitOverlayList[unitOverlayList.Count-1].gameObject.SetActive(true);
		}
		
	}

	
}