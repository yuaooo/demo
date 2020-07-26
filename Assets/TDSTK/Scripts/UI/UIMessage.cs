using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIMessage : MonoBehaviour {
		
		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIMessage instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		
		public bool enableZoom=true;
		public bool fadeOut=true;
		public bool scaleDown=true;
		
		public GameObject msgObj;
		private Transform msgT;
		private List<Transform> msgTList=new List<Transform>();
		
		private int msgCounter=1;
		private Color defaultColor;
		
		void Start(){
			msgT=msgObj.transform;
			msgObj.SetActive(false);
			
			CanvasGroup canvas=msgObj.GetComponent<CanvasGroup>();
			if(canvas==null) canvas=msgObj.AddComponent<CanvasGroup>();
			
			canvas.alpha=1;
			canvas.interactable=false;
			canvas.blocksRaycasts=false;
			
			defaultColor=msgObj.GetComponent<Text>().color;
		}
		
		
		
		
		public static void Display(string msg, Color color=default(Color)){ instance._Display(msg, color);	}
		void _Display(string msg, Color color=default(Color)){
			if(msgObj==null) return;
			
			if(color==default(Color)) color=defaultColor;
			
			int counter=msgTList.Count+1;
			for(int i=0; i<msgTList.Count; i++){
				Vector3 pos=msgT.localPosition+new Vector3(0, (counter-=1)*20, 0);
				TweenPosition(msgTList[i], .15f, pos);
			}
			
			GameObject obj=UI.Clone(msgObj, "message"+msgCounter);
			obj.GetComponent<RectTransform>().sizeDelta=new Vector2(0, 0);
			obj.GetComponent<Text>().color=color;
			obj.GetComponent<Text>().text=msg;
			obj.SetActive(true);
			
			msgTList.Add(obj.transform);
			StartCoroutine(DestroyMessage(obj));
			
			msgCounter+=1;
		}
		
		
		
		IEnumerator DestroyMessage(GameObject obj){
			if(enableZoom){
				TweenScale(obj.transform, 0.1f, new Vector3(1.1f, 1.1f, 1.1f));
				yield return new WaitForSeconds(0.2f);
				TweenScale(obj.transform, 0.2f, new Vector3(1.0f, 1.0f, 1.0f));
			}
			
			
			float duration=0;
			while(duration<1.25f){ duration+=Time.unscaledDeltaTime;	yield return null; }
			
			if(fadeOut) UIMainControl.FadeOut(obj.GetComponent<CanvasGroup>(), 0.5f);
			if(scaleDown) TweenScale(obj.transform, 0.5f, new Vector3(0.5f, 0.5f, 0.5f));
			
			duration=0;
			while(duration<0.75f){ duration+=Time.unscaledDeltaTime; 	yield return null; }
			
			msgTList.Remove(obj.transform);
			Destroy(obj);
		}
		
		
		
		private void TweenPosition(Transform objT, float duration, Vector3 targetPos){
			StartCoroutine(_TweenPosition(objT, 1f/duration, targetPos));
		}
		private IEnumerator _TweenPosition(Transform objT, float timeMul, Vector3 targetPos){
			Vector3 startPos=objT.localPosition;
			
			float duration=0;
			while(duration<1){
				if(objT==null) yield break;
				objT.localPosition=Vector3.Lerp(startPos, targetPos, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			if(objT!=null) objT.localPosition=targetPos;
		}
		
		
		
		
		
		private void TweenScale(Transform objT, float duration, Vector3 targetScale){
			StartCoroutine(_TweenScale(objT, 1f/duration, targetScale));
		}
		private IEnumerator _TweenScale(Transform objT, float timeMul, Vector3 targetScale){
			Vector3 startScale=objT.localScale;
			
			float duration=0;
			while(duration<1){
				if(objT==null) yield break;
				objT.localScale=Vector3.Lerp(startScale, targetScale, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			if(objT!=null) objT.localScale=targetScale;
		}
		
		
		
		
	}

}