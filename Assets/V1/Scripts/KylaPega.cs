using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KylaPega : MonoBehaviour {

	public Slider SliderMalo1;
	public Slider SliderMalo2;
	public float Ataque;

	void OnCollisionEnter(Collision jugador)
	{
		if (jugador.gameObject.tag == "malos" && Input.GetKeyDown(KeyCode.X))

		// if (jugador.gameObject.tag == "malos")
		{
			SliderMalo1.value = (SliderMalo1.value - (Ataque));
			SliderMalo2.value = (SliderMalo2.value - (Ataque));
		}
	}
}
