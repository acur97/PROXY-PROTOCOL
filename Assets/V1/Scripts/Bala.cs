using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bala : MonoBehaviour {

	private Slider SliderVida;
	public float Dolor = 10;
	public string tagDeKyla = "Kyla";
    public float velocidad;

	// Use this for initialization
	void Start ()
	{
		SliderVida = GameObject.Find ("SVida").GetComponent<Slider> ();
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
        StartCoroutine(AutoDestruccionBala);

    }

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.tag == tagDeKyla) 
		{
			SliderVida.value = (SliderVida.value - (Dolor/10));
			Destroy (gameObject);
		}
        if (other.gameObject.tag == "Piso")
        {
            Destroy(gameObject);
        }

    }
    IEnumerator AutoDestruccionBala()
    {
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);

    }
}
