using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDSTK;

namespace TDSTK{

	public class TDSPath : MonoBehaviour {

		public List<Transform> waypointList=new List<Transform>();
		
		
		public bool showGizmo=true;
		public Color gizmoColor=Color.blue;
		void OnDrawGizmos(){
			if(!showGizmo) return;
			
			Gizmos.color = gizmoColor;
			
			for(int i=1; i<waypointList.Count; i++){
				if(waypointList[i-1]!=null && waypointList[i]!=null)
					Gizmos.DrawLine(waypointList[i-1].position, waypointList[i].position);
			}
		}
		
	}

}