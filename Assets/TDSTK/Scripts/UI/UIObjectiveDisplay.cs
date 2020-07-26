//not in used atm

using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	
	//wip, not in used atm
	public class UIObjectiveDisplay : MonoBehaviour {

		public ObjectiveTracker objective;
		
		public Text lbObjectiveName;
		public Text lbObjectiveDesp;
		
		//~ public List<Text> lbObjective;
		//~ public Text lbObjective;
		
		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		//private static UIObjectiveDisplay instance;
		
		public void Awake(){
			if(objective==null) gameObject.SetActive(false);
			
			//instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			//canvasGroup.alpha=0;
			//thisObj.SetActive(false);
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
			
			//~ lbObjectiveName
		}
		
		
		// Update is called once per frame
		void Update () {
			if(objective.IsComplete()){
				lbObjectiveDesp.text="";
			}
			else{
				//int count=0;
				
				//~ if(objective.enableTimer){
					//~ if(objective.remainingDuration>0){
						//~ //float duration=objective.survivalDuration;
						//~ //int minO=(int)Mathf.Floor(duration/60);
						//~ //int secO=(int)Mathf.Floor(duration%60);
						
						//~ float remainDur=objective.remainingDuration;
						//~ int minR=(int)Mathf.Floor(remainDur/60);
						//~ int secR=(int)Mathf.Floor(remainDur%60);
						
						//~ //lbObjectiveDesp.text="survive for "+(minO>0 ? minO+"min " : "")+secO+"sec ("+(minR>0 ? minR+"min " : "")+secR+"sec remains)";
						//~ lbObjectiveDesp.text="- survive for "+(minR>0 ? minR+"min " : "")+secR+"sec";
					//~ }
					//~ else{
						//~ lbObjectiveDesp.text="- survive: completed";
					//~ }
				//~ }
				
				
				//~ if(objective.clearAllHostile){
					//~ lbObjectiveDespList.text="\n- Destroy all hostiles"
				//~ }
				//~ else if(objective.unitList.Count>0){
					//~ for(int i=0; i<objective.unitList.Count; i++){
						//~ lbObjectiveDespList.text="\n- Destroy "+objective.unitList[i].unitName;
					//~ }
				//~ }
				
				
				
			}
		}
		
	}

	
}