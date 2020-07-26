using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDSTK;
using TDSTK_UI;

namespace TDSTK{
	
	[CustomEditor(typeof(UIPerkTabList))]
	//[CanEditMultipleObjects]
	public class UIPerkListEditor : TDSEditorInspector {
	
		private static UIPerkTabList instance;
		
		private static bool showPerkList=true;
		
		
		void Awake(){
			instance = (UIPerkTabList)target;
			LoadDB();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			Undo.RecordObject (instance, "UIPerkTabList");
			
			//serializedObject.Update();
			
			
			//~ cont=new GUIContent("Manual Setup:", "");
			//~ instance.manuallySetupItem=EditorGUILayout.Toggle(cont, instance.manuallySetupItem);
			
			EditorGUILayout.Space();
			
			//if(!instance.manuallySetup){
				
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
									bool flag=instance.perkIDList.Contains(perk.ID) ? true : false;
									EditorGUILayout.LabelField(new GUIContent(" - enabled: ", "check to enable the perk in this level"), GUILayout.Width(70));
									flag=EditorGUILayout.Toggle(flag);
						
									if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
										if(!flag && !instance.perkIDList.Contains(perk.ID))
											instance.perkIDList.Remove(perk.ID);
										else if(flag) instance.perkIDList.Add(perk.ID);
									}
									
								GUILayout.EndHorizontal();
								
							GUILayout.EndVertical();
						
						GUILayout.EndHorizontal();
					}
						
				}
				
				EditorGUILayout.Space();
				EditorGUILayout.Space();
			//}
			
			
			
			//serializedObject.ApplyModifiedProperties();
			
			//DefaultInspector();
			
			DrawDefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		
		
		
		
		
		public void EnableAllPerk(){
			instance.perkIDList=new List<int>();
			for(int i=0; i<perkDB.perkList.Count; i++) instance.perkIDList.Add(perkDB.perkList[i].ID);
		}
		public void DisableAllPerk(){
			instance.perkIDList=new List<int>();
		}
		
		
	}

}