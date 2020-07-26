using UnityEngine;
using UnityEditor;

#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using System.Collections;

using TDSTK;

namespace TDSTK {

	public class MenuExtension : EditorWindow {
		
		[MenuItem ("Tools/TDSTK/New Scene", false, -100)]
		private static void NewScene(){
			CreateEmptyScene();
			
			GameObject obj=(GameObject)Instantiate(Resources.Load("Prefab_TDSTK/TDSTK_SimpleScene", typeof(GameObject)));
			obj.name="TDSTK_SimpleScene";
		}
		
		[MenuItem ("Tools/TDSTK/New Scene (Touch Input)", false, -100)]
		private static void NewScene_TouchControl(){
			CreateEmptyScene();
			
			GameObject obj=(GameObject)Instantiate(Resources.Load("Prefab_TDSTK/TDSTK_SimpleScene_TouchInput", typeof(GameObject)));
			obj.name="TDSTK_SimpleScene_TouchInput";
		}
		
		static void CreateEmptyScene(){
			#if UNITY_5_3_OR_NEWER
				EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
			#else
				EditorApplication.NewScene();
			#endif
			
			//EditorApplication.NewScene();
			GameObject camObj=Camera.main.gameObject; 	DestroyImmediate(camObj);
		}
		
		
		
		
		
		
		[MenuItem ("Tools/TDSTK/Progression Stats Editor", false, 10)]
		static void OpenProgressionStatsEditor () {
			ProgressStatsEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Ability Editor", false, 10)]
		static void OpenAbilityEditor () {
			AbilityEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Effect Editor", false, 10)]
		static void OpenEffectEditor () {
			EffectEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Collectible Editor", false, 10)]
		static void OpenCollectibleEditor () {
			CollectibleEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Weapon Editor", false, 10)]
		static void OpenWeaponEditor () {
			WeaponEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Perk Editor", false, 10)]
		static void OpenPerkEditor () {
			PerkEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/AI Unit Editor", false, 10)]
		static void OpenAIUnitEditor () {
			UnitAIEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Player Unit Editor", false, 10)]
		static void OpenPlayerUnitEditor () {
			UnitPlayerEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Unit Spawner Editor", false, 10)]
		static void OpenUnitSpawnerEditor () {
			UnitSpawnerEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Collectible Spawner Editor", false, 10)]
		static void OpenColtSpawnerEditor () {
			CollectibleSpawnerEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/DamageTableEditor", false, 10)]
		public static void OpenDamageTableEditor () {
			DamageTableEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TDSTK/Contact and Support Info", false, 100)]
		static void OpenSupportContactWindow () {
			SupportNContactWindow.Init();
		}
		
		
	}


}