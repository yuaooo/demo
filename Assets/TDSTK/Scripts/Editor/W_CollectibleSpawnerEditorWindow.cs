using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class CollectibleSpawnerEditorWindow : TDSEditorWindow {
		
		private static CollectibleSpawnerEditorWindow window;
		
		public static void Init (GameObject sltObj=null) {
			// Get existing open window or if none, make a new one:
			window = (CollectibleSpawnerEditorWindow)EditorWindow.GetWindow(typeof (CollectibleSpawnerEditorWindow), false, "Collectible Spawner Editor");
			window.minSize=new Vector2(400, 300);
			//window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			window.SetupCallback();
			
			GetAllSpawnerInScene();
			
			if(sltObj!=null){
				Selection.activeGameObject=sltObj;
				window.selectObj=null;
				window.OnSelectionChange();
			}
		}
		
		
		public void SetupCallback(){
			selectCallback=this.SelectItem;
			//SelectItem();
		}
		
		public override void Awake(){
			base.Awake();
			selectID=-1;
		}
		
		
		public override void Update(){
			base.Update();
		}
		
		
		private int editableCount=0;
		private GameObject selectObj;
		private CollectibleSpawner cltSpawner;
		void OnSelectionChange(){
			if(window==null) return;
			
			if(selectObj!=Selection.activeGameObject){
				selectObj=Selection.activeGameObject;
				CollectibleSpawner newSpawner=selectObj!=null ? selectObj.GetComponent<CollectibleSpawner>() : null;
				if(newSpawner!=cltSpawner){
					cltSpawner=newSpawner;
					
					selectID=-1;
					for(int i=0; i<spawnerList.Count; i++){
						if(cltSpawner==spawnerList[i]){
							selectID=i;
							break;
						}
					}
				}
			}
			
			editableCount = Selection.GetFiltered(typeof(CollectibleSpawner), SelectionMode.Editable).Length;
			Repaint();
		}
		
		
		private static List<CollectibleSpawner> spawnerList=new List<CollectibleSpawner>();
		public static void GetAllSpawnerInScene(){
			spawnerList=new List<CollectibleSpawner>();
			CollectibleSpawner[] spawners = FindObjectsOfType(typeof(CollectibleSpawner)) as CollectibleSpawner[];
			foreach(CollectibleSpawner spawner in spawners) spawnerList.Add(spawner);
			
			//if(spawnerList.Count>0){
			//	window.selectID=-1;
			//	window.Select(0);
			//}
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			Undo.RecordObject(this, "window");
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Refresh")) GetAllSpawnerInScene();
			
			for(int i=0; i<spawnerList.Count; i++) Undo.RecordObject(spawnerList[i], "CollectibleSpawner"+i);
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawSpawnerList(startX, startY, spawnerList);	
			startX=v2.x+25;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				if(cltSpawner==null){
					EditorGUI.HelpBox(new Rect(startX, startY, width+spaceX, 50), "Selected object doesn't contain a CollectibleSpawner component", MessageType.Warning);
				}
				else{
					Undo.RecordObject (cltSpawner, "Spawner");
					
					if(editableCount>1){
						EditorGUI.HelpBox(new Rect(startX, startY, width+2*spaceX, 30), "More than 1 Spawner is selected\nMulti-instance editing is not supported", MessageType.Warning);
						startY+=40;
					}
					
					v2=DrawSpawnerConfigurator(startX, startY, cltSpawner);
					contentWidth=425;
					contentHeight=v2.y-55;
				}
			
			GUI.EndScrollView();
			
			
			if(GUI.changed){
				SetDirtyTDS();
				for(int i=0; i<spawnerList.Count; i++) EditorUtility.SetDirty(spawnerList[i]);
			}
			
			return true;
		}
		
		
		protected Vector2 DrawSpawnerList(float startX, float startY, List<CollectibleSpawner> spawnerList){
			List<Item> list=new List<Item>();
			for(int i=0; i<spawnerList.Count; i++){
				if(spawnerList[i]==null){
					spawnerList.RemoveAt(i);
					i-=1;
					continue;
				}
				
				Item item=new Item(i, spawnerList[i].gameObject.name, null);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list, false, false, false);
		}
		
		
		Vector2 DrawSpawnerConfigurator(float startX, float startY, CollectibleSpawner spawner){
			
			startY+=10;
			
			cont=new GUIContent("Spawn Area:", "The area which the unit should be spawn in\nIf unspecified, the unit will simply be spawned at the position of the spawner");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			spawner.spawnArea=(TDSArea)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), spawner.spawnArea, typeof(TDSArea), true);
			
			startY+=10;
			
			if(GUI.Button(new Rect(startX+spaceX+width+20, startY, 80, height), "Assign Self")){
				TDSArea area=spawner.gameObject.GetComponent<TDSArea>();
				if(area==null) area=spawner.gameObject.AddComponent<TDSArea>();
				spawner.spawnArea=area;
			}
			
			cont=new GUIContent("Spawn Upon Start:", "Check to have the spawner start spawning as soon as the game start");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.spawnUponStart=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), spawner.spawnUponStart);
			
			cont=new GUIContent("Start Delay:", "Delay (in second) before the spawning start");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//if(spawner.spawnUponStart)
				spawner.startDelay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.startDelay);
			//else
			//	EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=10;
			
			cont=new GUIContent("Spawn Cooldown:", "The cooldown between each spawn attempt");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.spawnCD=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.spawnCD);
			
			cont=new GUIContent("Max Item Count:", "The maximum amount of active item in the game allowed by this spawner at any given item");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.maxItemCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), spawner.maxItemCount);
			
			cont=new GUIContent("Spawn Chance:", "The chance to successfully spawn an item during each cooldown cycle. Takes value from 0-1 with 0.3 being 30% to successfully spawn an item");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.spawnChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.spawnChance);
			
			cont=new GUIContent("Fail Modifier:", "A modifier to the spawn chance should a spawn attempt fail (to prevent the attempt fail too many time in a row). ie if modifier is set as 0.1, each fail attempt will increase the spawn chance by 10%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.failModifier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.failModifier);
			
			startY+=10;
				
			cont=new GUIContent("Spawn Item:", "The collectible item available to this spawner");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
			
			int enabledCount=0;
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				Collectible colt=collectibleDB.collectibleList[i];
				
			
				CollectibleSpawnInfo spawnInfo=null;
				
				bool enabled=false;
				for(int n=0; n<spawner.spawnItemList.Count; n++){
					enabled=spawner.spawnItemList[n].item==colt;
					if(enabled){
						spawnInfo=spawner.spawnItemList[n];
						break;
					}
				}
				bool enabledCached=enabled;
				
				float cachedX=startX;
				
				TDSEditorUtility.DrawSprite(new Rect(startX+10, startY+=spaceY, 30, 30), colt.icon, colt.desp);
				cont=new GUIContent(colt.collectibleName, colt.desp);	
				EditorGUI.LabelField(new Rect(startX+50, startY+=10, width, height), cont);
				enabled=EditorGUI.Toggle(new Rect(startX+=(width), startY, 20, height), enabled);
				
				if(spawnInfo!=null){
					cont=new GUIContent("Chance:", "Chance to spawn this item.\nThe way it works is a check against the individual item chance.\nA final candidate is then chosen over all item that pass the check.\nTakes value from 0-1 with 0.3 being 30% to pass the check");
					EditorGUI.LabelField(new Rect(startX+=35, startY, width, height), cont);
					spawnInfo.chance=EditorGUI.FloatField(new Rect(startX+55, startY, 40, height), spawnInfo.chance);
					
					cont=new GUIContent("Cooldown:", "The duration (in second) in which the item will be made unavailable after a successful spawn");
					EditorGUI.LabelField(new Rect(startX+=120, startY, width, height), cont);
					spawnInfo.cooldown=EditorGUI.FloatField(new Rect(startX+65, startY, 40, height), spawnInfo.cooldown);
				}
				
				if(enabled!=enabledCached){
					if(enabled){
						spawnInfo=new CollectibleSpawnInfo();
						spawnInfo.item=colt;
						spawner.spawnItemList.Insert(enabledCount, spawnInfo);
					}
					else spawner.spawnItemList.Remove(spawnInfo);
				}
				
				if(enabled) enabledCount+=1;
				
				startY+=4;
				
				startX=cachedX;
			}
			
			return new Vector2(startX, startY+(1.5f*spaceY));
		}
		
		
		
		
		
		
		
		
		
		void SelectItem(){ 
			Selection.activeGameObject=spawnerList[selectID].gameObject;
		}
	}
}
