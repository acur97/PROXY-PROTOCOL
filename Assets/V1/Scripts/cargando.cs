using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class cargando : MonoBehaviour
{
    [SerializeField]
    private bool mChangeLevel = true;
    private float timer = 1f;
    public GameObject fade;

    void Start()
    {
        mChangeLevel = true;
        fade.SetActive(false);
    }

    private void FixedUpdate()
    {
        timer = timer + 1;
        if (timer == 300)
        {
            timer = 1f;
        }
    }

    void Update()
    {
        if(mChangeLevel && timer > 280)
        {
            LoadNextSceneAsync();
            mChangeLevel = false;
        }
    }

    private void LoadNextSceneAsync()
        {
            fade.SetActive(true);
            //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
        }


}