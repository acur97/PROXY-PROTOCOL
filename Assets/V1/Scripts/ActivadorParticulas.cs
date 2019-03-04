using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivadorParticulas : MonoBehaviour {

    private void Start()
    {
        gameObject.SetActive(false);
    }

    void OnCollisionStay(Collision jugador)
    {
        if (jugador.gameObject.tag == "Kyla")
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
