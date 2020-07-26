using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK_UI{

	public class UIStatsTab	: MonoBehaviour {

		public Slider sliderExp;
		public Slider sliderHP;
		public Slider sliderEnergy;
		
		public Text lbLevel;
		public Text lbExpBar;
		public Text lbHPBar;
		public Text lbEnergyBar;
		public Text lbStats;
		
		private UnitPlayer player;
		private PlayerProgression progress;
		
		
		
		// Use this for initialization
		public void Init() {
			SetPlayer(GameControl.GetPlayer());
		}
		
		public void SetPlayer(UnitPlayer playerUnit){
			player=playerUnit;
			if(player!=null) progress=player.GetPlayerProgression();
		}
		
		
		void OnEnable(){
			TDS.onPerkPurchasedE += OnPerkPurchased;
		}
		void OnDisable(){
			TDS.onPerkPurchasedE -= OnPerkPurchased;
		}
		
		
		void OnPerkPurchased(Perk perk){ 
			StartCoroutine(DelayedShow());
		}
		
		IEnumerator DelayedShow(){
			yield return null;
			Show();
		}
		
		
		
		public void Show(){
			float playerFullHP=player.GetFullHitPoint();
			float playerFullEnergy=player.GetFullEnergy();
			
			sliderExp.value=Mathf.Max(0.01f, progress.GetCurrentLevelProgress());
			sliderHP.value=player.hitPoint/playerFullHP;
			sliderEnergy.value=player.energy/playerFullEnergy;
			
			lbLevel.text="lvl-"+progress.GetLevel();
			
			lbExpBar.text=progress.GetCurrentExp()+"/"+progress.GetCurrentLvllUpExp();
			lbHPBar.text=Mathf.Round(player.hitPoint)+"/"+Mathf.Round(playerFullHP);
			lbEnergyBar.text=Mathf.Round(player.energy)+"/"+Mathf.Round(playerFullEnergy);
			
			string statsText="";
			statsText+=player.GetHitPointRegen().ToString("f1")+"/sec\n";
			statsText+=player.GetEnergyRegen().ToString("f1")+"/sec\n";
			statsText+="\n";
			statsText+="x"+player.GetDamageMultiplier().ToString("f1")+"\n";
			statsText+="x"+player.GetCritChanceMultiplier().ToString("f1")+"\n";
			statsText+="x"+player.GetCritMulMultiplier().ToString("f1")+"\n";
			statsText+="x"+player.GetSpeedMultiplier().ToString("f1")+"\n";
			
			lbStats.text=statsText;
		}
		
	}

}