using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class ActivateTextAtLine : MonoBehaviour {

    public TextAsset theText;

    public int startLine;
    public int endLine;

    public TextBoxManager theTextBox;
    public bool destroyWhenActivated;
    public ThirdPersonUserControl player;


    void Start () {

        theTextBox = FindObjectOfType<TextBoxManager>();
        player = FindObjectOfType<ThirdPersonUserControl>();
	}
	
	// Update is called once per frame
	void Update () {
		

	}

    private void OnTriggerEnter(Collider other)
    {
        

        if (other.CompareTag ("Kyla"))
        {
            theTextBox.ReloadScript(theText);
            theTextBox.currentLine = startLine;
            theTextBox.endAtLine = endLine;
            theTextBox.EnableTextBox();
            player.canMove = true;

            if (destroyWhenActivated)
            {
                Destroy(gameObject);
            }
        }
    }
}
