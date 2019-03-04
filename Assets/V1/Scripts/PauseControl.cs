using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseControl : MonoBehaviour {

    public Transform canvas;
    public Transform UI;
  

    void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (canvas.gameObject.activeInHierarchy == false)
            {
                canvas.gameObject.SetActive(true);
                Time.timeScale = 0;
                UI.gameObject.SetActive(false);
                Cursor.visible = true;
            }
            else
            {
                canvas.gameObject.SetActive(false);
                Time.timeScale = 1;
                UI.gameObject.SetActive(true);
                Cursor.visible = false;
            }
            
        }
		
	}
}
