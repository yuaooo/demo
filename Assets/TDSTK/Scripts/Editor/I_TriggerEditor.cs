using UnityEngine;
using UnityEditor;

using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[CustomEditor(typeof(Trigger))]
	[CanEditMultipleObjects]
	public class TriggerEditor : TDSEditorInspector {
	
		private static Trigger instance;
		void Awake(){
			instance = (Trigger)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			Undo.RecordObject (instance, "Trigger");
			
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox(instance.GetEditorDescription(), MessageType.Info);
			
			EditorGUILayout.Space();
			
			DrawDefaultInspector();
			//DefaultInspector();
			
			EditorGUILayout.Space();
			
			
			
			EditorGUILayout.LabelField("Visual Effect and Sound", headerStyle);
			
			cont=new GUIContent("Effect Object:", "The effect object to spawn when the trigger is activated. Optional");
			instance.triggerEffObj=(GameObject)EditorGUILayout.ObjectField(cont, instance.triggerEffObj, typeof(GameObject), true);
			
			cont=new GUIContent(" - Spawn At Origin:", "Check to spawn the effect the position of the trigger transform\nOtherwise the effect will be spawn at the triggering object position");
			if(instance.triggerEffObj==null) EditorGUILayout.LabelField(cont, new GUIContent("-"));
			else instance.spawnEffectAtOrigin=EditorGUILayout.Toggle(cont, instance.spawnEffectAtOrigin);
			
			cont=new GUIContent(" - Auto Destroy:", "The effect object to spawn when the trigger is activated. Optional");
			if(instance.triggerEffObj==null) EditorGUILayout.LabelField(cont, new GUIContent("-"));
			else instance.autoDestroyEffObj=EditorGUILayout.Toggle(cont, instance.autoDestroyEffObj);
			
			cont=new GUIContent(" - Active Duration:", "Time in second until the effect object are destroyed after spawned");
			if(instance.triggerEffObj==null || !instance.autoDestroyEffObj) EditorGUILayout.LabelField(cont, new GUIContent("-"));
			else instance.effActiveDur=EditorGUILayout.FloatField(cont, instance.effActiveDur);
			
			
			
			if(instance.UseAltTriggerEffectObj()){
				EditorGUILayout.Space();
				
				//EditorGUILayout.LabelField("Alt Effect Object", headerStyle);
				
				cont=new GUIContent("Alt Effect Object:", "The alt effect object to spawn (at the secondary position) when the trigger is activated. Optional");
				instance.altTriggerEffObj=(GameObject)EditorGUILayout.ObjectField(cont, instance.altTriggerEffObj, typeof(GameObject), true);
				
				cont=new GUIContent(" - Auto Destroy:", "The effect object to spawn when the trigger is activated. Optional");
				if(instance.altTriggerEffObj==null) EditorGUILayout.LabelField(cont, new GUIContent("-"));
				else instance.autoDestroyAltEffObj=EditorGUILayout.Toggle(cont, instance.autoDestroyAltEffObj);
				
				cont=new GUIContent(" - Active Duration:", "Time in second until the effect object are destroyed after spawned");
				if(instance.altTriggerEffObj==null || !instance.autoDestroyEffObj) EditorGUILayout.LabelField(cont, new GUIContent("-"));
				else instance.altEffActiveDur=EditorGUILayout.FloatField(cont, instance.altEffActiveDur);
			}
			
			
			EditorGUILayout.Space();
			
			cont=new GUIContent("Triggered SFX:", "The audio clip to play when the trigger is triggered. Optional");
			instance.triggeredSFX=(AudioClip)EditorGUILayout.ObjectField(cont, instance.triggeredSFX, typeof(AudioClip), true);
			
			
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
	}
	
	
	
	[CustomEditor(typeof(TriggerHostile_DamagePlayer))]
	[CanEditMultipleObjects]
	public class TriggerHLostHPEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerHostile_Kill))]
	[CanEditMultipleObjects]
	public class TriggerHKillEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_CollectibleSpawner))]
	[CanEditMultipleObjects]
	public class TriggerPCSpawnerEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_UnitSpawner))]
	[CanEditMultipleObjects]
	public class TriggerPUSpawnerEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_Win))]
	[CanEditMultipleObjects]
	public class TriggerPWinEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_Damage))]
	[CanEditMultipleObjects]
	public class TriggerPDamageEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_RespawnPoint))]
	[CanEditMultipleObjects]
	public class TriggerPRespawnEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_Objective))]
	[CanEditMultipleObjects]
	public class TriggerPObjectiveEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_ActivateUnit))]
	[CanEditMultipleObjects]
	public class TriggerPActivateUnitEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_Teleport))]
	[CanEditMultipleObjects]
	public class TriggerPTeleportEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}
	
	[CustomEditor(typeof(TriggerPlayer_PlayerSwitch))]
	[CanEditMultipleObjects]
	public class TriggerPSwitchEditor : TriggerEditor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}

}