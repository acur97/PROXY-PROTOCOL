using UnityEngine;
using System.Collections;

public class DemoScript : MonoBehaviour {

    public string scene;
	public Color loadToColor = Color.white;
	

	public void fade()
        {
        Initiate.Fade(scene,loadToColor,0.5f);	
		}
	}