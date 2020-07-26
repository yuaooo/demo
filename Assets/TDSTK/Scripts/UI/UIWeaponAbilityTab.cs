using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIWeaponAbilityTab : MonoBehaviour {
		
		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIWeaponAbilityTab instance;
		//public static UIWeaponAbilityTab GetInstance(){ return instance; } 
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		
		public GameObject tabObject;
		
		public List<UISelectItem> abilityItemList=new List<UISelectItem>();
		public List<UISelectItem> weaponItemList=new List<UISelectItem>();
		
		IEnumerator Start(){
			yield return null;
			
			if(!UIMainControl.EnableItemSelectTab()) thisObj.SetActive(false);
			
			List<Ability> abilityList=AbilityManager.GetAbilityList();
			for(int i=0; i<abilityList.Count; i++){
				if(i==0) abilityItemList[i].Init();
				else abilityItemList.Add(UISelectItem.Clone(abilityItemList[0].rootObj, "Item"+(i+1)));
				
				abilityItemList[i].imgIcon.sprite=abilityList[i].icon;
				abilityItemList[i].label.text=abilityList[i].name;
				
				abilityItemList[i].selectHighlight.SetActive(false);
				//abilityItemList[i].buttonObj.SetActive(false);
			}
			if(abilityList.Count==0) abilityItemList[0].rootObj.SetActive(false); 
			
			
			UnitPlayer player=GameControl.GetPlayer();
			for(int i=0; i<player.weaponList.Count; i++){
				if(i==0) weaponItemList[i].Init();
				else weaponItemList.Add(UISelectItem.Clone(weaponItemList[0].rootObj, "Item"+(i+1)));
				
				weaponItemList[i].imgIcon.sprite=player.weaponList[i].icon;
				weaponItemList[i].label.text=player.weaponList[i].weaponName;
				
				weaponItemList[i].selectHighlight.SetActive(false);
				//weaponItemList[i].buttonObj.SetActive(false);
			}
			
			tabObject.SetActive(false);
		}
		
		
		
		void OnEnable(){
			TDS.onNewWeaponE += OnNewWeapon;
			TDS.onNewAbilityE += OnNewAbility;
		}
		void OnDisable(){
			TDS.onNewWeaponE -= OnNewWeapon;
			TDS.onNewAbilityE -= OnNewAbility;
		}
		
		
		void OnNewWeapon(Weapon weapon, int replaceIndex=-1){
			if(replaceIndex>=0){
				weaponItemList[replaceIndex].imgIcon.sprite=weapon.icon;
				weaponItemList[replaceIndex].label.text=weapon.weaponName;
			}
			else{
				int index=weaponItemList.Count;
				weaponItemList.Add(UISelectItem.Clone(weaponItemList[0].rootObj, "Item"+(index)));
				
				weaponItemList[index].imgIcon.sprite=weapon.icon;
				weaponItemList[index].label.text=weapon.weaponName;
				
				weaponItemList[index].rootObj.SetActive(true); 
				weaponItemList[index].selectHighlight.SetActive(false);
			}
		}
		void OnNewAbility(Ability ability, int replaceIndex=-1){
			if(replaceIndex>=0){
				abilityItemList[replaceIndex].imgIcon.sprite=ability.icon;
				abilityItemList[replaceIndex].label.text=ability.name;
			}
			else{
				int index=abilityItemList.Count;
				abilityItemList.Add(UISelectItem.Clone(abilityItemList[0].rootObj, "Item"+(index)));
				
				abilityItemList[index].imgIcon.sprite=ability.icon;
				abilityItemList[index].label.text=ability.name;
				
				abilityItemList[index].rootObj.SetActive(true); 
				abilityItemList[index].selectHighlight.SetActive(false);
			}
		}
		
		
		
		void UpdateTab(){
			for(int i=0; i<AbilityManager.GetAbilityCount(); i++){
				if(i==AbilityManager.GetSelectID()){
					abilityItemList[i].selectHighlight.SetActive(true);
					abilityItemList[i].button.interactable=false;
				}
				else{
					abilityItemList[i].selectHighlight.SetActive(false);
					abilityItemList[i].button.interactable=true;
				}
			}
			
			UnitPlayer player=GameControl.GetPlayer();
			for(int i=0; i<player.weaponList.Count; i++){
				string clip=player.weaponList[i].currentClip<0 ? "∞" : player.weaponList[i].currentClip.ToString();
				string ammo=player.weaponList[i].ammo<0 ? "∞" : player.weaponList[i].ammo.ToString();
				weaponItemList[i].labelAlt.text=clip+"/"+ammo;
				
				if(i==player.weaponID){
					weaponItemList[i].selectHighlight.SetActive(true);
					weaponItemList[i].button.interactable=false;
				}
				else{
					weaponItemList[i].selectHighlight.SetActive(false);
					weaponItemList[i].button.interactable=true;
				}
			}
		}
		
		public void OnSelectWeapon(GameObject butObj){
			int newID=0;
			for(int i=0; i<weaponItemList.Count; i++){
				if(butObj==weaponItemList[i].buttonObj){
					newID=i;	break;
				}
			}
			
			int weaponID=GameControl.GetPlayer().weaponID;
			if(newID==weaponID) return;
			
			weaponItemList[newID].selectHighlight.SetActive(true);
			weaponItemList[newID].button.interactable=false;
			
			weaponItemList[weaponID].selectHighlight.SetActive(false);
			weaponItemList[weaponID].button.interactable=true;
			
			GameControl.GetPlayer().SwitchWeapon(newID);
		}
		
		public void OnSelectAbility(GameObject butObj){
			int newID=0;
			for(int i=0; i<abilityItemList.Count; i++){
				if(butObj==abilityItemList[i].buttonObj){
					newID=i;	break;
				}
			}
			
			int abilityID=AbilityManager.GetSelectID();
			if(newID==abilityID) return;
			
			abilityItemList[newID].selectHighlight.SetActive(true);
			abilityItemList[newID].button.interactable=false;
			
			
			abilityItemList[abilityID].selectHighlight.SetActive(false);
			abilityItemList[abilityID].button.interactable=true;
			
			AbilityManager.Select(newID);
		}
		
		
		void Update () {
			if(Input.GetKeyDown(KeyCode.Tab)){
				_TurnTabOn();
			}
			else if(Input.GetKeyUp(KeyCode.Tab)){
				_TurnTabOff();
			}
		}
		
		
		public static void TurnTabOn(){ instance._TurnTabOn(); }
		public void _TurnTabOn(){
			Time.timeScale=0;
			Cursor.visible=true;
			
			isOn=true;
			
			TDSTouchInput.Hide();
			
			GameControl.GetPlayer().DisableFire();
			UpdateTab();
			tabObject.SetActive(true);
		}
		public static void TurnTabOff(){ instance._TurnTabOff(); }
		public void _TurnTabOff(){
			Time.timeScale=1;
			Cursor.visible=false;
			
			isOn=false;
			
			TDSTouchInput.Show();
			
			GameControl.GetPlayer().EnableFire();
			tabObject.SetActive(false);
		}
		
		
		public void OnCloseButton(){
			_TurnTabOff();
		}
		
		
		private bool isOn=false;
		public static bool IsOn(){ return instance.isOn; }
		
	}

}