using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class WeaponEditorWindow : TDSEditorWindow {
		
		private static WeaponEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (WeaponEditorWindow)EditorWindow.GetWindow(typeof (WeaponEditorWindow), false, "Weapon Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			if(prefabID>=0) window.selectID=TDSEditor.GetWeaponIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		
		
		public void SetupCallback(){
			selectCallback=this.SelectItem;
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
			
			SelectItem();
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<Weapon> weaponList=weaponDB.weaponList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(effectDB, "weaponDB");
			if(weaponList.Count>0) Undo.RecordObject(weaponList[selectID], "weapon");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			//if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			//if(abilityList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Weapon:");
			Weapon newWeapon=null;
			newWeapon=(Weapon)EditorGUI.ObjectField(new Rect(115, 7, 150, 17), newWeapon, typeof(Weapon), false);
			if(newWeapon!=null) Select(NewItem(newWeapon));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawWeaponList(startX, startY, weaponList);	
			startX=v2.x+25;
			
			if(weaponList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				//float cachedX=startX;
				v2=DrawWeaponConfigurator(startX, startY, weaponList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTDS();
			
			return true;
		}
		
		
		
		
		Vector2 DrawWeaponConfigurator(float startX, float startY, Weapon weapon){
			
			//float cachedX=startX;
			//float cachedY=startY;
			
			TDSEditorUtility.DrawSprite(new Rect(startX, startY, 60, 60), weapon.icon);
			startX+=65;
			
			float offsetY=TDSEditor.IsPrefab(weapon.gameObject) ? 5 : 0 ;
			
			cont=new GUIContent("Name:", "The weapon name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=offsetY, width, height), cont);
			weapon.weaponName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), weapon.weaponName);
			
			cont=new GUIContent("Icon:", "The weapon icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), weapon.icon, typeof(Sprite), false);
			
			cont=new GUIContent("Prefab:", "The prefab object of the weapon\nClick this to highlight it in the ProjectTab");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), weapon.gameObject, typeof(GameObject), false);
			
			startX-=65;
			startY+=spaceY;	//cachedY=startY;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Weapon Stats", headerStyle);
			
			cont=new GUIContent("Shoot Object:", "The prefab of the bullet/object fired by the weapon\nMust be a prefab with ShootObject component attached on it");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), weapon.shootObject, typeof(GameObject), false);
			
			cont=new GUIContent("ShootPoint:", "The transform which indicate the position where the shootObject will be fired from (Optional)\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the weapon transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the firing direction.\n");
			shootPointFoldout=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), shootPointFoldout, cont);
			int shootPointCount=weapon.shootPointList.Count;
			shootPointCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), shootPointCount);
			
			if(shootPointCount!=weapon.shootPointList.Count){
				while(weapon.shootPointList.Count<shootPointCount) weapon.shootPointList.Add(null);
				while(weapon.shootPointList.Count>shootPointCount) weapon.shootPointList.RemoveAt(weapon.shootPointList.Count-1);
			}
				
			if(shootPointFoldout){
				for(int i=0; i<weapon.shootPointList.Count; i++){
					int objID=GetObjectIDFromHList(weapon.shootPointList[i], objHList);
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					weapon.shootPointList[i] = (objHList[objID]==null) ? null : objHList[objID].transform;
				}
			}
			
			cont=new GUIContent("Shoot Point Delay:", "The delay in seconds between subsequent shot in each shoot point");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(weapon.shootPointList.Count>1)
				weapon.shootPointDelay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.shootPointDelay);
			else
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			
			startY+=5;
			
			cont=new GUIContent("Continous Fire:", "Check to enable continous firing on the weapon when holding down fire button");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.continousFire=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), weapon.continousFire);
			
			startY+=5;
			
			cont=new GUIContent("Range:", "The effective range of the weapon.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.range);
			
			cont=new GUIContent("Cooldown:", "The cooldown in seconds between subsequent shot");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.cooldown);
			
			cont=new GUIContent("Clip Size:", "How many times the weapon can be fired before it needs a reload. Set to -1 if the weapon can be fired indefinitely without reloading");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.clipSize=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), weapon.clipSize);
			
			cont=new GUIContent("Ammo Cap:", "How many ammo of the weapon the player can keep. Set to -1 if the weapon has unlimited ammo cap");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.ammoCap=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), weapon.ammoCap);
			
			cont=new GUIContent("Reload Duration:", "The duration in seconds to reload the weapon");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.reloadDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.reloadDuration);
			
			startY+=5;
			
			cont=new GUIContent("Recoil:", "The recoil magnitude of the weapon. The higher the value, the less accurate the weapon become when fired continously");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.recoilMagnitude=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.recoilMagnitude);
			
			cont=new GUIContent("Recoil Cam Shake:", "The camera shake magnitude whenever the weapon is fired");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.recoilCamShake=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.recoilCamShake);
			
			startY+=5;
			
			cont=new GUIContent("Spread:", "The number of shoot object split from each shoot point. This is to create a shotgun effect");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.spread=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), weapon.spread);
			weapon.spread=Mathf.Max(1, weapon.spread);
			
			cont=new GUIContent("Spread Angle:", "The angle (in degree) between each spread");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(weapon.spread>1)
				weapon.spreadAngle=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.spreadAngle);
			else
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=10;
			
			Vector2 v2=DrawAttackStats1(startX, startY+spaceY, weapon.aStats);
			startY=v2.y+20;
			
			startY+=30;
			
			
			//int abilityIdx=weapon.ability==null ? 0 : TDSEditor.GetEffectIndex(weapon.ability.ID);
			//if(abilityIdx==0) weapon.ability=null;
			int abilityIdx=weapon.abilityID>=0 ? TDSEditor.GetAbilityIndex(weapon.abilityID) : 0 ;
			
			TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), abilityIdx>0 ? abilityDB.abilityList[abilityIdx-1].icon : null);
			if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit ")) AbilityEditorWindow.Init();
			
			cont=new GUIContent("AltAttack(Ability):", "Ability that is being used as the weapon alternate fire mode (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont, headerStyle);
			
			abilityIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), abilityIdx, abilityLabel);
			if(abilityIdx>0) weapon.abilityID=abilityDB.abilityList[abilityIdx-1].ID;
			else weapon.abilityID=-1;
			
			//if(abilityIdx>0) weapon.ability=abilityDB.abilityList[abilityIdx-1];
			//else weapon.ability=null;
			
			
			startY+=15;
			
			
			cont=new GUIContent("Shoot SFX:", "Audio clip to play when the weapon fires (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.shootSFX=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), weapon.shootSFX, typeof(AudioClip), false);
			
			cont=new GUIContent("Reload SFX:", "Audio clip to play when the weapon reloads (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.reloadSFX=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), weapon.reloadSFX, typeof(AudioClip), false);
			
			
			startY+=15;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Weapon description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			weapon.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), weapon.desp, style);
			
			
			return new Vector2(startX, startY+200);
		}
		
		
		
		
		
		protected Vector2 DrawWeaponList(float startX, float startY, List<Weapon> weaponList){
			List<Item> list=new List<Item>();
			for(int i=0; i<weaponList.Count; i++){
				Item item=new Item(weaponList[i].ID, weaponList[i].weaponName, weaponList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		
		public static int NewItem(Weapon weapon){ return window._NewItem(weapon); }
		int _NewItem(Weapon weapon){
			if(weaponDB.weaponList.Contains(weapon)) return selectID;
			
			weapon.ID=GenerateNewID(weaponIDList);
			weaponIDList.Add(weapon.ID);
			
			weaponDB.weaponList.Add(weapon);
			
			UpdateLabel_Weapon();
			
			return weaponDB.weaponList.Count-1;
		}
		void DeleteItem(){
			weaponIDList.Remove(weaponDB.weaponList[deleteID].ID);
			weaponDB.weaponList.RemoveAt(deleteID);
			
			UpdateLabel_Weapon();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<weaponDB.weaponList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Weapon weapon=weaponDB.weaponList[selectID];
			weaponDB.weaponList[selectID]=weaponDB.weaponList[selectID+dir];
			weaponDB.weaponList[selectID+dir]=weapon;
			selectID+=dir;
		}
		
		void SelectItem(){ 
			if(weaponDB.weaponList.Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, weaponDB.weaponList.Count-1);
			UpdateObjectHierarchyList(weaponDB.weaponList[selectID].gameObject);
		}
	}
}
