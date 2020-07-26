using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public class Trigger : MonoBehaviour {
		
		public delegate void TriggerCallback(Trigger trigger);		//used for objective only
		protected List<TriggerCallback> triggerCallbackList=new List<TriggerCallback>();
		public void SetTriggerCallback(TriggerCallback callback){ triggerCallbackList.Add(callback); }
		
		
		public virtual string GetEditorDescription(){ return "Trigger base class is not intended to be used use child class instead"; }
		
		
		[Space(5)]
		[Tooltip("Check if the tirgger object is to be destroyed after it's triggered")]
		public bool destroyedAfterTriggered=true;
		
		[Space(5)] [HideInInspector]
		[Tooltip("The audio clip to play when the trigger is triggered")]
		public AudioClip triggeredSFX;
		
		
		
		[Tooltip("The effect object to spawn at the player position when the trigger is activated. Optional")] [HideInInspector]
		public GameObject triggerEffObj;
		[Tooltip("Check to auto destroy the trigger effect object")] [HideInInspector]
		public bool autoDestroyEffObj=true;
		[Tooltip("The active duration of the effect object")] [HideInInspector]
		public float effActiveDur=2;		//duration
		[Tooltip("Check to spawn the effect at player position, otherwise the effect will be spawn at the trigger position")] [HideInInspector]
		public bool spawnEffectAtOrigin=false;	//check to have the effect spawn at the position of the trigger, otherwise it spawned at triggering object (player/unit) position
		protected Vector3 effPos;
		
		[Space(10)]
		[Tooltip("The effect object to spawn at the new player position when the trigger is activated. Optional")] [HideInInspector]
		public GameObject altTriggerEffObj;	//for teleport and switch-player
		[Tooltip("Check to auto destroy the trigger effect object at new player position")] [HideInInspector]
		public bool autoDestroyAltEffObj=true;
		[Tooltip("The active duration of the effect object at new player position")] [HideInInspector]
		public float altEffActiveDur=2;
		protected Vector3 targetEffPos;
		
		public virtual bool UseAltTriggerEffectObj(){ return false; }
		
		
		public virtual void Awake(){
			gameObject.layer=TDS.GetLayerTrigger();
		}
		
		public virtual void OnEnable(){
			Renderer rend=gameObject.GetComponent<Renderer>();
			if(rend!=null) rend.enabled=false;
			
			Collider collider=gameObject.GetComponent<Collider>();
			if(collider==null) collider=gameObject.AddComponent<BoxCollider>();
			collider.isTrigger=true;
			
			if(triggerEffObj!=null) ObjectPoolManager.New(triggerEffObj, 1);
			if(altTriggerEffObj!=null) ObjectPoolManager.New(altTriggerEffObj, 1);
			
			effPos=transform.position;
		}
		
		
		public virtual void OnTriggerEnter(Collider collider){
			
		}
		
		
		//called when the trigger is triggered
		protected void Triggered(){
			AudioManager.PlaySound(triggeredSFX);
			
			if(triggerEffObj==null){
				if(!autoDestroyEffObj) ObjectPoolManager.Spawn(triggerEffObj, effPos, Quaternion.identity);
				else ObjectPoolManager.Spawn(triggerEffObj, effPos, Quaternion.identity, effActiveDur);
			}
			if(UseAltTriggerEffectObj() && altTriggerEffObj==null){
				if(!autoDestroyAltEffObj) ObjectPoolManager.Spawn(altTriggerEffObj, targetEffPos, Quaternion.identity);
				else ObjectPoolManager.Spawn(altTriggerEffObj, targetEffPos, Quaternion.identity, altEffActiveDur);
			}
			
			if(destroyedAfterTriggered) Destroy(gameObject);
		}
		
		
		protected virtual void OnDrawGizmos() {
			Gizmos.DrawIcon(transform.position, "Trigger.png", true);
			
			Collider collider=gameObject.GetComponent<Collider>();
			
			Vector3 scale=collider!=null ? collider.bounds.size : transform.localScale;
			scale.y=0;
			Gizmos.DrawWireCube(transform.position, scale);
			
			//if(collider != null) Gizmos.DrawWireCube(transform.position, collider.bounds.size);
			//else Gizmos.DrawWireCube(transform.position, transform.localScale);
		}
		
	}

}