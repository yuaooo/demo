using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDSTK;

namespace TDSTK_UI{

	public class UIPlayerHitEffect : MonoBehaviour {

		private Image sprite;
		private float alpha=0;
		private float alphaMin=-.1f;
		
		public bool visibleWhenDamaged=false;
		public float fadeRate=1.5f;
		
		void Awake(){
			sprite=gameObject.GetComponent<Image>();
		}
		
		void OnEnable(){
			TDS.onPlayerDamagedE += Hit;
		}
		void OnDisable(){
			TDS.onPlayerDamagedE -= Hit;
		}
		
		void Hit(float dmg){
			alpha=Mathf.Max(alphaMin, alpha);
			alpha=Mathf.Min(alpha+Mathf.Clamp(dmg, 0.005f, 0.5f), 1);
			
			TDS.CameraShake(Mathf.Min(dmg, 0.65f));
		}
		
		void Update(){
			alpha-=Time.deltaTime*fadeRate;
			if(alpha>alphaMin) sprite.color=new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
			
			if(visibleWhenDamaged){
				UnitPlayer player=GameControl.GetPlayer();
				if(player!=null) alphaMin=.9f-player.hitPoint/player.hitPointFull;
			}
		}
		
	}

}