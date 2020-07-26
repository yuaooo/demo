using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class PerkEditorWindow : TDSEditorWindow {
		
		private static PerkEditorWindow window;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (PerkEditorWindow)EditorWindow.GetWindow(typeof (PerkEditorWindow), false, "Perk Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
		}
		
		
		private static string[] perkTypeLabel;
		private static string[] perkTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_PerkType)).Length;
			perkTypeLabel=new string[enumLength];
			perkTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				perkTypeLabel[i]=((_PerkType)i).ToString();
				
				if((_PerkType)i==_PerkType.ModifyGeneralStats)	perkTypeTooltip[i]="Modify player general stats like HP, energy, etc.";
				if((_PerkType)i==_PerkType.AddWeapon) 			perkTypeTooltip[i]="Grant player a new weapon";
				if((_PerkType)i==_PerkType.ModifyWeapon) 		perkTypeTooltip[i]="Modify attribute of certain weapon(s) for player";
				if((_PerkType)i==_PerkType.AddAbility) 				perkTypeTooltip[i]="Grant player a new ability";
				if((_PerkType)i==_PerkType.ModifyAbility) 			perkTypeTooltip[i]="Modify attribute of certain ability(s) for player";
				if((_PerkType)i==_PerkType.Custom) 				perkTypeTooltip[i]="Do a custom effect by create an object from a designated prefab";
			}
		}
		
		
		public void SetupCallback(){
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
		}
		
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<Perk> perkList=perkDB.perkList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(perkDB, "PerkDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(perkList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawPerkList(startX, startY, perkList);	
			
			startX=v2.x+25;
			
			if(perkList.Count==0) return true;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				startY=DrawPerkConfigurator(startX, startY, perkList[selectID]);
				contentWidth=360;
				contentHeight=startY-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTDS();
			
			return true;
		}
		
		
		
		
		
		
		private bool foldGeneral=true;
		private float DrawPerkGeneralSetting(float startX, float startY, Perk perk){
			widthS=50;
			
			string text="General Setting "+(!foldGeneral ? "(show)" : "(hide)");
			foldGeneral=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldGeneral, text, foldoutStyle);
			if(foldGeneral){
				startX+=15;
				
					cont=new GUIContent("Repeatable:", "Check if the ability can be repeatably purchase. For perk that offer straight, one off bonus such as life and resource");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.repeatable=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), perk.repeatable);
					
					cont=new GUIContent("Limit:", "The amount of time the perk can be repeatably purchase.\nany value <=0 means it's unlimited");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(perk.repeatable) perk.limit=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.limit);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					
				
				startY+=8;
					
					cont=new GUIContent("Prerequisite Perk:", "Perks that needs to be purchased before this perk is unlocked and become available");
					EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont);
					
					for(int i=0; i<perk.prereq.Count+1; i++){
						int index=(i<perk.prereq.Count) ? TDSEditor.GetPerkIndex(perk.prereq[i]) : 0;
						index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width, height), index, perkLabel);
						if(index>0){
							int perkID=perkDB.perkList[index-1].ID;
							if(perkID!=perk.ID && !perk.prereq.Contains(perkID)){
								if(i<perk.prereq.Count) perk.prereq[i]=perkID;
								else perk.prereq.Add(perkID);
							}
						}
						else if(i<perk.prereq.Count){ perk.prereq.RemoveAt(i); i-=1; }
					}
					
				startY+=5;
					
					cont=new GUIContent("Min level required:", "Minimum level to reach before the perk becoming available. (level are specified in GameControl of each scene)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.minLevel=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.minLevel);
					
					cont=new GUIContent("Min PerkPoint req:", "Minimum perk point to have before the perk becoming available");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.minPerkPoint=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.minPerkPoint);
					
				startY+=5;
					
					cont=new GUIContent("Cost:", "The perk currency required to purchase the perk");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.cost=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.cost);
					
			}
				
			return startY+spaceY;
		}
		
		
		private bool foldStats=true;
		private float DrawPerkStats(float startX, float startY, Perk perk){
			widthS=50;
			
			string text="Modifiers Setting "+(!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldStats, text, foldoutStyle);
			if(foldStats){
				startX+=15;
				
				if(perk.type==_PerkType.ModifyGeneralStats){
					startY=DrawGeneralStats(startX, startY+2, perk);
				}
				else if(perk.type==_PerkType.AddWeapon){
					startY=DrawAddWeapon(startX, startY, perk);
				}
				else if(perk.type==_PerkType.AddAbility){
					startY=DrawAddAbility(startX, startY, perk);
				}
				else if(perk.type==_PerkType.ModifyWeapon){
					startY=DrawModifyWeapon(startX, startY+2, perk);
				}
				else if(perk.type==_PerkType.ModifyAbility){
					startY=DrawModifyAbility(startX, startY+2, perk);
				}
				else if(perk.type==_PerkType.Custom){
					startY=DrawCustom(startX, startY+2, perk);
				}
			}
				
			return startY+spaceY;
		}
		
		
		
		
		private float DrawModifyAbility(float startX, float startY, Perk perk){
			cont=new GUIContent("Apply To All:", "Check if the perk bonus applies to all player ability (when applicable)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.appliedToAllAbility=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), perk.appliedToAllAbility);
			
			if(!perk.appliedToAllAbility){
				cont=new GUIContent("Linked Abilities:", "The abilities that will gain the perk bonus");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				for(int i=0; i<perk.abilityIDList.Count+1; i++){
					int index=i<perk.abilityIDList.Count ? TDSEditor.GetAbilityIndex(perk.abilityIDList[i]) : 0 ;
					
					if(i>0) startY+=spaceY;
					EditorGUI.LabelField(new Rect(startX+spaceX-10, startY, width, height), "-");
					index = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), index, abilityLabel);
					
					if(index>0){
						int ID=abilityDB.abilityList[index-1].ID;
						if(!perk.abilityIDList.Contains(ID)){
							if(i<perk.abilityIDList.Count) perk.abilityIDList[i]=ID;
							else perk.abilityIDList.Add(ID);
						}
					}
					else if(i<perk.abilityIDList.Count){ perk.abilityIDList.RemoveAt(i); i-=1; }
				}
			}
			
			string stackText="\nValue stacks with multiple perks";
			
			startY+=5;
			
				cont=new GUIContent("Cost:", "Ability energy cost multiplier.\n-0.1 being 10% reduction in ability's energy cost"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abCost);
			
				cont=new GUIContent("Cooldown:", "Ability cooldown multiplier.\n-0.1 being 10% reduction in ability's cooldown"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abCooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abCooldown);
			
				cont=new GUIContent("Range:", "Ability range multiplier.\n0.1 being 10% increase in ability's range"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abRange=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abRange);
			
			startY+=5;
			
				cont=new GUIContent("Damage:", "Ability damage multiplier\n0.1 being 10% increase in ability's damage"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abDmg=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abDmg);
			
				cont=new GUIContent("Crit Chance:", "Ability critical chance multiplier\n0.1 being 10% increase in ability's damage"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abCrit=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abCrit);
			
				cont=new GUIContent("Crit Multiplier:", "Ability critical-multiplier's multiplier\n0.1 being 10% increase in ability's critical multiplier"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abCritMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abCritMul);
				
				cont=new GUIContent("AOE:", "Ability AOE radius multiplier\n0.1 being 10% increase in ability's AOE Radius"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abAOE=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.abAOE);
			
			startY+=5;
			
				int effectIdx=perk.abEffectID>=0 ? TDSEditor.GetEffectIndex(perk.abEffectID) : 0 ;
			
				cont=new GUIContent("New Effect:", "replace existing ability effect with the specified effect");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				effectIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effectIdx, effectLabel);
				if(effectIdx>0) perk.abEffectID=effectDB.effectList[effectIdx-1].ID;
				else perk.abEffectID=-1;
			
			return startY;
		}
		
		private float DrawModifyWeapon(float startX, float startY, Perk perk){
			cont=new GUIContent("Apply To All:", "Check if the perk bonus applies to all player weapon (when applicable)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.appliedToAllWeapon=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), perk.appliedToAllWeapon);
			
			if(!perk.appliedToAllWeapon){
				cont=new GUIContent("Linked Weapons:", "The weapons that will gain the perk bonus");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				for(int i=0; i<perk.weaponIDList.Count+1; i++){
					int index=i<perk.weaponIDList.Count ? TDSEditor.GetWeaponIndex(perk.weaponIDList[i]) : 0 ;
					
					if(i>0) startY+=spaceY;
					EditorGUI.LabelField(new Rect(startX+spaceX-10, startY, width, height), "-");
					index = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), index, weaponLabel);
					
					if(index>0){
						int ID=weaponDB.weaponList[index-1].ID;
						if(!perk.weaponIDList.Contains(ID)){
							if(i<perk.weaponIDList.Count) perk.weaponIDList[i]=ID;
							else perk.weaponIDList.Add(ID);
						}
					}
					else if(i<perk.weaponIDList.Count){ perk.weaponIDList.RemoveAt(i); i-=1; }
				}
			}
			
			string stackText="\nValue stacks with multiple perks";
			
			startY+=5;
			
				cont=new GUIContent("Damage:", "Weapon damage multiplier\n0.1 being 10% increase in weapon's damage"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapDmg=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapDmg);
			
				cont=new GUIContent("Crit Chance:", "Weapon critical chance multiplier\n0.1 being 10% increase in weapon's critical chance"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapCrit=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapCrit);
			
				cont=new GUIContent("Crit Multiplier:", "Weapon critical-multiplier multiplier\n0.1 being 10% increase in weapon's critical-multiplier"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapCritMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapCritMul);
				
				cont=new GUIContent("AOE:", "Weapon AOE radius multiplier\n0.1 being 10% increase in weapon's AOE radius"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapAOE=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapAOE);
				
			startY+=5;
			
				cont=new GUIContent("range:", "Weapon range multiplier\n0.1 being 10% increase in weapon's range"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapRange=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapRange);
			
				cont=new GUIContent("Cooldown:", "Weapon cooldown multiplier\n-0.1 being 10% reduction in weapon's cooldown"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapCooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapCooldown);
			
				cont=new GUIContent("Clip Size:", "Weapon clip size multiplier\n0.1 being 10% increase in weapon's clip size"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapClipSize=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapClipSize);
				
				cont=new GUIContent("Ammo Cap:", "Weapon ammo cap multiplier\n0.1 being 10% increase in weapon's ammo cap"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapAmmoCap=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapAmmoCap);
			
				cont=new GUIContent("Reload:", "Weapon reload duration multiplier\n-0.1 being 10% reduction in weapon's reload duration"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapReloadDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapReloadDuration);
				
				cont=new GUIContent("Recoil:", "Weapon recoil multiplier\n-0.1 being 10% reduction in weapon's recoil"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.weapRecoilMagnitude=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.weapRecoilMagnitude);
			
			startY+=5;
				
				int effectIdx=perk.weapEffectID>=0 ? TDSEditor.GetEffectIndex(perk.weapEffectID) : 0 ;
			
				cont=new GUIContent("New Effect:", "replace existing weapon attack effect with the specified effect");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				effectIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effectIdx, effectLabel);
				if(effectIdx>0) perk.weapEffectID=effectDB.effectList[effectIdx-1].ID;
				else perk.weapEffectID=-1;
			
			startY+=5;
				
				int abIdx=perk.weapAbilityID>=0 ? TDSEditor.GetAbilityIndex(perk.weapAbilityID) : 0 ;
			
				cont=new GUIContent("New Alt-Attack:", "replace existing weapon alternate attack with the specified ability");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				abIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), abIdx, effectLabel);
				if(abIdx>0) perk.weapAbilityID=abilityDB.abilityList[abIdx-1].ID;
				else perk.weapAbilityID=-1;
			
			return startY;
		}
		
		private float DrawCustom(float startX, float startY, Perk perk){
			cont=new GUIContent("Custom Object:", "The object to create in game upon purchasing the perk.\nCustom script can be attached on this object to achieve various effect.\nThe object will be placed under the hierarchy of the player object");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.customObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), perk.customObject, typeof(GameObject), false);
			
			return startY;
		}
		
		private float DrawAddWeapon(float startX, float startY, Perk perk){
			
			startY+=35;
			
				int weaponIdx=perk.newWeaponID>=0 ? TDSEditor.GetWeaponIndex(perk.newWeaponID) : 0 ;
		
				TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), weaponIdx>0 ? weaponDB.weaponList[weaponIdx-1].icon : null);
				if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit ")) WeaponEditorWindow.Init();
				
				cont=new GUIContent("New Weapon:", "New weapon to be made available to player");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont, headerStyle);
				
				weaponIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weaponIdx, weaponLabel);
				if(weaponIdx>0) perk.newWeaponID=weaponDB.weaponList[weaponIdx-1].ID;
				else perk.newWeaponID=-1;
			
			
				cont=new GUIContent("Replace Existing:", "Check if the new weapon is to replace player's current weapon");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.replaceExisting=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), perk.replaceExisting);
			
			
				if(!perk.replaceExisting){
					weaponIdx=perk.replaceWeaponID>=0 ? TDSEditor.GetWeaponIndex(perk.replaceWeaponID) : 0 ;
					
					cont=new GUIContent("Replacing:", "If the new weapon is to replace a existing player's weapon\n\nIf no matching weapon is found during runtime, the new weapon will simply be added");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					
					weaponIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weaponIdx, weaponLabel);
					if(weaponIdx>0) perk.replaceWeaponID=weaponDB.weaponList[weaponIdx-1].ID;
					else perk.replaceWeaponID=-1;
				}
				else{
					cont=new GUIContent("Replacing:", "If the new weapon is to replace a existing player's ability\n\nIf no matching weapon is found during runtime, the new weapon will simply be added");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				}
				
			return startY;
		}
		
		private float DrawAddAbility(float startX, float startY, Perk perk){
			startY+=45;
			
				int abilityIdx=perk.newAbilityID>=0 ? TDSEditor.GetAbilityIndex(perk.newAbilityID) : 0 ;
		
				TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), abilityIdx>0 ? abilityDB.abilityList[abilityIdx-1].icon : null);
				if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit ")) AbilityEditorWindow.Init();
				
				cont=new GUIContent("New Ability:", "New ability to be made available to player");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont, headerStyle);
				
				abilityIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), abilityIdx, abilityLabel);
				if(abilityIdx>0) perk.newAbilityID=abilityDB.abilityList[abilityIdx-1].ID;
				else perk.newAbilityID=-1;
			
			
				abilityIdx=perk.replaceAbilityID>=0 ? TDSEditor.GetAbilityIndex(perk.replaceAbilityID) : 0 ;
		
				cont=new GUIContent("Replace Existing:", "If the new ability is to replace a existing player's ability\n\nIf no matching ability is found during runtime, the new ability will simply be added");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				abilityIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), abilityIdx, abilityLabel);
				if(abilityIdx>0) perk.replaceAbilityID=abilityDB.abilityList[abilityIdx-1].ID;
				else perk.replaceAbilityID=-1;
				
			return startY;
		}
		
		private float DrawGeneralStats(float startX, float startY, Perk perk){
			
			string stackText="\nValue stacks with multiple perks";
			
				cont=new GUIContent("hitPoint:", "Player direct hitpoint gained\n0.1 being 10% increase in player's hitpoint capacity");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.hitPoint=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.hitPoint);
			
				cont=new GUIContent("hitPoint Cap:", "Player direct hitpoint capacity gained"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.hitPointCap=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.hitPointCap);
			
				cont=new GUIContent("hitPoint Regen:", "Player direct hitpoint regeneration rate gained"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.hitPointRegen=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.hitPointRegen);
			
			startY+=5;
				
				cont=new GUIContent("Energy:", "Player direct energy gained");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.energy=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.energy);
			
				cont=new GUIContent("Energy Cap:", "Player direct energy capacity gained"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.energyCap=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.energyCap);
			
				cont=new GUIContent("Energy Regen:", "Player direct energy regeneration rate gained"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.energyRegen=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.energyRegen);
				
			startY+=5;
				
				cont=new GUIContent("Speed Mul.:", "Player move speed multiplier\n0.1 being 10% increase in player's move speed"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.moveSpeedMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.moveSpeedMul);
			
			startY+=5;
				
				cont=new GUIContent("Damage Mul.:", "Player damage multiplier\n0.1 being 10% increase in player's damage output (weapon's value)"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.dmgMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.dmgMul);
				
				cont=new GUIContent("Crit Chance Mul.:", "Player critical chance multiplier\n0.1 being 10% increase in player's critical chance (weapon's value)"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.critChanceMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.critChanceMul);
				
				cont=new GUIContent("Crit Multiplier Mul.:", "Player critical-multiplier multiplier\n0.1 being 10% increase in player's critical-multiplier (weapon's value)"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.CritMultiplierMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.CritMultiplierMul);
			
			startY+=5;
				
				cont=new GUIContent("Exp Gain Mul.:", "Player experience gain multiplier\n0.1 being 10% increase in player's experience gain"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.expGainMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.expGainMul);
			
				cont=new GUIContent("Perk Cur. Gain Mul.:", "Player perk currency gain multiplier\n0.1 being 10% increase in player's perk currency gain"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.creditGainMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.creditGainMul);
			
				//cont=new GUIContent("Credit Gain Mul.:", "Player credit gain multiplier\n0.1 being 10% increase in player's credit gain"+stackText);
				//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//perk.creditGainMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.creditGainMul);
				
				cont=new GUIContent("Score Gain Mul.:", "Player score multiplier\n0.1 being 10% increase in player's score gain"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.scoreGainMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.scoreGainMul);
				
				cont=new GUIContent("HP Gain Mul.:", "Player hitpoint multiplier (from external source like collectible creep kill)\n0.1 being 10% increase in player's hitpoint gain"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.hitPointGainMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.hitPointGainMul);
				
				cont=new GUIContent("Energy Gain Mul.:", "Player energy gain multiplier (from external source like collectible creep kill)\n0.1 being 10% increase in player's energy gain"+stackText);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.energyGainMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.energyGainMul);
					
			return startY;
		}
		
		
		private bool showTypeDesp=false;
		private float DrawPerkConfigurator(float startX, float startY, Perk perk){
			
			TDSEditorUtility.DrawSprite(new Rect(startX, startY, 60, 60), perk.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The perk name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/4, width, height), cont);
			perk.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), perk.name);
			
			cont=new GUIContent("Icon:", "The perk icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), perk.icon, typeof(Sprite), false);
			
			cont=new GUIContent("PerkID:", "The ID used to associate a perk item in perk menu to a perk when configuring perk menu manually");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.LabelField(new Rect(startX+spaceX-65, startY, width-5, height), perk.ID.ToString());
			
			startX-=65;
			startY+=15;//+spaceY-spaceY/2;
			
			
			if(showTypeDesp){
				EditorGUI.HelpBox(new Rect(startX, startY+=spaceY, width+spaceX, 40), perkTypeTooltip[(int)perk.type], MessageType.Info);
				startY+=45-height;
			}
		
			int type=(int)perk.type;
			cont=new GUIContent("Perk Type:", "What the perk does");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
			contL=new GUIContent[perkTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(perkTypeLabel[i], perkTypeTooltip[i]);
			type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), type, contL);
			perk.type=(_PerkType)type;
			
			showTypeDesp=EditorGUI.ToggleLeft(new Rect(startX+spaceX+width+2, startY, width, 20), "Show Description", showTypeDesp);
			
			startY+=spaceY;
			
			startY=DrawPerkGeneralSetting(startX, startY+spaceY, perk);
			
			startY=DrawPerkStats(startX, startY+spaceY, perk);
			
			
			startY+=15;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Perk description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY, 400, 20), cont);
			perk.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), perk.desp, style);
			
			return startY+170;
		}
		
		
		
		
		protected Vector2 DrawPerkList(float startX, float startY, List<Perk> perkList){
			List<Item> list=new List<Item>();
			for(int i=0; i<perkList.Count; i++){
				Item item=new Item(perkList[i].ID, perkList[i].name, perkList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		int NewItem(int cloneID=-1){
			Perk perk=null;
			if(cloneID==-1){
				perk=new Perk();
				perk.name="New Perk";
			}
			else{
				perk=perkDB.perkList[selectID].Clone();
			}
			perk.ID=GenerateNewID(perkIDList);
			perkIDList.Add(perk.ID);
			
			perkDB.perkList.Add(perk);
			
			UpdateLabel_Perk();
			
			return perkDB.perkList.Count-1;
		}
		void DeleteItem(){
			perkIDList.Remove(perkDB.perkList[deleteID].ID);
			perkDB.perkList.RemoveAt(deleteID);
			
			UpdateLabel_Perk();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<perkDB.perkList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Perk perk=perkDB.perkList[selectID];
			perkDB.perkList[selectID]=perkDB.perkList[selectID+dir];
			perkDB.perkList[selectID+dir]=perk;
			selectID+=dir;
		}
		
		
	}
}
