//the component that control the selection, cooldown and activation of ability

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class AbilityManager : MonoBehaviour {
		
		private List<Ability> abilityList=new List<Ability>();
		public static List<Ability> GetAbilityList(){ return instance.abilityList; }
		public static int GetAbilityCount(){ return instance.abilityList.Count; }
		
		private int selectedID=-99;
		public static int GetSelectID(){ return instance.selectedID; }
		
		
		private static AbilityManager instance;
		public static AbilityManager GetInstance(){ return instance; }
		
		
		void Awake(){
			//make sure there's only 1 instance of this
			if(instance!=null){
				Destroy(this);
				return;
			}
			
			instance=this;
		}
		
		
		
		//called by the current UnitPlayer to setup ability
		public static void SetupAbility(List<int> abIDlist, bool enableAll){ 
			if(instance!=null) instance._SetupAbility(abIDlist, enableAll);
			else Debug.LogWarning("No AbilityManager in the scene, Abilities has been disabled", null);
		}
		public void _SetupAbility(List<int> abIDlist, bool enableAll){
			abilityList=new List<Ability>();
			
			//cloen the ability from DB list (so they dont get modified in runtime)
			if(enableAll) abilityList=AbilityDB.LoadClone();
			else{
				for(int i=0; i<abIDlist.Count; i++){
					abilityList.Add(AbilityDB.CloneItem(abIDlist[i]));
					abilityList[i].Init();
				}
			}
			
			//select an ability, if there's any
			Select((abilityList.Count==0) ? -1 : 0 );
		}
		
		public static void AddAbility(int abID, int replaceID=-1){ instance._AddAbility(abID, replaceID); }
		public void _AddAbility(int abID, int replaceID=-1){
			Ability newAbility=AbilityDB.CloneItem(abID);
			if(newAbility==null) return;
			
			int slotID=-1;
			if(replaceID>=0){
				for(int i=0; i<abilityList.Count; i++){
					if(abilityList[i].ID==replaceID){ slotID=i;	break; }
				}
			}
			
			if(slotID<0){
				slotID=abilityList.Count;
				abilityList.Add(null);
			}
			
			abilityList[slotID]=newAbility;
			abilityList[slotID].Init();
			
			if(replaceID<0 || slotID<0) TDS.NewAbility(abilityList[slotID]);
			else TDS.NewAbility(newAbility, slotID);
			
			Select(slotID);
		}
		
		
		//check if a particular ability is ready based on the passed index
		public static string IsAbilityReady(int index=-1){ return instance._IsAbilityReady(index); }
		public string _IsAbilityReady(int index=-1){
			index=index<0 ? selectedID : index ;	//if no index has been passed, check the selected ability
			if(index<0 || index>=abilityList.Count) return "Invalid ability";
			
			return abilityList[index].IsReady();
		}
		
		
		public static void TriggerAbility(int ID){	//for trigger and collectible that activate an ability
			if(instance==null) return;
			
			LaunchAbility(AbilityDB.CloneItem(ID), false);
		}
		
		
		//function call to launch an ability, based on the passed index
		//for player ability, check the avaibility of the ability before launch
		public static void LaunchAbility(int index=-1){ instance._LaunchAbility(index); }
		public void _LaunchAbility(int index=-1){
			if(instance==null) return;
			
			//make sure we have a valid selected ability
			index=index<0 ? selectedID : index ;
			if(index<0 || index>=abilityList.Count) return;
			
			//check if the selected ability is ready
			string status=abilityList[index].IsReady();
			//if the abilty is not ready, fire event explaining why (for UI)
			if(status!=""){
				TDS.AbilityActivationFailFail(status);
				return; 
			}
			
			//call function to launch ability
			LaunchAbility(abilityList[index]);
		}
		
		//function call to launch the passed ability
		//for launching weapon alt-fire
		public static void LaunchAbility(Ability ability, bool useCostNCD=true){
			bool teleport=ability.type==_AbilityType.Movement & ability.moveType==_MoveType.Teleport;
			if(ability.type==_AbilityType.AOE || ability.type==_AbilityType.Shoot || teleport){
				//get the hit point and activate the ability on that particular spot
				Ray ray = CameraControl.GetMainCamera().ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit, Mathf.Infinity)) ability.Activate(hit.point);
				else ability.Activate(GameControl.GetPlayer().thisT.position); //use player position if there's no valid position
			}
			else{
				//activate the ability on the player position
				ability.Activate(GameControl.GetPlayer().thisT.position);
			}
		}
		
		
		
		// Update is called once per frame
		void Update () {
			if(!GameControl.EnableAbility()) return;
			
			//iterates cooldown
			for(int i=0; i<abilityList.Count; i++) abilityList[i].currentCD-=Time.deltaTime;
			
			//change selected ability
			if(Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
			if(Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
			if(Input.GetKeyDown(KeyCode.Alpha3)) Select(2);
			if(Input.GetKeyDown(KeyCode.Alpha4)) Select(3);
			if(Input.GetKeyDown(KeyCode.Alpha5)) Select(4);
			if(Input.GetKeyDown(KeyCode.Alpha6)) Select(5);
			if(Input.GetKeyDown(KeyCode.Alpha7)) Select(6);
			if(Input.GetKeyDown(KeyCode.Alpha8)) Select(7);
			if(Input.GetKeyDown(KeyCode.Alpha9)) Select(8);
			if(Input.GetKeyDown(KeyCode.Alpha0)) Select(9);
		}
		
		
		//select a particular ability
		public static void Select(int newID){
			if(newID==instance.selectedID) return;
			if(newID<0 || newID>=instance.abilityList.Count) return;
			
			instance.selectedID=newID;
			TDS.SwitchAbility(GetSelectedAbility());	//launch the ability switch event to inform the UI
		}
		
		
		public static Ability GetSelectedAbility(){
			if(instance.abilityList.Count==0) return null;
			return instance.abilityList[instance.selectedID];
		}
		
		
	}
	
	
}