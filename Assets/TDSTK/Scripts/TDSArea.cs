using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	[ExecuteInEditMode]
	public class TDSArea : MonoBehaviour {

		public enum _AreaType{ Square, Circle }
		public _AreaType type;
		
		private Transform thisT;
		
		void Awake(){
			thisT=transform;
		}
		
		
		public Quaternion GetRotation(){
			return Quaternion.Euler(0, thisT.rotation.eulerAngles.y, 0);
		}
		
		
		public Vector3 GetPosition(){
			if(type==_AreaType.Square){
				float x=Random.Range(-thisT.localScale.x, thisT.localScale.x);
				float z=Random.Range(-thisT.localScale.z, thisT.localScale.z);
				Vector3 v=thisT.position+thisT.rotation*new Vector3(x, 0, z);
				return v;
			}
			else if(type==_AreaType.Circle){
				Vector3 dir=new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
				return thisT.position+thisT.rotation*dir*Random.Range(0, GetMaximumScale());
			}
			
			return thisT.position;
		}
		
		float GetMaximumScale(){
			if(thisT!=null){
				float scale=Mathf.Max(thisT.localScale.x, thisT.localScale.y);
				return Mathf.Max(scale, thisT.localScale.z);
			}
			else{
				float scale=Mathf.Max(transform.localScale.x, transform.localScale.y);
				return Mathf.Max(scale, transform.localScale.z);
			}
		}
		
		[HideInInspector] public Color gizmoColor;
		void OnDrawGizmos() {
			Gizmos.color = gizmoColor;
			
			if(type==_AreaType.Square){
				Vector3 p1=transform.position+transform.rotation*new Vector3(transform.localScale.x, 0, transform.localScale.z);
				Vector3 p2=transform.position+transform.rotation*new Vector3(transform.localScale.x, 0, -transform.localScale.z);
				Vector3 p3=transform.position+transform.rotation*new Vector3(-transform.localScale.x, 0, transform.localScale.z);
				Vector3 p4=transform.position+transform.rotation*new Vector3(-transform.localScale.x, 0, -transform.localScale.z);
				
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p1, p3);
				Gizmos.DrawLine(p2, p4);
				Gizmos.DrawLine(p3, p4);
			}
			else if(type==_AreaType.Circle){
				Gizmos.DrawWireSphere(transform.position, GetMaximumScale());
			}
			
			//Gizmos.DrawIcon(transform.position, "SpawnArea.png", true);
		}
		
	}

}