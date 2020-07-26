using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	public class TDS{
		public static int GetLayerPlayer()			{ return 8; }
		public static int GetLayerAIUnit()			{ return 9; }
		public static int GetLayerShootObject()	{ return 10; }
		public static int GetLayerCollectible()	{ return 11; }
		public static int GetLayerTrigger()			{ return 12; }
		public static int GetLayerTerrain()			{ return 13; }
		
		
		
		public delegate void GameMessageHandler(string msg);
		public static event GameMessageHandler onGameMessageE;
		public static void OnGameMessage(string msg){ if(onGameMessageE!=null) onGameMessageE(msg); }
		
		
		
		public delegate void GameOverHandler(bool won);
		public static event GameOverHandler onGameOverE;
		public static void GameOver(bool won){ if(onGameOverE!=null) onGameOverE(won); }
		
		
		public delegate void ObjectiveCompletedHandler();
		public static event ObjectiveCompletedHandler onObjectiveCompletedE;
		public static void ObjectiveCompleted(){ if(onObjectiveCompletedE!=null) onObjectiveCompletedE(); }
		
		
		public delegate void TimeWarningHandler();
		public static event TimeWarningHandler onTimeWarningE;
		public static void TimeWarning(){ if(onTimeWarningE!=null) onTimeWarningE(); }
		
		public delegate void TimeUpHandler();
		public static event TimeUpHandler onTimeUpE;
		public static void TimeUp(){ if(onTimeUpE!=null) onTimeUpE(); }
		
		
		//public delegate void GainLifeHandler();
		//public static event GainLifeHandler onTimeUpE;
		//public static void TimeUp(){ if(onTimeUpE!=null) onTimeUpE(); }
		
		
		public delegate void NewUnitHandler(Unit unit);	//fired when a new unit is added to the game
		public static event NewUnitHandler onNewUnitE;	
		public static void NewUnit(Unit unit){ if(onNewUnitE!=null) onNewUnitE(unit); }
		
		public delegate void GainEffectHandler(Effect effect);
		public static event GainEffectHandler onGainEffectE;	
		public static void GainEffect(Effect effect){ if(onGainEffectE!=null) onGainEffectE(effect); }
		
		
		
		
		
		public delegate void PlayerDamagedHandler(float dmg);
		public static event PlayerDamagedHandler onPlayerDamagedE;	
		public static void PlayerDamaged(float dmg){ if(onPlayerDamagedE!=null) onPlayerDamagedE(dmg); }
		
		public delegate void PlayerDestroyedHandler();
		public static event PlayerDestroyedHandler onPlayerDestroyedE;	
		public static void PlayerDestroyed(){ if(onPlayerDestroyedE!=null) onPlayerDestroyedE(); }
		
		public delegate void PlayerRespawnedHandler();
		public static event PlayerRespawnedHandler onPlayerRespawnedE;	
		public static void PlayerRespawned(){ if(onPlayerRespawnedE!=null) onPlayerRespawnedE(); }
		
		
		public delegate void ReloadingHandler();
		public static event ReloadingHandler onReloadingE;	
		public static void Reloading(){ if(onReloadingE!=null) onReloadingE(); }
		
		public delegate void SwitchWeaponHandler(Weapon weapon);
		public static event SwitchWeaponHandler onSwitchWeaponE;	
		public static void SwitchWeapon(Weapon weapon){ if(onSwitchWeaponE!=null) onSwitchWeaponE(weapon); }
		
		public delegate void SwitchAbilityHandler(Ability ability);
		public static event SwitchAbilityHandler onSwitchAbilityE;	
		public static void SwitchAbility(Ability ability){ if(onSwitchAbilityE!=null) onSwitchAbilityE(ability); }
		
		
		public delegate void NewAbilityHandler(Ability ability, int replaceIndex=-1);	//fired when player gain new ability (via perk)
		public static event NewAbilityHandler onNewAbilityE;	
		public static void NewAbility(Ability ability, int replaceIndex=-1){ if(onNewAbilityE!=null) onNewAbilityE(ability, replaceIndex); }
		
		public delegate void NewWeaponHandler(Weapon weapon, int replaceIndex=-1);	//fired when player gain new ability (via perk)
		public static event NewWeaponHandler onNewWeaponE;	
		public static void NewWeapon(Weapon weapon, int replaceIndex=-1){ if(onNewWeaponE!=null) onNewWeaponE(weapon, replaceIndex); }
		
		
		public delegate void FireFailHandler(string msg);
		public static event FireFailHandler onFireFailE;	
		public static void FireFail(string msg){ if(onFireFailE!=null) onFireFailE(msg); }
		
		public delegate void FireAltFailHandler(string msg);
		public static event FireAltFailHandler onFireAltFailE;	
		public static void FireAltFail(string msg){ if(onFireAltFailE!=null) onFireAltFailE(msg); }
		
		public delegate void AbilityActivationFailHandler(string msg);
		public static event AbilityActivationFailHandler onAbilityActivationFailE;	
		public static void AbilityActivationFailFail(string msg){ if(onAbilityActivationFailE!=null) onAbilityActivationFailE(msg); }
		
		
		
		public delegate void LevelUpHandler(UnitPlayer player);
		public static event LevelUpHandler onPlayerLevelUpE;	
		public static void PlayerLevelUp(UnitPlayer player){ if(onPlayerLevelUpE!=null) onPlayerLevelUpE(player); }
		
		public delegate void PerkCurrencyHandler(int value);
		public static event PerkCurrencyHandler onPerkCurrencyE;	
		public static void OnPerkCurrency(int value){ if(onPerkCurrencyE!=null) onPerkCurrencyE(value); }
		
		public delegate void PerkPurchasedHandler(Perk perk);
		public static event PerkPurchasedHandler onPerkPurchasedE;	
		public static void PerkPurchased(Perk perk){ if(onPerkPurchasedE!=null) onPerkPurchasedE(perk); }
		
		
		
		public delegate void CameraShakeHandler(float mag);
		public static event CameraShakeHandler onCameraShakeE;	
		public static void CameraShake(float mag){ if(onCameraShakeE!=null) onCameraShakeE(mag); }
		
	}

}