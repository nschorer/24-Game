using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AK;

public class TwentyFourGame : MonoBehaviour
{
	public Text currentResult;
	public Button[] buttons;
	public Text scoreText;
	public Image difficultyImage;
	public Sprite[] difficultySprites;
	public Text timerText;
	public int timeLimit = 500;
	public Text inputText;
	public LevelManager levelManager;
	public AudioClip numberSound;
	public AudioClip operationSound;
	public AudioClip deleteSound;
	public AudioClip skipSound;
	public AudioClip is24Sound;
	public AudioClip only30SecSound;

	private const int cardQueueSize = 10;
	private const string cardRegEx = @"\b[1-3]:\d{4}\b";
	private const string txtFile = "Problems_24";
	private const string audioFolder = "CasualGameSounds";

	private List<String> easyCards;
	private List<String> mediumCards;
	private List<String> hardCards;
	private ExpressionSolver solver;
	private Queue<string> recentCards = new Queue<string>();
	private bool inputChanged;
	private int score;
	private float timer;
	private bool is24;
	private bool gameover;
	private int diffPoints;
	private int[] buttonIndexes = new int[4];
	private float countdown;
	private bool only30SecWarningGiven;

	void Start (){
		// Force set the resolution
		Screen.SetResolution(800, 600, false);

		// Read in all of the cards from the text file, and randomly select one to display first
		LoadCards (txtFile);
		NewCard ();

		// Prepare the expression solver, which can dynamically evaluate math expressions
		solver = new ExpressionSolver ();
		//print(solver.SymbolicateExpression("3+1").Evaluate());

		// Prepare remaining UI elements
		score = 0;
		UpdateScoreText();
		timer = timeLimit;
	}

	void Update ()
	{
		TickTimer ();

		if (gameover) {
			GoToEndScreen();
		}

		if (inputChanged) {
			EvaluateExpression ();
		}

		// Note: countdown is different from timer
		// timer is the total game time
		// countdown is the half second we wait after the player puts in a correct solution before loading the next card
		countdown -= Time.deltaTime;
		if (is24 && countdown <= 0) {
			NewCard ();
			UpdateScoreText();
		}
	}

	// Read the text file and parse out the cards into different arrays
	private void LoadCards (string fileName)
	{
		// Read the file into txtAssets
		TextAsset txtAssets = (TextAsset)Resources.Load (txtFile);
		// Scan the file to find the cards
		Regex rgx = new Regex (cardRegEx);
		MatchCollection matches = rgx.Matches (txtAssets.text);
		if (matches.Count > 0) {
			List<String> cards = new List<String>();

			foreach (Match match in matches) {
				cards.Add(match.Value);
			}

			// Use a regular expression to sort the cards
			easyCards = cards.FindAll(x => x[0] == '1');
			mediumCards = cards.FindAll(x => x[0] == '2'); 
			hardCards = cards.FindAll(x => x[0] == '3');  
		}
	}

	// Load a new card into the buttons
	private void NewCard (int difficulty = 0)
	{
		// We need to specify 'System' because there is also Unity.Random
		System.Random rnd = new System.Random ();
		int index;
		string randomCard = "";
		bool validCard = false;

		if (difficulty == 0) {
			difficulty = rnd.Next (1, 4);
		}

		// Find a random card in the array based on the difficulty
		while (!validCard) {

			if (difficulty == 1) {
				index = rnd.Next (0, easyCards.Count);
				randomCard = easyCards [index];
				difficultyImage.sprite = difficultySprites [0];
			} else if (difficulty == 2) {
				index = rnd.Next (0, mediumCards.Count);
				randomCard = mediumCards [index];
				difficultyImage.sprite = difficultySprites [1];
			} else if (difficulty == 3) {
				index = rnd.Next (0, hardCards.Count);
				randomCard = hardCards [index];
				difficultyImage.sprite = difficultySprites [2];
			} else {
				Debug.LogError ("TwentyFourGame.NewCard: error finding difficulty");
			}

			// Make sure we didn't recently have this card
			if (randomCard != "" && !recentCards.Contains (randomCard))
				validCard = true;
		}

		// Remember the last 10 cards that we've pulled so we don't repeat cards too frequently
		recentCards.Enqueue (randomCard);
		if (recentCards.Count > cardQueueSize)
			recentCards.Dequeue ();

		// Set the numbers for the new card
		AssignButtons (randomCard);

		// reset various variables
		diffPoints = difficulty;
		is24 = false;
		inputText.text = "";
		inputChanged = true;

		// Make sure button does not stay highlighted after being selected
		EventSystem.current.SetSelectedGameObject(null);

	}

	// Set the numbers for the new card
	private void AssignButtons (string newCard){
		buttons[0].GetComponentInChildren<Text>().text = newCard[2].ToString();
		buttons[1].GetComponentInChildren<Text>().text = newCard[3].ToString();
		buttons[2].GetComponentInChildren<Text>().text = newCard[4].ToString();
		buttons[3].GetComponentInChildren<Text>().text = newCard[5].ToString();

		for (int i = 0; i < buttons.Length; i++) {
			buttons [i].enabled = true;
			buttonIndexes [i] = -1;
			buttons[i].interactable = true;
		}
	}

	private void UpdateScoreText (){
		scoreText.text = "Score: " + score.ToString();
	}

	public void EvaluateExpression ()
	{
		// Yeah...this is lazy, but we'll get a runtime error if it's null
		double result = -99999;
		try {
			// 'x' must be changed to '*' in order for the solver to read it as multiplication
			string expression = inputText.text.Replace('x', '*');
			result = solver.SymbolicateExpression (expression).Evaluate ();
		} catch {
			result = -99999;
		}

		if (result == -99999) {
			currentResult.text = "--";
		} else {
			currentResult.text = result.ToString ();
		}

		// Check if they got 24
		if (IsValidSolution(result)) {
			// Award points now. That way, if they solve it with only .1 seconds left, they still get the points.
			score += diffPoints;
			currentResult.color = Color.white;
			// Play noise clip
			AudioSource.PlayClipAtPoint(is24Sound, Vector3.zero);
			// Instead of immediately loading the next card, we wait the fraction of a second
			// to give the player a moment of satisfaction
			Countdown();
		} else {
			currentResult.color = Color.black;
		}

		inputChanged = false;
	}

	// Solution must equal 24 and must use all available numbers
	private bool IsValidSolution (double result)
	{
		bool isValid = true;

		if (result != 24) {
			isValid = false;
		} else {
			foreach (int bIdx in buttonIndexes) {
				if (bIdx == -1) isValid = false;
			}
		}

		return isValid;
	}

	// Player found a solution. Wait a moment before loading next card.
	private void Countdown(){
		countdown = 0.5f;
		is24 = true;
	}

	// When timer runs out game is over.
	private void TickTimer (){
		if (!gameover) {
			timer -= Time.deltaTime;
			timerText.text = Mathf.CeilToInt (timer).ToString ();

			if ((timer <= 30f) && !only30SecWarningGiven) {
				// Play noise clip
				GameObject.FindObjectOfType<TimeAlmostUp>().Animate();
				AudioSource.PlayClipAtPoint(only30SecSound, Vector3.zero);
				only30SecWarningGiven = true;
			} else if (timer <= 0f) {
				gameover = true;
			}
		}
	}

	private void GoToEndScreen (){
		Stats.SaveStats(score);
		levelManager.LoadNextLevel ();
	}

	// User clicks on a number button.
	public void NumberButtonClicked (){
		Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button> ();
		string number = clickedButton.GetComponentInChildren<Text> ().text;

		// Play noise clip
		AudioSource.PlayClipAtPoint(numberSound, Vector3.zero);

		// Since the user has selected this number, make sure they can't use it again
		int newIndex = inputText.text.Length;
		for (int i = 0; i < buttons.Length; i++) {
			if (buttons[i] == clickedButton) {
				buttonIndexes[i] = newIndex;
				// Do NOT use 'enabled' property. Buttons will no longer react to script.
				buttons[i].interactable = false;
			}
		}
	
		AddToInput(number);
	}

	// User clicks on an operation button.
	public void OperationButtonClicked(string operand){
		AddToInput(operand);
		// Play noise clip
		AudioSource.PlayClipAtPoint(operationSound, Vector3.zero);
		// Make sure button does not stay highlighted after being selected
		EventSystem.current.SetSelectedGameObject(null);
	}

	// Add to the expression string.
	private void AddToInput(string newInput){
		string currentInput = inputText.text;
		inputText.text += newInput;
		inputChanged = true;
	}

	// User clicks on the delete button.
	public void DeleteButtonClicked ()
	{
		int lastCharIndex = inputText.text.Length - 1;

		// Are we deleting a number? Make sure it's selectable again.
		for (int i = 0; i < buttons.Length; i++) {
			if (buttonIndexes[i] == lastCharIndex) {
				buttonIndexes[i] = -1;
				buttons [i].interactable = true;
			}
		}

		// Remove the last character from the expression string
		if (lastCharIndex >= 0) {
			inputText.text = inputText.text.Remove(inputText.text.Length-1);
			// Play noise clip
			AudioSource.PlayClipAtPoint(deleteSound, Vector3.zero);
		}

		// Make sure button does not stay highlighted after being selected
		EventSystem.current.SetSelectedGameObject(null);

		inputChanged = true;
	}

	// User clicks on the skip button.
	public void SkipButtonClicked (){
		// Play noise clip
		AudioSource.PlayClipAtPoint(skipSound, Vector3.zero);

		score -= 1;
		UpdateScoreText();
		NewCard();
	}

}
