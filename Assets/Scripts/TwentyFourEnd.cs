using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwentyFourEnd : MonoBehaviour {

	public Text scoreText;
	public Text evaluationText;
	public LevelManager levelManager;
	public AudioClip buttonSound;
	public AudioClip doneSound;

	void Start (){
		// Force set the resolution
		Screen.SetResolution(800, 600, false);

		int score = Stats.getScore ();
		scoreText.text = score.ToString ();

		AudioSource.PlayClipAtPoint(doneSound, Vector3.zero);

		if (score < 25) {
			evaluationText.text = "I bet you could do even better next time!";
		} else if (score < 60) {
			evaluationText.text = "You've played this before, haven't you?";
		} else {
			evaluationText.text = "Teach me your ways!";
		}
	}
	
	public void BackToTitleButtonClicked (){
		AudioSource.PlayClipAtPoint(buttonSound, Vector3.zero);
		levelManager.LoadStart();
	}
}
