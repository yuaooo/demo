using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class UnitPlayerDB : MonoBehaviour {
		
		
		public List<UnitPlayer> unitList=new List<UnitPlayer>();
		
		public static UnitPlayerDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_UnitPlayer", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<UnitPlayerDB>();
		}
		
		public static List<UnitPlayer> Load(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_UnitPlayer", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			UnitPlayerDB instance=obj.GetComponent<UnitPlayerDB>();
			return instance.unitList;
		}
		
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<UnitPlayerDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_UnitPlayer.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
