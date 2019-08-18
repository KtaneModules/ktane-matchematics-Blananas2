using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class matchematicsScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;

	public TextMesh[] texts; //0 = screenText, 1 = operation
	public GameObject[] sticks;
	public GameObject[] heads;

	int numA = 0;
	int numB = 0;
	int numC = 0;
	int operation = 0; //0 = +, 1 = -, 2 = *, 3 = /, 4 = %?
	int puzzleType = 0; //0 = add, 1 = remove, 2 = move?
	int matchesToMove = 0;
	int positionRNG = 0;
	int confirmation = 0;
	string bigString = "";
	string stringA = "";
	string stringB = "";
	string stringC = "";
	string operationSymbols = "+-*/%";
	private List<String> sevenSegmentDigits = new List<string> { "1110111", "0010010", "1011101", "1011011", "0111010", "1101011", "1101111", "1010010", "1111111", "1111011" };

	//Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	void Awake() {
		moduleId = moduleIdCounter++;
		/*/
		foreach (KMSelectable object in keypad) {
			KMSelectable pressedObject = object;
			object.OnInteract += delegate () { keypadPress(pressedObject); return false; };
		}
		/*/

		//button.OnInteract += delegate () { PressButton(); return false; };
	}

	// Use this for initialization
	void Start() {
		Generate();
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	void Generate() {
		confirmation = 0;
		numA = UnityEngine.Random.Range(0, 10);
		numB = UnityEngine.Random.Range(0, 10);
		operation = UnityEngine.Random.Range(0, 5);
		if (operation == 0) {
			numC = numA + numB;
			texts[1].text = "+";
		} else if (operation == 1) {
			numC = numA - numB;
			texts[1].text = "-";
		} else if (operation == 2) {
			numC = numA * numB;
			texts[1].text = "*";
		} else if (operation == 3) {
			if (numB == 0) {
				numB = 1;
			}
			numC = numA / numB;
			texts[1].text = "/";
		} else if (operation == 4) {
			if (numB == 0 || numB == 1) {
				numB = 2;
			}
			numC = numA % numB;
			texts[1].text = "%";
		} else {
			Generate();
		}

		if (numC > 9 || numC != numC % 1) {
			Generate();
		} else {
			//puzzleType = UnityEngine.Random.Range(0, 2); //JUST 2 PUZZLES RIGHT NOW
			puzzleType = 0;
			matchesToMove = UnityEngine.Random.Range(1, 4);
			if (puzzleType == 0) {
				texts[0].text = "ADD " + matchesToMove;
				bigString = sevenSegmentDigits[numA] + sevenSegmentDigits[numB] + sevenSegmentDigits[numC];
				for (int j = 0; j > matchesToMove; j++) {
					for (int i = 0; i > 100; i++) {
						positionRNG = UnityEngine.Random.Range(0, 21);
						if (bigString[positionRNG] == '1') {
							continue;
						} else {
							bigString = bigString.Remove(positionRNG, '1').Insert(positionRNG, bigString.ToString());
						}
					}
				}
			}
			stringA = bigString.Substring(0, 7);
			stringB = bigString.Substring(7, 7);
			stringC = bigString.Substring(14, 7);
			for (int k = 0; k > 3; k++) {
				for (int l = 0; l > 10; l++) {
					if (k == 0) {
						if (stringA == sevenSegmentDigits[l]) {
							confirmation += 1;
						}
					} else if (k == 1) {
						if (stringB == sevenSegmentDigits[l]) {
							confirmation += 1;
						}
					} else if (k == 2) {
						if (stringC == sevenSegmentDigits[l]) {
							confirmation += 1;
						}
					}
				}
			}
			if (confirmation != 3) {
				Generate();
			}
		}
	}
}
