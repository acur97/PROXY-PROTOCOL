using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlayer : MonoBehaviour {

    private GameObject ob;

    private void Awake()
    {
        ob = GameObject.FindGameObjectWithTag("Music");
        if (ob != null)
        {
            ob.GetComponent<MusicControl>().StopMusic();
        }
    }
    
}
