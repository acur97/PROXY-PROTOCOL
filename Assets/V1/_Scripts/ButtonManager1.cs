using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;




public class ButtonManager1 : MonoBehaviour
{


  
    

    public void NewGameButton ()
    {
        
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1;

    }

    public void QuitGameButton()
    {
        Application.Quit();
    }

}
