using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class countdownTimer : MonoBehaviour {

       
    public ActivateTimer active;
    public Text timerSeconds;
    public GameObject timerr;
    public GameObject pared;

    private void Start()
    {
        active.GetComponent<ActivateTimer>();
        active.enabled = false;
        pared.SetActive(false);

        timerr.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Kyla"))
        {
            timerSeconds.enabled = true;
            active.enabled = true;
            pared.SetActive(true);
            timerr.SetActive(true);
        }
        
    }
        
    
}
