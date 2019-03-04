using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Skip1 : MonoBehaviour
{
    public GameObject texto;
    private float clics;
    public VideoPlayer video;
    public GameObject fade;

    private void Start()
    {
        clics = 0;
        fade.SetActive(false);
    }

    private void LateUpdate()
    {
        if (video.isPlaying == false)
        {
            fade.SetActive(true);
            LoadNextSceneAsync();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            clics = clics + 1;
            texto.SetActive(true);
        }

        if (clics == 2)
        {
            LoadNextSceneAsync();
        }
    }

    private void LoadNextSceneAsync()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
        clics = 0;
    }

}