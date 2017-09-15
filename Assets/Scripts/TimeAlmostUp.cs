using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeAlmostUp : MonoBehaviour {

	private Animator anim;

	void Start () {
		// Text starts out invisible
		Color invisible = gameObject.GetComponent<Text>().color;
		invisible.a = 0f;
		gameObject.GetComponent<Text>().color = invisible;

		anim = gameObject.GetComponent<Animator>();
	}

	// Make text appear and fade out when 30 seconds left
	public void Animate(){
		anim.SetTrigger("only30Sec");
	}
}
