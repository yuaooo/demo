using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class UnitPlayerEditorWindow : TDSEditorWindow {
		
		private static UnitPlayerEditorWindow window;
		
		public static void Init() {
			// Get existing open window or if none, make a new one:
			window = (UnitPlayerEditorWindow)EditorWindow.GetWindow(typeof (UnitPlayerEditorWindow), false, "Player Unit Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
			
			window.OnSelectionChange();
		}
		
		
		private static string[] movementModeLabel;
		private static string[] movementModeTooltip;
		
		private static string[] turretAimModeLabel;
		private static string[] turretAimModeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_MovementMode)).Length;
			movementModeLabel=new string[enumLength];
			movementModeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				movementModeLabel[n]=((_MovementMode)n).ToString();
				
				if((_MovementMode)n==_MovementMode.Rigid) 		movementModeTooltip[n]="Using direct digital input input value.\nUnit move at constant speed with no acceleration and decceleration. ";
				if((_MovementMode)n==_MovementMode.FreeForm)	movementModeTooltip[n]="Unit accelerates and deccelerates according to input and retain momentum, with option to boost speed and brake";
			}
			
			
			enumLength = Enum.GetValues(typeof(_TurretAimMode)).Length;
			turretAimModeLabel=new string[enumLength];
			turretAimModeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				turretAimModeLabel[n]=((_TurretAimMode)n).ToString();
				
				if((_TurretAimMode)n==_TurretAimMode.ScreenSpace) turretAimModeTooltip[n]="Using unit's position on screen with respect to cursor to determine aim direction.\nMaybe inaccurate at certain angle when camera angle is not upright (view from top down)";
				if((_TurretAimMode)n==_TurretAimMode.Raycast)		turretAimModeTooltip[n]="Using raycasting on the terrain collider to determine aim direction.\nRequire a terrain with collider to work";
			}
		}
		
		
		public void SetupCallback(){
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
			
			List<UnitPlayer> unitList=unitPlayerDB.unitList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(unitPlayerDB, "unitPlayerDB");
			if(unitList.Count>0 && selectID>=0) Undo.RecordObject(unitList[selectID], "unitPlayer");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Player Unit:");
			UnitPlayer newUnit=null;
			newUnit=(UnitPlayer)EditorGUI.ObjectField(new Rect(125, 7, 140, 17), newUnit, typeof(UnitPlayer), false);
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
			
			if(unitList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				UnitPlayer unitToEdit=selectedUnitList.Count!=0 ? selectedUnitList[0] : unitList[selectID];
				
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
		
		
		
		Vector2 DrawUnitConfigurator(float startX, float startY, UnitPlayer unit){
			
			Vector2 v2=DrawUnitBaseStats(startX, startY, unit, true);
			startY=v2.y;
			
			startY+=10;
			
			int objID=GetObjectIDFromHList(unit.turretObj, objHList);
			cont=new GUIContent("Turret Object:", "The pivot transform on the unit to track the shoot direction");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
			unit.turretObj = (objHList[objID]==null) ? null : objHList[objID].transform;
			
			objID=GetObjectIDFromHList(unit.weaponMountPoint, objHList);
			cont=new GUIContent("WeaponMount:", "The transform where the weapon object to be anchored to as child object");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
			unit.weaponMountPoint = (objHList[objID]==null) ? null : objHList[objID].transform;
			
			startY+=10;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Weapons", headerStyle);
			
			cont=new GUIContent("Enable All   -  ", "Check to enable all weapons");
			EditorGUI.LabelField(new Rect(startX+spaceX+50, startY, width, height), cont);
			unit.enableAllWeapons=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY, width, height), unit.enableAllWeapons);
			
			if(!unit.enableAllWeapons){
				int enabledCount=0;
				for(int i=0; i<weaponDB.weaponList.Count; i++){
					Weapon weapon=weaponDB.weaponList[i];
					
					bool enabled=unit.weaponList.Contains(weapon);
					bool enabledCached=enabled;
					
					TDSEditorUtility.DrawSprite(new Rect(startX+20, startY+=spaceY, 30, 30), weapon.icon, weapon.desp);
					cont=new GUIContent(weapon.weaponName, weapon.desp);
					EditorGUI.LabelField(new Rect(startX+65, startY+15, width, height), cont);
					enabled=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY+15, width, height), enabled);
					startY+=14;
					
					if(enabled!=enabledCached){
						if(enabled) unit.weaponList.Insert(enabledCount, weapon);
						else unit.weaponList.Remove(weapon);
					}
					
					if(enabled) enabledCount+=1;
				}
				
				startY+=10;
			}
			if(GUI.Button(new Rect(startX+spaceX+width-100, startY+=spaceY, 100, height-2), "Weapon Editor")) WeaponEditorWindow.Init();
			
			
			startY+=20;
			
			
			unit.enableAbility=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, width, height), unit.enableAbility);
			EditorGUI.LabelField(new Rect(startX+20, startY, width, height), "Abilities", headerStyle);
			
			if(unit.enableAbility){
				cont=new GUIContent("Enable All   -  ", "Check to enable all abilities");
				EditorGUI.LabelField(new Rect(startX+spaceX+50, startY, width, height), cont);
				unit.enableAllAbilities=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY, width, height), unit.enableAllAbilities);
				
				if(!unit.enableAllAbilities){
					int enabledCount=0;
					for(int i=0; i<abilityDB.abilityList.Count; i++){
						Ability ability=abilityDB.abilityList[i];
						
						bool enabled=unit.abilityIDList.Contains(ability.ID);
						bool enabledCached=enabled;
						
						TDSEditorUtility.DrawSprite(new Rect(startX+20, startY+=spaceY, 30, 30), ability.icon);
						cont=new GUIContent(ability.name, ability.desp);
						EditorGUI.LabelField(new Rect(startX+65, startY+15, width, height), cont);
						enabled=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY+15, width, height), enabled);
						startY+=14;
						
						if(enabled!=enabledCached){
							if(enabled) unit.abilityIDList.Insert(enabledCount, ability.ID);
							else unit.abilityIDList.Remove(ability.ID);
						}
						
						if(enabled) enabledCount+=1;
					}
					
					startY+=10;
				}
				if(GUI.Button(new Rect(startX+spaceX+width-100, startY+=spaceY, 100, height-2), "Ability Editor")) AbilityEditorWindow.Init();
			}
			
			startY+=10;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Movement & Control", headerStyle);
			
			cont=new GUIContent("EnableTurretAiming:", "Check to allow turret object to rotate and aim towards cursor position");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.enableTurretRotate=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.enableTurretRotate);
			
			cont=new GUIContent("Turret Aim Mode:", "The way in which the turret aim direction will be calculated");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(unit.enableTurretRotate){
				int aimMode=(int)unit.turretAimMode;
				contL=new GUIContent[turretAimModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretAimModeLabel[i], turretAimModeTooltip[i]);
				aimMode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), aimMode, contL);
				unit.turretAimMode=(_TurretAimMode)aimMode;
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
			
			startY+=10;
			
			cont=new GUIContent("FaceTravelDirection:", "Enable to have the unit's transform rotates to face its travel direction");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.faceTravelDirection=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.faceTravelDirection);
			
			cont=new GUIContent("x-axis movement:", "Check to enabled unit movement in x-axis");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.enabledMovementX=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.enabledMovementX);
			
			cont=new GUIContent("z-axis movement:", "Check to enabled unit movement in z-axis");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.enabledMovementZ=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), unit.enabledMovementZ);
			
			startY+=10;
			
			int mode=(int)unit.movementMode;
			cont=new GUIContent("Movement Mode:", "The way in which the movement of the unit is handled with regard to the input");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			contL=new GUIContent[movementModeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(movementModeLabel[i], movementModeTooltip[i]);
			mode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), mode, contL);
			unit.movementMode=(_MovementMode)mode;
			
			cont=new GUIContent("Move Speed:", "The move speed of the unit");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.moveSpeed=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.moveSpeed);
			
			cont=new GUIContent("Boost Speed Mul:", "Speed-Multiplier(rigid mode)/Acceleration-Multiplier(free-form mode) when using boost (left-shift by default)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.boostMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.boostMultiplier);
			
			cont=new GUIContent("Boost Energy Rate:", "Energy consumption rate (per second) when using boost (left-shift by default)\n0.3 being 30% of total energy per second");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.boostEnergyRate=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.boostEnergyRate);
			
			if(unit.movementMode==_MovementMode.FreeForm){
				cont=new GUIContent("Acceleration:", "The acceleration of the unit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.acceleration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.acceleration);
				
				cont=new GUIContent("Decceleration:", "The rate of the unit losing it's speed");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.decceleration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.decceleration);
				
				cont=new GUIContent("Active Braking:", "Stopping power when using active braking (space by default)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.activeBrakingRate=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), unit.activeBrakingRate);
			}
			
			startY+=10;
			
			
			cont=new GUIContent("Enable Limit ", "Check to limit player movement to a defined area");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			unit.useLimit=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 30, height), unit.useLimit);
			
			if(unit.useLimit){
				cont=new GUIContent("Show", "");
				limitFoldout=EditorGUI.Foldout(new Rect(startX+spaceX+40, startY, spaceX, height), limitFoldout, cont);
				
				if(limitFoldout){
					cont=new GUIContent("x-axis (min/max):", "Stopping power when using active braking (space by default)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.minPosX=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 70, height), unit.minPosX);
					unit.maxPosX=EditorGUI.FloatField(new Rect(startX+spaceX+75, startY, 70, height), unit.maxPosX);
					
					cont=new GUIContent("z-axis (min/max):", "Stopping power when using active braking (space by default)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.minPosZ=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 70, height), unit.minPosZ);
					unit.maxPosZ=EditorGUI.FloatField(new Rect(startX+spaceX+75, startY, 70, height), unit.maxPosZ);
				}
			}
			
			startY+=10;
			
			
			Vector2 v3=DrawDestroyEffectObj(startX, startY+spaceY, unit);
			startY=v3.y+20;
			
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Unit description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			unit.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), unit.desp, style);
			
			
			return new Vector2(startX, startY+200);
		}
		
		private bool limitFoldout=false;
		
		protected Vector2 DrawUnitList(float startX, float startY, List<UnitPlayer> unitList){
			List<Item> list=new List<Item>();
			for(int i=0; i<unitList.Count; i++){
				Item item=new Item(unitList[i].prefabID, unitList[i].unitName, unitList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list, true, true, selectedUnitList.Count==0);
		}
		
		
		
		public static int NewItem(UnitPlayer unit){ return window._NewItem(unit); }
		int _NewItem(UnitPlayer unit){
			if(unitPlayerDB.unitList.Contains(unit)) return selectID;
			
			unit.prefabID=GenerateNewID(unitPlayerIDList);
			unitPlayerIDList.Add(unit.prefabID);
			
			unitPlayerDB.unitList.Add(unit);
			
			UpdateLabel_UnitPlayer();
			
			return unitPlayerDB.unitList.Count-1;
		}
		void DeleteItem(){
			unitPlayerIDList.Remove(unitPlayerDB.unitList[deleteID].prefabID);
			unitPlayerDB.unitList.RemoveAt(deleteID);
			
			UpdateLabel_UnitPlayer();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<unitPlayerDB.unitList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			UnitPlayer unit=unitPlayerDB.unitList[selectID];
			unitPlayerDB.unitList[selectID]=unitPlayerDB.unitList[selectID+dir];
			unitPlayerDB.unitList[selectID+dir]=unit;
			selectID+=dir;
		}
		
		
		
		
		
		private SerializedObject srlObj;
		public List<UnitPlayer> selectedUnitList=new List<UnitPlayer>();	//unit selected in hierarchy or project-tab
		
		void SerializeItemInUnitList(){
			srlObj=null;
			if(unitPlayerDB!=null && unitPlayerDB.unitList.Count>0){
				if(selectID<0) selectID=0;
				srlObj = new SerializedObject((UnityEngine.Object)unitPlayerDB.unitList[selectID]);
			}
		}
		
		void SelectItem(){
			selectID=Mathf.Clamp(selectID, 0, unitPlayerDB.unitList.Count-1);
			Selection.activeGameObject=unitPlayerDB.unitList[selectID].gameObject;
			SerializeItemInUnitList();
		}
		
		void OnSelectionChange(){
			if(window==null) return;
			
			srlObj=null;
			
			selectedUnitList=new List<UnitPlayer>();
			
			UnityEngine.Object[] filtered = Selection.GetFiltered(typeof(UnitPlayer), SelectionMode.Editable);
			for(int i=0; i<filtered.Length; i++) selectedUnitList.Add((UnitPlayer)filtered[i]);
			
			//if no no relevent object is selected
			if(selectedUnitList.Count==0){
				SelectItem();
				if(unitPlayerDB.unitList.Count>0 && selectID>=0) 
					UpdateObjectHierarchyList(unitPlayerDB.unitList[selectID].gameObject); 
			}
			else{
				//only one relevent object is selected
				if(selectedUnitList.Count==1){
					//if the selected object is a prefab and match the selected item in editor, do nothing
					if(selectID>0 && selectedUnitList[0]==unitPlayerDB.unitList[selectID]){
						UpdateObjectHierarchyList(selectedUnitList[0].gameObject); 
					}
					//if the selected object doesnt match...
					else{
						//if the selected object existed in DB
						if(TDSEditor.ExistInDB(selectedUnitList[0])){
							window.selectID=TDSEditor.GetUnitPlayerIndex(selectedUnitList[0].prefabID)-1;
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
