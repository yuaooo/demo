using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class DamageTableDB : MonoBehaviour {

		public List<ArmorType> armorTypeList=new List<ArmorType>();
		public List<DamageType> damageTypeList=new List<DamageType>();
		
		public static DamageTableDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_DamageTable", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<DamageTableDB>();
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<DamageTableDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_DamageTable.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}

}