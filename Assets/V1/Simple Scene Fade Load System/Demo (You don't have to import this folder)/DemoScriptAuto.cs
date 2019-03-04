using UnityEngine;
using System.Collections;

public class DemoScriptAuto : MonoBehaviour
{

    public string scene;
    public Color loadToColor = Color.white;
    private bool prendido = false;

    private void Start()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    void LateUpdate()
    {
        if (gameObject.activeSelf == true)
        {
            fade();
            prendido = true;
        }
    }

    public void fade()
    {
        if (prendido == true)
        {
            Initiate.Fade(scene, loadToColor, 0.25f);
        }
    }
}