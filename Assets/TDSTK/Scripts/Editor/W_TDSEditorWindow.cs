using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class TDSEditorWindow : EditorWindow {
		
		//repaint the editor windows when undo is performed, an item could have been added/removed
		public virtual void Awake(){ Undo.undoRedoPerformed += RepaintWindow; }
		void RepaintWindow(){ 
			TDSEditor.dirty=!TDSEditor.dirty;
		}	
		
		
		//for item list
		protected delegate void EditorCallback();		
		protected EditorCallback selectCallback;
		protected EditorCallback shiftItemUpCallback;
		protected EditorCallback shiftItemDownCallback;
		//protected EditorCallback NewItemCallback;
		protected EditorCallback deleteItemCallback;
		
		protected bool minimiseList=false;
		protected Rect visibleRectList;
		protected Rect contentRectList;
		protected Vector2 scrollPosList;
		
		public int deleteID=-1;
		public int selectID=0;
		protected void Select(int ID){
			if(selectID==ID) return;
			
			selectID=ID;
			GUI.FocusControl ("");
			
			if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
			if(selectID*35>scrollPosList.y+visibleRectList.height-40) scrollPosList.y=selectID*35-visibleRectList.height+40;
			
			if(selectCallback!=null) selectCallback();
		}
		
		protected Vector2 DrawList(float startX, float startY, float winWidth, float winHeight, List<Item> list, bool drawRemove=true, bool shiftItem=true, bool clampSelectID=true){
			float width=minimiseList ? 60 : 260;
			
			if(!minimiseList && shiftItem){
				if(GUI.Button(new Rect(startX+180, startY-20, 40, 18), "up")){
					if(shiftItemUpCallback!=null) shiftItemUpCallback();
					else Debug.Log("call back is null");
					if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
				}
				if(GUI.Button(new Rect(startX+222, startY-20, 40, 18), "down")){
					if(shiftItemDownCallback!=null) shiftItemDownCallback();	
					else Debug.Log("call back is null");
					if(visibleRectList.height-35<selectID*35) scrollPosList.y=(selectID+1)*35-visibleRectList.height+5;
				}
			}
			
			
			visibleRectList=new Rect(startX, startY, width+15, winHeight-startY-5);
			contentRectList=new Rect(startX, startY, width, list.Count*35+5);
			
			GUI.color=new Color(.8f, .8f, .8f, 1f);
			GUI.Box(visibleRectList, "");
			GUI.color=Color.white;
			
			scrollPosList = GUI.BeginScrollView(visibleRectList, scrollPosList, contentRectList);
			
				startY+=5;	startX+=5;
			
				for(int i=0; i<list.Count; i++){
					
					TDSEditorUtility.DrawSprite(new Rect(startX, startY+(i*35), 30, 30), list[i].icon);
					
					if(minimiseList){
						if(selectID==i) GUI.color = new Color(0, 1f, 1f, 1f);
						if(GUI.Button(new Rect(startX+35, startY+(i*35), 30, 30), "")) Select(i);
						GUI.color = Color.white;
						continue;
					}
					
					if(selectID==i) GUI.color = new Color(0, 1f, 1f, 1f);
					if(GUI.Button(new Rect(startX+35, startY+(i*35), 150+(!drawRemove ? 60 : 0), 30), list[i].name)) Select(i);
					GUI.color = Color.white;
					
					if(!drawRemove) continue;
					
					if(deleteID==i){
						if(GUI.Button(new Rect(startX+190, startY+(i*35), 60, 15), "cancel")) deleteID=-1;
						
						GUI.color = Color.red;
						if(GUI.Button(new Rect(startX+190, startY+(i*35)+15, 60, 15), "confirm")){
							if(selectID>=deleteID) Select(Mathf.Max(0, selectID-1));
							if(deleteItemCallback!=null) deleteItemCallback();
							else Debug.Log("callback is null");
							deleteID=-1;
						}
						GUI.color = Color.white;
					}
					else{
						if(GUI.Button(new Rect(startX+190, startY+(i*35), 60, 15), "remove")) deleteID=i;
					}
				}
			
			GUI.EndScrollView();
			
			if(clampSelectID) selectID=Mathf.Clamp(selectID, 0, list.Count-1);
			
			return new Vector2(startX+width+10, startY);
		}
		
		
		
		
		
		
		
		
		protected SerializedProperty srlPpt;	//serialized property (memeber of the serialized obj)
		
		protected bool pptBoolValue;			//temp bool value of the serialized property (use for enable/disable flag such as anchor-down, enable-attack and what not)
		protected int pptIntValue;				//temp int value of the serialized property (use for enable/disable flag such as anchor-down, enable-attack and what not)
		
		protected bool showVar;					//temp bool value for variable depending on (enable/disable flag such as anchor-down, destroy effect)
		
		protected Vector2 DrawUnitBaseStats(float startX, float startY, Unit unit, bool isPlayer){
			TDSEditorUtility.DrawSprite(new Rect(startX, startY, 60, 60), unit.icon);
			startX+=65;
			
			float offsetY=TDSEditor.IsPrefab(unit.gameObject) ? 5 : 0 ;
			
			cont=new GUIContent("Name:", "The unit name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=offsetY, width, height), cont);
			unit.unitName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), unit.unitName);
			
			cont=new GUIContent("Icon:", "The unit icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), unit.icon, typeof(Sprite), false);
			
			if(TDSEditor.IsPrefab(unit.gameObject)){
				cont=new GUIContent("Prefab:", "The prefab object of the unit\nClick this to highlight it in the ProjectTab");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), unit.gameObject, typeof(GameObject), false);
			}
			else{
				EditorGUI.HelpBox(new Rect(startX, startY+=spaceY, width+spaceX-65, height+5), "The unit being edited is not a prefab", MessageType.Warning);
				startY+=5;
			}
			
			startX-=65;
			startY+=spaceY;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Basic Stats", headerStyle);
			
			startY+=5;
			
			cont=new GUIContent("Hit Point:", "The hit-point capacity of the unit");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.hitPointFull=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.hitPointFull);
			
			unit.startHitPointAtFull=EditorGUI.Toggle(new Rect(startX+spaceX+50, startY, 40, height), unit.startHitPointAtFull);
			EditorGUI.LabelField(new Rect(startX+spaceX+65, startY, width, height), " - start at full");
			
			cont=new GUIContent("Hit Point Regen:", "The amount of hit-point regenerated per second");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.hpRegenRate=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.hpRegenRate);
			
			cont=new GUIContent("Hit Point Stagger:", "The duration in second in which the hit-point regen would stop after the unit being hit");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(unit.hpRegenRate>0) unit.hpRegenStagger=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.hpRegenStagger);
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			if(isPlayer){
				startY+=10;
				
				cont=new GUIContent("Energy:", "The energy capacity of the unit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.energyFull=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.energyFull);
				
				unit.startEnergyAtFull=EditorGUI.Toggle(new Rect(startX+spaceX+50, startY, 40, height), unit.startEnergyAtFull);
				EditorGUI.LabelField(new Rect(startX+spaceX+65, startY, width, height), " - start at full");
				
				cont=new GUIContent("Energy Rate:", "The rate (per second) in which the energy will recharge");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.energyRate=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.energyRate);
			}
			
			startY+=10;
			
			cont=new GUIContent("Armor Type:", "The armor type of the unit\nArmor type can be configured in Damage Armor Table Editor");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(armorTypeLabel.Length>0)
				unit.armorType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.armorType, armorTypeLabel);
			else{
				if(GUI.Button(new Rect(startX+spaceX, startY, 83, height-2), "Add Type")){
					DamageTableEditorWindow.Init();
				}
			}
			
			return new Vector2(startX, startY);
		}
		
		
		
		//not in used, wip
		/*
		protected Vector2 DrawAttackStats(string propertyName, float startX, float startY, SerializedObject aStats, bool showAOE=true, bool showPhysics=true, string label="Attack Stats"){
			
			EditorGUI.LabelField(new Rect(startX, startY, width+50, height), label, headerStyle);	startY+=spaceY;
			
			string pf="attackStats.";
			//SerializedProperty spas= aStats.FindProperty("attackStats");
			SerializedProperty spas= aStats.FindProperty(propertyName);
			string lbSp=" - ";
			
			cont=new GUIContent(lbSp+"Damage Type:", "The damage type of the unit\nDamage type can be configured in Damage Armor Table Editor");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			if(damageTypeLabel.Length>0){
				srlPpt=spas.FindPropertyRelative("damageType");
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			//Debug.Log(srlPpt.intValue+"    "+srlPpt.hasMultipleDifferentValues);
				EditorGUI.BeginChangeCheck();
				int value=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), srlPpt.intValue, damageTypeLabel);
				if(EditorGUI.EndChangeCheck()) srlPpt.intValue=value;
				EditorGUI.showMixedValue=false;
			
				//aStats.damageType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), aStats.damageType, damageTypeLabel);
			}
			else{
				if(GUI.Button(new Rect(startX+spaceX, startY, 83, height-2), "Add Type")) DamageTableEditorWindow.Init();
			}
			
			cont=new GUIContent(lbSp+"Damage (Min/Max):", "Damage value done to the target's hit-point.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"damageMin"), contN);
			EditorGUI.PropertyField(new Rect(startX+spaceX+42, startY, 40, height), aStats.FindProperty(pf+"damageMax"), contN);
			
			startY+=10;
			
			if(showAOE){
				cont=new GUIContent(lbSp+"AOE Radius:", "Area of effect radius of the attack. Any hostile unit within the area is affected by the attack");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"aoeRadius"), contN);
				
				cont=new GUIContent(lbSp+"Diminishing AOE22:", "Check if damage value diminished the further away the target is from the center of the aoe");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(aStats.FindProperty(pf+"aoeRadius").floatValue>0) 
					EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"diminishingAOE"));
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				startY+=10;
			}
			
			cont=new GUIContent(lbSp+"Critical Chance:", "The chance of the attack to score a critical. Takes value from 0-1 with 0.3 being 30% to score a critical");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"critChance"), contN);
			
			cont=new GUIContent(lbSp+"Critical Multiplier:", "The multiplier to be applied to damage if the attack scores a critical.\n - 1.5 for 150% of normal damage, 2 for 200% and so on");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"critChance"), contN);
			if(aStats.FindProperty(pf+"critChance").floatValue>0) 
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"critMultiplier"), contN);
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=10;
			
			if(showPhysics){
				cont=new GUIContent(lbSp+"Impact Force:", "If the attack will applies a knock back force to the target\nOnly applies if the attack is a direct hit from a shoot object");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"impactForce"), contN);
				
				cont=new GUIContent(lbSp+"Explosion Radius:", "The radius in which all unit is affected by explosion force");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"explosionRadius"), contN);
				
				cont=new GUIContent(lbSp+"Explosion Force:", "The force of the explosion which pushes all affect unit away from the impact point");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(aStats.FindProperty(pf+"explosionRadius").floatValue>0) 
					EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), aStats.FindProperty(pf+"explosionRadius"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			}
			
			startY+=30;
			
			srlPpt=aStats.FindProperty(pf+"effectID");
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			int effectIdx=srlPpt.intValue>=0 ? TDSEditor.GetEffectIndex(srlPpt.intValue) : 0 ;
			
			if(!srlPpt.hasMultipleDifferentValues)
				TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), effectIdx>0 ? effectDB.effectList[effectIdx-1].icon : null);
			if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit")) EffectEditorWindow.Init();
			
			cont=new GUIContent(lbSp+"Attack Effect:", "Special effect that applies with each hit (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont);
			
			EditorGUI.BeginChangeCheck();
			effectIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effectIdx, effectLabel);
			if(EditorGUI.EndChangeCheck()){
				if(effectIdx>0) srlPpt.intValue=effectDB.effectList[effectIdx-1].ID;
				else srlPpt.intValue=-1;
			}
			EditorGUI.showMixedValue=false;
			
			return new Vector2(startX, startY);
		}
		*/
		
		protected Vector2 DrawAttackStats1(float startX, float startY, AttackStats aStats, bool showAOE=true, bool showPhysics=true, string label="Attack Stats"){
			
			EditorGUI.LabelField(new Rect(startX, startY, width+50, height), label, headerStyle);	startY+=spaceY;
			
			string lbSp=" - ";
			
			cont=new GUIContent(lbSp+"Damage Type:", "The damage type of the unit\nDamage type can be configured in Damage Armor Table Editor");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			if(damageTypeLabel.Length>0)
				aStats.damageType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), aStats.damageType, damageTypeLabel);
			else{
				if(GUI.Button(new Rect(startX+spaceX, startY, 83, height-2), "Add Type")) DamageTableEditorWindow.Init();
			}
			
			cont=new GUIContent(lbSp+"Damage (Min/Max):", "Damage value done to the target's hit-point.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			aStats.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.damageMin);
			aStats.damageMax=EditorGUI.FloatField(new Rect(startX+spaceX+42, startY, 40, height), aStats.damageMax);
			
			startY+=10;
			
			if(showAOE){
				cont=new GUIContent(lbSp+"AOE Radius:", "Area of effect radius of the attack. Any hostile unit within the area is affected by the attack");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				aStats.aoeRadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.aoeRadius);
				
				//used in pre-version1.2, dimishingAOE is wrongly spelt
				//cont=new GUIContent(lbSp+"Diminishing AOE:", "Check if damage value diminished the further away the target is from the center of the aoe");
				//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//if(aStats.aoeRadius>0) aStats.dimishingAOE=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), aStats.dimishingAOE);
				//else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent(lbSp+"Diminishing AOE:", "Check if damage value diminished the further away the target is from the center of the aoe");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(aStats.aoeRadius>0) aStats.diminishingAOE=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), aStats.diminishingAOE);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				startY+=10;
			}
			
			cont=new GUIContent(lbSp+"Critical Chance:", "The chance of the attack to score a critical. Takes value from 0-1 with 0.3 being 30% to score a critical");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			aStats.critChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.critChance);
			
			cont=new GUIContent(lbSp+"Critical Multiplier:", "The multiplier to be applied to damage if the attack scores a critical.\n - 1.5 for 150% of normal damage, 2 for 200% and so on");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(aStats.critChance>0) aStats.critMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.critMultiplier);
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=10;
			
			if(showPhysics){
				cont=new GUIContent(lbSp+"Impact Force:", "If the attack will applies a knock back force to the target\nOnly applies if the attack is a direct hit from a shoot object");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				aStats.impactForce=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.impactForce);
				
				cont=new GUIContent(lbSp+"Explosion Radius:", "The radius in which all unit is affected by explosion force");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				aStats.explosionRadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.explosionRadius);
				
				cont=new GUIContent(lbSp+"Explosion Force:", "The force of the explosion which pushes all affect unit away from the impact point");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(aStats.explosionRadius>0) aStats.explosionForce=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), aStats.explosionForce);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			}
			
			startY+=30;
			
			
			
			int effectIdx=aStats.effectID>=0 ? TDSEditor.GetEffectIndex(aStats.effectID) : 0 ;
			
			TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), effectIdx>0 ? effectDB.effectList[effectIdx-1].icon : null);
			if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit")) EffectEditorWindow.Init();
			
			cont=new GUIContent(lbSp+"Attack Effect:", "Special effect that applies with each hit (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont);
			
			effectIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effectIdx, effectLabel);
			if(effectIdx>0) aStats.effectID=effectDB.effectList[effectIdx-1].ID;
			else aStats.effectID=-1;
			
			
			return new Vector2(startX, startY);
		}
		
		
		
		
		
		protected Vector2 DrawDestroyEffectObj(float startX, float startY, SerializedObject srlObj){
			cont=new GUIContent("DestroyCamShake:", "The camera shake magnitude whenever the unit is destroyed");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, width, height), srlObj.FindProperty("destroyCamShake"), contN);
			
			startY+=10;
			
			cont=new GUIContent("DestroyedEffectObj:", "The object to be spawned when the unit is destroyed (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, width, height), srlObj.FindProperty("destroyedEffectObj"), contN);
			
			cont=new GUIContent("AutoDestroy Effect:", "Check if the effect object needs to be removed from the game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(srlObj.FindProperty("destroyedEffectObj").objectReferenceValue!=null){
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, width, height), srlObj.FindProperty("autoDestroyDObj"), contN);
				
				if(srlObj.FindProperty("autoDestroyDObj").boolValue){
					cont=new GUIContent(" - Duration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX+spaceX+15, startY, width, height), cont);
					EditorGUI.PropertyField(new Rect(startX+spaceX+width-58, startY, 40, height), srlObj.FindProperty("dObjActiveDuration"), contN);
				}
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			return new Vector2(startX, startY);
		}
		protected Vector2 DrawDestroyEffectObj(float startX, float startY, Unit unit){
			cont=new GUIContent("DestroyCamShake:", "The camera shake magnitude whenever the unit is destroyed");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			unit.destroyCamShake=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.destroyCamShake);
			
			startY+=10;
			
			cont=new GUIContent("DestroyedEffectObj:", "The object to be spawned when the unit is destroyed (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.destroyedEffectObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.destroyedEffectObj, typeof(GameObject), false);
			
			cont=new GUIContent("AutoDestroy Effect:", "Check if the effect object needs to be removed from the game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(unit.destroyedEffectObj!=null){
				unit.autoDestroyDObj=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.autoDestroyDObj);
				
				if(unit.autoDestroyDObj){
					cont=new GUIContent(" - Duration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX+spaceX+15, startY, width, height), cont);
					unit.dObjActiveDuration=EditorGUI.FloatField(new Rect(startX+spaceX+width-58, startY, 40, height), unit.dObjActiveDuration);
				}
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			
			return new Vector2(startX, startY);
		}
		
		
		
		
		
		
		
		//for info/stats configuration
		protected float contentHeight=0;
		protected float contentWidth=0;
		
		protected Vector2 scrollPos;
		
		protected GUIContent cont;
		protected GUIContent contN=GUIContent.none;
		protected GUIContent[] contL;
		
		protected float spaceX=120;
		protected float spaceY=18;
		protected float width=150;
		protected float widthS=50;
		protected float height=16;
		
		protected static GUIStyle headerStyle;
		protected static GUIStyle foldoutStyle;
		protected static GUIStyle conflictStyle;
		
		protected bool shootPointFoldout=false;
		
		
		private bool state_Play=false;
		private bool state_Editor=false;
		public static bool TDS_Changed=false;
		public virtual void Update(){
			if(EditorApplication.isPlaying!=state_Play){
				state_Play=EditorApplication.isPlaying;
				Repaint();
			}
			
			if(TDSEditor.dirty!=state_Editor){
				state_Editor=TDSEditor.dirty;
				Repaint();
			}
		}
		
		public virtual bool OnGUI(){
			if(Application.isPlaying){
				EditorGUILayout.HelpBox("Cannot edit while game is playing", MessageType.Info);
				return false;
			}
			
			return true;
		}
		
		
		private static bool loaded=false;
		protected static void LoadDB(){
			if(loaded) return;
			
			loaded=true;
			
			LoadDamageTable();
			LoadProgressionStats();
			LoadEffect();
			
			LoadAbility();
			LoadPerk();
			LoadWeapon();
			LoadCollectible();
			
			LoadUnitAI();
			LoadUnitPlayer();
			
			
			headerStyle=new GUIStyle("Label");
			headerStyle.fontStyle=FontStyle.Bold;
			
			foldoutStyle=new GUIStyle("foldout");
			foldoutStyle.fontStyle=FontStyle.Bold;
			foldoutStyle.normal.textColor = Color.black;
			
			conflictStyle=new GUIStyle("Label");
			conflictStyle.normal.textColor = Color.red;
		}
		
		
		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		protected static void LoadDamageTable(){ TDSEditor.LoadDamageTable(); }
		protected static void UpdateLabel_DamageTable(){ TDSEditor.UpdateLabel_DamageTable(); }
		public static void SetDamageDB(DamageTableDB db, string[] dmgLabel, string[] armLabel){
			damageTableDB=db;
			damageTypeLabel=dmgLabel;
			armorTypeLabel=armLabel;
		}
		
		
		protected static ProgressionStatsDB progressDB;
		//protected static string[] damageTypeLabel;
		//protected static string[] armorTypeLabel;
		protected static void LoadProgressionStats(){ TDSEditor.LoadProgressStats(); }
		//protected static void UpdateLabel_DamageTable(){ TDSEditor.UpdateLabel_DamageTable(); }
		public static void SetProgressionStats(ProgressionStatsDB db){	//, string[] dmgLabel, string[] armLabel){
			progressDB=db;
			//damageTypeLabel=dmgLabel;
			//armorTypeLabel=armLabel;
		}
		
		
		protected static EffectDB effectDB;
		protected static List<int> effectIDList=new List<int>();
		protected static string[] effectLabel;
		protected static void LoadEffect(){ TDSEditor.LoadEffect(); }
		protected static void UpdateLabel_Effect(){ TDSEditor.UpdateLabel_Effect(); }
		public static void SetEffectDB(EffectDB db, List<int> IDList, string[] label){
			effectDB=db;
			effectIDList=IDList;
			effectLabel=label;
		}
		
		
		protected static AbilityDB abilityDB;
		protected static List<int> abilityIDList=new List<int>();
		protected static string[] abilityLabel;
		protected static void LoadAbility(){ TDSEditor.LoadAbility(); }
		protected static void UpdateLabel_Ability(){ TDSEditor.UpdateLabel_Ability(); }
		public static void SetAbilityDB(AbilityDB db, List<int> IDList, string[] label){
			abilityDB=db;
			abilityIDList=IDList;
			abilityLabel=label;
		}
		
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		protected static void LoadPerk(){ TDSEditor.LoadPerk(); }
		protected static void UpdateLabel_Perk(){ TDSEditor.UpdateLabel_Perk(); }
		public static void SetPerkDB(PerkDB db, List<int> IDList, string[] label){
			perkDB=db;
			perkIDList=IDList;
			perkLabel=label;
		}
		
		
		protected static WeaponDB weaponDB;
		protected static List<int> weaponIDList=new List<int>();
		protected static string[] weaponLabel;
		protected static void LoadWeapon(){ TDSEditor.LoadWeapon(); }
		protected static void UpdateLabel_Weapon(){ TDSEditor.UpdateLabel_Weapon(); }
		public static void SetWeaponDB(WeaponDB db, List<int> IDList, string[] label){
			weaponDB=db;
			weaponIDList=IDList;
			weaponLabel=label;
		}
		
		
		protected static CollectibleDB collectibleDB;
		protected static List<int> collectibleIDList=new List<int>();
		protected static string[] collectibleLabel;
		protected static void LoadCollectible(){ TDSEditor.LoadCollectible(); }
		protected static void UpdateLabel_Collectible(){ TDSEditor.UpdateLabel_Collectible(); }
		public static void SetCollectibleDB(CollectibleDB db, List<int> IDList, string[] label){
			collectibleDB=db;
			collectibleIDList=IDList;
			collectibleLabel=label;
		}
		
		
		protected static UnitAIDB unitAIDB;
		protected static List<int> unitAIIDList=new List<int>();
		protected static string[] unitAILabel;
		protected static void LoadUnitAI(){ TDSEditor.LoadUnitAI(); }
		protected static void UpdateLabel_UnitAI(){ TDSEditor.UpdateLabel_UnitAI(); }
		public static void SetUnitAIDB(UnitAIDB db, List<int> IDList, string[] label){
			unitAIDB=db;
			unitAIIDList=IDList;
			unitAILabel=label;
		}
		
		
		protected static UnitPlayerDB unitPlayerDB;
		protected static List<int> unitPlayerIDList=new List<int>();
		protected static string[] unitPlayerLabel;
		protected static void LoadUnitPlayer(){ TDSEditor.LoadUnitPlayer(); }
		protected static void UpdateLabel_UnitPlayer(){ TDSEditor.UpdateLabel_UnitPlayer(); }
		public static void SetUnitPlayerDB(UnitPlayerDB db, List<int> IDList, string[] label){
			unitPlayerDB=db;
			unitPlayerIDList=IDList;
			unitPlayerLabel=label;
		}
		
		
		
		
		protected List<GameObject> objHList=new List<GameObject>();
		protected string[] objHLabelList=new string[0];
		protected void UpdateObjectHierarchyList(GameObject obj){
			TDSEditorUtility.GetObjectHierarchyList(obj, this.SetObjListCallback);
		}
		protected void SetObjListCallback(List<GameObject> objList, string[] labelList){
			objHList=objList;
			objHLabelList=labelList;
		}
		protected static int GetObjectIDFromHList(Transform objT, List<GameObject> objHList){
			if(objT==null) return 0;
			for(int i=1; i<objHList.Count; i++){
				if(objT==objHList[i].transform) return i;
			}
			return 0;
		}
		
		
		protected int GetEffectIndexFromID(int ID){
			for(int i=0; i<effectDB.effectList.Count; i++){
				if(effectDB.effectList[i].ID==ID) return i;
			}
			return -1;
		}
		
		
		protected static int GenerateNewID(List<int> list){
			int ID=0;
			while(list.Contains(ID)) ID+=1;
			return ID;
		}
		
		
		protected void SetDirtyTDS(){ 
			EditorUtility.SetDirty(damageTableDB);
			EditorUtility.SetDirty(progressDB);
			EditorUtility.SetDirty(abilityDB);
			EditorUtility.SetDirty(perkDB);
			EditorUtility.SetDirty(effectDB);
			EditorUtility.SetDirty(weaponDB);
			EditorUtility.SetDirty(collectibleDB);
			EditorUtility.SetDirty(unitAIDB);
			EditorUtility.SetDirty(unitPlayerDB);
			
			for(int i=0; i<weaponDB.weaponList.Count; i++) EditorUtility.SetDirty(weaponDB.weaponList[i]);
			for(int i=0; i<collectibleDB.collectibleList.Count; i++) EditorUtility.SetDirty(collectibleDB.collectibleList[i]);
			for(int i=0; i<unitAIDB.unitList.Count; i++) EditorUtility.SetDirty(unitAIDB.unitList[i]);
			for(int i=0; i<unitPlayerDB.unitList.Count; i++) EditorUtility.SetDirty(unitPlayerDB.unitList[i]);
		}
		
		
	}
	
	
	
}
