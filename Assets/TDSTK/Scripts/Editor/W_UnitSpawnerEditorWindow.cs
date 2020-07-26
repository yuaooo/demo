using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class UnitSpawnerEditorWindow : TDSEditorWindow {
		
		private static UnitSpawnerEditorWindow window;
		
		public static void Init(GameObject sltObj=null) {
			// Get existing open window or if none, make a new one:
			window = (UnitSpawnerEditorWindow)EditorWindow.GetWindow(typeof (UnitSpawnerEditorWindow), false, "Unit Spawner Editor");
			window.minSize=new Vector2(400, 300);
			//window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
			
			GetAllSpawnerInScene();
			
			if(sltObj!=null){
				Selection.activeGameObject=sltObj;
				window.selectObj=null;
				window.OnSelectionChange();
			}
		}
		
		
		private static string[] spawnModeLabel;
		private static string[] spawnModeTooltip;
		
		private static string[] overrideModeLabel;
		private static string[] overrideModeTooltip;
		
		private static string[] limitModeLabel;
		private static string[] limitModeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_SpawnMode)).Length;
			spawnModeLabel=new string[enumLength];
			spawnModeTooltip=new string[enumLength];
			for(int n=0; n<enumLength; n++){
				spawnModeLabel[n]=((_SpawnMode)n).ToString();
				
				if((_SpawnMode)n==_SpawnMode.WaveBased) spawnModeTooltip[n]="The units will be spawned in waves";
				if((_SpawnMode)n==_SpawnMode.FreeForm) 	spawnModeTooltip[n]="The units will be spawned in continously";
			}
			
			
			enumLength = Enum.GetValues(typeof(_OverrideMode)).Length;
			overrideModeLabel=new string[enumLength];
			overrideModeTooltip=new string[enumLength];
			for(int n=0; n<enumLength; n++){
				overrideModeLabel[n]=((_OverrideMode)n).ToString();
				
				if((_OverrideMode)n==_OverrideMode.Replace) overrideModeTooltip[n]="Replace the default value";
				if((_OverrideMode)n==_OverrideMode.Addition) overrideModeTooltip[n]="Add to the default value";
				if((_OverrideMode)n==_OverrideMode.Multiply) overrideModeTooltip[n]="Multiply the default value";
			}
			
			
			enumLength = Enum.GetValues(typeof(_SpawnLimitType)).Length;
			limitModeLabel=new string[enumLength];
			limitModeTooltip=new string[enumLength];
			for(int n=0; n<enumLength; n++){
				limitModeLabel[n]=((_SpawnLimitType)n).ToString();
				
				if((_SpawnLimitType)n==_SpawnLimitType.Count) limitModeTooltip[n]="Spawning stops after a specific number of unit has been spawned";
				if((_SpawnLimitType)n==_SpawnLimitType.Timed) limitModeTooltip[n]="Spawning stops after the specified timer runs out";
				if((_SpawnLimitType)n==_SpawnLimitType.None) limitModeTooltip[n]="Infinite spawn";
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
		private UnitSpawner unitSpawner;
		void OnSelectionChange(){
			//check the current editing against the selected object in the hierarchy
			if(selectObj!=Selection.activeGameObject){
				selectObj=Selection.activeGameObject;
				UnitSpawner newSpawner=selectObj!=null ? selectObj.GetComponent<UnitSpawner>() : null;
				if(newSpawner!=unitSpawner){
					unitSpawner=newSpawner;
					
					selectID=-1;
					for(int i=0; i<spawnerList.Count; i++){
						if(unitSpawner==spawnerList[i]){
							selectID=i;
							break;
						}
					}
				}
			}
			
			editableCount = Selection.GetFiltered(typeof(UnitSpawner), SelectionMode.Editable).Length;
			Repaint();
		}
		
		
		
		private static List<UnitSpawner> spawnerList=new List<UnitSpawner>();
		public static void GetAllSpawnerInScene(){
			spawnerList=new List<UnitSpawner>();
			UnitSpawner[] spawners = FindObjectsOfType(typeof(UnitSpawner)) as UnitSpawner[];
			foreach (UnitSpawner spawner in spawners) spawnerList.Add(spawner);
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
			
				if(unitSpawner==null){
					EditorGUI.HelpBox(new Rect(startX, startY, width+2*spaceX, 50), "Selected object doesn't contain a UnitSpawner component", MessageType.Warning);
				}
				else{
					Undo.RecordObject (unitSpawner, "Spawner");
					
					if(editableCount>1){
						EditorGUI.HelpBox(new Rect(startX, startY, width+2*spaceX, 30), "More than 1 UnitSpawner is selected\nMulti-instance editing is not supported", MessageType.Warning);
						startY+=40;
					}
					
					v2=DrawSpawnerConfigurator(startX, startY, unitSpawner);
					contentWidth=spaceX+width;
					if(unitSpawner.spawnMode==_SpawnMode.WaveBased) contentWidth=700;
					contentHeight=v2.y-55;
				}
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTDS();
			
			return true;
		}
		
		
		protected Vector2 DrawSpawnerList(float startX, float startY, List<UnitSpawner> spawnerList){
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
		
		
		Vector2 DrawOverrideHitPoint(float startX, float startY, UnitSpawner spawner){
			cont=new GUIContent("Override HitPoint:", "Enable to override the default hit-point value specific on the unit prefab when spawning new unit");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.overrideHitPoint=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), spawner.overrideHitPoint);
			
			if(spawner.overrideHitPoint){
			
				int mode=(int)spawner.overrideHPMode;
				cont=new GUIContent(" - Override Mode:", "The manner in which the default hit-point value will be override");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				contL=new GUIContent[overrideModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(overrideModeLabel[i], overrideModeTooltip[i]);
				mode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), mode, contL);
				spawner.overrideHPMode=(_OverrideMode)mode;
				
				cont=new GUIContent(" - Starting Value:", "The starting overriding value when the spawn first started. The value can be set to increased gradually by adjusting the increment");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				spawner.startingHitPoint=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.startingHitPoint);
				
				if(spawner.spawnMode==_SpawnMode.FreeForm)
					cont=new GUIContent(" - Increment:", "How much the overriding value increase with each passing time step");
				else if(spawner.spawnMode==_SpawnMode.WaveBased)
					cont=new GUIContent(" - Increment:", "How much the overriding value increase with each successive wave");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				spawner.hitPointIncrement=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.hitPointIncrement);
				
				if(spawner.spawnMode==_SpawnMode.FreeForm){
					cont=new GUIContent(" - Time Step:", "Time (in second) for the overriding value to increase. The value increases by the value specified in increment with every time step");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.hitPointTimeStep=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.hitPointTimeStep);
				}
				
			}
			
			return new Vector2(startX, startY+spaceY);
		}
		
		
		Vector2 DrawSpawnUnitList(float startX, float startY, UnitSpawner spawner){
			for(int i=0; i<spawner.spawnUnitList.Count; i++){
				if(spawner.spawnUnitList[i]==null){
					spawner.spawnUnitList.RemoveAt(i); i-=1;
				}
			}
			
			cont=new GUIContent("Spawn Unit:", "The potential unit prefab to be spawned, check to enable");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
			
			int enabledCount=0;
			for(int i=0; i<unitAIDB.unitList.Count; i++){
				UnitAI unit=unitAIDB.unitList[i];
				
				bool enabled=spawner.spawnUnitList.Contains(unit);
				bool enabledCached=enabled;
				
				TDSEditorUtility.DrawSprite(new Rect(startX+10, startY+=spaceY, 30, 30), unit.icon, unit.desp);
				cont=new GUIContent(unit.unitName, unit.desp);
				EditorGUI.LabelField(new Rect(startX+50, startY+10, width, height), cont);
				enabled=EditorGUI.Toggle(new Rect(startX+spaceX+width-15, startY+10, width, height), enabled);
				startY+=14;
				
				if(enabled!=enabledCached){
					if(enabled) spawner.spawnUnitList.Insert(enabledCount, unit);
					else spawner.spawnUnitList.Remove(unit);
				}
				
				if(enabled) enabledCount+=1;
			}
			
			return new Vector2(startX, startY+spaceY);
		}
		
		
		Vector2 DrawSpawnerConfigurator(float startX, float startY, UnitSpawner spawner){
			
			int mode=(int)spawner.spawnMode;
			cont=new GUIContent("Spawn Mode:", "The spawn mode to use");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont, headerStyle);
			contL=new GUIContent[spawnModeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(spawnModeLabel[i], spawnModeTooltip[i]);
			mode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), mode, contL);
			spawner.spawnMode=(_SpawnMode)mode;
			
			
			if(spawner.spawnMode==_SpawnMode.WaveBased){
				cont=new GUIContent("- Endless", "Enable to activate endless mode (wave will be generated procedurally)");
				EditorGUI.LabelField(new Rect(startX+spaceX+width+25, startY, width, height), cont, headerStyle);
				spawner.endlessWave=EditorGUI.Toggle(new Rect(startX+spaceX+width+10, startY, 40, height), spawner.endlessWave);
			}
			
			
			startY+=10;
			
			
			cont=new GUIContent("Spawn Upon Start:", "Check to have the spawner start spawning automatically as soon as the game start");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.spawnUponStart=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), spawner.spawnUponStart);
			
			cont=new GUIContent("Start Delay:", "Delay (in second) before the spawning start");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//if(spawner.spawnUponStart)
				spawner.startDelay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.startDelay);
			//else
			//	EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
			
			startY+=10;
			
			cont=new GUIContent("Random Rotation:", "Check to have the unit spawned facing random rotation, otherwise the rotation of the spawn area will be used");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.randomRotation=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), spawner.randomRotation);
			
			startY+=10;
			
			
			for(int i=0; i<spawner.spawnAreaList.Count-1; i++){
				if(spawner.spawnAreaList[i]==null){ spawner.spawnAreaList.RemoveAt(i); i-=1; }
			}
			if(spawner.spawnAreaList.Count==0) spawner.spawnAreaList.Add(null);
			
			cont=new GUIContent("Spawn Area:", "The area which the unit should be spawn in\nIf unspecified, the unit will simply be spawned at the position of the spawner\nRequire game object with a TDSArea component");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spawner.spawnAreaList[0]=(TDSArea)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), spawner.spawnAreaList[0], typeof(TDSArea), true);
			
			if(spawner.spawnAreaList[0]==null){
				TDSArea existingArea=spawner.gameObject.GetComponent<TDSArea>();
				if(GUI.Button(new Rect(startX+spaceX+width+10, startY, 75, height), "Add Area")){
					if(existingArea!=null) spawner.spawnAreaList[0]=existingArea;
					else spawner.spawnAreaList[0]=spawner.gameObject.AddComponent<TDSArea>();
				}
			}
			
			//only show extra spawn area in free form or endless mode
			if(spawner.spawnMode==_SpawnMode.FreeForm || (spawner.spawnMode==_SpawnMode.WaveBased && spawner.endlessWave)){
				EditorGUI.HelpBox(new Rect(startX+spaceX+width+20, startY, 1.5f*width, (spawner.spawnAreaList.Count+1)*spaceY), "Alternate spawn-area will be randomly selected in freeform and endless mode", MessageType.None);
				
				for(int i=1; i<spawner.spawnAreaList.Count; i++){
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), " - Alt "+i+":");
					spawner.spawnAreaList[i]=(TDSArea)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), spawner.spawnAreaList[i], typeof(TDSArea), true);
					
					if(GUI.Button(new Rect(startX+spaceX+width+10, startY, 55, height), "remove")){
						spawner.spawnAreaList.RemoveAt(i);
						i-=1;
					}
				}
				
				if(GUI.Button(new Rect(startX+spaceX, startY+=spaceY, width/2, height), "Add")) spawner.spawnAreaList.Add(null);
				if(GUI.Button(new Rect(startX+spaceX+width/2, startY, width/2, height), "Remove")) spawner.spawnAreaList.RemoveAt(spawner.spawnAreaList.Count-1);
			}
			
			startY+=10;
			
			
			Vector2 v2=DrawOverrideHitPoint(startX, startY, spawner);
			startY=v2.y-10;
			
			
			if(spawner.spawnMode==_SpawnMode.FreeForm){
				
				int modeL=(int)spawner.limitType;
				cont=new GUIContent("Limit Type:", "Which mode to determine if the spawning is finished");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
				contL=new GUIContent[limitModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(limitModeLabel[i], limitModeTooltip[i]);
				modeL = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), modeL, contL);
				spawner.limitType=(_SpawnLimitType)modeL;
				
				if(spawner.limitType==_SpawnLimitType.Count){
					cont=new GUIContent(" - Count Limit:", "The maximum amount of unit to be spawned. The spawner will stop spawning after it has spawned this many unit.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.limitSpawnCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), spawner.limitSpawnCount);
				}
				else if(spawner.limitType==_SpawnLimitType.Timed){
					cont=new GUIContent(" - Time Limit:", "The time duration in second which the spawner will be spawning. The spawner will stop spawning when the time is due");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.limitSpawnTime=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), spawner.limitSpawnTime);
				}
				else if(spawner.limitType==_SpawnLimitType.None){
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "");
					EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "Infinite Spawn");
				}
				
				
				cont=new GUIContent("Spawn Cooldown:", "The cooldown between each unit spawn");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				spawner.spawnCD=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.spawnCD);
				
				cont=new GUIContent("Active Limit:", "The maximum amount of active spawned unit at any given time");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				spawner.activeLimit=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), spawner.activeLimit);
				
				
				startY+=15;
				
				
				v2=DrawSpawnUnitList(startX, startY, spawner);
				startY=v2.y;
				
			}
			else if(spawner.spawnMode==_SpawnMode.WaveBased){
				cont=new GUIContent("Waves Cooldown:", "cooldown between each wave");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				spawner.delayBetweenWave=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), spawner.delayBetweenWave);
				
				
				startY+=10;
				
				
				if(spawner.endlessWave){
					cont=new GUIContent("MaxSubWaveCount:", "Maximum subwave allow for each generated wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.maxSubWaveCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), spawner.maxSubWaveCount);
					
					startY+=5;
					float cachedX=startX;	float spX=70;
					
					cont=new GUIContent("Unit Count:", "starting unit count for each wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.unitCount=EditorGUI.IntField(new Rect(startX+spX, startY, 40, height), spawner.unitCount);
					
					cont=new GUIContent("Increment:", "increment in unit count for each subsequent wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.unitCountInc=EditorGUI.IntField(new Rect(startX+spX, startY, 40, height), spawner.unitCountInc);
					
					startY-=2*spaceY;	startX+=125;
					
					cont=new GUIContent("Credit:", "starting unit count for each wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.startingCredit=EditorGUI.IntField(new Rect(startX+spX, startY, 40, height), spawner.startingCredit);
					
					cont=new GUIContent("Increment:", "increment in unit count for each subsequent wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.creditIncrement=EditorGUI.IntField(new Rect(startX+spX, startY, 40, height), spawner.creditIncrement);
					
					startY-=2*spaceY;	startX+=125;
					
					cont=new GUIContent("Score:", "starting unit count for each wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.startingScore=EditorGUI.IntField(new Rect(startX+spX, startY, 40, height), spawner.startingScore);
					
					cont=new GUIContent("Increment:", "increment in unit count for each subsequent wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					spawner.scoreIncrement=EditorGUI.IntField(new Rect(startX+spX, startY, 40, height), spawner.scoreIncrement);
					
					
					startY+=15;
					startX=cachedX;
					
					v2=DrawSpawnUnitList(startX, startY, spawner);
					startY=v2.y;
				}
				else{
				
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Waves Count: "+spawner.waveList.Count, headerStyle);
					if(GUI.Button(new Rect(startX+spaceX, startY, 40, 16), "+")){
						spawner.waveList.Add(new Wave());
					}
					if(GUI.Button(new Rect(startX+spaceX+50, startY, 40, 16), "-")){
						if(spawner.waveList.Count>1) spawner.waveList.RemoveAt(spawner.waveList.Count-1);
					}
				
					for(int i=0; i<spawner.waveList.Count; i++){
						Wave wave=spawner.waveList[i];
						
						startY+=spaceY+10;
						
						GUI.Box(new Rect(startX+25, startY, 655, (2+wave.subWaveList.Count)*spaceY+15), "");
						
						startX+=5;
						startY+=5;
						
						EditorGUI.LabelField(new Rect(startX, startY, width, height), (i+1)+".");
						
						startX+=25;
						//cont=new GUIContent("Credit:", "The credit gained upon clearing the wave");
						//EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
						//wave.creditGain=EditorGUI.IntField(new Rect(startX+45, startY, 40, height), wave.creditGain);
						
						cont=new GUIContent("Score:", "The score gained upon clearing the wave");
						EditorGUI.LabelField(new Rect(startX+110, startY, width, height), cont);
						wave.scoreGain=EditorGUI.IntField(new Rect(startX+154, startY, 40, height), wave.scoreGain);
						
						
						GUI.color=new Color(1f, 0.7f, 0.7f, 1f);
						if(GUI.Button(new Rect(startX+585, startY, 55, 16), "remove")){
							spawner.waveList.RemoveAt(i);	i-=1;
							continue;
						}
						GUI.color=Color.white;
						
						startY+=5;
						
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Sub Waves:");
						if(GUI.Button(new Rect(startX+spaceX-20, startY, 40, 16), "+")){
							wave.subWaveList.Add(new SubWave()); 
						}
						if(GUI.Button(new Rect(startX+spaceX+30, startY, 40, 16), "-")){
							if(wave.subWaveList.Count>1) wave.subWaveList.RemoveAt(wave.subWaveList.Count-1);
						}
						
						for(int n=0; n<wave.subWaveList.Count; n++){
							SubWave subWave=wave.subWaveList[n];
							
							EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, 15), "-");
							
							int unitIdx=subWave.unitPrefab!=null ? TDSEditor.GetUnitAIIndex(subWave.unitPrefab.prefabID) : 0 ;
							unitIdx = EditorGUI.Popup(new Rect(startX+20, startY, width, 15), unitIdx, unitAILabel);
							if(unitIdx==0) subWave.unitPrefab=null;
							else if(unitIdx>0) subWave.unitPrefab=unitAIDB.unitList[unitIdx-1];
							
							float cachedX=startX;
							
							cont=new GUIContent("Count:", "The number of unit to be spawned");
							EditorGUI.LabelField(new Rect(startX+=180, startY, width, height), cont);
							subWave.count=EditorGUI.IntField(new Rect(startX+45, startY, 25, height), subWave.count);
							
							cont=new GUIContent("Delay:", "Delay (in second) before the spawning start");
							EditorGUI.LabelField(new Rect(startX+=90, startY, width, height), cont);
							subWave.startDelay=EditorGUI.FloatField(new Rect(startX+42, startY, 25, height), subWave.startDelay);
							
							cont=new GUIContent("Interval:", "The spawn interval (in second) between each unit");
							EditorGUI.LabelField(new Rect(startX+=80, startY, width, height), cont);
							subWave.interval=EditorGUI.FloatField(new Rect(startX+52, startY, 25, height), subWave.interval);
							
							cont=new GUIContent("Alt-Area:", "The area which the unit of this wave should be spawned, replacing the default area (optional)");
							EditorGUI.LabelField(new Rect(startX+=90, startY, width, height), cont);
							wave.spawnArea=(TDSArea)EditorGUI.ObjectField(new Rect(startX+60, startY, 100, height), wave.spawnArea, typeof(TDSArea), true);
				
							
							if(wave.subWaveList.Count>1){
								if(GUI.Button(new Rect(startX+180, startY, 20, 16), "X")){
									wave.subWaveList.RemoveAt(n);	n-=1;
								}
							}
							
							startX=cachedX;
						}
						
						startX-=5;
						startY+=5;
						
						startX-=25;
					}
				}
			}
			
			return new Vector2(startX, startY+(2*spaceY));
		}
		
		
		
		
		void SelectItem(){ 
			Selection.activeGameObject=spawnerList[selectID].gameObject;
		}
	}
}
