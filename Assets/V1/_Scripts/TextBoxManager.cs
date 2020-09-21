using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]

    public class TextBoxManager : MonoBehaviour
    {

        public GameObject textBox;
        public TextAsset textFile;        
        public Text theText;
        public ThirdPersonUserControl player;        
        

        public int currentLine;
        public int endAtLine;
        public string[] textLines;

        public bool isActive;
        public bool stopPlayerMovement;


        void Start()
        {
            player = FindObjectOfType<ThirdPersonUserControl>();

            if (textFile != null)
            {
                textLines = textFile.text.Split('\n');
            }

            if(endAtLine == 0)
            {
                endAtLine = textLines.Length - 1;
            }

            if (isActive)
            {
                EnableTextBox();
            }
            else
            {
                DisableTextBox();
            }

            
        }

        
        void Update()
        {
            if (!isActive)
            {
                return;
            }

            theText.text = textLines[currentLine];

            if (Input.GetKeyDown(KeyCode.C))
            {
                currentLine += 1;
                
            }

            if(currentLine > endAtLine)
            {
                DisableTextBox();
            }                       
            
        }


        

        public void EnableTextBox()
        {
            textBox.SetActive(true);
            isActive = true;

            if (stopPlayerMovement)
            {
                player.canMove = false;
            }
        }
        
        public void DisableTextBox()
        {
            textBox.SetActive(false);
            isActive = false;

            player.canMove = true;
        }

        public void ReloadScript(TextAsset theText)
        {
            if (theText != null)
            {
                textLines = new string[1];
                textLines = theText.text.Split('\n');
            }
        }
    }
}