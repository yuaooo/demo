using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class AudioManager : MonoBehaviour {
		
		private static AudioManager instance;
		
		private List<AudioSource> audioSourceList=new List<AudioSource>();
		
		private static float volume=.75f;
		//private static float volumeMusic=.75f;
		//private static float volumeUI=.75f;
		//private static float volumeSFX=.75f;
		
		
		void Awake(){
			instance=this;
			
			//create a list of audio source component, which will when be used to play all the sfx
			audioSourceList=new List<AudioSource>();
			for(int i=0; i<10; i++){
				GameObject obj=new GameObject();
				obj.name="AudioSource"+(i+1);
				
				AudioSource src=obj.AddComponent<AudioSource>();
				src.playOnAwake=false;
				src.loop=false;
				src.spatialBlend=0;
				src.volume=volume;
				
				obj.transform.parent=transform;
				obj.transform.localPosition=Vector3.zero;
				
				audioSourceList.Add(src);
			}

      PlaySound(backgroundClip);
      CreateMoveIdleAudioSource();
		}
		
		
		
		//set the volume of all the audioSource, not in used atm
		public static void SetVolume(float val){ instance._SetVolume(val); }
		public void _SetVolume(float val){
			if(Mathf.Abs(volume-val)<0.05f) return;
			
			volume=val;
			for(int i=0; i<audioSourceList.Count; i++) audioSourceList[i].volume=volume;
		}
		
		
		
		//call to play a specific clip
		public static void PlaySound(AudioClip clip){ 
			if(instance==null) return;
			instance._PlaySound(clip);
		}
		public void _PlaySound(AudioClip clip){ 
			if(clip==null) return;
			int idx=GetUnusedAudioSourceIndex();
			
			audioSourceList[idx].clip=clip;
			audioSourceList[idx].Play();
		}
		
		//check for the next free, unused audioObject
		private int GetUnusedAudioSourceIndex(){
			for(int i=0; i<audioSourceList.Count; i++){
				if(!audioSourceList[i].isPlaying) return i;
			}
			return 0;	//if everything is used up, use item number zero
		}
		
		
		//listen to all the relevant event and called the corresponding function (which play the corresponding audio)
		void OnEnable(){
			TDS.onGameOverE += OnGameOver;
			TDS.onObjectiveCompletedE += OnObjectiveCompleted;
			
			TDS.onTimeWarningE += OnTimeWarning;
			TDS.onTimeUpE += OnTimeUp;
			
			TDS.onPlayerDamagedE += OnPlayerDamaged;
			TDS.onPlayerDestroyedE += OnPlayerDestroyed;
			TDS.onPlayerRespawnedE += OnPlayerRespawned;
			
			TDS.onFireFailE += OnFireFail;
			TDS.onFireAltFailE += OnFireAltFail;
			TDS.onAbilityActivationFailE += OnAbilityActivationFail;
			
			TDS.onPlayerLevelUpE += OnPlayerLevelUp;
			TDS.onPerkPurchasedE += OnPerkPurchasedUp;
		}
		void OnDisable(){
			TDS.onGameOverE -= OnGameOver;
			TDS.onObjectiveCompletedE -= OnObjectiveCompleted;
			
			TDS.onTimeWarningE -= OnTimeWarning;
			TDS.onTimeUpE -= OnTimeUp;
			
			TDS.onPlayerDamagedE -= OnPlayerDamaged;
			TDS.onPlayerDestroyedE -= OnPlayerDestroyed;
			TDS.onPlayerRespawnedE -= OnPlayerRespawned;
			
			TDS.onFireFailE -= OnFireFail;
			TDS.onFireAltFailE -= OnFireAltFail;
			TDS.onAbilityActivationFailE -= OnAbilityActivationFail;
			
			TDS.onPlayerLevelUpE -= OnPlayerLevelUp;
			TDS.onPerkPurchasedE -= OnPerkPurchasedUp;
		}
		
		
		
		[Space(10)]
		[Tooltip("audioclip to play when the level is won")]
		public AudioClip gameWinClip;
		[Tooltip("audioclip to play when the level is lost")]
		public AudioClip gameLostClip;
		void OnGameOver(bool won){ PlaySound(won ? gameWinClip : gameLostClip); }
		
		[Tooltip("audioclip to play when the objective is completed")]
		public AudioClip objectiveCompletedClip;
		void OnObjectiveCompleted(){ PlaySound(objectiveCompletedClip); }
		
		
		[Space(10)]
		[Tooltip("audioclip to play when the time is counting down and almost up")]
		public AudioClip timeWarningClip;
		void OnTimeWarning(){ PlaySound(timeWarningClip); }
		
		[Tooltip("audioclip to play when the time is up")]
		public AudioClip timesUpClip;
		void OnTimeUp(){ PlaySound(timesUpClip); }
		
		
		[Space(10)]
		[Tooltip("audioclip to play when the player is damaged")]
		public AudioClip playerDamagedClip;
		void OnPlayerDamaged(float dmg){ PlaySound(playerDamagedClip); }
		
		[Tooltip("audioclip to play when the player is destroyed")]
		public AudioClip playerDestroyedClip;
		void OnPlayerDestroyed(){ PlaySound(playerDestroyedClip); }
		
		[Tooltip("audioclip to play when the player is respawned")]
		public AudioClip playerRespawnedClip;
		void OnPlayerRespawned(){ PlaySound(playerRespawnedClip); }
		
		
		[Space(10)]
		[Tooltip("audioclip to play when the player's weapon failed to fire (out of bullet or reloading)")]
		public AudioClip fireFailedClip;
		void OnFireFail(string msg){ PlaySound(fireFailedClip); }
		
		[Tooltip("audioclip to play when the player's weapon alt-ability failed to fire")]
		public AudioClip fireAltFailedClip;
		void OnFireAltFail(string msg){ PlaySound(fireAltFailedClip); }
		
		[Tooltip("audioclip to play when the player's ability fail to fire")]
		public AudioClip fireAbilityFailedClip;
		void OnAbilityActivationFail(string msg){ PlaySound(fireAbilityFailedClip); }
		
		
		[Space(10)]
		[Tooltip("audioclip to play when the player level up")]
		public AudioClip playerLevelUpClip;
		void OnPlayerLevelUp(UnitPlayer player){ PlaySound(playerLevelUpClip); }
		
		[Tooltip("audioclip to play when the player successfully purchased a perk")]
		public AudioClip perkPurchasedClip;
		void OnPerkPurchasedUp(Perk perk){ PlaySound(perkPurchasedClip); }


    [Tooltip("audioclip to play in background")]
    public AudioClip backgroundClip;

    [Space(10)]
		[Tooltip("audioclip to play when the player unit is moving")]
		public bool playMoveIdleSound=false;
		public float moveIdleVolume=0.5f;
		
		[Tooltip("audioclip to play when the player unit is moving")]
		public AudioClip playerMoveClip;
		
		[Tooltip("audioclip to play when the player unit is not moving")]
		public AudioClip playerIdleClip;
		
		private UnitPlayer player;
		private AudioSource moveIdleAudioSource;
		
		void Update(){
			player=GameControl.GetPlayer();
			
			if(player!=null && playMoveIdleSound){
				if(player.GetVelocity()>0.15f){
					if(playerMoveClip!=null){
						if(moveIdleAudioSource.clip!=playerMoveClip){
							moveIdleAudioSource.clip=playerMoveClip;
							moveIdleAudioSource.Play();
						}
						else{
							if(!moveIdleAudioSource.isPlaying) moveIdleAudioSource.Play();
						}
					}
					else if(moveIdleAudioSource.isPlaying) moveIdleAudioSource.Stop();
				}
				else{
					if(playerIdleClip!=null){
						if(moveIdleAudioSource.clip!=playerIdleClip){
							moveIdleAudioSource.clip=playerIdleClip;
							moveIdleAudioSource.Play();
						}
						else{
							if(!moveIdleAudioSource.isPlaying) moveIdleAudioSource.Play();
						}
					}
					else{
						if(moveIdleAudioSource.isPlaying) moveIdleAudioSource.Stop();
					}
				}
			}
		}
		
		private void CreateMoveIdleAudioSource(){
			if(moveIdleAudioSource!=null) return;
			
			GameObject mObj=new GameObject();
			mObj.name="moveIdleAudioSource";
			mObj.transform.parent=transform;
			mObj.transform.localPosition=Vector3.zero;
			
			moveIdleAudioSource=mObj.AddComponent<AudioSource>();
			moveIdleAudioSource.playOnAwake=false;
			moveIdleAudioSource.loop=true;
			moveIdleAudioSource.spatialBlend=0;
			moveIdleAudioSource.volume=volume*moveIdleVolume;
		}
		
	}

}