using UnityEngine;
using System.Collections;

public class RTEX : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        new RenderTexture(2, 2, 2, RenderTextureFormat.ARGB32);
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    void OnGUI()
    {
        Graphics.Blit(new Texture(), new Material(Shader.Find("Diffuse")));
    }
}
