using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIPerkTabListDisplay : MonoBehaviour {
		
		private RectTransform contentRect;
		public List<UIPerkItem> itemList=new List<UIPerkItem>();
		
		public Text lbDesp;
		
		private PlayerPerk playerPerk;
		private PlayerProgression progress;
		
		public void SetPlayer(UnitPlayer player){
			if(player!=null){
				progress=player.GetPlayerProgression();
				playerPerk=player.GetPlayerPerk();
			}
		}
		
		// Use this for initialization
		public bool Init() {
			lbDesp.text=null;
			
			SetPlayer(GameControl.GetPlayer());
			
			if(progress==null || playerPerk==null) return false;
			
			if(progress!=null){
				List<PerkUnlockingAtLevel> unlockList=progress.stats.perkUnlockingAtLevelList;
				
				int count=0;
				
				for(int i=0; i<unlockList.Count; i++){
					int level=unlockList[i].level;
					for(int n=0; n<unlockList[i].perkIDList.Count; n++){
						if(count==0) itemList[0].Init();
						else if(count>0) itemList.Add(UIPerkItem.Clone(itemList[0].rootObj, "Item"+(i+1)));
						
						int perkIndex=playerPerk.GetPerkIndex(unlockList[i].perkIDList[n]);
						Perk perk=playerPerk.GetPerkFromIndex(perkIndex);
						
						itemList[i].perkID=perkIndex;
						
						itemList[i].imgIcon.sprite=perk.icon;
						itemList[i].label.text=perk.name;
						itemList[i].labelAlt.text="- Unlocked at level "+level;
						itemList[i].purchasedHighlight.SetActive(perk.purchased>0);
						
						itemList[i].SetCallback(this.OnHoverItem, this.OnExitItem, null, null);
						
						count+=1;
					}
				}
				
				contentRect=itemList[0].rectT.parent.GetComponent<RectTransform>();
				contentRect.sizeDelta=new Vector2(contentRect.sizeDelta.x, itemList.Count*55+5);
			}
			
			if(progress==null || playerPerk==null) return false;
			
			return true;
		}
		
		
		
		
		public void Show(){
			for(int i=0; i<itemList.Count; i++){
				Perk perk=playerPerk.GetPerkFromIndex(itemList[i].perkID);
				
				itemList[i].SetActive(true);
				
				if(perk.purchased>0) itemList[i].labelAlt.text="- Unlocked";
				
				itemList[i].purchasedHighlight.SetActive(perk.purchased>0);
				itemList[i].selectHighlight.SetActive(perk.purchased<=0);
			}
		}
		
		
		public void OnHoverItem(GameObject butObj){
			int index=GetItemIndex(butObj);
			
			Perk perk=playerPerk.GetPerkFromIndex(itemList[index].perkID);
			lbDesp.text=perk.desp;
		}
		public void OnExitItem(GameObject butObj){
			
		}
		
		public int GetItemIndex(GameObject butObj){
			for(int i=0; i<itemList.Count; i++){
				if(butObj==itemList[i].rootObj) return i;
			}
			return -1;
		}
		
	}

}