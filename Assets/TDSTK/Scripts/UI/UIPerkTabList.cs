using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIPerkTabList : MonoBehaviour {
		
		[Header("UI Element Assignment")]
		public List<UIPerkItem> itemList=new List<UIPerkItem>();
		
		public Text lbPerkPoint;
		public Text lbPerkCurrency;
		
		
		[HideInInspector] public List<int> perkIDList=new List<int>();

		
		
		public Text lbDesp;
		
		private List<UIButton> unlockButtonList=new List<UIButton>();
		//private List<int> perkIndexList=new List<int>();	//perk index perkList corresponding to each item
		
		private PlayerPerk playerPerk;
		
		
		public void SetPlayer(UnitPlayer player){
			if(player!=null) playerPerk=player.GetPlayerPerk();
		}
		
		
		// Use this for initialization
		public void Init() {
			SetPlayer(GameControl.GetPlayer());
			
			
			if(playerPerk!=null){
				List<Perk> perkList=playerPerk.GetPerkList();
				
				int count=0;
				
				for(int i=0; i<perkIDList.Count; i++){
					for(int n=0; n<perkList.Count; n++){
						if(perkList[n].ID!=perkIDList[i]) continue;
						
						Perk perk=perkList[n];
						
						if(count==0) itemList[0].Init();
						else if(count>0) itemList.Add(UIPerkItem.Clone(itemList[0].rootObj, "Item"+(i+1)));
						
						itemList[i].perkID=n;		//perk index in perkList
						
						itemList[i].imgIcon.sprite=perk.icon;
						itemList[i].label.text=perk.name;
						
						//if(showCost) itemList[i].labelAlt2.text=perk.cost.ToString();
						//else itemList[i].labelAlt2.text="";
						
						//itemList[i].labelAlt.text=perk.purchased+"/"+perk.limit;
						
						itemList[i].SetCallback(this.OnHoverItem, this.OnExitItem, null, null);
						
						foreach(Transform child in itemList[i].rootObj.transform){
							if(child.name=="ButtonUnlock"){
								UIButton button=new UIButton(child.gameObject);
								button.SetCallback(this.OnHoverButton, this.OnExitButton, this.OnUnlockButton, null);
								unlockButtonList.Add(button);
							}
						}
						
						count+=1;
						
						break;
						
					}
				}
				
			}
			
			if(perkIDList.Count==0){
				if(itemList.Count>0){
					for(int i=0; i<itemList.Count; i++){
						if(itemList[i].rootObj!=null) itemList[i].rootObj.SetActive(false);
					}
				}
				itemList=new List<UIPerkItem>();
			}
			
			UpdateDisplay();
			
			lbDesp.text="";
		}
		
		
		
		
		void UpdateDisplay(){
			int currency=playerPerk.GetPerkCurrency();
			lbPerkCurrency.text=currency.ToString();
			
			for(int i=0; i<itemList.Count; i++){
				Perk perk=playerPerk.GetPerkList()[itemList[i].perkID];
				string limitText=perk.limit>0 ? "/"+perk.limit.ToString() : "" ;
				itemList[i].labelAlt.text=perk.purchased+limitText;
				
				bool valid=currency>=perk.cost;
				itemList[i].button.interactable=!(valid && perk.limit>0 && perk.purchased>=perk.limit);
			}
			
			lbDesp.text="";
		}
		
		
		public void Show(){
			UpdateDisplay();
		}
		
		
		
		public void OnUnlockButton(GameObject butObj, int pointerID=-1){
			int index=GetButtonIndex(butObj);
			if(index==-1) return;
			
			playerPerk.PurchasePerk(playerPerk.GetPerkFromIndex(itemList[index].perkID));
			
			UpdateDisplay();
		}
		public void OnHoverButton(GameObject butObj){
			int index=GetButtonIndex(butObj);
			if(index==-1) return;
			
			lbDesp.text=playerPerk.GetPerkList()[itemList[index].perkID].desp;
		}
		public void OnExitButton(GameObject butObj){
			int index=GetButtonIndex(butObj);
			if(index==-1) return;
			
			lbDesp.text="";
		}
		
		public void OnHoverItem(GameObject butObj){
			int index=GetItemIndex(butObj);
			if(index==-1) return;
			
			lbDesp.text=playerPerk.GetPerkFromIndex(itemList[index].perkID).desp;
		}
		public void OnExitItem(GameObject butObj){
			int index=GetItemIndex(butObj);
			if(index==-1) return;
			
			lbDesp.text="";
		}
		
		public int GetButtonIndex(GameObject butObj){
			for(int i=0; i<unlockButtonList.Count; i++){
				if(butObj==unlockButtonList[i].rootObj) return i;
			}
			return -1;
		}
		public int GetItemIndex(GameObject butObj){
			for(int i=0; i<itemList.Count; i++){
				if(butObj==itemList[i].rootObj) return i;
			}
			return -1;
		}
		
	}

}