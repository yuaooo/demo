using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class UIHUD : MonoBehaviour {
		
		//public Text lbCredit;
		public Text lbScore;
		
		public Text lbRespawnCount;
		
		public Text lbHP;
		public Text lbEnergy;
		
		//public Slider sliderHitPointBar;
		//public Slider sliderEnergyBar;
		
		public Slider sliderHPBar;
		public Slider sliderEnergyBar;
		
		private PlayerProgression playerProgress;
		
		//public RectTransform barHP;
		//public RectTransform barEnergy;
		//private float barHPLength;
		//private float barEnergyLength;
		
		private float hitPointFull;
		
		public UIButton uiButtonWeapon;
		public UIButton uiButtonAltFire;
		public GameObject abilityButtonObj;
		
		
		
		private int credit=0;
		private int score=0;
		private float minUpdateSpeed=0.05f;
		private float updateSpeed=0f;
		
		
		void Awake(){
			uiButtonWeapon.Init();
			uiButtonAltFire.Init();
			
			uiButtonWeapon.labelAlt2.text="";
			
			//barHPLength=barHP.parent.GetComponent<RectTransform>().sizeDelta.x;
			//barEnergyLength=barEnergy.parent.GetComponent<RectTransform>().sizeDelta.x;
		}
		
		void Start(){
			if(!GameControl.EnableAltFire()) uiButtonAltFire.rootObj.SetActive(false);
			if(!GameControl.EnableAbility()) abilityButtonObj.SetActive(false);
			
			credit=GameControl.GetCredits();
			score=GameControl.GetScore();
			
			UnitPlayer player=GameControl.GetPlayer();
			if(player!=null) playerProgress=player.GetPlayerProgression();
		}
		
		void OnEnable(){
			TDS.onSwitchWeaponE += OnSwitchWeapon;
			TDS.onReloadingE += OnReloading;
			
			TDS.onPlayerRespawnedE += OnPlayerRespawn;
		}
		void OnDisable(){
			TDS.onSwitchWeaponE -= OnSwitchWeapon;
			TDS.onReloadingE -= OnReloading;
			
			TDS.onPlayerRespawnedE -= OnPlayerRespawn;
		}
		
		void OnPlayerRespawn(){
			UnitPlayer player=GameControl.GetPlayer();
			if(player!=null) playerProgress=player.GetPlayerProgression();
		}
		
		// Update is called once per frame
		void Update () {
			
			
			
			lbRespawnCount.text=GameControl.GetPlayerLife().ToString();
			
			UnitPlayer player=GameControl.GetPlayer();
			if(player==null){
				lbHP.text="0/"+player.GetFullHitPoint();
				//barHP.sizeDelta=new Vector2(-barHPLength, 0);
				sliderHPBar.value=0;
				return;
			}
			
			
			//sliderHitPointBar.value=player.hitPoint/player.hitPointFull;
			//sliderEnergyBar.value=player.energy/player.energyFull;
			
			
			sliderHPBar.value=player.hitPoint/player.GetFullHitPoint();
			lbHP.text=Mathf.Round(player.hitPoint)+"/"+Mathf.Round(player.GetFullHitPoint());
			//barHP.sizeDelta=new Vector2((1-(player.hitPoint/player.GetFullHitPoint()))*-barHPLength, 0);
			
			sliderEnergyBar.value=player.energy/player.GetFullEnergy();
			lbEnergy.text=Mathf.Round(player.energy)+"/"+Mathf.Round(player.GetFullEnergy());
			//barEnergy.sizeDelta=new Vector2((1-(player.energy/player.GetFullEnergy()))*-barEnergyLength, 0);
			
			
			if(!reloading){
				string clip=player.GetCurrentClip()<0 ? "∞" : player.GetCurrentClip().ToString();
				string ammo=player.GetAmmo()<0 ? "∞" : player.GetAmmo().ToString();
				uiButtonWeapon.labelAlt.text=clip+"/"+ammo;
			}
			
			
			if(GameControl.EnableAltFire()){
				Ability ability=player.GetWeaponAbility();
				if(ability!=null){
					uiButtonAltFire.label.text=ability.currentCD<=0 ? "" : ability.currentCD.ToString("f1")+"s";
					uiButtonAltFire.button.interactable=ability.IsReady()=="" ? true : false;
				}
			}
			
			
			int creditTgt=GameControl.GetCredits();
			if(credit!=creditTgt){
				updateSpeed=Mathf.Max(minUpdateSpeed, Mathf.Abs(1f/(float)(creditTgt-credit)));
				credit=(int)Mathf.Round(Mathf.Lerp(credit, creditTgt, updateSpeed));
			}
			
			int scoreTgt=GameControl.GetScore();
			if(score!=scoreTgt){
				updateSpeed=Mathf.Max(minUpdateSpeed, Mathf.Abs(1f/(float)(scoreTgt-score)));
				score=(int)Mathf.Round(Mathf.Lerp(score, scoreTgt, updateSpeed));
			}
			
			//lbCredit.text="";	//lbCredit.text="Credits: "+(credit<0 ? "-" : "")+"$"+Mathf.Abs(credit);
			lbScore.text="Score: "+score;
		}
		
		
		private bool reloading=false;
		void OnReloading(){
			if(!reloading) StartCoroutine(ReloadRoutine());
		}
		IEnumerator ReloadRoutine(){
			yield return null;		//in case the the previous weapon was switch while reloading, give it a frame to stop the ReloadRoutine
			
			reloading=true;
			uiButtonWeapon.button.interactable=false;
			uiButtonWeapon.labelAlt.alignment=TextAnchor.MiddleLeft;
			
			UnitPlayer player=GameControl.GetPlayer();
			
			while(player!=null && player.Reloading()){
				string dot="";
				int count=(int)Mathf.Floor((Time.time*3)%4);
				for(int i=0; i<count; i++) dot+=".";
				for(int i=count; i<3; i++) dot+=" ";
				uiButtonWeapon.labelAlt.text="Reloading"+dot;
				
				float durationRemain=player.GetReloadDuration()-player.GetCurrentReload();
				uiButtonWeapon.labelAlt2.text=durationRemain<=0 ? "" : durationRemain.ToString("f1")+"s";
				
				yield return null;
			}
			
			uiButtonWeapon.labelAlt.text="";
			uiButtonWeapon.labelAlt.alignment=TextAnchor.MiddleRight;
			
			uiButtonWeapon.labelAlt2.text="";
			uiButtonWeapon.button.interactable=true;
			
			reloading=false;
		}
		
		
		void OnSwitchWeapon(Weapon weapon){
			uiButtonWeapon.imgIcon.sprite=weapon.icon;
			uiButtonWeapon.label.text=weapon.weaponName;
			
			if(GameControl.EnableAltFire()){
				if(weapon.ability!=null){
					uiButtonAltFire.imgIcon.sprite=weapon.ability.icon;
				}
				else{
					uiButtonAltFire.imgIcon.sprite=null;
					uiButtonAltFire.label.text="";
				}
			}
			
			if(reloading) reloading=false;
		}
		
	}

}