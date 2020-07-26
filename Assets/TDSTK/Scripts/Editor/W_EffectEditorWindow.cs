using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class EffectEditorWindow : TDSEditorWindow {
		
		private static EffectEditorWindow window;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (EffectEditorWindow)EditorWindow.GetWindow(typeof (EffectEditorWindow), false, "Effect Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			window.SetupCallback();
		}
		
		
		public void SetupCallback(){
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<Effect> effectList=effectDB.effectList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(effectDB, "EffectDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(effectList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawEffectList(startX, startY, effectList);	
			
			startX=v2.x+25;
			
			if(effectList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				//float cachedX=startX;
				v2=DrawEffectConfigurator(startX, startY, effectList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTDS();
			
			return true;
		}
		
		
		
		Vector2 DrawEffectConfigurator(float startX, float startY, Effect effect){
			
			//float cachedX=startX;
			//float cachedY=startY;
			
			TDSEditorUtility.DrawSprite(new Rect(startX, startY, 60, 60), effect.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The effect name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/2, width, height), cont);
			effect.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), effect.name);
			
			cont=new GUIContent("Icon:", "The effect icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), effect.icon, typeof(Sprite), false);
			
			startX-=65;
			startY+=10+spaceY;	//cachedY=startY;
			
			
			cont=new GUIContent("Show On UI:", "Check to show the effect icon on UI when it's applied on player\nOnly applies to default UI");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.showOnUI=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), effect.showOnUI);
			
			startY+=10;
			
			cont=new GUIContent("Hit Chance:", "The chance of the effect being applied successfully. Takes value from 0-1 with 0.7 being 70%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.hitChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.hitChance);
			
			cont=new GUIContent("Duration:", "The active duration of the effect in seconds");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.duration);
			
			startY+=10;
			
			cont=new GUIContent("Restore HitPoint:", "Amount of hit-point to be restored to the unit per seconds while the effect is active");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.restoreHitPoint=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.restoreHitPoint);
			
			cont=new GUIContent("Restore Energy:", "Amount of energy to be restored to the unit per seconds while the effect is active");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.restoreEnergy=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.restoreEnergy);
			
			startY+=10;
			
			cont=new GUIContent("Speed Multiplier:", "The speed multiplier to be applied to the unit's speed. Stacks with other effects.\n - 0.7 means 70% of the default speed\n - 1.5 means 150% of the default speed");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.speedMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.speedMul);
			
			cont=new GUIContent("Stun:", "Check if the effect stuns the target. Stunned unit cannot move or attack");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.stun=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), effect.stun);
			
			cont=new GUIContent("Invincible:", "Check if the effect makes the target invincible. Invincible unit are immuned to all damage and effect");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.invincible=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), effect.invincible);
			
			startY+=10;
			
			cont=new GUIContent("Damage Multiplier:", "The damage multiplier to be applied to the unit's damage. Stacks with other effects.\n - 0.7 means 70% of the default deamge\n - 1.5 means 150% of the default damage");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.damageMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.damageMul);
			
			cont=new GUIContent("Crit Chance Mul.:", "The critical chance multiplier to be applied to the unit's critical chance. Stacks with other effects.\n - 0.7 means 70% of the default value\n - 1.5 means 150% of the default value");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.critChanceMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.critChanceMul);
			
			cont=new GUIContent("Crit Multiplier Mul.:", "The critical multiplier to be applied to the unit's critical multiplier. Stacks with other effects.\n - 0.7 means 70% of the default value\n - 1.5 means 150% of the default value");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			effect.critMultiplierMul=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.critMultiplierMul);
			
			startY+=10;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Effect description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			effect.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), effect.desp, style);
			
			
			return new Vector2(startX, startY+200);
		}
		
		
		
		protected Vector2 DrawEffectList(float startX, float startY, List<Effect> effectList){
			List<Item> list=new List<Item>();
			for(int i=0; i<effectList.Count; i++){
				Item item=new Item(effectList[i].ID, effectList[i].name, effectList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		
		
		int NewItem(int cloneID=-1){
			Effect effect=null;
			if(cloneID==-1){
				effect=new Effect();
				effect.name="New Effect";
			}
			else{
				effect=effectDB.effectList[selectID].Clone();
			}
			effect.ID=GenerateNewID(effectIDList);
			effectIDList.Add(effect.ID);
			
			effectDB.effectList.Add(effect);
			
			UpdateLabel_Effect();
			
			return effectDB.effectList.Count-1;
		}
		void DeleteItem(){
			effectIDList.Remove(effectDB.effectList[deleteID].ID);
			effectDB.effectList.RemoveAt(deleteID);
			
			UpdateLabel_Effect();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<effectDB.effectList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Effect effect=effectDB.effectList[selectID];
			effectDB.effectList[selectID]=effectDB.effectList[selectID+dir];
			effectDB.effectList[selectID+dir]=effect;
			selectID+=dir;
		}
		
		
	}
}
