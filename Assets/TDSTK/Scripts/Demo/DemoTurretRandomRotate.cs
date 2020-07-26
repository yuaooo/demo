using UnityEngine;
using System.Collections;

public class DemoTurretRandomRotate : MonoBehaviour {

	public float min=-30;
	public float max=30;
	
	private Quaternion targetRot;
	private float rotateSpeed;
	
	// Use this for initialization
	void Start () {
		StartCoroutine(RotateRoutine());
	}
	
	IEnumerator RotateRoutine(){
		yield return new WaitForSeconds(Random.Range(1f, 5f));
		while(true){
			rotateSpeed=Random.Range(3, 6);
			float val=Random.Range(min, max);
			targetRot=Quaternion.Euler(0, val, 0);
			yield return new WaitForSeconds(Random.Range(3f, 5f));
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.localRotation=Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime*rotateSpeed);
	}
	
}
