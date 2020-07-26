using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(ShootObject))]
	[CanEditMultipleObjects]
	public class ShootObjectEditor : TDSEditorInspector {
		
		private string[] typeLabel;
		private string[] typeTooltip;
		
		private string[] splitLabel;
		private string[] splitTooltip;
		
		private static ShootObject instance;
		void Awake(){ 
			instance = (ShootObject)target;
			LoadDB();
			
			InitLabel();
		}
		
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_SOType)).Length;
			typeLabel=new string[enumLength];
			typeTooltip=new string[enumLength];
			for(int n=0; n<enumLength; n++){
				typeLabel[n]=((_SOType)n).ToString();
				
				if((_SOType)n==_SOType.Simple)  typeTooltip[n]="A simple project which travels in a straight line";
				if((_SOType)n==_SOType.Homing) typeTooltip[n]="A homing project which will swerve towards the a target position";
				if((_SOType)n==_SOType.Beam) 	 typeTooltip[n]="A heat scan shoot object which instantly hit the object in it's path";
			}
			
			enumLength = Enum.GetValues(typeof(ShootObject._SplitMode)).Length;
			splitLabel=new string[enumLength];
			splitTooltip=new string[enumLength];
			for(int n=0; n<enumLength; n++){
				splitLabel[n]=((ShootObject._SplitMode)n).ToString();
				
				if((ShootObject._SplitMode)n==ShootObject._SplitMode.Aim) 	splitTooltip[n]="The split shoot object aims for random target";
				if((ShootObject._SplitMode)n==ShootObject._SplitMode.AOE) 	splitTooltip[n]="The split shoot object just spread even in all direction";
			}
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("type");
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			
			EditorGUI.BeginChangeCheck();
			
			cont=new GUIContent("Type:", "Type of the shoot object");
			contL=new GUIContent[typeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(typeLabel[i], typeTooltip[i]);
			int type = EditorGUILayout.Popup(cont, srlPpt.enumValueIndex, contL);
			
			if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=type;
			EditorGUI.showMixedValue=false;
			
			
			EditorGUILayout.Space();
			
			
			if(srlPpt.hasMultipleDifferentValues){
				EditorGUILayout.HelpBox("Editing of type specify attribute is unavailble when selecting multiple shoot object of different type", MessageType.Warning);
			}
			else if(!srlPpt.hasMultipleDifferentValues){
				if(srlPpt.enumValueIndex==(int)_SOType.Simple || srlPpt.enumValueIndex==(int)_SOType.Homing){
					if(srlPpt.enumValueIndex==(int)_SOType.Simple) EditorGUILayout.LabelField("Simple Setting", headerStyle);
					if(srlPpt.enumValueIndex==(int)_SOType.Homing) EditorGUILayout.LabelField("Homing Setting", headerStyle);
					
					cont=new GUIContent("Speed:", "The travelling speed of the shoot object");
					SerializedProperty speedP=serializedObject.FindProperty("speed");
					EditorGUI.showMixedValue=speedP.hasMultipleDifferentValues;
					EditorGUI.BeginChangeCheck();
					float speed=EditorGUILayout.FloatField(cont, speedP.floatValue);
					if(EditorGUI.EndChangeCheck()) speedP.floatValue=speed;
					
					EditorGUILayout.Space();
					
					if(srlPpt.enumValueIndex==(int)_SOType.Simple){
						cont=new GUIContent("Split Upon Hit:", "When checked, the shoot object will split into more shoot object upon hitting a target");
						SerializedProperty splitP=serializedObject.FindProperty("splitUponHit");
						EditorGUI.showMixedValue=splitP.hasMultipleDifferentValues;
						EditorGUI.BeginChangeCheck();
						bool splitUponHit=EditorGUILayout.Toggle(cont, splitP.boolValue);
						if(EditorGUI.EndChangeCheck()) splitP.boolValue=splitUponHit;
						
						cont=new GUIContent("   - Split Count:", "The number of of split shoot object generated");
						if(splitP.boolValue){
							EditorGUILayout.PropertyField(serializedObject.FindProperty("splitCount"), cont);
							serializedObject.FindProperty("splitCount").intValue=Mathf.Max(1, serializedObject.FindProperty("splitCount").intValue);
						}
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent("   - Split Range:", "The range limit of the split shoot object");
						if(splitP.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("splitRange"), cont);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent("   - Split Type:", "The type of the split (aim or random)");
						if(splitP.boolValue){
							contL=new GUIContent[splitLabel.Length];
							for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(splitLabel[i], splitTooltip[i]);
							
							SerializedProperty splitRP=serializedObject.FindProperty("splitMode");
							EditorGUI.showMixedValue=splitRP.hasMultipleDifferentValues;
							
							EditorGUI.BeginChangeCheck();
							int spMode = EditorGUILayout.Popup(cont, splitRP.enumValueIndex, contL);
							if(EditorGUI.EndChangeCheck()) splitRP.enumValueIndex=spMode;
						}
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						EditorGUILayout.Space();
						
						cont=new GUIContent("Shoot Through All:", "Check if the shoot object penetrate any target it hits and carry on");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("shootThrough"), cont);
					
					}
					else if(srlPpt.enumValueIndex==(int)_SOType.Homing){
						cont=new GUIContent("Spread:", "The deviation of the hit point from actual target position, making the aim less accurate or more spread out when multiple homing shoot object is fired together");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("spread"), cont);
						
						cont=new GUIContent("Tracking Duration:", "The duration in second in which the shoot object will track and homing on the target. This is to give the target a change to evade and out run the attack");
						SerializedProperty trackSpdT=serializedObject.FindProperty("trackingDuration");
						EditorGUI.showMixedValue=trackSpdT.hasMultipleDifferentValues;
						EditorGUI.BeginChangeCheck();
						float trackSpd=EditorGUILayout.FloatField(cont, trackSpdT.floatValue);
						if(EditorGUI.EndChangeCheck()) trackSpdT.floatValue=trackSpd;
					}
					
					EditorGUILayout.Space();
					
					cont=new GUIContent("Destroy Delay:", "The delay in second before the game object is destroyed upon hit. This is to enabled the trail-renderer/particle-effect to clear naturally");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileDestroyDelay"), cont);
					
					cont=new GUIContent("Deactivate Object:", "The object to deactivate upon hit (so it's not visible while the trail-renderer/particle-effect clear up)");
					if(serializedObject.FindProperty("projectileDestroyDelay").floatValue>0)
						EditorGUILayout.PropertyField(serializedObject.FindProperty("hideObject"), cont);
					else EditorGUILayout.LabelField(cont, new GUIContent("-"));
				
				}
				else if(srlPpt.enumValueIndex==(int)_SOType.Beam){
					EditorGUILayout.LabelField("Beam Setting", headerStyle);
					
					cont=new GUIContent("LineRenderer:", "The line-renderer component (optional)");
					if(!serializedObject.isEditingMultipleObjects)
						EditorGUILayout.PropertyField(serializedObject.FindProperty("line"), cont);
					else EditorGUILayout.LabelField(cont, new GUIContent("Cannot edit multiple instance"));
					
					cont=new GUIContent("Beam Duration:", "The active duration of the shoot object");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("beamDuration"), cont);
					
					EditorGUILayout.Space();
					
					cont=new GUIContent("Shoot Through All:", "Check if the shoot object penetrate any target it hits and carry on");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("shootThrough"), cont);
				}
				else if(srlPpt.enumValueIndex==(int)_SOType.Point){
					EditorGUILayout.LabelField("-", "No Setting required");
				}
				
			}
			
			
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Common Setting", headerStyle);
			
			//~ cont=new GUIContent("Hit Detection Radius:", "The size of the shoot object hit box. The higher the value, the easier it's to hit something");
			//~ EditorGUILayout.PropertyField(serializedObject.FindProperty("hitRadius"), cont);
			
			cont=new GUIContent("Hit Detection Radius:", "The size of the shoot object hit box. The higher the value, the easier it's to hit something");
			SerializedProperty hitRadiusP=serializedObject.FindProperty("hitRadius");
			EditorGUI.showMixedValue=hitRadiusP.hasMultipleDifferentValues;
			EditorGUI.BeginChangeCheck();
			float hitRadius=EditorGUILayout.FloatField(cont, hitRadiusP.floatValue);
			if(EditorGUI.EndChangeCheck()) hitRadiusP.floatValue=hitRadius;
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent("Impact Cam Shake:", "Camera shake magnitude upon hitting something");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("impactCamShake"), cont);
			
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("shootEffect");
			cont=new GUIContent("Shoot Effect Object:", "The game object to spawned as the visual effect at the shoot point when the shoot object is fired");
			EditorGUILayout.PropertyField(srlPpt, cont);
			
			cont=new GUIContent("Destroy shoot Effect:", "Check if the effect object needs to be removed from the game");
			if(srlPpt.objectReferenceValue!=null)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyShootEffect"), cont);
			else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			cont=new GUIContent("Destroy shoot Delay:", "The delay in seconds before the effect object is destroyed");
			if(srlPpt.objectReferenceValue!=null && serializedObject.FindProperty("destroyShootEffect").boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyShootDuration"), cont);
			else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			
			EditorGUILayout.Space();
			
			
			srlPpt=serializedObject.FindProperty("hitEffect");
			cont=new GUIContent("Hit Effect Object:", "The game object to spawned as the visual effect upon hitting something");
			EditorGUILayout.PropertyField(srlPpt, cont);
			
			cont=new GUIContent("Destroy Hit Effect:", "Check if the effect object needs to be removed from the game");
			if(srlPpt.objectReferenceValue!=null){
				EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyHitEffect"), cont);
			}
			else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			cont=new GUIContent("Destroy Hit Delay:", "The delay in seconds before the effect object is destroyed");
			if(srlPpt.objectReferenceValue!=null && serializedObject.FindProperty("destroyShootEffect").boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyHitDuration"), cont);
			else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
	}

}