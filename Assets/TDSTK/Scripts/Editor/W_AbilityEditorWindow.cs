using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class AbilityEditorWindow : TDSEditorWindow {
		
		private static AbilityEditorWindow window;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (AbilityEditorWindow)EditorWindow.GetWindow(typeof (AbilityEditorWindow), false, "Ability Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
		}
		
		
		private static string[] abilityTypeLabel;
		private static string[] abilityTypeTooltip;
		
		private static string[] moveTypeLabel;
		private static string[] moveTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_AbilityType)).Length;
			abilityTypeLabel=new string[enumLength];
			abilityTypeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				abilityTypeLabel[n]=((_AbilityType)n).ToString();
				
				if((_AbilityType)n==_AbilityType.AOE) 		abilityTypeTooltip[n]="AOE around the selected position, hit all enemy in range";
				if((_AbilityType)n==_AbilityType.AOESelf) 	abilityTypeTooltip[n]="AOE around the player, hit all enemy in range";
				if((_AbilityType)n==_AbilityType.All) 			abilityTypeTooltip[n]="Hit all enemy in on the map";
				if((_AbilityType)n==_AbilityType.Self) 		abilityTypeTooltip[n]="Target player itself";
				if((_AbilityType)n==_AbilityType.Shoot) 	abilityTypeTooltip[n]="Fire a shoot object that trigger upon hit\n(like a normal attack)";
				if((_AbilityType)n==_AbilityType.Movement)abilityTypeTooltip[n]="Move the player";
				if((_AbilityType)n==_AbilityType.Custom) 	abilityTypeTooltip[n]="No function atm";
			}
			
			
			enumLength = Enum.GetValues(typeof(_MoveType)).Length;
			moveTypeLabel=new string[enumLength];
			moveTypeTooltip=new string[enumLength];
			
			for(int n=0; n<enumLength; n++){
				moveTypeLabel[n]=((_MoveType)n).ToString();
				
				if((_MoveType)n==_MoveType.Blink) 		moveTypeTooltip[n]="Blink forward instantly";
				if((_MoveType)n==_MoveType.Dash) 		moveTypeTooltip[n]="Dash forward";
				if((_MoveType)n==_MoveType.Teleport) 	moveTypeTooltip[n]="Teleport to the location specify by the cursor";
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
			
			List<Ability> abilityList=abilityDB.abilityList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(abilityDB, "AbilityDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(abilityList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawAbilityList(startX, startY, abilityList);	
			
			startX=v2.x+25;
			
			if(abilityList.Count==0) return true;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				//float cachedX=startX;
				v2=DrawAbilityConfigurator(startX, startY, abilityList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTDS();
			
			return true;
		}
		
		
		
		Vector2 DrawAbilityConfigurator(float startX, float startY, Ability ability){
			
			//~ //float cachedX=startX;
			//~ //float cachedY=startY;
			
			TDSEditorUtility.DrawSprite(new Rect(startX, startY, 60, 60), ability.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The ability name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/2, width, height), cont);
			ability.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), ability.name);
			
			cont=new GUIContent("Icon:", "The ability icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), ability.icon, typeof(Sprite), false);
			
			startX-=65;
			startY+=10+spaceY;	//cachedY=startY;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Ability Type & Stats", headerStyle);
			
			int type=(int)ability.type;
			cont=new GUIContent("Ability Type:", "Type of the ability which determine the targeting mechanic and potential target");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			contL=new GUIContent[abilityTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(abilityTypeLabel[i], abilityTypeTooltip[i]);
			type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), type, contL);
			ability.type=(_AbilityType)type;
			
			startY+=5;
			
			cont=new GUIContent("Cost:", "The energy cost to launch the ability");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.cost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.cost);
			
			cont=new GUIContent("Cooldown:", "The cooldown in second for the ability before it can be used again after it's used");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.cooldown);
			
			startY+=10;
			
			
			if(ability.type==_AbilityType.Shoot){
				cont=new GUIContent("Range:", "The shoot range of the ability");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.range);
				
				cont=new GUIContent("Shoot Object:", "The prefab of the bullet/object fired by the weapon\nMust be a prefab with ShootObject component attached on it");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), ability.shootObject, typeof(GameObject), false);
				
				cont=new GUIContent("Shoot Pos Offset:", "The relative shoot position to the position of the player transform, in player transform space");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.shootPosOffset=EditorGUI.Vector3Field(new Rect(startX+spaceX, startY, width, height), "", ability.shootPosOffset);
			}
			
			
			if(ability.type!=_AbilityType.Movement && ability.type!=_AbilityType.Custom){
				startY+=10;
				
				bool showAOE=ability.type==_AbilityType.AOE | ability.type==_AbilityType.AOESelf | ability.type==_AbilityType.Shoot;
				Vector2 v2=DrawAttackStats1(startX, startY+spaceY, ability.aStats, showAOE);
				startY=v2.y+15;
			}
			
			
			if(ability.type==_AbilityType.Movement){
				int mType=(int)ability.moveType;
				cont=new GUIContent("Move Type:", "Type of the move ability");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				contL=new GUIContent[moveTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(moveTypeLabel[i], moveTypeTooltip[i]);
				mType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), mType, contL);
				ability.moveType=(_MoveType)mType;
				
				startY+=10;				
				
				cont=new GUIContent("Range:", "The range limit of the move");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.range);
				
				cont=new GUIContent("Duration:", "The duration of the dash");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.moveType==_MoveType.Dash) ability.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.duration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				startY+=15;
			}
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Miscellaneous", headerStyle);
			
			cont=new GUIContent("Launch Object:", "The object to be spawned when the ability is launched (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.launchObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), ability.launchObj, typeof(GameObject), false);
			
			cont=new GUIContent("AutoDestroy Effect:", "Check if the launch object needs to be removed from the game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(ability.launchObj!=null){
				ability.autoDestroyLaunchObj=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), ability.autoDestroyLaunchObj);
				
				if(ability.autoDestroyLaunchObj){
					cont=new GUIContent(" - Duration:", "The delay in seconds before the launch object is destroyed");
					EditorGUI.LabelField(new Rect(startX+spaceX+15, startY, width, height), cont);
					ability.launchObjActiveDuration=EditorGUI.FloatField(new Rect(startX+spaceX+width-58, startY, 40, height), ability.launchObjActiveDuration);
				}
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=5;
			
			cont=new GUIContent("Launch SFX:", "Audio clip to play when the ability is launched (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.launchSFX=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), ability.launchSFX, typeof(AudioClip), false);
			
			
			startY+=15;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Ability description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			ability.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), ability.desp, style);
			
			
			return new Vector2(startX, startY+200);
		}
		
		
		
		protected Vector2 DrawAbilityList(float startX, float startY, List<Ability> abilityList){
			List<Item> list=new List<Item>();
			for(int i=0; i<abilityList.Count; i++){
				Item item=new Item(abilityList[i].ID, abilityList[i].name, abilityList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		int NewItem(int cloneID=-1){
			Ability ability=null;
			if(cloneID==-1){
				ability=new Ability();
				ability.name="New Ability";
			}
			else{
				ability=abilityDB.abilityList[selectID].Clone();
			}
			ability.ID=GenerateNewID(abilityIDList);
			abilityIDList.Add(ability.ID);
			
			abilityDB.abilityList.Add(ability);
			
			UpdateLabel_Ability();
			
			return abilityDB.abilityList.Count-1;
		}
		void DeleteItem(){
			abilityIDList.Remove(abilityDB.abilityList[deleteID].ID);
			abilityDB.abilityList.RemoveAt(deleteID);
			
			UpdateLabel_Ability();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<abilityDB.abilityList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Ability ability=abilityDB.abilityList[selectID];
			abilityDB.abilityList[selectID]=abilityDB.abilityList[selectID+dir];
			abilityDB.abilityList[selectID+dir]=ability;
			selectID+=dir;
		}
		
		
	}
}
