using UnityEngine;
using System.Collections;

public class controladorAnim : MonoBehaviour 
{
	Animator anim;
	int jumpHash = Animator.StringToHash("salto");
	int runHash = Animator.StringToHash("correr");


	void Start ()
	{
		anim = GetComponent<Animator>();
	}


	void Update ()
	{
		float move = Input.GetAxis ("Vertical");
		anim.SetFloat("velocidad", move);
		Debug.Log (move);

		if (Input.GetKey ("space")) {
			anim.SetBool (jumpHash, true);
		} else
			anim.SetBool (jumpHash, false);

		if (Input.GetKey (KeyCode.A)) {
			anim.SetBool (runHash, true);
		} else
			anim.SetBool (runHash, false);
	}
}