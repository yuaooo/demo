using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	public class TDSUtility : MonoBehaviour{
		public static Vector3 GetRandPosFromPoint(Vector3 origin, float radius){
			Vector3 dir=new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
			return origin+dir*Random.Range(0, radius);
		}
		
		public static void DestroyColliderRecursively(Transform root){
			foreach(Transform child in root) {
				if(child.GetComponent<Collider>()!=null) {
					Destroy(child.GetComponent<Collider>());
				}
				DestroyColliderRecursively(child);
			}
		}
		
		public static Unit GetUnitOfParentTransform(Transform root){
			Unit unit=root.GetComponent<Unit>();
			if(unit==null){
				if(root.parent!=null) return GetUnitOfParentTransform(root.parent);
				else return null;
			}
			return unit;
		}
	}
	
}
