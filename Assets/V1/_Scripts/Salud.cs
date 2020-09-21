using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Salud : MonoBehaviour {

    public Slider SliderVida;
    public Slider SliderStamina;
    public float VidaKyra = 100f;
    public float PorcentajeEstamina = 1000f;
    public GameObject UI;
    private float timer = 1f;
    public float TiempoEsperaMuerto = 20f;
    public GameObject VideoMuerte;
    public Animator AnimatorKyla;

    [Range(0, 1000)]
    private float timer2 = 1f;

    void startFalso()
    {
        AnimatorKyla = GetComponent<Animator>();
    }

    void Start()
    {
        SliderVida.value = VidaKyra;
        SliderStamina.value = PorcentajeEstamina;
    }

    void FixedUpdate()
    {
        if (SliderVida.value == 0)
        {
            Time.timeScale = 0.05f;
            AnimatorKyla.SetTrigger("Muerta");
            VideoMuerte.gameObject.SetActive(true);
            UI.gameObject.SetActive(false);

            timer = timer + 1;
            if (timer == TiempoEsperaMuerto)
            {
                timer = 1f;
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
                Time.timeScale = 1f;
            }

        }
        else
        {
            UI.gameObject.SetActive(true);
            VideoMuerte.gameObject.SetActive(false);
        }

        
        if (SliderStamina.value < 1000)
        {
            timer2 = timer2 + 1;
            SliderStamina.value = SliderStamina.value + 1;
        }
    }
}
