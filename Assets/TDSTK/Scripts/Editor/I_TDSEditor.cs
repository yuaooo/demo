using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	public class TDSEditorInspector : Editor {
		
		protected static bool styleDefined=false;
		protected static GUIStyle headerStyle;
		protected static GUIStyle foldoutStyle;
		protected static GUIStyle conflictStyle;
		protected static GUIStyle toggleHeaderStyle;
		
		protected GUIContent cont;
		protected GUIContent contN=GUIContent.none;
		protected GUIContent[] contL;
		
		
		public override void OnInspectorGUI(){
			DefineStyle();
		}
		
		
		protected static bool showDefaultEditor=false;
		protected void DefaultInspector(){
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showDefaultEditor=EditorGUILayout.Foldout(showDefaultEditor, "Show default editor", foldoutStyle);
			EditorGUILayout.EndHorizontal();
			if(showDefaultEditor) DrawDefaultInspector();
			
			EditorGUILayout.Space();
		}
		
		
		protected static void DefineStyle(){
			if(styleDefined) return;
			styleDefined=true;
			
			headerStyle=new GUIStyle("Label");
			headerStyle.fontStyle=FontStyle.Bold;
			headerStyle.normal.textColor = Color.black;
			
			toggleHeaderStyle=new GUIStyle("Toggle");
			toggleHeaderStyle.fontStyle=FontStyle.Bold;
			//toggleHeaderStyle.normal.textColor = Color.black;
			
			foldoutStyle=new GUIStyle("foldout");
			foldoutStyle.fontStyle=FontStyle.Bold;
			foldoutStyle.normal.textColor = Color.black;
			
			conflictStyle=new GUIStyle("Label");
			conflictStyle.normal.textColor = Color.red;
		}
		
		
		private static bool loaded=false;
		protected static void LoadDB(){
			if(loaded) return;
			
			loaded=true;
			
			LoadDamageTable();
			LoadEffect();
			LoadAbility();
			LoadPerk();
			LoadWeapon();
			LoadCollectible();
			
			LoadUnitAI();
			LoadUnitPlayer();
		}
		
		
		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		protected static void LoadDamageTable(){ TDSEditor.LoadDamageTable(); }
		protected static void UpdateLabel_DamageTable(){ TDSEditor.UpdateLabel_DamageTable(); }
		public static void SetDamageDB(DamageTableDB db, string[] dmgLabel, string[] armLabel){
			damageTableDB=db;
			damageTypeLabel=dmgLabel;
			armorTypeLabel=armLabel;
		}
		
		
		protected static EffectDB effectDB;
		protected static List<int> effectIDList=new List<int>();
		protected static string[] effectLabel;
		protected static void LoadEffect(){ TDSEditor.LoadEffect(); }
		protected static void UpdateLabel_Effect(){ TDSEditor.UpdateLabel_Effect(); }
		public static void SetEffectDB(EffectDB db, List<int> IDList, string[] label){
			effectDB=db;
			effectIDList=IDList;
			effectLabel=label;
		}
		
		
		protected static AbilityDB abilityDB;
		protected static List<int> abilityIDList=new List<int>();
		protected static string[] abilityLabel;
		protected static void LoadAbility(){ TDSEditor.LoadAbility(); }
		protected static void UpdateLabel_Ability(){ TDSEditor.UpdateLabel_Ability(); }
		public static void SetAbilityDB(AbilityDB db, List<int> IDList, string[] label){
			abilityDB=db;
			abilityIDList=IDList;
			abilityLabel=label;
		}
		
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		protected static void LoadPerk(){ TDSEditor.LoadPerk(); }
		protected static void UpdateLabel_Perk(){ TDSEditor.UpdateLabel_Perk(); }
		public static void SetPerkDB(PerkDB db, List<int> IDList, string[] label){
			perkDB=db;
			perkIDList=IDList;
			perkLabel=label;
		}
		
		
		protected static WeaponDB weaponDB;
		protected static List<int> weaponIDList=new List<int>();
		protected static string[] weaponLabel;
		protected static void LoadWeapon(){ TDSEditor.LoadWeapon(); }
		protected static void UpdateLabel_Weapon(){ TDSEditor.UpdateLabel_Weapon(); }
		public static void SetWeaponDB(WeaponDB db, List<int> IDList, string[] label){
			weaponDB=db;
			weaponIDList=IDList;
			weaponLabel=label;
		}
		
		
		protected static CollectibleDB collectibleDB;
		protected static List<int> collectibleIDList=new List<int>();
		protected static string[] collectibleLabel;
		protected static void LoadCollectible(){ TDSEditor.LoadCollectible(); }
		protected static void UpdateLabel_Collectible(){ TDSEditor.UpdateLabel_Collectible(); }
		public static void SetCollectibleDB(CollectibleDB db, List<int> IDList, string[] label){
			collectibleDB=db;
			collectibleIDList=IDList;
			collectibleLabel=label;
		}
		
		
		protected static UnitAIDB unitAIDB;
		protected static List<int> unitAIIDList=new List<int>();
		protected static string[] unitAILabel;
		protected static void LoadUnitAI(){ TDSEditor.LoadUnitAI(); }
		protected static void UpdateLabel_UnitAI(){ TDSEditor.UpdateLabel_UnitAI(); }
		public static void SetUnitAIDB(UnitAIDB db, List<int> IDList, string[] label){
			unitAIDB=db;
			unitAIIDList=IDList;
			unitAILabel=label;
		}
		
		
		protected static UnitPlayerDB unitPlayerDB;
		protected static List<int> unitPlayerIDList=new List<int>();
		protected static string[] unitPlayerLabel;
		protected static void LoadUnitPlayer(){ TDSEditor.LoadUnitPlayer(); }
		protected static void UpdateLabel_UnitPlayer(){ TDSEditor.UpdateLabel_UnitPlayer(); }
		public static void SetUnitPlayerDB(UnitPlayerDB db, List<int> IDList, string[] label){
			unitPlayerDB=db;
			unitPlayerIDList=IDList;
			unitPlayerLabel=label;
		}
		
		
		
		
		
		
		protected void SpaceH(float space=-1){
			if(space==-1) EditorGUILayout.LabelField("");
			else EditorGUILayout.LabelField("", GUILayout.Width(space));
		}
		
		
		protected List<GameObject> objHList=new List<GameObject>();
		protected string[] objHLabelList=new string[0];
		protected void UpdateObjectHierarchyList(GameObject obj){
			TDSEditorUtility.GetObjectHierarchyList(obj, this.SetObjListCallback);
		}
		protected void SetObjListCallback(List<GameObject> objList, string[] labelList){
			objHList=objList;
			objHLabelList=labelList;
		}
		protected static int GetObjectIDFromHList(Transform objT, List<GameObject> objHList){
			if(objT==null) return 0;
			for(int i=1; i<objHList.Count; i++){
				if(objT==objHList[i].transform) return i;
			}
			return 0;
		}
		
		
		
		
		protected SerializedProperty srlPpt;
		
		protected const float labelWidth=125;
		protected const float fieldWidth=50;
		protected const float fieldWidthL=140;
		protected const float fieldWidthS=10;
		
		protected void PropertyFieldL(SerializedProperty property, GUIContent gcon){ PropertyField(property, gcon, fieldWidthL); }
		protected void PropertyFieldS(SerializedProperty property, GUIContent gcon){ PropertyField(property, gcon, fieldWidthS); }
		protected void PropertyField(SerializedProperty property, GUIContent gcon, float width=fieldWidth){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(gcon, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.PropertyField(property, contN, GUILayout.MaxWidth(width));
			EditorGUILayout.EndHorizontal();
		}
		protected void InvalidField(GUIContent gcon){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(gcon, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.LabelField("-", GUILayout.MaxWidth(fieldWidthS));
			EditorGUILayout.EndHorizontal();
		}
		
		
		
		protected void DrawDestroyValue(){
			cont=new GUIContent("Score On Destroy:", "Score gained by player when the unit is destroyed");
			PropertyField(serializedObject.FindProperty("valueScore"), cont);
			
			cont=new GUIContent("HitPoint On Destroy:", "Hit-Point gained by player when the unit is destroyed");
			PropertyField(serializedObject.FindProperty("valueHitPoint"), cont);
			
			cont=new GUIContent("Energy On Destroy:", "Energy gained by player when the unit is destroyed");
			PropertyField(serializedObject.FindProperty("valueEnergy"), cont);
			
			cont=new GUIContent("Exp On Destroy:", "Experience gained by player when the unit is destroyed");
			PropertyField(serializedObject.FindProperty("valueExp"), cont);
		}
		
		protected void DrawDestroyEffect(){
			cont=new GUIContent("DestroyCamShake:", "The camera shake magnitude whenever the unit is destroyed");
			PropertyField(serializedObject.FindProperty("destroyCamShake"), cont);
			
			EditorGUILayout.Space();
			
			srlPpt=serializedObject.FindProperty("destroyedEffectObj");
			cont=new GUIContent("DestroyedEffectObj:", "The object to be spawned when the unit is destroyed (optional)");
			PropertyFieldL(srlPpt, cont);
			
			cont=new GUIContent("AutoDestroy Effect:", "Check if the effect object needs to be removed from the game");
			if(srlPpt.objectReferenceValue!=null)
				PropertyField(serializedObject.FindProperty("autoDestroyDObj"), cont);
			else InvalidField(cont);
			
			cont=new GUIContent(" - Duration:", "The delay in seconds before the effect object is destroyed");
			if(srlPpt.objectReferenceValue!=null && serializedObject.FindProperty("autoDestroyDObj").boolValue)
				PropertyField(serializedObject.FindProperty("dObjActiveDuration"), cont);
			else InvalidField(cont);
		}
		
		protected void DrawDropSetting(){
			srlPpt=serializedObject.FindProperty("useDropManager");
			cont=new GUIContent("Use DropManager:", "Check to use DropManager to determine what the unit drops");
			PropertyField(srlPpt, cont);
			
			bool useDropManager=srlPpt.boolValue && !srlPpt.hasMultipleDifferentValues;
			
			cont=new GUIContent("Drop Object:", "The game object to drop when the unit is destroyed");
			if(!useDropManager) PropertyFieldL(serializedObject.FindProperty("dropObject"), cont);
			else InvalidField(cont);
			
			cont=new GUIContent("Drop Chance:", "The chance for the object to drop. Takes value from 0-1 with 0.3 being 30% chance to drop");
			if(!useDropManager) PropertyField(serializedObject.FindProperty("dropChance"), cont);
			else InvalidField(cont);
		}
		
		protected void DrawSpawnUponDestroy(){
			srlPpt=serializedObject.FindProperty("spawnUponDestroy");
			
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			int unitIdx=srlPpt.hasMultipleDifferentValues ? 0 : -1;//(srlPpt.objectReferenceValue!=null ? TDSEditor.GetUnitAIIndex(srlPpt.objectReferenceValue) : 0);//TDSEditor.GetUnitAIIndex(unit.spawnUponDestroy.prefabID) : 0 ;
			if(unitIdx==-1){
				if(srlPpt.objectReferenceValue!=null) unitIdx=((Unit)srlPpt.objectReferenceValue).prefabID;
				else unitIdx=0;
			}
			
			EditorGUILayout.BeginHorizontal();
			cont=new GUIContent("SpawnUponDestroy:", "The unit to spawn when this unit is destroyed (optional)");
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			
			EditorGUI.BeginChangeCheck();
			unitIdx = EditorGUILayout.Popup(unitIdx, unitAILabel, GUILayout.MaxWidth(fieldWidthL));
			if(EditorGUI.EndChangeCheck()){
				if(unitIdx==0) srlPpt.objectReferenceValue=null;
				else if(unitIdx>0) srlPpt.objectReferenceValue=unitAIDB.unitList[unitIdx-1];
			}
			EditorGUILayout.EndHorizontal();
			
			
			cont=new GUIContent("Spawn Count:", "Number of unit to spawn");
			if(srlPpt.objectReferenceValue!=null) PropertyField(serializedObject.FindProperty("dropChance"), cont);
			else InvalidField(cont);
			
		}
		
		
		protected void DrawAttackStats(string propertyName, bool showAOE=true, bool showPhysics=true, string label="Attack Stats"){
			
			string lbSp=" - ";
			SerializedProperty aStatsP=serializedObject.FindProperty(propertyName);
			
			EditorGUILayout.LabelField(label, headerStyle);
			
			
			srlPpt=aStatsP.FindPropertyRelative("damageType");
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			EditorGUI.BeginChangeCheck();
			cont=new GUIContent(lbSp+"Damage Type:", "The damage type of the unit\nDamage type can be configured in Damage Armor Table Editor");
			contL=new GUIContent[damageTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(damageTypeLabel[i], "");
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			int armorType = EditorGUILayout.Popup(srlPpt.intValue, contL, GUILayout.MaxWidth(fieldWidthL));
			EditorGUILayout.EndHorizontal();
			
			if(EditorGUI.EndChangeCheck()) srlPpt.intValue=armorType;
			EditorGUI.showMixedValue=false;
			
			
			EditorGUILayout.BeginHorizontal();
			cont=new GUIContent(lbSp+"Damage(Min/Max):", "Damage value done to the target's hit-point.");
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.PropertyField(aStatsP.FindPropertyRelative("damageMin"), contN, GUILayout.MaxWidth(fieldWidth));
			EditorGUILayout.PropertyField(aStatsP.FindPropertyRelative("damageMax"), contN, GUILayout.MaxWidth(fieldWidth));
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			
			
			if(showAOE){
				cont=new GUIContent(lbSp+"AOE Radius:", "Area of effect radius of the attack. Any hostile unit within the area is affected by the attack");
				PropertyField(aStatsP.FindPropertyRelative("aoeRadius"), cont);
				
				cont=new GUIContent(lbSp+"Diminishing AOE:", "Check if damage value diminished the further away the target is from the center of the aoe");
				if(aStatsP.FindPropertyRelative("aoeRadius").floatValue>0)
					PropertyField(aStatsP.FindPropertyRelative("dimishingAOE"), cont);
				else InvalidField(cont);
			}
			
			
			EditorGUILayout.Space();
			
			
			cont=new GUIContent(lbSp+"Critical Chance:", "The chance of the attack to score a critical. Takes value from 0-1 with 0.3 being 30% to score a critical");
			PropertyField(aStatsP.FindPropertyRelative("critChance"), cont);
			
			cont=new GUIContent(lbSp+"Critical Multiplier:", "The multiplier to be applied to damage if the attack scores a critical.\n - 1.5 for 150% of normal damage, 2 for 200% and so on");
			if(aStatsP.FindPropertyRelative("critChance").floatValue>0)
				PropertyField(aStatsP.FindPropertyRelative("critMultiplier"), cont);
			else InvalidField(cont);
			
			
			EditorGUILayout.Space();
			
			
			if(showPhysics){
				cont=new GUIContent(lbSp+"Impact Force:", "If the attack will applies a knock back force to the target\nOnly applies if the attack is a direct hit from a shoot object");
				EditorGUI.BeginChangeCheck(); EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
				float impactForce = EditorGUILayout.FloatField(aStatsP.FindPropertyRelative("impactForce").floatValue, GUILayout.MaxWidth(fieldWidth));
				if(EditorGUI.EndChangeCheck()) aStatsP.FindPropertyRelative("impactForce").floatValue=impactForce;
				EditorGUILayout.EndHorizontal();
				
				cont=new GUIContent(lbSp+"Explosion Radius:", "The radius in which all unit is affected by explosion force");
				PropertyField(aStatsP.FindPropertyRelative("explosionRadius"), cont);
				
				cont=new GUIContent(lbSp+"Explosion Force:", "The force of the explosion which pushes all affect unit away from the impact point");
				if(aStatsP.FindPropertyRelative("explosionRadius").floatValue>0)
					PropertyField(aStatsP.FindPropertyRelative("explosionForce"), cont);
				else InvalidField(cont);
			}
			
			
			EditorGUILayout.Space();
			
			
			
			srlPpt=aStatsP.FindPropertyRelative("effectID");
			EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
			int effectIdx=srlPpt.intValue>=0 ? TDSEditor.GetEffectIndex(srlPpt.intValue) : 0 ;
			
			//if(!srlPpt.hasMultipleDifferentValues)
				//TDSEditorUtility.DrawSprite(new Rect(startX+spaceX+width-40, startY+spaceY-45, 40, 40), effectIdx>0 ? effectDB.effectList[effectIdx-1].icon : null);
			//if(GUI.Button(new Rect(startX+spaceX, startY-2, 40, height-2), "Edit")) EffectEditorWindow.Init();
			
			EditorGUILayout.BeginHorizontal();	EditorGUI.BeginChangeCheck();
			
			cont=new GUIContent(lbSp+"Attack Effect:", "Special effect that applies with each hit (optional)");
			EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(labelWidth));
			
			effectIdx=EditorGUILayout.Popup(effectIdx, effectLabel, GUILayout.MaxWidth(fieldWidthL));
			if(EditorGUI.EndChangeCheck()){
				if(effectIdx>0) srlPpt.intValue=effectDB.effectList[effectIdx-1].ID;
				else srlPpt.intValue=-1;
			}
			EditorGUI.showMixedValue=false;
			EditorGUILayout.EndHorizontal();
			
		}
	}

}