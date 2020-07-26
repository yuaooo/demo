using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(CameraControl))]
	public class CameraControlEditor : TDSEditorInspector {
	
		private static CameraControl instance;
		void Awake(){ 
			instance = (CameraControl)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "CameraControl");
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Track Speed:", "How responsive is the tracking to the player movement");
				instance.trackSpeed=EditorGUILayout.FloatField(cont, instance.trackSpeed);
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Position Offset:", "the position offset of the camera pivot point to the actual player position");
				instance.posOffset=EditorGUILayout.Vector3Field(cont, instance.posOffset);
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Shake Multiplier:", "The multiplier to the camera shake magnitude for various impact");
				instance.shakeMultiplier=EditorGUILayout.FloatField(cont, instance.shakeMultiplier);
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Dynamic Zoom:", "Enable the camera to zoom based on player's speed");
				instance.enableDynamicZoom=EditorGUILayout.Toggle(cont, instance.enableDynamicZoom);
				
				cont=new GUIContent(" - Normalize Factor:", "A value used to normalize the zooming range to player speed. Recommend to keep this value in porpotion to unit acceleration");
				if(instance.enableDynamicZoom) instance.zoomNormalizeFactor=EditorGUILayout.FloatField(cont, instance.zoomNormalizeFactor);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
				
				cont=new GUIContent(" - Zoom Speed:", "The speed in which the zoom level respond to player change of speed");
				if(instance.enableDynamicZoom) instance.zoomSpeed=EditorGUILayout.FloatField(cont, instance.zoomSpeed);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Limit:", "Limit the camera to a set area");
				instance.enableLimit=EditorGUILayout.Toggle(cont, instance.enableLimit);
				
				EditorGUILayout.BeginHorizontal();
				cont=new GUIContent(" - X-axis(min/max):", "The minimum and maximum limit of the pivot position in x-axis");
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(116));
				if(instance.enableLimit){
					instance.minPosX=EditorGUILayout.FloatField(instance.minPosX);
					instance.maxPosX=EditorGUILayout.FloatField(instance.maxPosX);
				}
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				cont=new GUIContent(" - Z-axis(min/max):", "The minimum and maximum limit of the pivot position in z-axis");
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(116));
				if(instance.enableLimit){
					instance.minPosZ=EditorGUILayout.FloatField(instance.minPosZ);
					instance.maxPosZ=EditorGUILayout.FloatField(instance.maxPosZ);
				}
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}