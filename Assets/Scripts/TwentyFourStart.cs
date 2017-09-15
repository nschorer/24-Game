using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwentyFourStart : MonoBehaviour {

	public Button startButton;
	public Button howToPlayButton;
	public Button exitButton;
	public Button okButton;
	public GameObject instructionsPanel;
	public Text instructionsText;
	public LevelManager levelManager;
	public AudioClip buttonSound;

	// Use this for initialization
	void Start () {

		Screen.SetResolution(800, 600, false);

		// Some buttons are immediately visible, some aren't
		ShowInstructions(false);

	}

	// Either we show the title screen and its buttons
	// or we show the instruction screen
	private void ShowInstructions (bool show = true)
	{
		// Primary screen
		startButton.gameObject.SetActive(!show);
		howToPlayButton.gameObject.SetActive(!show);
		exitButton.gameObject.SetActive(!show);

		// Instructions screen
		okButton.gameObject.SetActive(show);
		instructionsPanel.gameObject.SetActive(show);
		instructionsText.gameObject.SetActive(show);
	}

	public void StartButtonClicked(){
		AudioSource.PlayClipAtPoint(buttonSound, Vector3.zero);
		if (levelManager == null) Debug.LogError("LevelManager not assigned.");
		levelManager.LoadNextLevel();
	}

	public void HowToPlayButtonClicked (){
		AudioSource.PlayClipAtPoint(buttonSound, Vector3.zero);
		ShowInstructions();
	}

	public void ExitButtonClicked (){
		AudioSource.PlayClipAtPoint(buttonSound, Vector3.zero);
		Application.Quit();
	}

	public void OkButtonClicked (){
		AudioSource.PlayClipAtPoint(buttonSound, Vector3.zero);
		ShowInstructions(false);
	}
}
