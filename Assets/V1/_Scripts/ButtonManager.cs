using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;




public class ButtonManager : MonoBehaviour
{

    private void Start()
    {

        Cursor.visible = true;
    }

    public void NewGameButton ()
    {
        SceneManager.LoadScene("Cinematografica");

    }
    public void QuitGameButton()
    {
        Application.Quit();
    }


}
