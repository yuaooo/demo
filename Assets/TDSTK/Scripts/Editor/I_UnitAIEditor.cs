using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(UnitAI))]
	[CanEditMultipleObjects]
	public class UnitAIEditor : TDSEditorInspector {
	
		private static UnitAI instance;
		void Awake(){
			instance = (UnitAI)target;
			LoadDB();
			
			InitLabel();
		}
		
		
		private static string[] behaviourLabel;
		private static string[] behaviourTooltip;
		
		void InitLabel(){
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
		
		
		void OnEnable(){
			if(serializedObject.isEditingMultipleObjects) return;
			UpdateObjectHierarchyList(instance.gameObject); 
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			
			//EditorGUILayout.HelpBox("Editing UnitAI component using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
			if(GUILayout.Button("Unit Editor Window")){
				UnitAIEditorWindow.Init();
			}
			
			
			if(TDSEditor.IsPrefab(instance.gameObject)){
				if(!TDSEditor.ExistInDB(instance)){
					EditorGUILayout.Space();
					
					EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible by other editor.", MessageType.Warning);
					GUI.color=new Color(1f, 0.7f, .2f, 1f);
					if(GUILayout.Button("Add Prefab to Database")){
						UnitAIEditorWindow.Init();
						UnitAIEditorWindow.NewItem(instance);
						UnitAIEditorWindow.Init();		//call again to select the instance in editor window
					}
					GUI.color=Color.white;
				}
				
				EditorGUILayout.Space();
			}
			
			
			EditorGUILayout.Space();
			
			
			
			cont=new GUIContent("Unit Name:", "The unit name to be displayed in game");
			PropertyFieldL(serializedObject.FindProperty("unitName"), cont);
			
			DrawFullEditor();
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		protected bool showMoveSetting=true;
		protected bool showAttackSetting=true;
		protected bool showShootPoint=true;
		
		
		void DrawFullEditor(){
			serializedObject.Update();
			
			cont=new GUIContent("Unit Name:", "The unit name to be displayed in game");
			PropertyFieldL(serializedObject.FindProperty("unitName"), cont);
			
			cont=new GUIContent("Icon:", "The unit icon to be displayed in game and editor, must be a sprite");
			PropertyFieldL(serializedObject.FindProperty("icon"), cont);
			
			
			EditorGUILayout.Space();
			
			
			EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("Hit Point:", "The hit-point capacity of the unit");
				srlPpt=serializedObject.FindProperty("hitPointFull");
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
				float hitPointFull=EditorGUILayout.FloatField(srlPpt.floatValue, GUILayout.MaxWidth(fieldWidth));
				if(EditorGUI.EndChangeCheck()) srlPpt.floatValue=hitPointFull;
				
				EditorGUILayout.PropertyField(serializedObject.FindProperty("startHitPointAtFull"), contN, GUILayout.MaxWidth(10));
				EditorGUILayout.LabelField("-start at full", GUILayout.MaxWidth(70));
			EditorGUILayout.EndHorizontal();
			
			cont=new GUIContent("Hit Point Regen:", "The amount of hit-point regenerated per second");
			PropertyField(serializedObject.FindProperty("hpRegenRate"), cont);
			
			cont=new GUIContent("Hit Point Stagger:", "The duration in second in which the hit-point regen would stop after the unit being hit");
			if(serializedObject.FindProperty("hpRegenRate").floatValue>0)
				PropertyField(serializedObject.FindProperty("hpRegenStagger"), cont);
			else InvalidField(cont);//EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("armorType");
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			EditorGUI.BeginChangeCheck();
			cont=new GUIContent("Armor Type:", "The armor type of the unit\nArmor type can be configured in Damage Armor Table Editor");
			contL=new GUIContent[armorTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(armorTypeLabel[i], "");
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			int armorType = EditorGUILayout.Popup(srlPpt.intValue, contL, GUILayout.MaxWidth(fieldWidthL));
			EditorGUILayout.EndHorizontal();
			
			if(EditorGUI.EndChangeCheck()) srlPpt.intValue=armorType;
			EditorGUI.showMixedValue=false;
			
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("behaviour");
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			
			EditorGUI.BeginChangeCheck();
			
			cont=new GUIContent("Base Behaviour:", "Type of the behaviour which determine the how the unit respond to presence of hostile unit");
			contL=new GUIContent[behaviourLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(behaviourLabel[i], behaviourTooltip[i]);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			int behaviour = EditorGUILayout.Popup(srlPpt.enumValueIndex, contL, GUILayout.MaxWidth(fieldWidthL));
			EditorGUILayout.EndHorizontal();
			
			if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=behaviour;
			EditorGUI.showMixedValue=false;
			
			cont=new GUIContent("Aggro Range(P):", "The range at which the unit will chasing down and attacking player\n\nUnit will not start attacking (shooting) until the target is within aggro range");
			if(srlPpt.enumValueIndex==(int)_Behaviour.Aggressive_Trigger || srlPpt.enumValueIndex==(int)_Behaviour.StandGuard)
				PropertyField(serializedObject.FindProperty("aggroRange"), cont);
			else InvalidField(cont);
			
			
			EditorGUILayout.Space();
			
			showMoveSetting=EditorGUILayout.Foldout(showMoveSetting, "Show Move Setting", foldoutStyle);
			
			if(showMoveSetting){
				srlPpt=serializedObject.FindProperty("anchorDown");
				cont=new GUIContent("Anchored To Point:", "Check if the unit is a static object. Means it will not moved and wont be affected by physics");
				PropertyFieldS(srlPpt, cont);
				
				bool anchorDown=!(srlPpt.boolValue && !srlPpt.hasMultipleDifferentValues);
				
				cont=new GUIContent(" - Move Speed(P):", "The move speed of the unit");
				if(anchorDown) PropertyField(serializedObject.FindProperty("moveSpeed"), cont);
				else InvalidField(cont);
				
				cont=new GUIContent(" - Rotate Speed(P):", "The rotate speed of the unit (degree per second)");
				if(anchorDown) PropertyField(serializedObject.FindProperty("rotateSpeed"), cont);
				else InvalidField(cont);
			
				cont=new GUIContent(" - Brake  Range(P):", "The range at which the unit will stop moving towards its target");
				if(anchorDown) PropertyField(serializedObject.FindProperty("brakeRange"), cont);
				else InvalidField(cont);
				
				
				EditorGUILayout.Space();
				
				
				srlPpt=serializedObject.FindProperty("stopOccasionally");
				cont=new GUIContent("Stop Ocassionally:", "Check to enable the target to stop every now and then when chasing the target");
				if(anchorDown) PropertyFieldS(srlPpt, cont);
				else InvalidField(cont);
				
				bool stopOccasionally=anchorDown && srlPpt.boolValue && !srlPpt.hasMultipleDifferentValues;
				
				cont=new GUIContent("  - Stop Chance:", "Chance that determine how often the unit will stop. takes value from 0-1 with 0 as never and 1 as every cooldown");
				if(stopOccasionally) PropertyField(serializedObject.FindProperty("stopRate"), cont);
				else InvalidField(cont);
				
				cont=new GUIContent("  - Stop Duration:", "How long (in second) the unit will stop for");
				if(stopOccasionally) PropertyField(serializedObject.FindProperty("stopDuration"), cont);
				else InvalidField(cont);
			
				cont=new GUIContent("  - Stop Cooldown:", "How long (in second) before the unit will try a stop maneuver after the last stop maneuver");
				if(stopOccasionally) PropertyField(serializedObject.FindProperty("stopCooldown"), cont);
				else InvalidField(cont);
				
				
				EditorGUILayout.Space();
				
				
				srlPpt=serializedObject.FindProperty("evadeOccasionally");
				cont=new GUIContent("Evade Ocassionally:", "Check to enable the target to perform a evade maneuver and then when chasing the target");
				if(anchorDown) PropertyFieldS(srlPpt, cont);
				else InvalidField(cont);
				
				bool evadeOccasionally=anchorDown && (srlPpt.boolValue && !srlPpt.hasMultipleDifferentValues);
				
				cont=new GUIContent("  - Evade Chance:", "Chance that determine how often the unit will evade. takes value from 0-1 with 0 as never and 1 as every cooldown");
				if(evadeOccasionally) PropertyField(serializedObject.FindProperty("evadeRate"), cont);
				else InvalidField(cont);
				
				cont=new GUIContent("  - Evade Duration:", "How long (in second) the unit will perform the maneuver for");
				if(evadeOccasionally) PropertyField(serializedObject.FindProperty("evadeDuration"), cont);
				else InvalidField(cont);
			
				cont=new GUIContent("  - Evade Cooldown:", "How long (in second) before the unit will try a evade maneuver after the last evade maneuver");
				if(evadeOccasionally) PropertyField(serializedObject.FindProperty("evadeCooldown"), cont);
				else InvalidField(cont);
			
			}
			
			
			EditorGUILayout.Space();
			
			showAttackSetting=EditorGUILayout.Foldout(showAttackSetting, "Show Attack Setting", foldoutStyle);
			
			if(showAttackSetting){
				cont=new GUIContent("Range Attack:", "Check if the unit can perform any range attack");
				srlPpt=serializedObject.FindProperty("enableRangeAttack");
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				
				EditorGUI.BeginChangeCheck(); 	EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(cont, headerStyle, GUILayout.MaxWidth(labelWidth));
				bool rangeAttack=EditorGUILayout.Toggle(srlPpt.boolValue, toggleHeaderStyle);
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue=rangeAttack;
				EditorGUILayout.EndHorizontal();
				
				//~ Debug.Log(EditorGUILayout.GetControlRect());
				//~ GUI.Box(EditorGUILayout.GetControlRect(), "");
				
				bool enableRangeAttack=srlPpt.boolValue && !srlPpt.hasMultipleDifferentValues;
				if(enableRangeAttack){
					string lbSp=" - ";
					
					cont=new GUIContent(lbSp+"Shoot Periodically:", "Check to enable the target to shoot after every attack cooldown, regardless of existance or range of target");
					PropertyField(serializedObject.FindProperty("shootPeriodically"), cont);
					
					cont=new GUIContent(lbSp+"AlwaysOnTarget:", "Check to have the unit shoot object always fire towards target, regardless of aim direction");
					PropertyField(serializedObject.FindProperty("alwaysShootTowardsTarget"), cont);
					
					EditorGUILayout.Space();
					
					cont=new GUIContent(lbSp+"Shoot Object:", "The prefab of the bullet/object fired by the unit\nMust be a prefab with ShootObject component attached on it\n\n*Required for range attack to work");
					//~ PropertyFieldL(serializedObject.FindProperty("shootObject"), cont);
					srlPpt=serializedObject.FindProperty("shootObject");
					EditorGUI.BeginChangeCheck(); 	EditorGUILayout.BeginHorizontal();
						
						if(srlPpt.objectReferenceValue!=null) EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
						else EditorGUILayout.LabelField(cont, conflictStyle, GUILayout.MaxWidth(labelWidth));
						//EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(LabelWidth));
						
						GameObject sObj=(GameObject)EditorGUILayout.ObjectField(srlPpt.objectReferenceValue, typeof(GameObject), false, GUILayout.MaxWidth(fieldWidthL));
					
						//~ int objID=GetObjectIDFromHList(instance.turretObj, objHList);
						//~ objID = EditorGUILayout.Popup(objID, objHLabelList, GUILayout.MaxWidth(fieldWidthL));
						//~ instance.turretObj = (objHList[objID]==null) ? null : objHList[objID].transform;
						
						if(EditorGUI.EndChangeCheck()) srlPpt.objectReferenceValue=sObj;
					EditorGUILayout.EndHorizontal();
					
					
					cont=new GUIContent(lbSp+"Turret Object:", "The pivot transform on the unit to track the shoot direction");
					if(serializedObject.isEditingMultipleObjects){
						EditorGUILayout.LabelField(cont, new GUIContent("Cannot edit multiple instance"));
					}
					else{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
						
						int objID=GetObjectIDFromHList(instance.turretObj, objHList);
						objID = EditorGUILayout.Popup(objID, objHLabelList, GUILayout.MaxWidth(fieldWidthL));
						instance.turretObj = (objHList[objID]==null) ? null : objHList[objID].transform;
						
						EditorGUILayout.EndHorizontal();
					}
					
					cont=new GUIContent(lbSp+"TurretTrackSpeed:", "The tracking/turning speed of the turret (degree/second). Slower turret will have a harder time keeping up with target.");
					if(serializedObject.FindProperty("turretObj").objectReferenceValue!=null)
						PropertyField(serializedObject.FindProperty("turretTrackingSpeed"), cont);
					else InvalidField(cont);
					
					EditorGUILayout.Space();
					
					cont=new GUIContent(lbSp+"ShootPoint:", "The transform which indicate the position where the shootObject will be fired from (Optional)\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the weapon transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the firing direction.\n");
					if(serializedObject.isEditingMultipleObjects){
						EditorGUILayout.LabelField(cont, new GUIContent("Cannot edit multiple instance"));
					}
					else{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
						
						int count=EditorGUILayout.IntField(instance.shootPointList.Count, GUILayout.MaxWidth(fieldWidth));
					
						SpaceH(15);
						showShootPoint=EditorGUILayout.Foldout(showShootPoint, "    ");
						EditorGUILayout.EndHorizontal();
						
						while(instance.shootPointList.Count>count) instance.shootPointList.RemoveAt(instance.shootPointList.Count-1);
						while(instance.shootPointList.Count<count) instance.shootPointList.Add(null);
					
						if(showShootPoint){
							for(int i=0; i<instance.shootPointList.Count; i++){
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField("    - element "+i+":", GUILayout.MaxWidth(labelWidth));
								int objID=GetObjectIDFromHList(instance.shootPointList[i], objHList);
								objID = EditorGUILayout.Popup(objID, objHLabelList, GUILayout.MaxWidth(fieldWidthL));
								instance.shootPointList[i] = (objHList[objID]==null) ? null : objHList[objID].transform;
								EditorGUILayout.EndHorizontal();
							}
						}
					}
					
					EditorGUILayout.Space();
					
					cont=new GUIContent(lbSp+"Shoot Periodically:", "Check to enable the target to shoot after every attack cooldown, regardless of existance or range of target");
					PropertyField(serializedObject.FindProperty("shootPeriodically"), cont);
					
					cont=new GUIContent(lbSp+"Attack Range:", "The attack range of the unit");
					PropertyField(serializedObject.FindProperty("range"), cont);
					
					cont=new GUIContent(lbSp+"Attack Cooldown:", "The cooldown duration in seconds between each subsequent attack");
					PropertyField(serializedObject.FindProperty("cooldown"), cont);
					
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginHorizontal();
						cont=new GUIContent(lbSp+"First Attack Delay:", "The delay in second before the unit can perform it's first attack. To prevent unit to fire immediately after being spawned or as soon as the game start");
						//EditorGUILayout.PropertyField(serializedObject.FindProperty("firstAttackDelay"), cont);
						
						srlPpt=serializedObject.FindProperty("firstAttackDelay");
						EditorGUI.BeginChangeCheck();	EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
						EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
						EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
						float firstAttackDelay=EditorGUILayout.FloatField(srlPpt.floatValue, GUILayout.MaxWidth(fieldWidth));
						if(EditorGUI.EndChangeCheck()) srlPpt.floatValue=firstAttackDelay;
						
						EditorGUILayout.PropertyField(serializedObject.FindProperty("randFirstAttackDelay"), contN, GUILayout.MaxWidth(10));
						EditorGUILayout.LabelField(new GUIContent("-Randomize", "Randomize the first attack delay between 0 and the specified value"), GUILayout.MaxWidth(75));
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.Space();
					
					DrawAttackStats("attackStats", true, false, "   Attack Stats (Range)");
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
				
				}
				
				
				
				cont=new GUIContent("AttackOnContact:", "Check if the unit can attack when comes into contact with hostile unit");
				srlPpt=serializedObject.FindProperty("enableContactAttack");
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				
				EditorGUI.BeginChangeCheck();	EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(cont, headerStyle, GUILayout.MaxWidth(labelWidth));
				bool contactAttack=EditorGUILayout.Toggle(srlPpt.boolValue, toggleHeaderStyle);
				if(EditorGUI.EndChangeCheck()) srlPpt.boolValue=contactAttack;
				EditorGUILayout.EndHorizontal();
				
				bool enableContactAttack=srlPpt.boolValue && !srlPpt.hasMultipleDifferentValues;
				if(enableContactAttack){
					string lbSp=" - ";
					
					cont=new GUIContent(lbSp+"Attack Cooldown:", "The cooldown duration in seconds between each subsequent attack");
					PropertyField(serializedObject.FindProperty("contactCooldown"), cont);
					
					EditorGUILayout.Space();
					
					DrawAttackStats("contactAttackStats", false, false, "   Attack Stats (On Contact)");
				}
				
			}
			
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			
			EditorGUILayout.LabelField("Miscellaneous", headerStyle);
			
			cont=new GUIContent("DestroyUponContact:", "Check to destroy unit upon coming into contact with a hostile");
			PropertyField(serializedObject.FindProperty("destroyUponPlayerContact"), cont);
			
			EditorGUILayout.Space();
			
			DrawDestroyValue();
			
			EditorGUILayout.Space();
			
			DrawDestroyEffect();
			
			EditorGUILayout.Space();
			
			DrawDropSetting();
			
			EditorGUILayout.Space();
			
			DrawSpawnUponDestroy();
			
			EditorGUILayout.Space();
			
			serializedObject.ApplyModifiedProperties();
		}
		
		
		
	}

}