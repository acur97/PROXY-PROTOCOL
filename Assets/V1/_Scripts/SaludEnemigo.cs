using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class SaludEnemigo : MonoBehaviour {

	public Slider SliderVidaMalo;
	public float Vida = 100f;
    public Animator AnimatorEnemy;
    public GameObject explocion;

    private bool asesinado = false;

    void starFalso()
    {
        AnimatorEnemy = GetComponent<Animator>();
        asesinado = false;
    }

	void Start () {
		SliderVidaMalo.value = Vida;
	}

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "Sword")
        {
            SliderVidaMalo.value = SliderVidaMalo.value - 50f;
        }
    }

    void FixedUpdate()
	{
		if (SliderVidaMalo.value == 0)
		{
            asesinado = true;
            explocion.SetActive(true);
        }
        if (asesinado == true)
        {
            AnimatorEnemy.SetTrigger("Muerto");
            StartCoroutine("Matado");
        }
	}

    IEnumerator Matado()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);

    }
}
