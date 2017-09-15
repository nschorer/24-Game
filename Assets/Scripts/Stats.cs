using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour {

	private static int _score;

	// Make sure we can save the player's performance so we can review the details in the end screen.
	public static void SaveStats(int score){
		_score = score;
	}

	public static int getScore (){
		return _score;
	}
}
