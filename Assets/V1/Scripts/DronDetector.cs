using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


public class DronDetector : MonoBehaviour {

    Transform pillada;
    public string TagKyla = "Kyla";
    public NavMeshAgent NavDelDron;
    bool Detectada = false;
    public Light LuzDelDron;
    public Animator AnimDron;

    public bool estaDisparando = false;
    public Bala bala;
    public float velocidadBala;
    public float delayDeDisparos;
    float contadorBalas;
    public Transform PosicionBala;
    bool mirarAKyla = false;

    public Transform GuardiaDron;

    public float VistaDron = 15f;
    public float VistaPuntoGuardia = 2f;


    void Start ()
    {

        pillada = GameObject.FindGameObjectWithTag(TagKyla).transform;
        NavDelDron = GetComponent<NavMeshAgent>();
        LuzDelDron = GetComponent<Light>();
        AnimDron = GetComponent<Animator>();
        AnimDron.enabled = true;

    }


    void FixedUpdate() {
        estaDisparando = false;
 

        RaycastHit hit;
        Ray RayoAmenazador = new Ray(transform.position + Vector3.down, transform.forward); //Enlazar el rayo con el objeto
        Debug.DrawRay(RayoAmenazador.origin, RayoAmenazador.direction, Color.red);

        if (Physics.Raycast(RayoAmenazador, out hit, VistaDron))
        {
            if (hit.collider.tag == TagKyla)
            {
                estaDisparando = true;

                if (estaDisparando)
                {
                    contadorBalas -= Time.deltaTime;
                    if (contadorBalas <= 0)
                    {
                        contadorBalas = delayDeDisparos;
                        Bala newbala = Instantiate(bala, PosicionBala.position, PosicionBala.rotation) as Bala;
                        newbala.velocidad = velocidadBala;
                    }
                    else
                    {
                        contadorBalas = 0;
                    }
                }
            }

        }
        if (Physics.Raycast(RayoAmenazador, out hit, VistaPuntoGuardia))
        {
            if (hit.collider.name == GuardiaDron.name)
            {
                AnimDron.enabled = true;
            }
        }


        if (Detectada == true)
        {
            AnimDron.enabled = false;
            transform.LookAt(pillada);
            LuzDelDron.color = Color.red;
            NavDelDron.SetDestination(pillada.position);
        }
 

        if (Detectada == false)
        {
            LuzDelDron.color = new Color32(203, 158, 116, 255);
            NavDelDron.SetDestination(GuardiaDron.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == TagKyla)
        {
            Detectada = true;
            
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.gameObject.tag == TagKyla)
        {
            Detectada = false;
        }
    }


}
