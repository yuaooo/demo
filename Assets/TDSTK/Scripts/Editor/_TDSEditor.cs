using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class TDSEditor {
		
		
		public static bool IsPrefabOrPrefabInstance(GameObject obj){
			PrefabType type=PrefabUtility.GetPrefabType(obj);
			return type==PrefabType.Prefab || type==PrefabType.PrefabInstance;
		}
		public static bool IsPrefabInstance(GameObject obj){
			return obj==null ? false : PrefabUtility.GetPrefabType(obj)==PrefabType.PrefabInstance;
		}
		public static bool IsPrefab(GameObject obj){
			return obj==null ? false : PrefabUtility.GetPrefabType(obj)==PrefabType.Prefab;
		}
		
		
		public static bool dirty=false; 	//a cache which the value is changed whenever something in the editor changed (name, icon, etc)
													//to let other custom editor not in focus to repaint
		
		
		public static int GetEffectIndex(int effectID){
			for(int i=0; i<effectDB.effectList.Count; i++){
				if(effectDB.effectList[i].ID==effectID) return (i+1);
			}
			return 0;
		}
		public static int GetAbilityIndex(int abilityID){
			for(int i=0; i<abilityDB.abilityList.Count; i++){
				if(abilityDB.abilityList[i].ID==abilityID) return (i+1);
			}
			return 0;
		}
		public static int GetPerkIndex(int perkID){
			for(int i=0; i<perkDB.perkList.Count; i++){
				if(perkDB.perkList[i].ID==perkID) return (i+1);
			}
			return 0;
		}
		public static int GetWeaponIndex(int weaponID){
			for(int i=0; i<weaponDB.weaponList.Count; i++){
				if(weaponDB.weaponList[i].ID==weaponID) return (i+1);
			}
			return 0;
		}
		public static int GetUnitAIIndex(int unitID){
			for(int i=0; i<unitAIDB.unitList.Count; i++){
				if(unitAIDB.unitList[i].prefabID==unitID) return (i+1);
			}
			return 0;
		}
		public static int GetUnitPlayerIndex(int unitID){
			for(int i=0; i<unitPlayerDB.unitList.Count; i++){
				if(unitPlayerDB.unitList[i].prefabID==unitID) return (i+1);
			}
			return 0;
		}
		public static int GetCollectibleIndex(int itemID){
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				if(collectibleDB.collectibleList[i].ID==itemID) return (i+1);
			}
			return 0;
		}
		
		
		public static bool ExistInDB(UnitAI unit){ return unitAIDB.unitList.Contains(unit); }
		public static bool ExistInDB(UnitPlayer unit){ return unitPlayerDB.unitList.Contains(unit); }
		public static bool ExistInDB(Collectible collectible){ return collectibleDB.collectibleList.Contains(collectible); }
		public static bool ExistInDB(Weapon weapon){ return weaponDB.weaponList.Contains(weapon); }
		
		
		private static bool loaded=false;
		public static void LoadDB(){
			if(loaded) return;
			
			loaded=true;
			
			LoadDamageTable();
			LoadProgressStats();
			LoadEffect();
			LoadAbility();
			LoadPerk();
			LoadWeapon();
			LoadCollectible();
			
			LoadUnitAI();
			LoadUnitPlayer();
		}
		
		
		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		public static void LoadDamageTable(){ 
			damageTableDB=DamageTableDB.LoadDB();
			UpdateLabel_DamageTable();
			
			TDSEditorWindow.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			TDSEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
		}
		public static void UpdateLabel_DamageTable(){
			damageTypeLabel=new string[damageTableDB.damageTypeList.Count];
			for(int i=0; i<damageTableDB.damageTypeList.Count; i++){ 
				damageTypeLabel[i]=damageTableDB.damageTypeList[i].name;
				if(damageTypeLabel[i]=="") damageTypeLabel[i]="unnamed";
			}
			
			armorTypeLabel=new string[damageTableDB.armorTypeList.Count];
			for(int i=0; i<damageTableDB.armorTypeList.Count; i++){
				armorTypeLabel[i]=damageTableDB.armorTypeList[i].name;
				if(armorTypeLabel[i]=="") armorTypeLabel[i]="unnamed";
			}
			
			TDSEditorWindow.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			TDSEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static ProgressionStatsDB progressDB;
		public static void LoadProgressStats(){ 
			progressDB=ProgressionStatsDB.LoadDB();
			//UpdateLabel_DamageTable();
			
			TDSEditorWindow.SetProgressionStats(progressDB);//, damageTypeLabel, armorTypeLabel);
			//TDSEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
		}
		
		
		
		protected static EffectDB effectDB;
		protected static List<int> effectIDList=new List<int>();
		protected static string[] effectLabel;
		public static void LoadEffect(){
			effectDB=EffectDB.LoadDB();
			
			for(int i=0; i<effectDB.effectList.Count; i++){
				if(effectDB.effectList[i]!=null){
					//effectDB.effectList[i].ID=i;
					effectIDList.Add(effectDB.effectList[i].ID);
				}
				else{
					effectDB.effectList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_Effect();
			
			TDSEditorWindow.SetEffectDB(effectDB, effectIDList, effectLabel);
			TDSEditorInspector.SetEffectDB(effectDB, effectIDList, effectLabel);
		}
		public static void UpdateLabel_Effect(){
			effectLabel=new string[effectDB.effectList.Count+1];
			effectLabel[0]="Unassigned";
			for(int i=0; i<effectDB.effectList.Count; i++){
				string name=effectDB.effectList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(effectLabel, name)>=0) name+="_";
				effectLabel[i+1]=name;
			}
			
			TDSEditorWindow.SetEffectDB(effectDB, effectIDList, effectLabel);
			TDSEditorInspector.SetEffectDB(effectDB, effectIDList, effectLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		protected static AbilityDB abilityDB;
		protected static List<int> abilityIDList=new List<int>();
		protected static string[] abilityLabel;
		public static void LoadAbility(){
			abilityDB=AbilityDB.LoadDB();
			
			for(int i=0; i<abilityDB.abilityList.Count; i++){
				if(abilityDB.abilityList[i]!=null){
					//abilityDB.abilityList[i].ID=i;
					abilityIDList.Add(abilityDB.abilityList[i].ID);
				}
				else{
					abilityDB.abilityList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_Ability();
			
			TDSEditorWindow.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
			TDSEditorInspector.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
		}
		public static void UpdateLabel_Ability(){
			abilityLabel=new string[abilityDB.abilityList.Count+1];
			abilityLabel[0]="Unassigned";
			for(int i=0; i<abilityDB.abilityList.Count; i++){
				string name=abilityDB.abilityList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(abilityLabel, name)>=0) name+="_";
				abilityLabel[i+1]=name;
			}
			
			TDSEditorWindow.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
			TDSEditorInspector.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		public static void LoadPerk(){
			perkDB=PerkDB.LoadDB();
			
			for(int i=0; i<perkDB.perkList.Count; i++){
				if(perkDB.perkList[i]!=null){
					//perkDB.perkList[i].ID=i;
					perkIDList.Add(perkDB.perkList[i].ID);
				}
				else{
					perkDB.perkList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_Perk();
			
			TDSEditorWindow.SetPerkDB(perkDB, perkIDList, perkLabel);
			TDSEditorInspector.SetPerkDB(perkDB, perkIDList, perkLabel);
		}
		public static void UpdateLabel_Perk(){
			perkLabel=new string[perkDB.perkList.Count+1];
			perkLabel[0]="Unassigned";
			for(int i=0; i<perkDB.perkList.Count; i++){
				string name=perkDB.perkList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(perkLabel, name)>=0) name+="_";
				perkLabel[i+1]=name;
			}
			
			TDSEditorWindow.SetPerkDB(perkDB, perkIDList, perkLabel);
			TDSEditorInspector.SetPerkDB(perkDB, perkIDList, perkLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		protected static WeaponDB weaponDB;
		protected static List<int> weaponIDList=new List<int>();
		protected static string[] weaponLabel;
		public static void LoadWeapon(){
			weaponDB=WeaponDB.LoadDB();
			
			for(int i=0; i<weaponDB.weaponList.Count; i++){
				if(weaponDB.weaponList[i]!=null){
					//weaponDB.weaponList[i].ID=i;
					weaponIDList.Add(weaponDB.weaponList[i].ID);
				}
				else{
					weaponDB.weaponList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_Weapon();
			
			TDSEditorWindow.SetWeaponDB(weaponDB, weaponIDList, weaponLabel);
			TDSEditorInspector.SetWeaponDB(weaponDB, weaponIDList, weaponLabel);
		}
		public static void UpdateLabel_Weapon(){
			weaponLabel=new string[weaponDB.weaponList.Count+1];
			weaponLabel[0]="Unassigned";
			for(int i=0; i<weaponDB.weaponList.Count; i++){
				string name=weaponDB.weaponList[i].weaponName;
				if(name=="") name="unnamed";
				while(Array.IndexOf(weaponLabel, name)>=0) name+="_";
				weaponLabel[i+1]=name;
			}
			
			TDSEditorWindow.SetWeaponDB(weaponDB, weaponIDList, weaponLabel);
			TDSEditorInspector.SetWeaponDB(weaponDB, weaponIDList, weaponLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		
		protected static CollectibleDB collectibleDB;
		protected static List<int> collectibleIDList=new List<int>();
		protected static string[] collectibleLabel;
		public static void LoadCollectible(){
			collectibleDB=CollectibleDB.LoadDB();
			
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				if(collectibleDB.collectibleList[i]!=null){
					//collectibleDB.collectibleList[i].ID=i;
					collectibleIDList.Add(collectibleDB.collectibleList[i].ID);
				}
				else{
					collectibleDB.collectibleList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_Collectible();
			
			TDSEditorWindow.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
			TDSEditorInspector.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
		}
		public static void UpdateLabel_Collectible(){
			collectibleLabel=new string[collectibleDB.collectibleList.Count+1];
			collectibleLabel[0]="Unassigned";
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				string name=collectibleDB.collectibleList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(collectibleLabel, name)>=0) name+="_";
				collectibleLabel[i+1]=name;
			}
			
			TDSEditorWindow.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
			TDSEditorInspector.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		
		protected static UnitAIDB unitAIDB;
		protected static List<int> unitAIIDList=new List<int>();
		protected static string[] unitAILabel;
		public static void LoadUnitAI(){
			unitAIDB=UnitAIDB.LoadDB();
			
			for(int i=0; i<unitAIDB.unitList.Count; i++){
				if(unitAIDB.unitList[i]!=null){
					//unitAIDB.unitList[i].prefabID=i;
					unitAIIDList.Add(unitAIDB.unitList[i].prefabID);
				}
				else{
					unitAIDB.unitList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_UnitAI();
			
			TDSEditorWindow.SetUnitAIDB(unitAIDB, unitAIIDList, unitAILabel);
			TDSEditorInspector.SetUnitAIDB(unitAIDB, unitAIIDList, unitAILabel);
		}
		public static void UpdateLabel_UnitAI(){
			unitAILabel=new string[unitAIDB.unitList.Count+1];
			unitAILabel[0]="Unassigned";
			for(int i=0; i<unitAIDB.unitList.Count; i++){
				string name=unitAIDB.unitList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(unitAILabel, name)>=0) name+="_";
				unitAILabel[i+1]=name;
			}
			
			TDSEditorWindow.SetUnitAIDB(unitAIDB, unitAIIDList, unitAILabel);
			TDSEditorInspector.SetUnitAIDB(unitAIDB, unitAIIDList, unitAILabel);
			
			dirty=!dirty;
		}
		
		
		
		
		
		protected static UnitPlayerDB unitPlayerDB;
		protected static List<int> unitPlayerIDList=new List<int>();
		protected static string[] unitPlayerLabel;
		public static void LoadUnitPlayer(){
			unitPlayerDB=UnitPlayerDB.LoadDB();
			
			for(int i=0; i<unitPlayerDB.unitList.Count; i++){
				if(unitPlayerDB.unitList[i]!=null){
					//unitPlayerDB.unitList[i].prefabID=i;
					unitPlayerIDList.Add(unitPlayerDB.unitList[i].prefabID);
				}
				else{
					unitPlayerDB.unitList.RemoveAt(i);
					i-=1;
				}
			}
			
			UpdateLabel_UnitPlayer();
			
			TDSEditorWindow.SetUnitPlayerDB(unitPlayerDB, unitPlayerIDList, unitPlayerLabel);
			TDSEditorInspector.SetUnitPlayerDB(unitPlayerDB, unitPlayerIDList, unitPlayerLabel);
		}
		public static void UpdateLabel_UnitPlayer(){
			unitPlayerLabel=new string[unitPlayerDB.unitList.Count+1];
			unitPlayerLabel[0]="Unassigned";
			for(int i=0; i<unitPlayerDB.unitList.Count; i++){
				string name=unitPlayerDB.unitList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(unitPlayerLabel, name)>=0) name+="_";
				unitPlayerLabel[i+1]=name;
			}
			
			TDSEditorWindow.SetUnitPlayerDB(unitPlayerDB, unitPlayerIDList, unitPlayerLabel);
			TDSEditorInspector.SetUnitPlayerDB(unitPlayerDB, unitPlayerIDList, unitPlayerLabel);
			
			dirty=!dirty;
		}
		
		
	}
	
	
	
}
