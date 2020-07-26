using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(PlayerPerk))]
	[CanEditMultipleObjects]
	public class PlayerPerkEditor : TDSEditorInspector {
	
		private static PlayerPerk instance;
		
		private static bool showPerkList=true;
		
		
		void Awake(){
			instance = (PlayerPerk)target;
			LoadDB();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			serializedObject.Update();
			
			
			EditorGUILayout.Space();
			cont=new GUIContent("Perk Currency:", "The amount of perk currency pocessed by the player unit.\nUsed to purchase perk");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("perkCurrency"), cont);
			EditorGUILayout.Space();
			
			
			showPerkList=EditorGUILayout.Foldout(showPerkList, "Show Valid Perk List");
			if(showPerkList){
					
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("EnableAll")){
					EnableAllPerkOnAll();
				}
				if(GUILayout.Button("DisableAll")){
					DisableAllPerkOnAll();
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
							
								EditorGUI.showMixedValue=!UnavailableListSharesValue(perk.ID);
							
								EditorGUI.BeginChangeCheck();
								bool flag=!instance.unavailableIDList.Contains(perk.ID) ? true : false;
								EditorGUILayout.LabelField(new GUIContent(" - enabled: ", "check to enable the perk in this level"), GUILayout.Width(70));
								flag=EditorGUILayout.Toggle(flag);
					
								if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
									if(!flag) RemoveIDFromAvaiList(perk.ID);
									else AddIDToAvaiList(perk.ID);
								}
								
								EditorGUI.showMixedValue=false;
								
								
								EditorGUI.showMixedValue=!PurchasedListSharesValue(perk.ID);
								
								if(!instance.unavailableIDList.Contains(perk.ID)){
									flag=instance.purchasedIDList.Contains(perk.ID);
									EditorGUILayout.LabelField(new GUIContent(" - purchased: ", "check to set the perk as purchased right from the start"), GUILayout.Width(80));
									flag=EditorGUILayout.Toggle(flag);
									
									if(!Application.isPlaying){
										if(!flag) RemoveIDFromPurchasedList(perk.ID);
										else AddIDToPurchasedList(perk.ID);
									}
								}
								
								EditorGUI.showMixedValue=false;
								
							GUILayout.EndHorizontal();
							
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
				}
					
			}
			
			EditorGUILayout.Space();
			
			serializedObject.ApplyModifiedProperties();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		
		
		private bool UnavailableListSharesValue(int ID){
			if(!serializedObject.isEditingMultipleObjects) return true;
			
			PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[0];
			bool flag=pPerkInstance.unavailableIDList.Contains(ID);
			
			for(int i=1; i<serializedObject.targetObjects.Length; i++){
				pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				if(flag!=pPerkInstance.unavailableIDList.Contains(ID)) return false;
			}
			
			return true;
		}
		
		private bool PurchasedListSharesValue(int ID){
			if(!serializedObject.isEditingMultipleObjects) return true;
			
			PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[0];
			bool flag=pPerkInstance.purchasedIDList.Contains(ID);
			
			for(int i=1; i<serializedObject.targetObjects.Length; i++){
				pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				if(flag!=pPerkInstance.purchasedIDList.Contains(ID)) return false;
			}
			
			return true;
		}
		
		
		public void RemoveIDFromAvaiList(int ID){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				if(!pPerkInstance.unavailableIDList.Contains(ID)) pPerkInstance.unavailableIDList.Add(ID);
			}
		}
		public void AddIDToAvaiList(int ID){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				pPerkInstance.unavailableIDList.Remove(ID);
			}
		}
		
		
		public void RemoveIDFromPurchasedList(int ID){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				pPerkInstance.purchasedIDList.Remove(ID);
			}
		}
		public void AddIDToPurchasedList(int ID){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				if(!pPerkInstance.purchasedIDList.Contains(ID)) pPerkInstance.purchasedIDList.Add(ID);
			}
		}
		
		
		
		
		public void EnableAllPerkOnAll(){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				pPerkInstance.unavailableIDList=new List<int>();
			}
		}
		public void DisableAllPerkOnAll(){
			List<int> allIDList=new List<int>();
			for(int i=0; i<perkDB.perkList.Count; i++) allIDList.Add(perkDB.perkList[i].ID);
			
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlayerPerk pPerkInstance=(PlayerPerk)serializedObject.targetObjects[i];
				pPerkInstance.unavailableIDList=new List<int>( allIDList );
			}
		}
		
		
	}

}