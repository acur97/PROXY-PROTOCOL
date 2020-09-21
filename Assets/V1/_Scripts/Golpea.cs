using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golpea : MonoBehaviour {

    public float Distancia = 2f;
    public GameObject other1;
    public GameObject other2;

    void Update ()
    {
        if (Input.GetKey(KeyCode.G))
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.SphereCast(ray, Distancia, out hit, 2))
            {
                Debug.Log("SphereCast con >> " + hit.transform.name);
                if (hit.transform.name == "AIThirdPersonController")
                    Destroy(other1);
                if (hit.transform.name == "AIThirdPersonController (1)")
                    Destroy(other2);

            }
        }
    }
}
