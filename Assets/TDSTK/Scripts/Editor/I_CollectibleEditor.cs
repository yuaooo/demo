using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(Collectible))]
	[CanEditMultipleObjects]
	public class CollectibleEditor : TDSEditorInspector {
	
		private static Collectible instance;
		void Awake(){
			instance = (Collectible)target;
			LoadDB();
			
			InitLabel();
		}
		
		
		private static string[] collectTypeLabel;
		private static string[] collectTypeTooltip;
		
		private static string[] weaponTypeLabel;
		private static string[] weaponTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_CollectType)).Length;
			collectTypeLabel=new string[enumLength];
			collectTypeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				collectTypeLabel[n]=((_CollectType)n).ToString();
				
				if((_CollectType)n==_CollectType.Self) 			collectTypeTooltip[n]="affects the player only";
				if((_CollectType)n==_CollectType.AOEHostile) collectTypeTooltip[n]="affects all hostile in aoe range";
				if((_CollectType)n==_CollectType.AllHostile) 	collectTypeTooltip[n]="affects all hostile in the scene";
				if((_CollectType)n==_CollectType.Ability) 		collectTypeTooltip[n]="activate an ability";
			}
			
			
			enumLength = Enum.GetValues(typeof(Collectible._WeaponType)).Length;
			weaponTypeLabel=new string[enumLength];
			weaponTypeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				weaponTypeLabel[n]=((Collectible._WeaponType)n).ToString();
				
				if((Collectible._WeaponType)n==Collectible._WeaponType.Add) 			weaponTypeTooltip[n]="add the weapon on top of what the player already has ";
				if((Collectible._WeaponType)n==Collectible._WeaponType.Replacement) weaponTypeTooltip[n]="replace the player current selected weapon";
				if((Collectible._WeaponType)n==Collectible._WeaponType.Temporary) 	weaponTypeTooltip[n]="temporary weapon until the new weapon runs out of ammo. Player cannot switch weapon while the new weapon is active";
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
			
			Undo.RecordObject (instance, "Collectible");
			
			EditorGUILayout.Space();
			
			
			//EditorGUILayout.HelpBox("Editing Collectible component using Inspector is not recommended.\nPlease use the editor window instead", MessageType.Info);
			if(GUILayout.Button("Collectible Editor Window")){
				CollectibleEditorWindow.Init();
			}
			
			
			if(TDSEditor.IsPrefab(instance.gameObject)){
				if(!TDSEditor.ExistInDB(instance)){
					EditorGUILayout.Space();
					
					EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible by other editor.", MessageType.Warning);
					GUI.color=new Color(1f, 0.7f, .2f, 1f);
					if(GUILayout.Button("Add Prefab to Database")){
						CollectibleEditorWindow.Init();
						CollectibleEditorWindow.NewItem(instance);
						CollectibleEditorWindow.Init();	//call again to select the instance in editor window
					}
					GUI.color=Color.white;
				}
				
				EditorGUILayout.Space();
			}
			
			EditorGUILayout.Space();
			
			DrawFullEditor();
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		private static bool showWeaponList=true;
		
		
		void DrawFullEditor(){
			serializedObject.Update();
			
			cont=new GUIContent("Name:", "The collectible name to be displayed in game");
			PropertyFieldL(serializedObject.FindProperty("collectibleName"), cont);
			
			cont=new GUIContent("Icon:", "The collectible icon to be displayed in game and editor, must be a sprite");
			PropertyFieldL(serializedObject.FindProperty("icon"), cont);
			
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("type");
			
			EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
		
			cont=new GUIContent("Collectible Type:", "What does the specific collectible do when it's triggered");
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
		
			contL=new GUIContent[collectTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(collectTypeLabel[i], collectTypeTooltip[i]);
			int type=EditorGUILayout.Popup(srlPpt.enumValueIndex, contL, GUILayout.MaxWidth(fieldWidthL));
			if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=type;
			
			EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			
			if(instance.type==_CollectType.Ability){
				EditorGUILayout.LabelField("Trigger Ability", headerStyle);
				
				srlPpt=serializedObject.FindProperty("abilityID");
				int abID=TDSEditor.GetAbilityIndex(srlPpt.intValue);
		
				EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			
				cont=new GUIContent(" - Trigger Ability:", "The ability to activate when triggered");
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			
				contL=new GUIContent[abilityLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(abilityLabel[i]);
				abID=EditorGUILayout.Popup(abID, contL, GUILayout.MaxWidth(fieldWidthL));
				if(EditorGUI.EndChangeCheck()) srlPpt.intValue=abID-1;
				
				EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();	
			}
			else{
				EditorGUILayout.LabelField("Instant Gain", headerStyle);
				
				srlPpt=serializedObject.FindProperty("life");
				
				EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				cont=new GUIContent("Life:", "The amount of respawn gained by the player");
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
				int life=EditorGUILayout.IntField(srlPpt.intValue, GUILayout.MaxWidth(fieldWidth));
				if(EditorGUI.EndChangeCheck()) srlPpt.intValue=life;
				EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Space();
				
				cont=new GUIContent("HitPoint:", "The amount of hit-point gained by the player");
				PropertyField(serializedObject.FindProperty("hitPoint"), cont);
				
				cont=new GUIContent("Energy:", "The amount of energy gained by the player");
				PropertyField(serializedObject.FindProperty("energy"), cont);
					
				EditorGUILayout.Space();
				
				cont=new GUIContent("Score:", "The amount of points gained by the player");
				PropertyField(serializedObject.FindProperty("score"), cont);
				
				EditorGUILayout.Space();
				
				cont=new GUIContent("Ammo:", "The amount of ammo gained by the player. If set as -1, the ammo count will be refilled to full");
				PropertyField(serializedObject.FindProperty("ammo"), cont);
				
				cont=new GUIContent("Weapon:", "The weapon in which the ammo gain of the collectible is intended for.");
				if(serializedObject.FindProperty("ammo").intValue!=0){
					weaponLabel[0]="All Weapons";
					
					srlPpt=serializedObject.FindProperty("ammoID");
					int weaponIdx=TDSEditor.GetWeaponIndex(srlPpt.intValue);
					
					//Weapon weapon=weaponIdx>0 ? weaponDB.weaponList[weaponIdx-1] : null ;
					//if(weapon!=null)
					//	TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), weapon!=null ? weapon.icon : null);
					
					EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
					EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
					EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
					weaponIdx=EditorGUILayout.Popup(weaponIdx, weaponLabel, GUILayout.MaxWidth(fieldWidthL));
					if(EditorGUI.EndChangeCheck()){
						if(weaponIdx>0) srlPpt.intValue=weaponDB.weaponList[weaponIdx-1].ID;
						else srlPpt.intValue=-1;
					}
					EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
					
					weaponLabel[0]="Unassigned";
				}
				else InvalidField(cont);
				
				
				EditorGUILayout.Space();
				
					cont=new GUIContent("Experience:", "The amount of experience gained by the player");
					PropertyField(serializedObject.FindProperty("exp"), cont);
					
					cont=new GUIContent("Perk Currency:", "The amount of perk currency gained by the player");
					PropertyField(serializedObject.FindProperty("perkCurrency"), cont);
				
				
				EditorGUILayout.Space();
				
				
					srlPpt=serializedObject.FindProperty("effectID");
					int effectIdx=srlPpt.intValue>=0 ? TDSEditor.GetEffectIndex(srlPpt.intValue) : 0 ;
				
					EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
					EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				
					cont=new GUIContent("Triggered Effect:", "Special effect that applies on target when triggered (optional)");
					EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
				
					effectIdx=EditorGUILayout.Popup(effectIdx, effectLabel, GUILayout.MaxWidth(fieldWidthL));
					if(EditorGUI.EndChangeCheck()){
						if(effectIdx>0) srlPpt.intValue=effectDB.effectList[effectIdx-1].ID;
						else srlPpt.intValue=-1;
					}
					EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
					
					
				EditorGUILayout.Space();
				
				
					srlPpt=serializedObject.FindProperty("gainWeapon");
					cont=new GUIContent("Gain Weapon:", "Weapon gained by player upon triggered (optional)");
					PropertyField(srlPpt, cont);
					
					bool gainWeapon=srlPpt.boolValue;
					
					if(gainWeapon){
						srlPpt=serializedObject.FindProperty("weaponType");
						int wType=TDSEditor.GetWeaponIndex(srlPpt.enumValueIndex);
				
						EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
						EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
					
						cont=new GUIContent(" - GainWeaponType:", "What the new weapon is for");
						EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
					
						contL=new GUIContent[weaponTypeLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(weaponTypeLabel[i], weaponTypeTooltip[i]);
						wType=EditorGUILayout.Popup(wType, contL, GUILayout.MaxWidth(fieldWidthL));
						if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=wType;
						
						EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
						
						
						cont=new GUIContent(" - Duration:", "The duration of the temporary weapon. Set to -1 for not time limit (limit by weapon ammo instead)");
						if(srlPpt.enumValueIndex==(int)Collectible._WeaponType.Temporary)
							PropertyField(serializedObject.FindProperty("tempWeapDuration"), cont);
						else InvalidField(cont);
						
						
						cont=new GUIContent(" - Random Weapon:", "Check if player will get random weapon out of a few potential candidates");
						PropertyField(serializedObject.FindProperty("randomWeapon"), cont);
						
						bool randWeapon=serializedObject.FindProperty("randomWeapon").boolValue;
						if(randWeapon){
							SerializedProperty enableAllWP=serializedObject.FindProperty("enableAllWeapon");
							EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
							EditorGUI.showMixedValue=enableAllWP.hasMultipleDifferentValues;
							
							cont=new GUIContent(" - EnableAllWeapon:", "Check if all weapon in the database are to be added to the random pool");
							EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));

							bool en=EditorGUILayout.Toggle(enableAllWP.boolValue, GUILayout.MaxWidth(fieldWidthS+25));
							if(EditorGUI.EndChangeCheck()){
								enableAllWP.boolValue=en;
								if(en) showWeaponList=true;
							}
							//~ PropertyField(serializedObject.FindProperty("enableAllWeapon"), cont);
							
							GUIStyle notSelectedStyle=new GUIStyle("Label");
							notSelectedStyle.normal.textColor = new Color(.2f, .2f, .2f, 1);
							
							SpaceH(22);
							
							bool enableAllWeapon=serializedObject.FindProperty("enableAllWeapon").boolValue;
							if(enableAllWeapon) showWeaponList=EditorGUILayout.Foldout(showWeaponList, "Show list");
							
							EditorGUILayout.EndHorizontal();
							
							if(enableAllWeapon && showWeaponList){
								srlPpt=serializedObject.FindProperty("weaponList");
								
								if(!serializedObject.isEditingMultipleObjects){
									int enabledCount=0;
									for(int i=0; i<weaponDB.weaponList.Count; i++){
										Weapon weapon=weaponDB.weaponList[i];
										bool enabled=false;
										for(int n=0; n<srlPpt.arraySize; n++){
											SerializedProperty elePpt=srlPpt.GetArrayElementAtIndex(n);
											if(elePpt.objectReferenceValue==(UnityEngine.Object)weapon){
												enabled=true;
												break;
											}
										}
										
										bool enabledCached=enabled;
										
										EditorGUILayout.BeginHorizontal();
										cont=new GUIContent("    - "+weapon.weaponName, weapon.desp);
										if(enabled) EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth+50));
										else EditorGUILayout.LabelField(cont, notSelectedStyle, GUILayout.MaxWidth(labelWidth+50));
										
										enabled=EditorGUILayout.Toggle(enabled);
										EditorGUILayout.EndHorizontal();
										
										//TDSEditorUtility.DrawSprite(new Rect(startX+20, startY+=spaceY, 30, 30), weapon.icon, weapon.desp);
										//~ cont=new GUIContent(weapon.weaponName, weapon.desp);
										//~ EditorGUI.LabelField(new Rect(startX+65, startY+15, width, height), cont);
										//~ enabled=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY+15, width, height), enabled);
										//~ startY+=14;
										
										if(enabled!=enabledCached){
											if(enabled){
												srlPpt.InsertArrayElementAtIndex(enabledCount);
												SerializedProperty elePpt=srlPpt.GetArrayElementAtIndex(enabledCount);
												elePpt.objectReferenceValue=weapon;
											}
											else srlPpt.DeleteArrayElementAtIndex(enabledCount);
										}
										
										if(enabled) enabledCount+=1;
									}
								}
								else{
									EditorGUILayout.LabelField("    - Cannot edit multiple instance");
								}
							}
						}
						else{
							
							srlPpt=serializedObject.FindProperty("weaponList");
							while(srlPpt.arraySize>1) srlPpt.DeleteArrayElementAtIndex(srlPpt.arraySize-1);
							while(srlPpt.arraySize<=0) srlPpt.InsertArrayElementAtIndex(0);
							
							SerializedProperty elePpt=srlPpt.GetArrayElementAtIndex(0);
							int weapIdx=elePpt.objectReferenceValue!=null ? TDSEditor.GetWeaponIndex(((Weapon)elePpt.objectReferenceValue).ID) : 0;
							
							EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
							EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
							
							cont=new GUIContent(" - Gain Weapon:", "Weapon gained by player upon triggered (optional)");
							EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
							
							weapIdx=EditorGUILayout.Popup(weapIdx, weaponLabel, GUILayout.MaxWidth(fieldWidthL));
							if(EditorGUI.EndChangeCheck()){
								if(weapIdx>0) elePpt.objectReferenceValue=weaponDB.weaponList[weapIdx-1];
								else elePpt.objectReferenceValue=null;
							}
							
							EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
							
							//~ if(cItem.weaponList.Count!=1) cItem.weaponList=new List<Weapon>{ null };
							
							//~ int weaponIdx1=cItem.weaponList[0]!=null ? TDSEditor.GetWeaponIndex(cItem.weaponList[0].ID) : 0;
							
							//~ //if(cItem.weaponList[0]!=null)
							//~ //	TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-41, 40, 40), cItem.weaponList[0].icon);
							
							//~ cont=new GUIContent("Gain Weapon:", "Weapon gained by player upon triggered (optional)");
							//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
							
							//~ weaponIdx1=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weaponIdx1, weaponLabel);
							//~ if(weaponIdx1>0) cItem.weaponList[0]=weaponDB.weaponList[weaponIdx1-1];
							//~ else cItem.weaponList[0]=null;
							
						}
					}
					
				}
				
			
			EditorGUILayout.Space();
			
			
			EditorGUILayout.LabelField("Miscellaneous", headerStyle);
			
			
			srlPpt=serializedObject.FindProperty("triggerEffectObj");
			
			EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			cont=new GUIContent("Triggered Effect Obj:", "The object to be spawned when the collectible is triggered (optional)");
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			GameObject sObj=(GameObject)EditorGUILayout.ObjectField(srlPpt.objectReferenceValue, typeof(GameObject), false, GUILayout.MaxWidth(fieldWidthL));
			if(EditorGUI.EndChangeCheck()) srlPpt.objectReferenceValue=sObj;
			EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
			
			cont=new GUIContent("AutoDestroy Effect:", "Check if the effect object needs to be removed from the game");
			if(srlPpt.objectReferenceValue!=null)
				PropertyField(serializedObject.FindProperty("autoDestroyEffectObj"), cont);
			else InvalidField(cont);
			
			cont=new GUIContent(" - Duration:", "The delay in seconds before the effect object is destroyed");
			if(srlPpt.objectReferenceValue!=null && serializedObject.FindProperty("autoDestroyEffectObj").boolValue)
				PropertyField(serializedObject.FindProperty("effectObjActiveDuration"), cont);
			else InvalidField(cont);
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Triggered SFX:", "Audio clip to play when the collectible is triggered (optional)");
			PropertyFieldL(serializedObject.FindProperty("triggerSFX"), cont);
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Self Destruct:", "Check if the item is to self-destruct if not collected in a set time frame");
			srlPpt=serializedObject.FindProperty("selfDestruct");
			PropertyField(srlPpt, cont);
			
			//~ EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
			//~ EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			
			//~ cont=new GUIContent("Self Destruct:", "Check if the item is to self-destruct if not collected in a set time frame");
			//~ EditorGUILayout.LabelField(cont);
			//~ bool flag=EditorGUILayout.Toggle(srlPpt.boolValue);
			//~ if(EditorGUI.EndChangeCheck()) srlPpt.boolValue=flag;
			//~ EditorGUI.showMixedValue=false; EditorGUILayout.EndHorizontal();
			
			//~ cont=new GUIContent("Active Duration:", "How long the item will stay active before it self destruct");
			//~ if(srlPpt.boolValue) PropertyField(serializedObject.FindProperty("selfDestructDuration"), cont);
			//~ else InvalidField(cont);
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("BlinkBeforeDestruct:", "Blink to give player warning before the object self-destruct");
			if(srlPpt.boolValue) PropertyField(serializedObject.FindProperty("blinkBeforeDestroy"), cont);
			else InvalidField(cont);
			
			cont=new GUIContent("Blink Duration:", "The long the item is gong to blink for");
			if(srlPpt.boolValue && serializedObject.FindProperty("blinkBeforeDestroy").boolValue)
				PropertyField(serializedObject.FindProperty("blinkDuration"), cont);
			else InvalidField(cont);
			
			cont=new GUIContent("Blink Object: ", "The mesh object to blink (The system will deactivate/activate the blink object for blinking. we only need to deactivate the child object and contain the mesh, not the whole item)");
			if(srlPpt.boolValue && serializedObject.FindProperty("blinkBeforeDestroy").boolValue){
				if(serializedObject.isEditingMultipleObjects){
					EditorGUILayout.LabelField(cont, new GUIContent("Cannot edit multiple instance"));
				}
				else{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
					
					int objID=GetObjectIDFromHList(instance.GetBlinkObjT(), objHList);
					objID = EditorGUILayout.Popup(objID, objHLabelList, GUILayout.MaxWidth(fieldWidthL));
					instance.blinkObj = (objHList[objID]==null) ? null : objHList[objID];
					
					EditorGUILayout.EndHorizontal();
				}
			}
			else InvalidField(cont);
			
			
			
			serializedObject.ApplyModifiedProperties();
		}
		
		
	}

}