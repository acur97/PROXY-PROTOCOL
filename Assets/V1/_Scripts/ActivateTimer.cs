using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ActivateTimer : MonoBehaviour {

    private float timer = 30f;
    public Text timerSeconds;
    public GameObject fade;

    void Start ()
    {
        timerSeconds.GetComponent<Text>();

        fade.SetActive(false);
	}

    // Update is called once per frame
    void Update() {

        timer -= Time.deltaTime;
        string minutes = ((int)timer / 60).ToString();
        string seconds = (timer % 60).ToString("f0");
        timerSeconds.text = minutes + " : " + seconds;

        if(timer <= 0)
        {
            fade.SetActive(true);
        }
    }
}
