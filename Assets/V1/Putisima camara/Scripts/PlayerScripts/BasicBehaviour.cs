using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class BasicBehaviour : MonoBehaviour
{
	public Transform playerCamera;                 // Reference to the camera that focus the player.
	public float sprintFOV = 170f;                 // the FOV to use on the camera when player is sprinting.
    public Slider SStamina;
	private ThirdPersonOrbitCam camScript;         // Reference to the third person camera script.
    private bool sprint;

    void Awake ()
	{
        camScript = playerCamera.GetComponent<ThirdPersonOrbitCam>();
    }

	void Update()
	{
        sprint = Input.GetButtonDown("Fire2");

        // Set the correct camera FOV.
        if (sprint == true && SStamina.value > 249)
        {
            camScript.SetFOV(sprintFOV);
        }
        else
        {
            camScript.ResetFOV();
        }
    }
    
}
