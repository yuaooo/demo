using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class CollectibleDB : MonoBehaviour {
		
		
		public List<Collectible> collectibleList=new List<Collectible>();
		
		public static CollectibleDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Collectible", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<CollectibleDB>();
		}
		
		public static List<Collectible> Load(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Collectible", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			CollectibleDB instance=obj.GetComponent<CollectibleDB>();
			return instance.collectibleList;
		}
		
		
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<CollectibleDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_Collectible.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
