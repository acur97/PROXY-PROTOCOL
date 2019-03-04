using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy2 : MonoBehaviour
{

    Transform player;
    Transform puntoGuardia;
    public string NombreDeGuardia;
    UnityEngine.AI.NavMeshAgent nav;
    public string NombreDelTag = "Kyla";
    public Animator Anim;
    bool corriendo = false;
    public float DistanciaParaAtacar;
    public string nombreParametroAtaque;
    public string parametroGuardia = "CaminarGuardia";
    public float disMinGuardia = 0.1f;
    bool atacando = false;
    public GameObject pared;



    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag(NombreDelTag).transform;
        puntoGuardia = GameObject.Find(NombreDeGuardia).transform;
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        atacando = false;

        RaycastHit hit;
        Ray RayoAmenazador = new Ray(transform.position + Vector3.up, transform.forward); //Enlazar el rayo con el objeto
        Debug.DrawRay(RayoAmenazador.origin, RayoAmenazador.direction, Color.red);


        if (Physics.Raycast(RayoAmenazador, out hit, DistanciaParaAtacar))
        {

            if (hit.collider.tag == NombreDelTag)
            {
                atacando = true;
            }
            else
            {
                atacando = false;
            }


        }


        if (Physics.Raycast(RayoAmenazador, out hit, Mathf.Infinity))
        {
            if (hit.collider.name == NombreDeGuardia)
            {
                Anim.SetBool(parametroGuardia, true);
            }
        }
        if (Physics.Raycast(RayoAmenazador, out hit, disMinGuardia))
        {
            if (hit.collider.name == NombreDeGuardia)
            {
                Anim.SetBool(parametroGuardia, false);
            }
        }


        if (atacando == true)
        {
            Anim.SetBool(nombreParametroAtaque, true);
        }

        if (atacando == false)
        {
            Anim.SetBool(nombreParametroAtaque, false);
        }


        if (corriendo == true)
        {

            nav.SetDestination(player.position);
        }

        if (corriendo == false)
        {
            nav.SetDestination(puntoGuardia.position);

        }



        if (pared.activeSelf == true)
        {
            corriendo = true;
            Anim.SetBool("Sigueme", true);
        }
        else
        {
            corriendo = false;
            Anim.SetBool("Sigueme", false);
        }
        
    }

}