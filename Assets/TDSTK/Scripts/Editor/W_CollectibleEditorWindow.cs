using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class CollectibleEditorWindow : TDSEditorWindow {
		
		private static CollectibleEditorWindow window;
		
		public static void Init() {
			// Get existing open window or if none, make a new one:
			window = (CollectibleEditorWindow)EditorWindow.GetWindow(typeof (CollectibleEditorWindow), false, "Collectible Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
			
			window.OnSelectionChange();
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
			
			List<Collectible> collectibleList=collectibleDB.collectibleList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(collectibleDB, "collectibleDB");
			if(collectibleList.Count>0 && selectID>=0) Undo.RecordObject(collectibleList[selectID], "collectible");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Collectible:");
			Collectible newCollectible=null;
			newCollectible=(Collectible)EditorGUI.ObjectField(new Rect(125, 7, 140, 17), newCollectible, typeof(Collectible), false);
			if(newCollectible!=null) Select(NewItem(newCollectible));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawCollectibleList(startX, startY, collectibleList);	
			startX=v2.x+25;
			
			if(collectibleList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
				
				if(srlObj.isEditingMultipleObjects){
					EditorGUI.HelpBox(new Rect(startX, startY, width+spaceX, 40), "More than 1 Collectible instance is selected\nMulti-instance editing is not supported\nTry use Inspector instead", MessageType.Warning);
					startY+=55;
				}
			
				Collectible cltToEdit=selectedCltList.Count!=0 ? selectedCltList[0] : collectibleList[selectID];
				
				Undo.RecordObject(cltToEdit, "cltToEdit");
				
				v2=DrawCollectibleConfigurator(startX, startY, cltToEdit);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
				srlObj.ApplyModifiedProperties();
			
				if(selectedCltList.Count>0 && TDSEditor.IsPrefabInstance(selectedCltList[0].gameObject)){
					PrefabUtility.RecordPrefabInstancePropertyModifications(selectedCltList[0]);
				}
			
			GUI.EndScrollView();
			
			if(GUI.changed){
				SetDirtyTDS();
				for(int i=0; i<selectedCltList.Count; i++) EditorUtility.SetDirty(selectedCltList[i]);
			}
			
			return true;
		}
		
		
		
		Vector2 DrawCollectibleConfigurator(float startX, float startY, Collectible cItem){
			
			//float cachedX=startX;
			//float cachedY=startY;
			
			TDSEditorUtility.DrawSprite(new Rect(startX, startY, 60, 60), cItem.icon);
			startX+=65;
			
			float offsetY=TDSEditor.IsPrefab(cItem.gameObject) ? 5 : 0 ;
			
			cont=new GUIContent("Name:", "The collectible name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=offsetY, width, height), cont);
			cItem.collectibleName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), cItem.collectibleName);
			
			cont=new GUIContent("Icon:", "The collectible icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			cItem.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), cItem.icon, typeof(Sprite), false);
			
			cont=new GUIContent("Prefab:", "The prefab object of the unit\nClick this to highlight it in the ProjectTab");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), cItem.gameObject, typeof(GameObject), false);
			
			startX-=65;
			startY+=spaceY;	//cachedY=startY;
			
			
			int type=(int)cItem.type;
			cont=new GUIContent("Target Type:", "The target which the collectible affects when triggered");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			contL=new GUIContent[collectTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(collectTypeLabel[i], collectTypeTooltip[i]);
			type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), type, contL);
			cItem.type=(_CollectType)type;
			
			
			startY+=10;
			
			if(cItem.type==_CollectType.Ability){
				int abID=(int)cItem.abilityID;
				cont=new GUIContent(" - Trigger Ability:", "The ability to activate when triggered");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				contL=new GUIContent[abilityLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(abilityLabel[i]);
				abID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), abID, contL);
				cItem.abilityID=abID;
			}
			else if(cItem.type==_CollectType.Self){
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Instant Gain", headerStyle);
				
				cont=new GUIContent("Life:", "The amount of respawn gained by the player");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.life=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), cItem.life);
				
				startY+=10;
				
				cont=new GUIContent("HitPoint:", "The amount of hit-point gained by the player");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.hitPoint=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), cItem.hitPoint);
				
				cont=new GUIContent("Energy:", "The amount of energy gained by the player");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.energy=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), cItem.energy);
				
				startY+=10;
				
				//cont=new GUIContent("Credits:", "The amount of credist gained by the player");
				//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//cItem.credit=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), cItem.credit);
				
				cont=new GUIContent("Score:", "The amount of points gained by the player");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.score=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), cItem.score);
				
				startY+=10;
				
				cont=new GUIContent("Ammo:", "The amount of ammo gained by the player. If set as -1, the ammo count will be refilled to full");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.ammo=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), cItem.ammo);
				
				
				if(cItem.ammo!=0){
					startY+=5;
					
					weaponLabel[0]="All Weapons";
					
					int weaponIdx=TDSEditor.GetWeaponIndex(cItem.ammoID);
					Weapon weapon=weaponIdx>0 ? weaponDB.weaponList[weaponIdx-1] : null ;
					
					if(weapon!=null)
						TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), weapon!=null ? weapon.icon : null);
					
					cont=new GUIContent(" - Weapon:", "The weapon in which the ammo gain of the collectible is intended for.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont);
					
					weaponIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weaponIdx, weaponLabel);
					if(weaponIdx>0) cItem.ammoID=weaponDB.weaponList[weaponIdx-1].ID;
					else cItem.ammoID=-1;
					
					weaponLabel[0]="Unassigned";
				}
				else{
					cont=new GUIContent("Weapon:", "The weapon in which the ammo gain of the collectible is intended for.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				}
				
				
				startY+=10;
				
				cont=new GUIContent("Experience:", "The amount of experience gained by the player.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.exp=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), cItem.exp);
				
				cont=new GUIContent("Perk Currency:", "The amount of perk currency gained by the player.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cItem.perkCurrency=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), cItem.perkCurrency);
				
				
				startY+=55;
				
				//int effectIdx=cItem.effect==null ? 0 : TDSEditor.GetEffectIndex(cItem.effect.ID);
				//if(effectIdx==0) cItem.effect=null;
				int effectIdx=cItem.effectID>=0 ? TDSEditor.GetEffectIndex(cItem.effectID) : 0 ;
				
				TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), effectIdx>0 ? effectDB.effectList[effectIdx-1].icon : null);
				if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit ")) EffectEditorWindow.Init();
				
				cont=new GUIContent("Triggered Effect:", "Special effect that applies on target when triggered (optional)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont, headerStyle);
				
				effectIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), effectIdx, effectLabel);
				if(effectIdx>0) cItem.effectID=effectDB.effectList[effectIdx-1].ID;
				else cItem.effectID=-1;
				
				//if(effectIdx>0) cItem.effect=effectDB.effectList[effectIdx-1];
				//else cItem.effect=null;
				
				
				startY+=10;
				
				
				cont=new GUIContent("Gain Weapon:", "Weapon gained by player upon triggered (optional)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
				cItem.gainWeapon=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 30, height), cItem.gainWeapon);
				
				if(cItem.gainWeapon){
					type=(int)cItem.weaponType;
					cont=new GUIContent(" - GainWeaponType:", "What the new weapon is for");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					contL=new GUIContent[weaponTypeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(weaponTypeLabel[i], weaponTypeTooltip[i]);
					type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), type, contL);
					cItem.weaponType=(Collectible._WeaponType)type;
					
					
					cont=new GUIContent(" - Duration:", "The duration of the temporary weapon. Set to -1 for not time limit (limit by weapon ammo instead)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(cItem.weaponType==Collectible._WeaponType.Temporary)
						cItem.tempWeapDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), cItem.tempWeapDuration);
					else 
						EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
					
					cont=new GUIContent(" - Random Weapon:", "Check if player will get random weapon out of a few potential candidates");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					cItem.randomWeapon=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 30, height), cItem.randomWeapon);
					
					if(cItem.randomWeapon){
						cont=new GUIContent(" - EnableAllWeapon:", "Check if all weapon in the database are to be added to the random pool");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						cItem.enableAllWeapon=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 30, height), cItem.enableAllWeapon);
						
						if(!cItem.enableAllWeapon){
							int enabledCount=0;
							for(int i=0; i<weaponDB.weaponList.Count; i++){
								Weapon weapon=weaponDB.weaponList[i];
								
								bool enabled=cItem.weaponList.Contains(weapon);
								bool enabledCached=enabled;
								
								TDSEditorUtility.DrawSprite(new Rect(startX+20, startY+=spaceY, 30, 30), weapon.icon, weapon.desp);
								cont=new GUIContent(weapon.weaponName, weapon.desp);
								EditorGUI.LabelField(new Rect(startX+65, startY+15, width, height), cont);
								enabled=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY+15, width, height), enabled);
								startY+=14;
								
								if(enabled!=enabledCached){
									if(enabled) cItem.weaponList.Insert(enabledCount, weapon);
									else cItem.weaponList.Remove(weapon);
								}
								
								if(enabled) enabledCount+=1;
							}
						}
					}
					else{
						if(cItem.weaponList.Count!=1) cItem.weaponList=new List<Weapon>{ null };
						
						int weaponIdx1=cItem.weaponList[0]!=null ? TDSEditor.GetWeaponIndex(cItem.weaponList[0].ID) : 0;
						
						if(cItem.weaponList[0]!=null)
							TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-41, 40, 40), cItem.weaponList[0].icon);
						
						cont=new GUIContent(" - Weapon Gained:", "Weapon gained by player upon triggered (optional)");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						
						weaponIdx1=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weaponIdx1, weaponLabel);
						if(weaponIdx1>0) cItem.weaponList[0]=weaponDB.weaponList[weaponIdx1-1];
						else cItem.weaponList[0]=null;
					}
				}
			}
			else{
				//if(cItem.type==_CollectType.AOEHostile){
					//cont=new GUIContent("AOE Range:", "");
					//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					//cItem.aoeRange=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), cItem.aoeRange);
				//}
				
				Vector2 v2=DrawAttackStats1(startX, startY+spaceY, cItem.aStats, cItem.type==_CollectType.AOEHostile, cItem.type==_CollectType.AOEHostile);
				startY=v2.y;
			}
			
			
			startY+=20;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Miscellaneous", headerStyle);
			
			cont=new GUIContent("Triggered Effect Obj:", "The object to be spawned when the collectible is triggered (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			cItem.triggerEffectObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), cItem.triggerEffectObj, typeof(GameObject), false);
			
			cont=new GUIContent("AutoDestroy Effect:", "Check if the effect object needs to be removed from the game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(cItem.triggerEffectObj!=null){
				cItem.autoDestroyEffectObj=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), cItem.autoDestroyEffectObj);
				
				if(cItem.autoDestroyEffectObj){
					cont=new GUIContent(" - Duration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX+spaceX+15, startY, width, height), cont);
					cItem.effectObjActiveDuration=EditorGUI.FloatField(new Rect(startX+spaceX+width-58, startY, 40, height), cItem.effectObjActiveDuration);
				}
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=5;
			
			cont=new GUIContent("Triggered SFX:", "Audio clip to play when the collectible is triggered (optional)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			cItem.triggerSFX=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), cItem.triggerSFX, typeof(AudioClip), false);
			
			
			startY+=10;
			
			cont=new GUIContent("Self Destruct:", "Check if the item is to self-destruct if not collected in a set time frame");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			cItem.selfDestruct=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), cItem.selfDestruct);
			
			cont=new GUIContent("Active Duration:", "How long the item will stay active before it self destruct");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(cItem.selfDestruct)
				cItem.selfDestructDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), cItem.selfDestructDuration);
			else 
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			
			startY+=5;
			
			cont=new GUIContent("BlinkBeforeDestruct:", "Blink to give player warning before the object self-destruct");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(cItem.selfDestruct)
				cItem.blinkBeforeDestroy=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), cItem.blinkBeforeDestroy);
			else
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			cont=new GUIContent("Blink Duration:", "The long the item is gong to blink for");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(cItem.selfDestruct && cItem.blinkBeforeDestroy)
				cItem.blinkDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), cItem.blinkDuration);
			else
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			int objID=cItem.blinkObj==null ? 0 : GetObjectIDFromHList(cItem.blinkObj.transform, objHList);
			cont=new GUIContent("Blink Object: ", "The mesh object to blink (The system will deactivate/activate the blink object for blinking. we only need to deactivate the child object and contain the mesh, not the whole item)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(cItem.selfDestruct && cItem.blinkBeforeDestroy){
				objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
				cItem.blinkObj = (objHList[objID]==null) ? null : objHList[objID];
			}
			else
				EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			
			startY+=15;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Item description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			cItem.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), cItem.desp, style);
			
			
			return new Vector2(startX, startY+200);
		}
		
		
		
		
		
		protected Vector2 DrawCollectibleList(float startX, float startY, List<Collectible> collectibleList){
			List<Item> list=new List<Item>();
			for(int i=0; i<collectibleList.Count; i++){
				Item item=new Item(collectibleList[i].ID, collectibleList[i].collectibleName, collectibleList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		
		public static int NewItem(Collectible collectible){ return window._NewItem(collectible); }
		int _NewItem(Collectible collectible){
			if(collectibleDB.collectibleList.Contains(collectible)) return selectID;
			
			collectible.ID=GenerateNewID(collectibleIDList);
			collectibleIDList.Add(collectible.ID);
			
			collectibleDB.collectibleList.Add(collectible);
			
			UpdateLabel_Collectible();
			
			return collectibleDB.collectibleList.Count-1;
		}
		void DeleteItem(){
			collectibleIDList.Remove(collectibleDB.collectibleList[deleteID].ID);
			collectibleDB.collectibleList.RemoveAt(deleteID);
			
			UpdateLabel_Collectible();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<collectibleDB.collectibleList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Collectible collectible=collectibleDB.collectibleList[selectID];
			collectibleDB.collectibleList[selectID]=collectibleDB.collectibleList[selectID+dir];
			collectibleDB.collectibleList[selectID+dir]=collectible;
			selectID+=dir;
		}
		
		
		
		
		
		private SerializedObject srlObj;
		public List<Collectible> selectedCltList=new List<Collectible>();	//unit selected in hierarchy or project-tab
		
		void SerializeItemInUnitList(){
			srlObj=null;
			if(collectibleDB!=null && collectibleDB.collectibleList.Count>0){
				if(selectID<0) selectID=0;
				srlObj = new SerializedObject((UnityEngine.Object)collectibleDB.collectibleList[selectID]);
			}
		}
		
		void SelectItem(){
			selectID=Mathf.Clamp(selectID, 0, collectibleDB.collectibleList.Count-1);
			Selection.activeGameObject=collectibleDB.collectibleList[selectID].gameObject;
			SerializeItemInUnitList();
		}
		
		void OnSelectionChange(){
			if(window==null) return;
			
			srlObj=null;
			
			selectedCltList=new List<Collectible>();
			
			UnityEngine.Object[] filtered = Selection.GetFiltered(typeof(Collectible), SelectionMode.Editable);
			for(int i=0; i<filtered.Length; i++) selectedCltList.Add((Collectible)filtered[i]);
			
			//if no no relevent object is selected
			if(selectedCltList.Count==0){
				SelectItem();
				if(collectibleDB.collectibleList.Count>0 && selectID>=0) 
					UpdateObjectHierarchyList(collectibleDB.collectibleList[selectID].gameObject); 
			}
			else{
				//only one relevent object is selected
				if(selectedCltList.Count==1){
					//if the selected object is a prefab and match the selected item in editor, do nothing
					if(selectID>0 && selectedCltList[0]==collectibleDB.collectibleList[selectID]){
						UpdateObjectHierarchyList(selectedCltList[0].gameObject); 
					}
					//if the selected object doesnt match...
					else{
						//if the selected object existed in DB
						if(TDSEditor.ExistInDB(selectedCltList[0])){
							window.selectID=TDSEditor.GetCollectibleIndex(selectedCltList[0].ID)-1;
							UpdateObjectHierarchyList(selectedCltList[0].gameObject); 
							SelectItem();
						}
						//if the selected object is not in DB
						else{
							selectID=-1;
							UpdateObjectHierarchyList(selectedCltList[0].gameObject); 
						}
					}
				}
				//selected multiple editable object
				else{
					selectID=-1;
					UpdateObjectHierarchyList(selectedCltList[0].gameObject); 
				}
				
				srlObj = new SerializedObject(filtered);
			}
			
			Repaint();
		}
		
	}
	
}
