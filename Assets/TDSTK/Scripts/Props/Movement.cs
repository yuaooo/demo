//utilities script to move various object


using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{

	public class Movement : MonoBehaviour {
		
		public enum _MovType{
			PingPoing,
			OneShot,
			Loop
		}
		
		[Tooltip("The movement type\nPingPong: move back and forth between the limits\nOneShot: Destroy object when it hits limit\nLoop: wrap back to origin and start over when hits limit")]
		public _MovType type;
		
		public enum _Axis{ x_axis, y_axis, z_axis, }
		
		[Tooltip("The direction to move")]
		public _Axis moveAxis;
		private Vector3 travelV;
		
		[Tooltip("The reference space of the move direction. Could be of the world or self")]
		public Space space;
		
		private Transform thisT;
		public Vector3 startPos;
		
		private float dir=1;
		
		[Tooltip("The distance limit of the object from the origin (object starting point)")]
		public float limit=15;	
		public float speed=5;	//movement speed
		
		[Tooltip("Check to randomize the move speed upon starting the script")]
		public bool randomizeSpeed=false;
		[Tooltip("minimum move speed, only applicable if RandomizeSpeed is enabled")]
		public float speedMin=3;
		[Tooltip("maximum move speed, only applicable if RandomizeSpeed is enabled")]
		public float speedMax=6;
		
		
		private UnitAI unit;
		
		
		void Awake(){
			thisT=transform;
			
			unit=GetComponent<UnitAI>();
		}
		
		
		void OnEnable(){
			startPos=thisT.position;
			
			if(randomizeSpeed) speed=Random.Range(speedMin, speedMax);
		}
		
		
		// Update is called once per frame
		void Update () {
			if(unit!=null && unit.IsStunned()) return;
			
			if(moveAxis==_Axis.x_axis) travelV=Vector3.right;
			if(moveAxis==_Axis.y_axis) travelV=Vector3.up;
			if(moveAxis==_Axis.z_axis) travelV=Vector3.forward;
			
			thisT.Translate(dir*travelV*speed*Time.deltaTime, space);
			
			//when hit limit, check move mode
			if(Vector3.Distance(thisT.position, startPos)>=limit){
				if(type==_MovType.PingPoing){
					dir*=-1;
					thisT.Translate(dir*travelV*speed*Time.deltaTime, space);
				}
				else if(type==_MovType.OneShot){
					if(unit!=null) unit.ClearUnit();
					else ObjectPoolManager.Unspawn(gameObject); //Destroy(gameObject);
				}
				else if(type==_MovType.Loop){
					thisT.position=startPos;
				}
			}
		}
		
	}

}
