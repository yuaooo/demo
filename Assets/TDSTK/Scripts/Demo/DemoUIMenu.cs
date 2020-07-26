using UnityEngine;
using UnityEngine.UI;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;

public class DemoUIMenu : MonoBehaviour {

	public bool loadMobileScene=false;
	
	public GameObject tooltipObj;
	private RectTransform tooltipRectT;
	private Vector3 tooltipStartingPos;
	
	public Text lbTooltip;
	
	
	void Start(){
		tooltipObj.SetActive(false);
		
		tooltipRectT=tooltipObj.GetComponent<RectTransform>();
		tooltipStartingPos=tooltipRectT.localPosition;
	}
	
	
	public void OnButton(int butIndex){
		
		string prefix=loadMobileScene ? "Mobile" : "Demo";
		string sceneName="";
		
		if(butIndex==0){
			sceneName=prefix+"ShootingRange";
		}
		else if(butIndex==1){
			sceneName=prefix+"SideScroller";
		}
		else if(butIndex==2){
			sceneName=prefix+"Arena";
		}
		else if(butIndex==3){
			sceneName=prefix+"Gauntlet";
		}
		
		#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(sceneName);
		#else
			Application.LoadLevel(sceneName);
		#endif
		
	}
	
	
	public void OnHoverButton(int butIndex){
		string text="";
		
		if(butIndex==0){
			text="A simple level just to show case various weapon and ability supported. There no gameplay, just unlimited weapons and targets.\n\nUses a simple, passive leveling system. Gain perks and stats automatically.";
		}
		else if(butIndex==1){
			text="An endless side scroller shooter. shoot enemies, collect bonus and power up, then shoot more enemy.\n\nHas no leveling system.";
		}
		else if(butIndex==2){
			text="With a whole range of different weapons and abilities, try to survive for 2 minutes in this arena.\n\nUses a 'World of Warcraft' style leveling system and skill tree.";
		}
		else if(butIndex==3){
			text="A complete level consists of a series of set piece. Get to the end of the level and destroy the boss to win.\n\nUses a 'Diablo' style attribute system for leveling.";
		}
		
		tooltipRectT.localPosition=tooltipStartingPos+new Vector3(0, -butIndex*45, 0);
		
		lbTooltip.text=text;
		tooltipObj.SetActive(true);
	}
	public void OnExitButton(int butIndex){
		tooltipObj.SetActive(false);
	}
	
	
	public void OnLink(){
		Application.OpenURL("http://songgamedev.co.uk/");
	}
	
}
