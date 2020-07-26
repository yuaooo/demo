//wave class defination used for unit spawner

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{
	
	[System.Serializable]
	public class SubWave{
		public int count=5;
		public Unit unitPrefab;
		public float startDelay=0;
		public float interval=1;
		
		public TDSArea spawnArea;
	}
	
	
	[System.Serializable]
	public class Wave{
		[HideInInspector] public int waveID=-1;
		public List<SubWave> subWaveList=new List<SubWave>();
		
		public int creditGain=0;
		public int scoreGain=0;
		
		public TDSArea spawnArea;
		
		[HideInInspector] public int activeUnitCount=0;	//only used in runtime
		
		[HideInInspector] public int subWaveSpawned=0;
		
		[HideInInspector] public bool spawned=false; //flag indicating weather all unit in the wave have been spawn, only used in runtime
		[HideInInspector] public bool cleared=false; 	//flag indicating weather the wave has been cleared, only used in runtime
		
		//public float duration=10;						//duration until next wave
		
		public Wave(){
			subWaveList.Add(new SubWave());
		}
		
		public void Completed(){
			cleared=true;
			GameControl.GainCredits(creditGain);
			GameControl.GainScore(scoreGain);
		}
	}
	
}
