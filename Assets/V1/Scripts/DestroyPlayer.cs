using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlayer : MonoBehaviour {

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Music").GetComponent<MusicControl>().StopMusic();
    }
    
}
