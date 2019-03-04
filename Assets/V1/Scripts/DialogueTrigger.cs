using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{


    public class DialogueTrigger : MonoBehaviour {

        [Header("Dialogue Config")]
        [SerializeField]
        private Text dialogueText;
        public GameObject textBox;
        [Multiline]
        [SerializeField]
        private string dialogue;
        [SerializeField]
        private float characterSpeed = 0.005f;

        

        private string[] dialogueLines;
        private int currentLine;
        

        private void Start()
        {
            
            dialogueLines = (dialogue.Split('\n'));
            
        }

        

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Kyla"))
            {
                StartCoroutine(ShowDialogue());
            }
        }

        private IEnumerator ShowDialogue()
        {
            while (currentLine < dialogueLines.Length)
            {
                int index = 0;
                dialogueText.text = "";
                while (index < dialogueLines[currentLine].Length)
                {
                    dialogueText.text += dialogueLines[currentLine][index++];
                    yield return new WaitForSeconds(characterSpeed);
                }

                yield return StartCoroutine(WaitForKeyDown(KeyCode.C));
            }

            //Aqui se sabe que terminó de leer todas las lineas
            dialogueText.text = "";
        }

        IEnumerator WaitForKeyDown(KeyCode keyCode)
        {
            while (!Input.GetKeyDown(keyCode))
            {
                yield return null;
            }
            currentLine++;
        }
    }
}