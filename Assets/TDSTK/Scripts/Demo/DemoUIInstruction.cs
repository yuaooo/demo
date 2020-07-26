using UnityEngine;
using System.Collections;

public class DemoUIInstruction : MonoBehaviour {

	private bool show=false;
	
	public GameObject instructionObjShow;
	public GameObject instructionObjHide;
	
	// Use this for initialization
	void Start () {
		UpdateShow();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)){
			show=!show;
			UpdateShow();
		}
	}
	
	
	void UpdateShow(){
		instructionObjShow.SetActive(show);
		instructionObjHide.SetActive(!show);
	}
	
}
