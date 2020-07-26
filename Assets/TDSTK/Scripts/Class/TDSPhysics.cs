//a class that handle all the physics

using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	public class TDSPhysics{
		//for applying force from 1 shootobject to its impact target
		public static void ApplyAttackForce(Vector3 impactPoint, Vector3 impactDir, GameObject hitObj, AttackStats aStats){
			float impactForce=aStats.impactForce*10;
			
			if(impactForce>0){
				Rigidbody rigidbody=hitObj.GetComponent<Rigidbody>();
				if(rigidbody!=null)
					rigidbody.AddForce(impactDir*impactForce, ForceMode.Impulse);
			}
			
			ApplyExplosionForce(impactPoint, aStats);
		}
		
		//for explosion
		public static void ApplyExplosionForce(Vector3 impactPoint, AttackStats aStats, bool ignorePlayer=false){
			float explosionForce=aStats.explosionForce*10;
			float explosionRadius=explosionForce>0 ? aStats.explosionRadius : 0;
			
			if(explosionRadius>0){
				LayerMask mask=ignorePlayer ? ~(1<<TDS.GetLayerPlayer()) : ~0;
				Collider[] cols=Physics.OverlapSphere(impactPoint, explosionRadius, mask);
				for(int i=0; i<cols.Length; i++){
					Rigidbody rd=cols[i].gameObject.GetComponent<Rigidbody>();
					if(rd!=null) rd.AddExplosionForce(explosionForce, impactPoint, explosionRadius, 0, ForceMode.Impulse);
				}
			}
		}
	}
	
}
