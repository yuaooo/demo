using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class EffectDB : MonoBehaviour {
		
		private static bool initiated=false;
		private static List<Effect> allEffectList=new List<Effect>();	//for storing all effect during runtime
		public static void Init(){
			if(initiated) return;
			initiated=true;
			
			GameObject obj=Resources.Load("DB_TDSTK/DB_Effect", typeof(GameObject)) as GameObject;
			allEffectList=new List<Effect>( obj.GetComponent<EffectDB>().effectList );
		}
		
		public static int GetEffectIndex(int ID){	//called during game initiation to assign correct index to each corresponding effect ID
			Init();
			for(int i=0; i<allEffectList.Count; i++){ if(allEffectList[i].ID==ID) return i; }
			return -1;
		}
		public static Effect CloneItem(int idx){	//called during runtime when an effect is actually needed (item now only store effect ID and index);
			Init();
			if(idx>0 && idx<allEffectList.Count) return allEffectList[idx].Clone();
			return null;
		}
		
		
		
		public List<Effect> effectList=new List<Effect>();
		
		public static EffectDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Effect", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<EffectDB>();
		}
		
		public static List<Effect> Load(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Effect", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			EffectDB instance=obj.GetComponent<EffectDB>();
			return instance.effectList;
		}
		
		public static List<Effect> LoadClone(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Effect", typeof(GameObject)) as GameObject;
			EffectDB instance=obj.GetComponent<EffectDB>();
			
			List<Effect> newList=new List<Effect>();
			
			if(instance!=null){
				for(int i=0; i<instance.effectList.Count; i++){
					newList.Add(instance.effectList[i].Clone());
				}
			}
			
			return newList;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<EffectDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_Effect.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
