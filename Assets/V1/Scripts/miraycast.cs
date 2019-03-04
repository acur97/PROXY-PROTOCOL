using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class miraycast : MonoBehaviour {

    public float DistanciaParaAtacar = 3;
    public string tagVictima = "Kyla";
    public Animator animAtacador;
    public string nombreDelParametro;
    

    private bool mirado;

	void Start () {

        animAtacador = GetComponent<Animator>();
		
	}
	

	void Update () {

        RaycastHit hit;
        Ray RayoAmenazador = new Ray(transform.position + Vector3.up, transform.forward); //Enlazar el rayo con el objeto
        Debug.DrawRay(RayoAmenazador.origin, RayoAmenazador.direction, Color.red);

        if (!mirado)
        {
            if (Physics.Raycast(RayoAmenazador, out hit, DistanciaParaAtacar))
            {
                if (hit.collider.tag == tagVictima)
                {
                    Debug.Log("Puedo Activar Animacion :D");
                    animAtacador.SetTrigger(nombreDelParametro);
                    //AnimacionDeAtaque(); //colocar un public que indique donde esta el animator que contiene el la animacion ataque
                }
            }
        }

	}

    //void AnimacionDeAtaque()
    //{

    //}
}
