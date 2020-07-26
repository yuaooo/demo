using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class WeaponDB : MonoBehaviour {
		
		private static bool initiated=false;
		private static List<Weapon> allweaponList=new List<Weapon>();	//for storing all effect during runtime
		public static void Init(){
			if(initiated) return;
			initiated=true;
			
			GameObject obj=Resources.Load("DB_TDSTK/DB_Weapon", typeof(GameObject)) as GameObject;
			allweaponList=new List<Weapon>( obj.GetComponent<WeaponDB>().weaponList );
		}
		public static Weapon GetPrefab(int ID){
			Init();
			for(int i=0; i<allweaponList.Count; i++){
				if(allweaponList[i].ID==ID) return allweaponList[i];
			}
			return null;
		}
		
		
		public List<Weapon> weaponList=new List<Weapon>();
		
		public static WeaponDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Weapon", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<WeaponDB>();
		}
		
		public static List<Weapon> Load(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Weapon", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			WeaponDB instance=obj.GetComponent<WeaponDB>();
			return instance.weaponList;
		}
		
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<WeaponDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_Weapon.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
