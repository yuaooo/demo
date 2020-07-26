using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK {

	public class AbilityDB : MonoBehaviour {
		
		private static bool initiated=false;
		private static List<Ability> allAbilityList=new List<Ability>();	//for storing all effect during runtime
		public static void Init(){
			if(initiated) return;
			initiated=true;
			
			GameObject obj=Resources.Load("DB_TDSTK/DB_Ability", typeof(GameObject)) as GameObject;
			allAbilityList=new List<Ability>( obj.GetComponent<AbilityDB>().abilityList );
		}
		public static Ability CloneItem(int ID){
			Init();
			for(int i=0; i<allAbilityList.Count; i++){
				if(allAbilityList[i].ID==ID) return allAbilityList[i].Clone();
			}
			return null;
		}
		
		
		public List<Ability> abilityList=new List<Ability>();
		
		public static AbilityDB LoadDB(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Ability", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			return obj.GetComponent<AbilityDB>();
		}
		
		public static List<Ability> Load(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Ability", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			AbilityDB instance=obj.GetComponent<AbilityDB>();
			return instance.abilityList;
		}
		
		public static List<Ability> LoadClone(){
			GameObject obj=Resources.Load("DB_TDSTK/DB_Ability", typeof(GameObject)) as GameObject;
			AbilityDB instance=obj.GetComponent<AbilityDB>();
			
			List<Ability> newList=new List<Ability>();
			
			if(instance!=null){
				for(int i=0; i<instance.abilityList.Count; i++){
					newList.Add(instance.abilityList[i].Clone());
				}
			}
			
			return newList;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<AbilityDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDSTK/Resources/DB_TDSTK/DB_Ability.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
	}
	
}
