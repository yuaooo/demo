using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(Unit))]
	[CanEditMultipleObjects]
	public class UnitEditor : TDSEditorInspector {
	
		private static Unit instance;
		void Awake(){
			instance = (Unit)target;
			LoadDB();
		}
		
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "Unit");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("This is the base class for AI and player unit which doesn't do anything in game.\n\nUse this only if the object is a static destructible in the game, otherwise use UnitAI", MessageType.Info);
			
			EditorGUILayout.Space();
			
			DrawUnitSetting();
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		protected void DrawUnitSetting(){
			
			EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("Hit Point:", "The hit-point capacity of the unit");
				srlPpt=serializedObject.FindProperty("hitPointFull");
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
				float hitPointFull=EditorGUILayout.FloatField(srlPpt.floatValue, GUILayout.MaxWidth(fieldWidth));
				if(EditorGUI.EndChangeCheck()) srlPpt.floatValue=hitPointFull;
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("anchorDown");
			cont=new GUIContent("Anchored To Point:", "Check if the unit is a static object. Means it will not moved and wont be affected by physics");
			PropertyField(srlPpt, cont);
			
			
			EditorGUILayout.Space();
			
			
			DrawDestroyValue();
			
			
			EditorGUILayout.Space();
			
			DrawDestroyEffect();
			
			
			EditorGUILayout.Space();
			
			DrawDropSetting();
			
			
			EditorGUILayout.Space();
			
			
			DrawSpawnUponDestroy();
			
		}
		
	}

}