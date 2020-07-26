using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIBuffIcons : MonoBehaviour {

		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIGameOver instance;
		
		public List<UIObject> itemList=new List<UIObject>();

		public void Awake(){
			//instance=this;
			//thisObj=gameObject;
			//canvasGroup=thisObj.GetComponent<CanvasGroup>();
			//if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			for(int i=0; i<1; i++){
				if(i==0) itemList[0].Init();
				else itemList.Add(UIObject.Clone(itemList[0].rootObj, "icon"+(i+1)));
				itemList[i].rootObj.SetActive(false);
			}
		}

		
		void OnEnable(){
			TDS.onGainEffectE += NewEffect;
		}
		void OnDisable(){
			TDS.onGainEffectE -= NewEffect;
		}
		
		
		void NewEffect(Effect effect) {
			if(effect.ID<0) return;
			int idx=GetUnusedItem();
			StartCoroutine(ShowRoutine(effect, idx));
		}
		
		IEnumerator ShowRoutine(Effect effect, int idx){
			itemList[idx].imgIcon.sprite=effect.icon;
			itemList[idx].label.text=effect.duration.ToString("f1")+"s";
			
			itemList[idx].rootObj.SetActive(true);
			while(effect.duration>0 && !effect.expired){
				itemList[idx].label.text=effect.duration.ToString("f1")+"s";
				yield return null;
			}
			itemList[idx].rootObj.SetActive(false);
		}
		
		
		private int GetUnusedItem(){
			for(int i=0; i<itemList.Count; i++){
				if(itemList[i].rootObj.activeInHierarchy) continue;
				return i;
			}
			
			itemList.Add(UIObject.Clone(itemList[0].rootObj, "icon"+(itemList.Count+1)));
			return itemList.Count-1;
		}
		
	}

}