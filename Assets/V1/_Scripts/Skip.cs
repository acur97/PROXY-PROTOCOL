using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Skip : MonoBehaviour
{
    private float clics;
    public GameObject texto;
    public VideoPlayer video;

    private void Start()
    {
        texto.SetActive(false);
        clics = 0;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            clics = clics + 1;
            texto.SetActive(true);
        }

        if (clics == 2)
        {
            LoadNextSceneAsync();
        }

        if (video.isPlaying == false)
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