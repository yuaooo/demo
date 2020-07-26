using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDSTK;
using TDSTK_UI;

namespace TDSTK{
	
	[CustomEditor(typeof(UILevelPerkMenu))]
	//[CanEditMultipleObjects]
	public class UILevelPerkMenuEditor : TDSEditorInspector {
	
		private static UILevelPerkMenu instance;
		
		
		void Awake(){
			instance = (UILevelPerkMenu)target;
			
			LoadDB();
			
			InitLabel();
		}
		
		private static string[] perkTypeLabel;
		private static string[] perkTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(UILevelPerkMenu._PerkTabType)).Length;
			perkTypeLabel=new string[enumLength];
			perkTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				perkTypeLabel[i]=((UILevelPerkMenu._PerkTabType)i).ToString();
				if((UILevelPerkMenu._PerkTabType)i==UILevelPerkMenu._PerkTabType.LevelList) 
					perkTypeTooltip[i]="A simple non-interactable list showing the perk to be unlocked at specific level";
				if((UILevelPerkMenu._PerkTabType)i==UILevelPerkMenu._PerkTabType.RepeatableList) 
					perkTypeTooltip[i]="RepeatableList - a list type perk menu designed for attribute style perk";
				if((UILevelPerkMenu._PerkTabType)i==UILevelPerkMenu._PerkTabType.Item) 
					perkTypeTooltip[i]="A full perk menu, with option to configure custom tech or skill-tree";
				if((UILevelPerkMenu._PerkTabType)i==UILevelPerkMenu._PerkTabType.None) 
					perkTypeTooltip[i]="Don't how any perk menu";
			}
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			Undo.RecordObject (instance, "UILevelPerkMenu");
			
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Dont Hide:", "Check if this menu is the only menu in the scene (being used as inter-level upgrade screen for instance)");
				instance.LevelPerkMenuOnly=EditorGUILayout.Toggle(cont, instance.LevelPerkMenuOnly);
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Enable Stats Tab:", "Check to enable the level and stats panel");
				instance.enableStatsTab=EditorGUILayout.Toggle(cont, instance.enableStatsTab);
			
			EditorGUILayout.Space();
			
				int perkTabType=(int)instance.perkTabType;
				cont=new GUIContent("Perk Menu Type:", "The type of perk menu to use");
				contL=new GUIContent[perkTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(perkTypeLabel[i], perkTypeTooltip[i]);
				perkTabType = EditorGUILayout.Popup(cont, perkTabType, contL);
				instance.perkTabType=(UILevelPerkMenu._PerkTabType)perkTabType;
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}