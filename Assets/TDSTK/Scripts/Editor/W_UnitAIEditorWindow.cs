using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	[CanEditMultipleObjects]
	public class UnitAIEditorWindow : TDSEditorWindow {
		
		private static UnitAIEditorWindow window;
		
		public static void Init() {
			// Get existing open window or if none, make a new one:
			window = (UnitAIEditorWindow)EditorWindow.GetWindow(typeof (UnitAIEditorWindow), false, "AI Unit Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
			
			window.OnSelectionChange();
		}
		
		
		private static string[] behaviourLabel;
		private static string[] behaviourTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_Behaviour)).Length;
			behaviourLabel=new string[enumLength];
			behaviourTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				behaviourLabel[n]=((_Behaviour)n).ToString();
				
				if((_Behaviour)n==_Behaviour.StandGuard) 		behaviourTooltip[n]="Only chase and attack when aggro-ed, break pursuit when target is out of range";
				if((_Behaviour)n==_Behaviour.Aggressive) 			behaviourTooltip[n]="Actively look for target at all time";
				if((_Behaviour)n==_Behaviour.Aggressive_Trigger)behaviourTooltip[n]="StandGuard until aggo-ed, once aggro-ed switch to Aggressive";
				if((_Behaviour)n==_Behaviour.Passive)				behaviourTooltip[n]="Don't repsond to player";
			}
		}
		
		
		void SetupCallback(){
			selectCallback=this.SelectItem;
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
		}
		
		
		
		public override void Awake(){
			base.Awake();
		}
		
		
		void OnEnable(){
			OnSelectionChange();
		}
		
		
		public override void Update(){
			base.Update();
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<UnitAI> unitList=unitAIDB.unitList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(unitAIDB, "unitAIDB");
			if(unitList.Count>0 && selectID>=0) Undo.RecordObject(unitList[selectID], "unitAI");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New AI Unit:");
			UnitAI newUnit=null;
			newUnit=(UnitAI)EditorGUI.ObjectField(new Rect(125, 7, 140, 17), newUnit, typeof(UnitAI), false);
			if(newUnit!=null) Select(NewItem(newUnit));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawUnitList(startX, startY, unitList);	
			
			startX=v2.x+25;
			
			if(unitList.Count==0 || srlObj==null) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
				
				srlObj.Update();
			
				if(srlObj.isEditingMultipleObjects){
					EditorGUI.HelpBox(new Rect(startX, startY, width+spaceX, 40), "More than 1 UnitAI instance is selected\nMulti-instance editing is not supported\nTry use Inspector instead", MessageType.Warning);
					startY+=55;
				}
			
				UnitAI unitToEdit=selectedUnitList.Count!=0 ? selectedUnitList[0] : unitList[selectID];
				
				Undo.RecordObject(unitToEdit, "unitToEdit");
				
				v2=DrawUnitConfigurator(startX, startY, unitToEdit);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
				srlObj.ApplyModifiedProperties();
			
				if(selectedUnitList.Count>0 && TDSEditor.IsPrefabInstance(selectedUnitList[0].gameObject)){
					PrefabUtility.RecordPrefabInstancePropertyModifications(selectedUnitList[0]);
				}
			
			GUI.EndScrollView();
			
			
			if(GUI.changed){
				SetDirtyTDS();
				for(int i=0; i<selectedUnitList.Count; i++) EditorUtility.SetDirty(selectedUnitList[i]);
			}
			
			return true;
		}
		
		
		
		private bool showMoveSetting=true;
		private bool showAttackSetting=true;
		Vector2 DrawUnitConfigurator(float startX, float startY, UnitAI unit){
			
			Vector2 v2=DrawUnitBaseStats(startX, startY, unit, false);
			startY=v2.y;
			
			startY+=10;
			
			
			//~ int type=(int)unit.behaviour;
			//~ cont=new GUIContent("Base Behaviour:", "Type of the behaviour which determine the how the unit respond to presence of hostile unit");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//~ contL=new GUIContent[behaviourLabel.Length];
			//~ for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(behaviourLabel[i], behaviourTooltip[i]);
			//~ type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), type, contL);
			//~ unit.behaviour=(_Behaviour)type;
			
			//~ cont=new GUIContent("Aggro Range:", "The range at which the unit will chasing down and attacking player\n\nUnit will not start attacking (shooting) until the target is within aggro range");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//~ if(unit.behaviour==_Behaviour.Aggressive_Trigger || unit.behaviour==_Behaviour.StandGuard)
				//~ unit.aggroRange=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.aggroRange);
			//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			srlPpt=srlObj.FindProperty("behaviour");
			
			
			srlObj.Update ();
			EditorGUI.BeginChangeCheck();
			cont=new GUIContent("Base Behaviour:", "Type of the behaviour which determine the how the unit respond to presence of hostile unit");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			contL=new GUIContent[behaviourLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(behaviourLabel[i], behaviourTooltip[i]);
			pptIntValue = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.enumValueIndex, contL);	EditorGUI.EndProperty();
			if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex = pptIntValue;
			
			
			cont=new GUIContent("Aggro Range(P):", "The range at which the unit will chasing down and attacking player\n\nUnit will not start attacking (shooting) until the target is within aggro range");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(srlPpt.enumValueIndex==(int)_Behaviour.Aggressive_Trigger || srlPpt.enumValueIndex==(int)_Behaviour.StandGuard)
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("aggroRange"), contN);
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			srlObj.ApplyModifiedProperties ();
			
			
			startY+=10;
			
			
			showMoveSetting = EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), showMoveSetting, "Show Move Setting", foldoutStyle);
			if(showMoveSetting){
				startX+=15; width-=15;
				
				//~ cont=new GUIContent("Anchored To Point:", "Check if the unit is a static object. Means it will not moved and wont be affected by physics");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
				//~ unit.anchorDown=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.anchorDown);
				
				//~ cont=new GUIContent(" - Move Speed:", "The move speed of the unit");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(!unit.anchorDown) unit.moveSpeed=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.moveSpeed);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				//~ cont=new GUIContent(" - Rotate Speed:", "The rotate speed of the unit (degree per second)");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(!unit.anchorDown) unit.rotateSpeed=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.rotateSpeed);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				//~ cont=new GUIContent(" - Brake  Range:", "The range at which the unit will stop moving towards its target");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(!unit.anchorDown) unit.brakeRange=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.brakeRange);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				
				srlPpt=srlObj.FindProperty("anchorDown");
				
				EditorGUI.BeginChangeCheck();
				cont=new GUIContent("Anchored To Point:", "Check if the unit is a static object. Means it will not moved and wont be affected by physics");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
				pptBoolValue=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.boolValue);	EditorGUI.EndProperty();
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue = pptBoolValue;
				
				showVar=!srlPpt.hasMultipleDifferentValues & !srlPpt.boolValue;
				
				cont=new GUIContent(" - Move Speed(P):", "The move speed of the unit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("moveSpeed"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent(" - Rotate Speed(P):", "The rotate speed of the unit (degree per second)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("rotateSpeed"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent(" - Brake  Range(P):", "The range at which the unit will stop moving towards its target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("brakeRange"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				
				startY+=10;
				
				
				//~ cont=new GUIContent("Stop Ocassionally:", "Check to enable the target to stop every now and then when chasing the target");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ unit.stopOccasionally=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.stopOccasionally);
				
				//~ cont=new GUIContent("  - Stop Chance:", "Chance that determine how often the unit will stop. takes value from 0-1 with 0 as never and 1 as every cooldown");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(unit.stopOccasionally) unit.stopRate=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.stopRate);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				//~ cont=new GUIContent("  - Stop Duration:", "How long (in second) the unit will stop for");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(unit.stopOccasionally) unit.stopDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.stopDuration);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				//~ cont=new GUIContent("  - Stop Cooldown:", "How long (in second) before the unit will try to stop after the last stop");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(unit.stopOccasionally) unit.stopCooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.stopCooldown);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				
				srlPpt=srlObj.FindProperty("stopOccasionally");
				
				EditorGUI.BeginChangeCheck();
				cont=new GUIContent("Stop Ocassionally:", "Check to enable the target to stop every now and then when chasing the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
				pptBoolValue=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.boolValue);	EditorGUI.EndProperty();
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue = pptBoolValue;
				
				showVar=!srlPpt.hasMultipleDifferentValues & !pptBoolValue;
				
				cont=new GUIContent("  - Stop Chance:", "Chance that determine how often the unit will stop. takes value from 0-1 with 0 as never and 1 as every cooldown");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("stopRate"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent("  - Stop Duration:", "How long (in second) the unit will stop for");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("stopDuration"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent("  - Stop Cooldown:", "How long (in second) before the unit will try a stop maneuver after the last stop maneuver");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("stopCooldown"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				
				startY+=10;
				
				
				//~ cont=new GUIContent("Evade Ocassionally:", "Check to enable the target to perform a evade maneuver and then when chasing the target");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ unit.evadeOccasionally=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.evadeOccasionally);
				
				//~ cont=new GUIContent("  - Evade Chance:", "Chance that determine how often the unit will evade. takes value from 0-1 with 0 as never and 1 as every cooldown");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(unit.evadeOccasionally) unit.evadeRate=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.evadeRate);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				//~ cont=new GUIContent("  - Evade Duration:", "How long (in second) the unit will perform the maneuver for");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(unit.evadeOccasionally) unit.evadeDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.evadeDuration);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				//~ cont=new GUIContent("  - Stop Cooldown:", "How long (in second) before the unit will try a evade maneuver after the last evade maneuver");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ if(unit.evadeOccasionally) unit.evadeCooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.evadeCooldown);
				//~ else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				
				srlPpt=srlObj.FindProperty("evadeOccasionally");
				
				EditorGUI.BeginChangeCheck();
				cont=new GUIContent("Evade Ocassionally:", "Check to enable the target to perform a evade maneuver and then when chasing the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
				pptBoolValue=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.boolValue);	EditorGUI.EndProperty();
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue = pptBoolValue;
				
				showVar=!srlPpt.hasMultipleDifferentValues & !pptBoolValue;
				
				cont=new GUIContent("  - Evade Chance:", "Chance that determine how often the unit will evade. takes value from 0-1 with 0 as never and 1 as every cooldown");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("evadeRate"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent("  - Evade Duration:", "How long (in second) the unit will perform the maneuver for");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("evadeDuration"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent("  - Evade Cooldown:", "How long (in second) before the unit will try a evade maneuver after the last evade maneuver");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(showVar) EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("evadeCooldown"), contN);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				
				startX-=15; width+=15;
			}
			
			startY+=10;
			
			showAttackSetting = EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), showAttackSetting, "Show Attack Setting", foldoutStyle);
			if(showAttackSetting){
				startX+=15; width-=15; spaceX+=10; //width-=20;
				
				startY+=5;
				
				//~ cont=new GUIContent("Range Attack:", "Check if the unit can perform any range attack");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
				//~ unit.enableAttack=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.enableAttack);
				
				srlPpt=srlObj.FindProperty("enableRangeAttack");
				
				EditorGUI.BeginChangeCheck();
				cont=new GUIContent("Range Attack:", "Check if the unit can perform any range attack");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
				pptBoolValue=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.boolValue);	EditorGUI.EndProperty();
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue = pptBoolValue;
				
				bool enableRangeAttack=!srlPpt.hasMultipleDifferentValues & pptBoolValue;
				
				//if(unit.enableAttack){
				if(enableRangeAttack){
					string lbSp=" - ";
					
					cont=new GUIContent(lbSp+"Shoot Periodically:", "Check to enable the target to shoot after every attack cooldown, regardless of existance or range of target");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
					unit.shootPeriodically=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.shootPeriodically);
					
					cont=new GUIContent(lbSp+"AlwaysOnTarget:", "Check to have the unit shoot object always fire towards target, regardless of aim direction");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.alwaysShootTowardsTarget=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.alwaysShootTowardsTarget);
					
					
					startY+=10;
					
					
					cont=new GUIContent(lbSp+"Shoot Object:", "The prefab of the bullet/object fired by the unit\nMust be a prefab with ShootObject component attached on it\n\n*Required for range attack to work");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, unit.shootObject!=null ? new GUIStyle("Label") : conflictStyle);
					unit.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.shootObject, typeof(GameObject), false);
					
					int objID=GetObjectIDFromHList(unit.turretObj, objHList);
					cont=new GUIContent(lbSp+"Turret Object:", "The pivot transform on the unit to track the shoot direction");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.turretObj = (objHList[objID]==null) ? null : objHList[objID].transform;
					
					cont=new GUIContent(lbSp+"TurretTrackSpeed:", "The tracking speed of the turret. Slower turret will have a harder time keeping up with target.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.turretObj!=null) unit.turretTrackingSpeed=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.turretTrackingSpeed);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
					
					
					startY+=10;
					
					
					cont=new GUIContent("ShootPoint:", "The transform which indicate the position where the shootObject will be fired from (Optional)\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the weapon transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the firing direction.\n");
					shootPointFoldout=EditorGUI.Foldout(new Rect(startX+3, startY+=spaceY, spaceX, height), shootPointFoldout, cont);
					int shootPointCount=unit.shootPointList.Count;
					shootPointCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), shootPointCount);
					
					if(shootPointCount!=unit.shootPointList.Count){
						while(unit.shootPointList.Count<shootPointCount) unit.shootPointList.Add(null);
						while(unit.shootPointList.Count>shootPointCount) unit.shootPointList.RemoveAt(unit.shootPointList.Count-1);
					}
						
					if(shootPointFoldout){
						for(int i=0; i<unit.shootPointList.Count; i++){
							objID=GetObjectIDFromHList(unit.shootPointList[i], objHList);
							EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
							objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
							unit.shootPointList[i] = (objHList[objID]==null) ? null : objHList[objID].transform;
						}
					}
					
					cont=new GUIContent(lbSp+"Shoot Point Delay:", "The delay in seconds between subsequent shot in each shoot point");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.shootPointList.Count>1)
						unit.shootPointDelay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.shootPointDelay);
					else
						EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
					
					
					startY+=10;
					
					
					cont=new GUIContent(lbSp+"Attack Range:", "The attack range of the unit");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.range);
					
					cont=new GUIContent(lbSp+"Attack Cooldown:", "The cooldown duration in seconds between each subsequent attack");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.cooldown);
					
					startY+=5;
					
					cont=new GUIContent(lbSp+"First Attack Delay:", "The delay in second before the unit can perform it's first attack. To prevent unit to fire immediately after being spawned or as soon as the game start");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.firstAttackDelay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.firstAttackDelay);
					
					cont=new GUIContent("Randomize", "Randomize the first attack delay between 0 and the specified value");
					unit.randFirstAttackDelay=EditorGUI.Toggle(new Rect(startX+spaceX+45, startY, 20, height), unit.randFirstAttackDelay);
					EditorGUI.LabelField(new Rect(startX+spaceX+45+15, startY, width, height), cont);
					
					
					startY+=10;
					
					
					v2=DrawAttackStats1(startX, startY+spaceY, unit.attackStats, true, false, "   Attack Stats (Range)");
					startY=v2.y;
				}
				
				
				
				startY+=10;
				
				
				
				
				
				srlPpt=srlObj.FindProperty("enableContactAttack");
				
				EditorGUI.BeginChangeCheck();
				cont=new GUIContent("AttackOnContact:", "Check if the unit can attack when comes into contact with hostile unit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
				pptBoolValue=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.boolValue);	EditorGUI.EndProperty();
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue = pptBoolValue;
				
				bool enableContactAttack=!srlPpt.hasMultipleDifferentValues & pptBoolValue;
				
				if(enableContactAttack){
					string lbSp=" - ";
					
					cont=new GUIContent(lbSp+"Attack Cooldown:", "The cooldown duration in seconds between each subsequent attack");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
					unit.contactCooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.contactCooldown);
					
					startY+=10;
					
					v2=DrawAttackStats1(startX, startY+spaceY, unit.contactAttackStats, false, false, "   Attack Stats (On Contact)");
					startY=v2.y;
				}
				
				
				startX-=15; width+=15; spaceX-=10; //width+=20;
			}
			
			
			startY+=20;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Misc", headerStyle);
			
			cont=new GUIContent("DestroyUponContact:", "Check to destroy unit upon coming into contact with a hostile");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//unit.destroyUponPlayerContact=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), unit.destroyUponPlayerContact);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("destroyUponPlayerContact"), contN);
			
			startY+=10;
			
			cont=new GUIContent("Score On Destroy:", "Score gained by player when the unit is destroyed");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//unit.valueScore=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), unit.valueScore);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("valueScore"), contN);
			
			cont=new GUIContent("HitPoint On Destroy:", "Hit-Point gained by player when the unit is destroyed");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//unit.valueHitPoint=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), unit.valueHitPoint);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("valueHitPoint"), contN);
			
			cont=new GUIContent("Energy On Destroy:", "Energy gained by player when the unit is destroyed");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//unit.valueEnergy=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), unit.valueEnergy);
			EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("valueEnergy"), contN);
			
			//cont=new GUIContent("Credit On Destroy:", "Credit gained by player when the unit is destroyed");
			//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//unit.valueCredits=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), unit.valueCredits);
			
			startY+=10;
			
			
			//Vector2 v3=DrawDestroyEffectObj(startX, startY+spaceY, unit);
			Vector2 v3=DrawDestroyEffectObj(startX, startY+spaceY, srlObj);
			startY=v3.y+10;
			
			
			srlPpt=srlObj.FindProperty("useDropManager");
				
			EditorGUI.BeginChangeCheck();
			cont=new GUIContent("Use DropManager:", "Check to use DropManager to determine what the unit drops");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			pptBoolValue=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), EditorGUI.BeginProperty(new Rect(), contN, srlPpt), srlPpt.boolValue);	EditorGUI.EndProperty();
			if(EditorGUI.EndChangeCheck()) srlPpt.boolValue = pptBoolValue;
			
			showVar=!srlPpt.hasMultipleDifferentValues & !pptBoolValue;
			
			cont=new GUIContent("Drop Object:", "The game object to drop when the unit is destroyed");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(showVar) 
				//unit.dropObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.dropObject, typeof(GameObject), false);
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, width, height), srlObj.FindProperty("dropObject"), contN);
			else 
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			cont=new GUIContent("Drop Chance:", "The chance for the object to drop. Takes value from 0-1 with 0.3 being 30% chance to drop");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(showVar) 
				//unit.dropChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.dropChance);
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("dropChance"), contN);
			else 
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			
			startY+=10;
			
			
			
			
			EditorGUI.showMixedValue=srlObj.FindProperty("spawnUponDestroy").hasMultipleDifferentValues;
			int unitIdx=unit.spawnUponDestroy!=null ? TDSEditor.GetUnitAIIndex(unit.spawnUponDestroy.prefabID) : 0 ;
			
			cont=new GUIContent("SpawnUponDestroy:", "The unit to spawn when this unit is destroyed (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			
			EditorGUI.BeginChangeCheck();
			unitIdx = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unitIdx, unitAILabel);
			if(EditorGUI.EndChangeCheck()){
				if(unitIdx==0){
					unit.spawnUponDestroy=null;
					srlObj.FindProperty("spawnUponDestroy").objectReferenceValue=null;
				}
				else if(unitIdx>0){
					unit.spawnUponDestroy=unitAIDB.unitList[unitIdx-1];
					srlObj.FindProperty("spawnUponDestroy").objectReferenceValue=unitAIDB.unitList[unitIdx-1];
				}
			}
			
			EditorGUI.showMixedValue=false;
			
			
			
			cont=new GUIContent("Spawn Count:", "Number of unit to spawn");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(unit.spawnUponDestroy!=null){
				EditorGUI.PropertyField(new Rect(startX+spaceX, startY, 40, height), srlObj.FindProperty("spawnUponDestroyCount"), contN);
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			
			startY+=15;
			
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Unit description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			unit.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), unit.desp, style);
			
			
			return new Vector2(startX, startY+200);
		}
		
		
		
		protected Vector2 DrawUnitList(float startX, float startY, List<UnitAI> unitList){
			List<Item> list=new List<Item>();
			for(int i=0; i<unitList.Count; i++){
				Item item=new Item(unitList[i].prefabID, unitList[i].unitName, unitList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list, true, true, selectedUnitList.Count==0);
		}
		
		
		
		public static int NewItem(UnitAI unit){ return window._NewItem(unit); }
		int _NewItem(UnitAI unit){
			if(unitAIDB.unitList.Contains(unit)) return selectID;
			
			unit.prefabID=GenerateNewID(unitAIIDList);
			unitAIIDList.Add(unit.prefabID);
			
			unitAIDB.unitList.Add(unit);
			
			UpdateLabel_UnitAI();
			
			return unitAIDB.unitList.Count-1;
		}
		void DeleteItem(){
			unitAIIDList.Remove(unitAIDB.unitList[deleteID].prefabID);
			unitAIDB.unitList.RemoveAt(deleteID);
			
			UpdateLabel_UnitAI();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<unitAIDB.unitList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			UnitAI unit=unitAIDB.unitList[selectID];
			unitAIDB.unitList[selectID]=unitAIDB.unitList[selectID+dir];
			unitAIDB.unitList[selectID+dir]=unit;
			selectID+=dir;
		}
		
		
		
		
		
		private SerializedObject srlObj;		//serialized obj (of the selected units)
		
		public List<UnitAI> selectedUnitList=new List<UnitAI>();	//unit selected in hierarchy or project-tab
		
		void SerializeItemInUnitList(){
			srlObj=null;
			if(unitAIDB!=null && unitAIDB.unitList.Count>0){
				if(selectID<0) selectID=0;
				srlObj = new SerializedObject((UnityEngine.Object)unitAIDB.unitList[selectID]);
			}
		}
		
		void SelectItem(){
			selectID=Mathf.Clamp(selectID, 0, unitAIDB.unitList.Count-1);
			Selection.activeGameObject=unitAIDB.unitList[selectID].gameObject;
			SerializeItemInUnitList();
		}
		
		void OnSelectionChange(){
			if(window==null) return;
			
			srlObj=null;
			
			selectedUnitList=new List<UnitAI>();
			
			UnityEngine.Object[] filtered = Selection.GetFiltered(typeof(UnitAI), SelectionMode.Editable);
			for(int i=0; i<filtered.Length; i++) selectedUnitList.Add((UnitAI)filtered[i]);
			
			//if no no relevent object is selected
			if(selectedUnitList.Count==0){
				SelectItem();
				if(unitAIDB.unitList.Count>0 && selectID>=0) 
					UpdateObjectHierarchyList(unitAIDB.unitList[selectID].gameObject); 
			}
			else{
				//only one relevent object is selected
				if(selectedUnitList.Count==1){
					//if the selected object is a prefab and match the selected item in editor, do nothing
					if(selectID>0 && selectedUnitList[0]==unitAIDB.unitList[selectID]){
						UpdateObjectHierarchyList(selectedUnitList[0].gameObject); 
					}
					//if the selected object doesnt match...
					else{
						//if the selected object existed in DB
						if(TDSEditor.ExistInDB(selectedUnitList[0])){
							window.selectID=TDSEditor.GetUnitAIIndex(selectedUnitList[0].prefabID)-1;
							UpdateObjectHierarchyList(selectedUnitList[0].gameObject); 
							SelectItem();
						}
						//if the selected object is not in DB
						else{
							selectID=-1;
							UpdateObjectHierarchyList(selectedUnitList[0].gameObject); 
						}
					}
				}
				//selected multiple editable object
				else{
					selectID=-1;
					UpdateObjectHierarchyList(selectedUnitList[0].gameObject); 
				}
				
				srlObj = new SerializedObject(filtered);
			}
			
			Repaint();
		}
		
		
	}
	
	
}
