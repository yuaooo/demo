using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class UnitAIDB : MonoBehaviour {
		
		
		public List<UnitAI> unitList=new List<UnitAI>();
		
		public static UnitAIDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_UnitAI", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<UnitAIDB>();
		}
		
		public static List<UnitAI> Load(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_UnitAI", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			UnitAIDB instance=obj.GetComponent<UnitAIDB>();
			return instance.unitList;
		}
		
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<UnitAIDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_UnitAI.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
