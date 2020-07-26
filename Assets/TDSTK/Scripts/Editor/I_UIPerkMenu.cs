using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDSTK;
using TDSTK_UI;

namespace TDSTK{
	
	[CustomEditor(typeof(UIPerkTab))]
	//[CanEditMultipleObjects]
	public class UIPerkTabEditor : TDSEditorInspector {
	
		private static UIPerkTab instance;
		
		private static bool showPerkList=true;
		
		
		void Awake(){
			instance = (UIPerkTab)target;
			LoadDB();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			Undo.RecordObject (instance, "UIPerkTab");
			
			//serializedObject.Update();
			
			EditorGUILayout.Space();
			
			cont=new GUIContent("Manual Setup:", "Check to manually setup the button item in the menu");
			instance.manualSetup=EditorGUILayout.Toggle(cont, instance.manualSetup);
			
			if(!instance.manualSetup){
				
				showPerkList=EditorGUILayout.Foldout(showPerkList, "Show Perk List");
				if(showPerkList){
					
					EditorGUILayout.HelpBox("Choose perk that will shows up in the UI", MessageType.Info);
					
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button("EnableAll")){
						EnableAllPerk();
					}
					if(GUILayout.Button("DisableAll")){
						DisableAllPerk();
					}
					EditorGUILayout.EndHorizontal ();
					
					for(int i=0; i<perkDB.perkList.Count; i++){
						Perk perk=perkDB.perkList[i];
						
						GUILayout.BeginHorizontal();
							
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							Rect rect=GUILayoutUtility.GetLastRect();
							TDSEditorUtility.DrawSprite(rect, perk.icon, perk.desp, false);
							
							GUILayout.BeginVertical();
								EditorGUILayout.Space();
								GUILayout.Label(perk.name, GUILayout.ExpandWidth(false));
						
								GUILayout.BeginHorizontal();
									
									EditorGUI.BeginChangeCheck();
									bool flag=!instance.unAvaiPerkIDList.Contains(perk.ID) ? true : false;
									EditorGUILayout.LabelField(new GUIContent(" - enabled: ", "check to enable the perk in this level"), GUILayout.Width(70));
									flag=EditorGUILayout.Toggle(flag);
						
									if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
										if(!flag && !instance.unAvaiPerkIDList.Contains(perk.ID))
											instance.unAvaiPerkIDList.Add(perk.ID);
										else if(flag) instance.unAvaiPerkIDList.Remove(perk.ID);
									}
									
								GUILayout.EndHorizontal();
								
							GUILayout.EndVertical();
						
						GUILayout.EndHorizontal();
					}
						
				}
				
				EditorGUILayout.Space();
				EditorGUILayout.Space();
			}
			
			
			
			//serializedObject.ApplyModifiedProperties();
			
			//DefaultInspector();
			
			DrawDefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		
		
		
		
		
		public void EnableAllPerk(){
			instance.unAvaiPerkIDList=new List<int>();
		}
		public void DisableAllPerk(){
			instance.unAvaiPerkIDList=new List<int>();
			for(int i=0; i<perkDB.perkList.Count; i++) instance.unAvaiPerkIDList.Add(perkDB.perkList[i].ID);
		}
		
		
	}

}