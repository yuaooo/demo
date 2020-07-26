//this is just to adjust the texture scale of the various grid object in the environment to show the correct grid size in runtime
//adjust the texture scale of the material to match the scale of the transform

using UnityEngine;
using System.Collections;

public class AutoAdjustGrid : MonoBehaviour {
	
	public float gridSize=0.5f;
	
	// Use this for initialization
	void Start () {
		Renderer rend=transform.GetComponent<Renderer>();
		if(rend==null) return;
		
		Material mat=rend.material;
		
		mat.mainTextureScale=new Vector2(transform.localScale.x*gridSize, transform.localScale.y*gridSize);
	}
	
}