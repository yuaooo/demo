using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIPerkTab : MonoBehaviour {
		
		private PlayerPerk playerPerk;
		
		[Space(5)]
		//public bool manuallySetup=false;
		public bool manualSetup=false;
		
		//[HideInInspector] public List<int> perkIDList=new List<int>();
		[HideInInspector] public List<int> unAvaiPerkIDList=new List<int>();
		
		[Header("PerkItemList (for manual setup)")]
		public List<UIPerkItem> perkItemList=new List<UIPerkItem>();
		private int selectID=0;
		
		[Header("UI Element Assignment")]
		public Text lbPerkPoint;
		public Text lbPerkRsc;
		
		public Text lbPerkName;
		public Text lbPerkDesp;
		public Text lbPerkReq;
		public Text lbPerkCost;
		
		public UIButton butPurchase;
		public UIButton butClose;
		
		
		public void SetPlayer(UnitPlayer player){
			if(player!=null) playerPerk=player.GetPlayerPerk();
		}
		
		
		public void Init(){
			SetPlayer(GameControl.GetPlayer());
			
			if(!manualSetup){
				layoutGroup.enabled=true;
				
				int count=0;
				
				List<Perk> perkList=playerPerk.GetPerkList();
				for(int i=0; i<perkList.Count; i++){
					if(unAvaiPerkIDList.Contains(perkList[i].ID)) continue;
					
					if(count==0) perkItemList[0].Init();
					else if(count>0) perkItemList.Add(UIPerkItem.Clone(perkItemList[0].rootObj, "PerkButton"+(count+1)));
					
					perkItemList[count].imgIcon.sprite=perkList[i].icon;
					perkItemList[count].perkID=perkList[i].ID;
					perkItemList[count].selectHighlight.SetActive(count==0);
					
					perkItemList[count].SetCallback(null, null, this.OnPerkItem, null);
					
					count+=1;
				}
				
				UpdateContentRectSize();
			}
			else{
				for(int i=0; i<perkItemList.Count; i++){
					perkItemList[i].Init();
					perkItemList[i].selectHighlight.SetActive(i==0);
					perkItemList[i].SetCallback(null, null, this.OnPerkItem, null);
				}
			}
			
			butPurchase.Init();
			if(butClose.rootObj!=null) butClose.Init();
			
			UpdatePerkItemList();
			UpdateDisplay();
		}
		
		
		
		
		void OnDisable(){ 
			butPurchase.SetActive(true);
			for(int i=0; i<perkItemList.Count; i++) perkItemList[i].SetActive(true);
		}
		
		
		
		public GridLayoutGroup layoutGroup;
		private void UpdateContentRectSize(){
			int rowCount=(int)Mathf.Ceil(perkItemList.Count/(float)layoutGroup.constraintCount);
			float size=rowCount*layoutGroup.cellSize.y+rowCount*layoutGroup.spacing.y+layoutGroup.padding.top;
			
			RectTransform contentRect=layoutGroup.gameObject.GetComponent<RectTransform>();
			contentRect.sizeDelta=new Vector2(contentRect.sizeDelta.x, size);
		}
		
		
		
		public void OnPerkItem(GameObject butObj, int pointerID=-1){
			int ID=GetButtonID(butObj);
			
			perkItemList[selectID].selectHighlight.SetActive(false);
			
			selectID=ID;
			
			perkItemList[selectID].selectHighlight.SetActive(true);
			UpdateDisplay();
		}
		
		int GetButtonID(GameObject butObj){
			for(int i=0; i<perkItemList.Count; i++){
				if(perkItemList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		
		void UpdateDisplay(){
			if(playerPerk==null) return;
			
			Perk perk=playerPerk.GetPerk(perkItemList[selectID].perkID);
			
			lbPerkName.text=perk.name;
			lbPerkDesp.text=perk.desp+"    ";
			
			if(perk.Purchased()){
				lbPerkReq.text="";
				
				lbPerkCost.text="";
				butPurchase.label.text="Purchased";
				butPurchase.button.interactable=false;
				return;
			}
			
			butPurchase.label.text="Purchase";
			
			string text=perk.IsAvailable();
			if(text==""){
				lbPerkCost.text="Cost: "+perk.cost;
				lbPerkReq.text="";
				butPurchase.button.interactable=true;
			}
			else{
				lbPerkCost.text="";
				lbPerkReq.text=text;
				butPurchase.button.interactable=false;
			}
		}
		
		void UpdatePerkItemList(){
			UnitPlayer player=GameControl.GetPlayer();
			if(player!=null) playerPerk=player.GetPlayerPerk();
			
			if(playerPerk==null) return;
			
			if(lbPerkPoint!=null) lbPerkPoint.text=playerPerk.GetPerkPoint().ToString();
			if(lbPerkRsc!=null) lbPerkRsc.text="Currency: "+playerPerk.GetPerkCurrency().ToString();
			
			for(int i=0; i<perkItemList.Count; i++){
				bool purchased=playerPerk.IsPerkPurchased(perkItemList[i].perkID);
				bool available=playerPerk.IsPerkAvailable(perkItemList[i].perkID)=="";
				perkItemList[i].purchasedHighlight.SetActive(purchased);
				perkItemList[i].unavailableHighlight.SetActive(!(purchased || available));
				if(perkItemList[i].connector!=null) perkItemList[i].connector.SetActive(purchased);
			}
		}
		
		
		
		public void OnPurchaseButton(){
			string text=playerPerk.PurchasePerk(perkItemList[selectID].perkID);
			
			if(text!=""){
				UIMessage.Display(text);
				return;
			}
			
			UpdatePerkItemList();
			
			UpdateDisplay();
		}
		
		
		public void Show(){
			UpdatePerkItemList();
			UpdateDisplay();
		}
		
		
	}

}