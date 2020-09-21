using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DannoEspada1 : MonoBehaviour {

	public Slider SliderVida;
    public float Dolor;
    public string NombreDelTag = "Kyla";



    void OnCollisionEnter(Collision jugador)
    {
		if (jugador.gameObject.tag == "Kyla")
        {
            SliderVida.value = (SliderVida.value - (Dolor/10));
        }
    }

    void OnCollisionStay(Collision jugador)
    {
        if (jugador.gameObject.tag == "Kyla")
        {
            SliderVida.value = (SliderVida.value - (Dolor / 20));
        }
    }
}
