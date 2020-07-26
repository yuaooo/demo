using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	[RequireComponent (typeof (UnitPlayer))]
	public class PlayerPerk : MonoBehaviour {
		
		[Space(10)]
		public bool enablePerk=true;
		
		
		public int perkCurrency=0;
		[HideInInspector] public int perkPoint=0;
		public int GetPerkPoint(){ return perkPoint; }
		
		
		//[HideInInspector]
		public List<int> unavailableIDList=new List<int>(); 	//ID list of perk available for this level, modified in editor
		[HideInInspector] public List<int> purchasedIDList=new List<int>(); 		//ID list of perk pre-purcahsed for this level, modified in editor
		[HideInInspector] public List<Perk> perkList=new List<Perk>();
		public List<Perk> GetPerkList(){ return perkList; }
		public int GetPerkListCount(){ return perkList.Count; }
		
		
		[HideInInspector] public UnitPlayer player;
		public void SetPlayer(UnitPlayer unit){
			player=unit;
			Init();
		}
		
		//[HideInInspector] 
		public bool init=false;
		public void Init(){
			if(init) return;
			init=true;
			
			//if(!enablePerk) return;
			
			//loading the perks from DB
			List<Perk> dbList=PerkDB.Load();
			for(int i=0; i<dbList.Count; i++){
				if(!unavailableIDList.Contains(dbList[i].ID)){
					Perk perk=dbList[i].Clone();
					perkList.Add(perk);
				}
			}
			
			for(int i=0; i<perkList.Count; i++) perkList[i].SetPlayer(player);
			
			StartCoroutine(UnlockPurchasedPerk());
		}
		
		IEnumerator UnlockPurchasedPerk(){
			yield return null;
			yield return null;
			for(int i=0; i<perkList.Count; i++){
				if(purchasedIDList.Contains(perkList[i].ID)) PurchasePerk(perkList[i], false);
			}
		}
		
		
		
		
		public void Save(){
			PlayerPrefs.SetInt("p"+player.playerID+"_perk", 1);	//to indicate save exist when loading
			int perkCount=0;
			for(int i=0; i<perkList.Count; i++){
				if(perkList[i].purchased>0){
					PlayerPrefs.SetInt("p"+player.playerID+"_perk_"+perkCount, perkList[i].ID);
					PlayerPrefs.SetInt("p"+player.playerID+"_perk_"+perkCount+"_purchased", perkList[i].purchased);
					perkCount+=1;
				}
			}
			PlayerPrefs.SetInt("p"+player.playerID+"_perk_count", perkCount);
			PlayerPrefs.SetInt("p"+player.playerID+"_perk_currency", perkCurrency);
		}
		public void Load(){
			purchasedIDList=new List<int>();
			
			if(PlayerPrefs.HasKey("p"+player.playerID+"_perk")){
				perkCurrency=PlayerPrefs.GetInt("p"+player.playerID+"_perk_currency", 0);
			
				Debug.Log("p"+player.playerID+"_perk_currency    "+perkCurrency);
				
				int perkCount=PlayerPrefs.GetInt("p"+player.playerID+"_perk_count", 0);
				for(int i=0; i<perkCount; i++){
					int perkID=PlayerPrefs.GetInt("p"+player.playerID+"_perk_"+i, -1);
					int purchased=PlayerPrefs.GetInt("p"+player.playerID+"_perk_"+i+"_purchased", 0);
					if(perkID>0){
						for(int n=0; n<purchased; n++) purchasedIDList.Add(perkID);
					}
				}
			}
			
			for(int i=0; i<perkList.Count; i++){
				while(purchasedIDList.Contains(perkList[i].ID)){
					PurchasePerk(perkList[i], false, false);
					purchasedIDList.Remove(perkList[i].ID);
				}
			}
		}
		public void DeleteSave(int playerID=0){
			int perkCount=PlayerPrefs.GetInt("p"+playerID+"_perk_count", 0);
			for(int i=0; i<perkCount; i++){
				PlayerPrefs.DeleteKey("p"+playerID+"_perk_"+i);
				PlayerPrefs.DeleteKey("p"+playerID+"_perk_"+i+"_purchased");
			}
			
			PlayerPrefs.DeleteKey("p"+playerID+"_perk_count");
			PlayerPrefs.DeleteKey("p"+playerID+"_perk_currency");
			PlayerPrefs.DeleteKey("p"+playerID+"_perk");
		}
		
		
		
		
		
		public int GetPerkCurrency(){ return perkCurrency; }
		public void SetPerkCurrency(int value){
			perkCurrency=value;
			CurrencyChanged();
		}
		public void SpendCurrency(int value){
			perkCurrency=Mathf.Max(0, perkCurrency-value);
			CurrencyChanged();
		}
		public void GainCurrency(int value){ 
			perkCurrency+=value;
			CurrencyChanged();
		}
		private void CurrencyChanged(){
			TDS.OnPerkCurrency(perkCurrency);
			if(player.SaveUponChange()) Save();
		}
		
		
		
		
		
		
		
		//public static Perk GetPerk(int perkID){ return instance._GetPerk(perkID); }
		public Perk GetPerkFromIndex(int index){ return perkList[index]; }
		public Perk GetPerk(int perkID){
			for(int i=0; i<perkList.Count; i++){ if(perkList[i].ID==perkID) return perkList[i]; }
			return null;
		}
		public int GetPerkIndex(int perkID){
			for(int i=0; i<perkList.Count; i++){ if(perkList[i].ID==perkID) return i; }
			return -1;
		}
		//public static string IsPerkAvailable(int perkID){ return instance._IsPerkAvailable(perkID); }
		public string IsPerkAvailable(int perkID){
			for(int i=0; i<perkList.Count; i++){ if(perkList[i].ID==perkID) return perkList[i].IsAvailable(); }
			return "PerkID doesnt correspond to any perk in the list   "+perkID;
		}
		//public static bool IsPerkPurchased(int perkID){ return instance._IsPerkPurchased(perkID); }
		public bool IsPerkPurchased(int perkID){
			for(int i=0; i<perkList.Count; i++){ if(perkList[i].ID==perkID) return perkList[i].purchased>0; }
			return false;
		}
		
		
		
		//public static string PurchasePerk(int perkID, bool useCurrency=true){ return instance._PurchasePerk(perkID, useCurrency); }
		public string PurchasePerk(int perkID, bool useCurrency=true, bool saving=true){ 
			for(int i=0; i<perkList.Count; i++){ if(perkList[i].ID==perkID) return PurchasePerk(perkList[i], useCurrency, saving); }
			return "PerkID doesnt correspond to any perk in the list";
		}
		//public static string PurchasePerk(Perk perk, bool useCurrency=true){ return instance._PurchasePerk(perk, useCurrency); }
		public string PurchasePerk(Perk perk, bool useCurrency=true, bool saving=true){
			
			string text=perk.Purchase(this, useCurrency); 
			if(text!=""){
				Debug.Log(text);
				return text;
			}
			
			for(int i=0; i<perkList.Count; i++){
				Perk perkTemp=perkList[i];
				if(perkTemp.purchased>0 || perkTemp.prereq.Count==0) continue;
				perkTemp.prereq.Remove(perk.ID);
			}
			
			TDS.PerkPurchased(perk);
			
			if(player.SaveUponChange()) Save();
			
			if(perk.type==_PerkType.ModifyGeneralStats){
				hitPointCap+=perk.hitPointCap;	
				energyCap+=perk.energyCap;		
				
				hitPointRegen+=perk.hitPointRegen;
				energyRegen+=perk.energyRegen;
				
				player.GainHitPoint(perk.hitPoint);
				player.GainEnergy(perk.energy);
				
				moveSpeedMul+=perk.moveSpeedMul;
				
				damageMul+=perk.dmgMul;
				critMul+=perk.critChanceMul;
				critMulMul+=perk.CritMultiplierMul;
				
				expGainMul+=perk.expGainMul;
				creditGainMul+=perk.creditGainMul;
				scoreGainMul+=perk.scoreGainMul;
				hitPointGainMul+=perk.hitPointGainMul;
				energyGainMul+=perk.energyGainMul;
			}
			else if(perk.type==_PerkType.AddWeapon && perk.newWeaponID>=0){
				
				Weapon newWeapon=WeaponDB.GetPrefab(perk.newWeaponID);
				
				if(newWeapon!=null){
					if(perk.replaceExisting){
						player.AddWeapon(newWeapon, true);
					}
					else if(perk.replaceWeaponID>=0){
						int replaceIndex=-1;
						for(int i=0; i<player.weaponList.Count; i++){
							if(perk.replaceWeaponID==player.weaponList[i].ID){
								replaceIndex=i;
								break;
							}
						}
						
						if(replaceIndex>=0){
							player.SwitchWeapon(replaceIndex);
							player.AddWeapon(newWeapon, true);
						}
						else player.AddWeapon(newWeapon);
					}
					else{
						player.AddWeapon(newWeapon);
					}
				}
				
				for(int i=0; i<perkList.Count; i++){
					if(perkList[i].purchased>0 && perkList[i].type==_PerkType.ModifyWeapon){
						if(perkList[i].appliedToAllWeapon || perkList[i].weaponIDList.Contains(perk.newWeaponID)){
							if(perkList[i].weapEffectID>=0) player.ChangeWeaponEffect(perk.newWeaponID, perkList[i].weapEffectID);
							if(perkList[i].weapAbilityID>=0) player.ChangeWeaponAbility(perk.newWeaponID, perkList[i].weapAbilityID);
						}
					}
				}
			}
			else if(perk.type==_PerkType.AddAbility && perk.newAbilityID>=0){
				AbilityManager.AddAbility(perk.newAbilityID, perk.replaceAbilityID);
				
				for(int i=0; i<perkList.Count; i++){
					if(perkList[i].purchased>0 && perkList[i].type==_PerkType.ModifyAbility){
						if(perkList[i].appliedToAllAbility || perkList[i].abilityIDList.Contains(perk.newAbilityID)){
							if(perkList[i].abEffectID>=0) player.ChangeAbilityEffect(perk.newAbilityID, perkList[i].abEffectID);
						}
					}
				}
			}
			else if(perk.type==_PerkType.ModifyWeapon){
				if(perk.appliedToAllWeapon){
					weapStatG.ModifyWithPerk(perk);
					if(perk.weapEffectID>=0) player.ChangeAllWeaponEffect(perk.weapEffectID);
					if(perk.weapAbilityID>=0) player.ChangeAllWeaponAbility(perk.weapAbilityID);
				}
				else{
					for(int i=0; i<perk.weaponIDList.Count; i++){
						WeaponStatMultiplier item=GetWeaponStatMul(perk.weaponIDList[i]);
						
						if(item==null){
							item=new WeaponStatMultiplier();
							item.prefabID=perk.weaponIDList[i];
							weapStatList.Add(item);
						}
						
						item.ModifyWithPerk(perk);
						
						if(perk.weapEffectID>=0) player.ChangeWeaponEffect(perk.weaponIDList[i], perk.weapEffectID);
						if(perk.weapAbilityID>=0) player.ChangeWeaponAbility(perk.weaponIDList[i], perk.weapAbilityID);
					}
				}
			}
			else if(perk.type==_PerkType.ModifyAbility){
				if(perk.appliedToAllAbility){
					abilityStatG.ModifyWithPerk(perk);
					if(perk.weapEffectID>=0) player.ChangeAllAbilityEffect(perk.weapEffectID);
				}
				else{
					for(int i=0; i<perk.abilityIDList.Count; i++){
						AbilityStatMultiplier item=GetAbilityStatMul(perk.abilityIDList[i]);
						
						if(item==null){
							item=new AbilityStatMultiplier();
							item.prefabID=perk.abilityIDList[i];
							abilityStatList.Add(item);
						}
						
						item.ModifyWithPerk(perk);
						
						if(perk.abEffectID>=0) player.ChangeAbilityEffect(perk.abilityIDList[i], perk.abEffectID);
					}
				}
			}
			else if(perk.type==_PerkType.Custom){
				GameObject obj=(GameObject)Instantiate(perk.customObject);
				obj.name=perk.name+"_CustomObject";
				obj.transform.parent=transform;
				obj.transform.localPosition=Vector3.zero;
				obj.transform.localRotation=Quaternion.identity;
			}
			
			return "";
		}
		
		
		
		
		[Header("Player General Stats")]
		public float hitPointCap=0;
		public float hitPointRegen=0;
		public float energyCap=0;
		public float energyRegen=0;
		
		public float GetBonusHitPoint(){ return hitPointCap; }
		public float GetBonusEnergy(){ return energyCap; }
		public float GetBonusHitPointRegen(){ return hitPointRegen; }
		public float GetBonusEnergyRegen(){ return energyRegen; }
		
		
		public float moveSpeedMul=0;
		public float GetMoveSpeedMul(){ return moveSpeedMul; }
		
		
		public float damageMul=0;
		public float critMul=0;
		public float critMulMul=0;
		public float GetDamageMul(){ return damageMul; }
		public float GetCritMul(){ return critMul; }
		public float GetCirtMulMul(){ return critMulMul; }
		
		
		public float expGainMul=0;		//experience
		public float creditGainMul=0;
		public float scoreGainMul=0;
		public float hitPointGainMul=0;
		public float energyGainMul=0;
		
		public float GetExpGainMul(){ return expGainMul; }
		public float GetCreditGainMul(){ return creditGainMul; }
		public float GetScoreGainMul(){ return scoreGainMul; }
		public float GetHitPointGainMul(){ return hitPointGainMul; }
		public float GetEnergyGainMul(){ return energyGainMul; }
		
		
		
		
		
		
		[Header("Weapons Multiplier")]
		public WeaponStatMultiplier weapStatG=new WeaponStatMultiplier();
		public List<WeaponStatMultiplier> weapStatList=new List<WeaponStatMultiplier>();
		
		public float GetWeaponDamageMul(int ID){ return weapStatG.dmg+GetWeaponStatMul_Damage(ID); }
		public float GetWeaponCritMul(int ID){ return weapStatG.crit+GetWeaponStatMul_Crit(ID); }
		public float GetWeaponCritMulMul(int ID){ return weapStatG.critMul+GetWeaponStatMul_CritMul(ID); }
		public float GetWeaponAOEMul(int ID){ return weapStatG.aoe+GetWeaponStatMul_AOE(ID); }
		public float GetWeaponRangeMul(int ID){ return weapStatG.range+GetWeaponStatMul_Range(ID); }
		public float GetWeaponCDMul(int ID){ return weapStatG.cooldown+GetWeaponStatMul_CD(ID); }
		public float GetWeaponClipSizeMul(int ID){ return weapStatG.clipSize+GetWeaponStatMul_Clip(ID); }
		public float GetWeaponAmmoCapMul(int ID){ return weapStatG.ammoCap+GetWeaponStatMul_AmmoCap(ID); }
		public float GetWeaponReloadDurMul(int ID){ return weapStatG.reloadDuration+GetWeaponStatMul_ReloadDur(ID); }
		public float GetWeaponRecoilMagMul(int ID){ return weapStatG.recoilMagnitude+GetWeaponStatMul_RecoilMag(ID); }
		
		public float GetWeaponStatMul_Damage(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.dmg : 0 ; }
		public float GetWeaponStatMul_Crit(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.crit : 0 ; }
		public float GetWeaponStatMul_CritMul(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.critMul : 0 ; }
		public float GetWeaponStatMul_AOE(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.aoe : 0 ; }
		public float GetWeaponStatMul_Range(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.range : 0 ; }
		public float GetWeaponStatMul_CD(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.cooldown : 0 ; }
		public float GetWeaponStatMul_Clip(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.clipSize : 0 ; }
		public float GetWeaponStatMul_AmmoCap(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.ammoCap : 0 ; }
		public float GetWeaponStatMul_ReloadDur(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.reloadDuration : 0 ; }
		public float GetWeaponStatMul_RecoilMag(int ID){ return (GetWeaponStatMul(ID)!=null) ? tempWItem.recoilMagnitude : 0 ; }
		
		private WeaponStatMultiplier tempWItem;
		public WeaponStatMultiplier GetWeaponStatMul(int ID){
			tempWItem=null;
			for(int i=0; i<weapStatList.Count; i++){
				if(weapStatList[i].prefabID==ID){ tempWItem=weapStatList[i]; break; }
			}
			return tempWItem;
		}
		
		
		
		
		
		
		[Header("Abilities Multiplier")]
		public AbilityStatMultiplier abilityStatG=new AbilityStatMultiplier();
		public List<AbilityStatMultiplier> abilityStatList=new List<AbilityStatMultiplier>();
		
		public float GetAbilityCostMul(int ID){ return abilityStatG.abCost+GetAbilityStatMul_Cost(ID); }
		public float GetAbilityCooldownMul(int ID){ return abilityStatG.abCooldown+GetAbilityStatMul_Cooldown(ID); }
		public float GetAbilityRangeMul(int ID){ return abilityStatG.abRange+GetAbilityStatMul_Range(ID); }
		public float GetAbilityDamageMul(int ID){ return abilityStatG.abDmg+GetAbilityStatMul_Damage(ID); }
		public float GetAbilityCritMul(int ID){ return abilityStatG.abCrit+GetAbilityStatMul_Crit(ID); }
		public float GetAbilityCritMulMul(int ID){ return abilityStatG.abCritMul+GetAbilityStatMul_CritMul(ID); }
		public float GetAbilityAOEMul(int ID){ return abilityStatG.abAOE+GetAbilityStatMul_AOE(ID); }
		
		public float GetAbilityStatMul_Cost(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abCost : 0 ; }
		public float GetAbilityStatMul_Cooldown(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abCooldown : 0 ; }
		public float GetAbilityStatMul_Range(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abRange : 0 ; }
		public float GetAbilityStatMul_Damage(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abDmg : 0 ; }
		public float GetAbilityStatMul_Crit(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abCrit : 0 ; }
		public float GetAbilityStatMul_CritMul(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abCritMul : 0 ; }
		public float GetAbilityStatMul_AOE(int ID){ return (GetAbilityStatMul(ID)!=null) ? tempABItem.abAOE : 0 ; }
		
		private AbilityStatMultiplier tempABItem;
		public AbilityStatMultiplier GetAbilityStatMul(int ID){
			tempABItem=null;
			for(int i=0; i<abilityStatList.Count; i++){
				if(abilityStatList[i].prefabID==ID){ tempABItem=abilityStatList[i]; break; }
			}
			return tempABItem;
		}
		
		
		
		
		[System.Serializable]
		public class WeaponStatMultiplier{
			public int prefabID=0;
			public float dmg=0;
			public float crit=0;
			public float critMul=0;
			public float aoe=0;
			public float range=0;
			public float cooldown=0;
			public float clipSize=0;
			public float ammoCap=0;
			public float reloadDuration=0;
			public float recoilMagnitude=0;
			
			public void ModifyWithPerk(Perk perk){
				dmg+=perk.weapDmg;
				crit+=perk.weapCrit;
				critMul+=perk.weapCritMul;
				aoe+=perk.weapAOE;
				range+=perk.weapRange;
				cooldown+=perk.weapCooldown;
				clipSize+=perk.weapClipSize;
				ammoCap+=perk.weapAmmoCap;
				reloadDuration+=perk.weapReloadDuration;
				recoilMagnitude+=perk.weapRecoilMagnitude;
			}
		}
		
		[System.Serializable]
		public class AbilityStatMultiplier{
			public int prefabID=0;
			public float abCost;
			public float abCooldown;
			public float abRange=0;
			public float abDmg=0;
			public float abCrit=0;
			public float abCritMul=0;
			public float abAOE=0;
			
			
			public void ModifyWithPerk(Perk perk){
				abCost+=perk.abCost;
				abCooldown+=perk.abCooldown;
				abRange+=perk.abRange;
				abDmg+=perk.abDmg;
				abCrit+=perk.abCrit;
				abCritMul+=perk.abCritMul;
				abAOE+=perk.abAOE;
			}
		}
		
	}

}