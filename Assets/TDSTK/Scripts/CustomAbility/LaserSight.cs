using UnityEngine;
using System.Collections;

using TDSTK;

public class LaserSight : MonoBehaviour {

	IEnumerator Start(){
		if(transform.parent==null){
			Destroy(gameObject);
			yield break;
		}
		
		Unit unit=transform.parent.GetComponent<Unit>();
		
		if(unit==null) yield break;
		
		transform.parent=unit.turretObj;
		transform.localRotation=Quaternion.identity;
	}
	
}
