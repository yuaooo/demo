//scroll the texture offset of the renderer material
//used in side scroller level

using UnityEngine;
using System.Collections;

public class TextureScroll : MonoBehaviour {
	
	public float scrollSpeedX = 0.5F;
	public float scrollSpeedY = 0.5F;
    public Renderer rend;
	
    void Start() {
        rend = GetComponent<Renderer>();
    }
	
    void Update() {
        float offsetX = Time.time * scrollSpeedX * 0.1f;
        float offsetY = Time.time * scrollSpeedY * 0.1f;
        rend.material.SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));
    }
	
}
