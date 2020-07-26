using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class ProgressStatsEditorWindow : TDSEditorWindow {
		
		private static ProgressStatsEditorWindow window;
		
		private bool sumRecursively=true;
		private float expTHM=1.5f;
		private int expTHC=10;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (ProgressStatsEditorWindow)EditorWindow.GetWindow(typeof (ProgressStatsEditorWindow), false, "Progress Stats Editor");
			window.minSize=new Vector2(400, 300);
			//~ window.maxSize=new Vector2(375, 800);
			
			LoadDB();
			
			window.sumRecursively=progressDB.stats.sumRecursively;
			window.expTHM=progressDB.stats.expTHM;
			window.expTHC=progressDB.stats.expTHC;
		}
		
		
		
		
		private PlayerProgression selectedComponent;
		void OnEnable(){
			OnSelectionChange();
		}
		void OnSelectionChange(){
			if(window==null) return;
			
			if(Selection.activeGameObject!=null)
				selectedComponent=Selection.activeGameObject.GetComponent<PlayerProgression>();
			else selectedComponent=null;
			
			Repaint();
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			if(Selection.activeGameObject!=null)
				selectedComponent=Selection.activeGameObject.GetComponent<PlayerProgression>();
			else selectedComponent=null;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(progressDB, "ProgressDB");
			if(selectedComponent!=null) Undo.RecordObject(selectedComponent, "PlayerProgression");
			
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTDS();
			
			LevelProgressionStats stats=selectedComponent!=null ? selectedComponent.stats : progressDB.stats;
			
			
			
			float startX=5;	float startY=5;	spaceX+=25;
			
			if(selectedComponent!=null) EditorGUI.HelpBox(new Rect(startX, startY, 250, 25), "Editing selected component", MessageType.Info);
			else EditorGUI.HelpBox(new Rect(startX, startY, 250, 25), "Editing Global Setting (DB)", MessageType.Info);
			startY+=35;
			
			cont=new GUIContent("Level Cap:", "");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont, headerStyle);
			stats.levelCap=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), stats.levelCap);
			
			if(stats.levelCap!=stats.expThresholdList.Count){
				EditorGUI.HelpBox(new Rect(startX+spaceX+widthS+20, startY, 250, 40), "Experience list doesn't match level cap.\nPlease Regenerate Experience List", MessageType.Warning);
				//EditorGUI.HelpBox(new Rect(startX, startY+spaceY, 250, 40), "Experience list doesn't match level cap.\nPlease Regenerate Experience List", MessageType.Warning);
				//startY+=2*spaceY;
			}
			
			startY=DrawPerLevelGain(startX, startY+spaceY+15, stats);
			
			spaceX-=25;
			
			DrawAddPerkGained(startX+spaceX+width-80, startY+spaceY, stats);
			
			startY=DrawExpListGenerator(startX, startY+spaceY, stats);
			
			
			startY+=2*spaceY;
			
			visibleRectList=new Rect(startX, startY, window.position.width-10, window.position.height-startY-5);
			contentRectList=new Rect(startX, startY, window.position.width-25, contentLength);
			
			GUI.color=new Color(.8f, .8f, .8f, .8f);
			GUI.Box(visibleRectList, "");
			GUI.color=Color.white;
			
			scrollPosList = GUI.BeginScrollView(visibleRectList, scrollPosList, contentRectList);
				
				float cachedY=startY;
				startY=DrawLevelList(startX, startY+5, stats);
				contentLength=startY-cachedY;
			
			GUI.EndScrollView();
			
			if(selectedComponent!=null) PrefabUtility.RecordPrefabInstancePropertyModifications(selectedComponent);
			
			if(GUI.changed){
				if(selectedComponent!=null) EditorUtility.SetDirty(selectedComponent);
				SetDirtyTDS();
			}
			
			return true;
		}
		
		private float contentLength=0;
		
		
		
		
		
		private int perkGainLvl=2;
		private int perkGainID=-1;
		private float DrawAddPerkGained(float startX, float startY, LevelProgressionStats stats){
			spaceX-=65;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+30, height), "Add Perk Gained At lvl:", headerStyle);
			
			cont=new GUIContent(" - Level:", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perkGainLvl=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perkGainLvl);
			perkGainLvl=Mathf.Clamp(perkGainLvl, 1, stats.levelCap);
			
			
			int perkIdx=perkGainID>=0 ? TDSEditor.GetPerkIndex(perkGainID) : 0 ;
			
			cont=new GUIContent(" - Perk:", "Perk to be gained");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			
			perkIdx=EditorGUI.Popup(new Rect(startX+spaceX, startY, widthS+65, height), perkIdx, perkLabel);
			if(perkIdx>0) perkGainID=perkDB.perkList[perkIdx-1].ID;
			else perkGainID=-1;
			
			spaceX+=65;
			
			
			if(perkGainID>=0 && GUI.Button(new Rect(startX+15, startY+=spaceY+2, spaceX+widthS-15, height+2), "Add Entry")){
				PerkUnlockingAtLevel item=null;
				int index=0;
				
				for(int i=0; i<stats.perkUnlockingAtLevelList.Count; i++){
					if(stats.perkUnlockingAtLevelList[i].level==perkGainLvl){
						item=stats.perkUnlockingAtLevelList[i];
						break;
					}
					
					if(stats.perkUnlockingAtLevelList[i].level>perkGainLvl) index+=1;
				}
				
				if(item!=null){
					if(!item.perkIDList.Contains(perkGainID))	item.perkIDList.Add(perkGainID);
				}
				else{
					item=new PerkUnlockingAtLevel(perkGainLvl);
					item.perkIDList.Add(perkGainID);
					
					stats.perkUnlockingAtLevelList.Insert(index, item);
				}
				
				perkGainID=-1;
				
				stats.RearrangePerkUnlockingList();
			}
			
			return startY;
		}
		
		
		private float DrawExpListGenerator(float startX, float startY, LevelProgressionStats stats){
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Generate Exp List:", headerStyle);
			
			cont=new GUIContent(" - Sum Recursively:", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			sumRecursively=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), sumRecursively);
			
			cont=new GUIContent(" - Increment Rate:", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			expTHM=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), expTHM);
			
			cont=new GUIContent(" - Starting Value:", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			expTHC=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), expTHC);
			
			if(stats.expThresholdList.Count!=stats.levelCap) GUI.color=new Color(0, 1f, 1f, 1f);
			
			if(GUI.Button(new Rect(startX+15, startY+=spaceY+2, spaceX+widthS-15, height+2), "Generate Exp")){
				stats.expThresholdList=PlayerProgression.GenerateExpTH(sumRecursively, expTHM, expTHC, stats.levelCap);
				
				stats.sumRecursively=sumRecursively;
				stats.expTHM=expTHM;
				stats.expTHC=expTHC;
			}
			
			//public float expTHM=1.5f;
			//public int expTHC=10;
			
			string textRS=stats.sumRecursively ? "Σ(" : "";
			string textRE=stats.sumRecursively ? ")" : "";
			cont=new GUIContent("exp = "+textRS+"("+stats.expTHM+"*lvl)+"+stats.expTHC+textRE, "The formula used to generate current exp list");
			//cont=new GUIContent(" - Starting Value:", "");
			EditorGUI.LabelField(new Rect(startX+spaceX+width-80, startY, width*2, height), cont);
			
			GUI.color=Color.white;
			
			return startY;
		}
		
		
		
		private float DrawLevelList(float startX, float startY, LevelProgressionStats stats){
			
			startX+=60;
			
			EditorGUI.LabelField(new Rect(startX, startY, width, height), "Exp to level:", headerStyle);
			EditorGUI.LabelField(new Rect(startX+width-15, startY, width, height), "Perk Gained:", headerStyle);
			
			startX+=5;
			
			//string textRS=stats.sumRecursively ? "Σ(" : "";
			//string textRE=stats.sumRecursively ? ")" : "";
			
			//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+10, height), "exp = "+textRS+"("+stats.expTHM+"*lvl)+"+stats.expTHC+textRE, headerStyle);
			
				for(int i=0; i<stats.expThresholdList.Count; i++){
					float cachedX=startX;
					
					cont=new GUIContent(" - lvl "+(i+1)+":", "");
					EditorGUI.LabelField(new Rect(startX-60, startY+=spaceY, width, height), cont);
					
					if(i==0) EditorGUI.LabelField(new Rect(startX, startY, widthS+spaceX-85, height), "-");
					else stats.expThresholdList[i]=EditorGUI.IntField(new Rect(startX, startY, widthS+spaceX-85, height), stats.expThresholdList[i]);
					
					int index=-1;
					for(int n=0; n<stats.perkUnlockingAtLevelList.Count; n++){
						if(stats.perkUnlockingAtLevelList[n].level==i+1){ index=n;	break; }
					}
					
					if(index>=0){
						float cachedX2=startX+=width-20;
						
						PerkUnlockingAtLevel item=stats.perkUnlockingAtLevelList[index];
						for(int n=0; n<item.perkIDList.Count; n++){
							int perkIdx=TDSEditor.GetPerkIndex(item.perkIDList[n]);
							
							if(perkIdx<=0){ item.perkIDList.RemoveAt(n); n-=1; }
							else{
								cont=new GUIContent(perkDB.perkList[perkIdx-1].name, perkDB.perkList[perkIdx-1].desp);
								EditorGUI.LabelField(new Rect(startX, startY, width*.75f, height), cont);
								if(GUI.Button(new Rect(startX-height-5, startY, height, height), "-")){ item.perkIDList.RemoveAt(n); n-=1; }
							}
							
							startX+=width*.75f;
						}
						
						startX=cachedX2;
						
						if(item.perkIDList.Count==0){
							stats.perkUnlockingAtLevelList.RemoveAt(index);
							stats.RearrangePerkUnlockingList();
						}
					}
					
					startX=cachedX;
				}
			
			return startY+spaceY+5;
		}
		
		
		
		
		private bool foldPerLevelGain=true;
		protected float DrawPerLevelGain(float startX, float startY, LevelProgressionStats stats){
			string text="Stats Gain Per Level "+(!foldPerLevelGain ? "(show):" : "(hide):");
			foldPerLevelGain=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldPerLevelGain, text, foldoutStyle);
			if(foldPerLevelGain){
			
					cont=new GUIContent(" - HitPoint:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.hitPointGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.hitPointGain);
					
					cont=new GUIContent(" - HitPoint Regen:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.hitPointRegenGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.hitPointRegenGain);
					
				startY+=5;
				
					cont=new GUIContent(" - Energy:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.energyGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.energyGain);
					
					cont=new GUIContent(" - Energy Regen:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.energyRegenGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.energyRegenGain);
					
				startY+=5;
				
					cont=new GUIContent(" - Speed Multiplier:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.speedMulGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.speedMulGain);
					
				startY+=5;
				
					cont=new GUIContent(" - Damage Multiplier:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.dmgMulGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.dmgMulGain);
					
					cont=new GUIContent(" - Critical Chance Mul.:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.critChanceMulGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.critChanceMulGain);
					
					cont=new GUIContent(" - Critical Multiplier Mul.:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.critMultiplierMulGain=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.critMultiplierMulGain);
				
				startY+=10;
				
					cont=new GUIContent(" - Perk Currency:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.perkCurrencyGain=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), stats.perkCurrencyGain);
				
			}
			
			return startY;
		}
		
		
	}
}
