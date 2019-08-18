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

	private int numA = 0;
	private int numB = 0;
	private int numC = 0;
	private Operation operation = Operation.ADD;
	private int puzzleType = 0; //0 = add, 1 = remove, 2 = move?
	private int matchesToMove = 0;
	private int positionRNG = 0;
	private int confirmation = 0;
	private string bigString = "";
	private string stringA = "";
	private string stringB = "";
	private string stringC = "";
	private string operationSymbols = "+-*/%";
	private List<String> sevenSegmentDigits = new List<string> { "1110111", "0010010", "1011101", "1011011", "0111010", "1101011", "1101111", "1010010", "1111111", "1111011" };

	//Logging
	private static int moduleIdCounter = 1;
	private int moduleId;
	private bool moduleSolved;

	private void Awake() {
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
	private void Start() {
		Generate();
	}
	
	// Update is called once per frame
	private void Update() {
		
	}

	private void Generate() {
		confirmation = 0;
		operation = (Operation) UnityEngine.Random.Range(0, 5);
		switch (operation) {
			case Operation.SUB: case Operation.ADD: {
				numA = UnityEngine.Random.Range(0, 10);
				numC = UnityEngine.Random.Range(0, 10);
				numB = numC - numA;
				if (numB < 0) {
					numB = -numB;
					operation = Operation.SUB;
					texts[1].text = "-";
				} else {
					operation = Operation.ADD;
					texts[1].text = "+";
				}
				break;
			}
			case Operation.MUL: {
				numC = UnityEngine.Random.Range(0, 10);
				var factors = GetDigitFactorTupleRandom(numC);
				numA = factors[0];
				numB = factors[1];
				texts[1].text = "*";
				break;
			}
			case Operation.DIV: {
				numA = UnityEngine.Random.Range(0, 10);
				var factors = GetDigitFactorTupleRandom(numC);
				numB = factors[0];
				numC = factors[1];
				texts[1].text = "/";
				break;
			}
			case Operation.MOD: {
				numA = UnityEngine.Random.Range(1, 10);
				numB = UnityEngine.Random.Range(2, 10);
				numC = numA % numB;
				texts[1].text = "%";
				break;
			}
			default: {
				Generate();
				break;
			}
		}
		//puzzleType = UnityEngine.Random.Range(0, 2); //JUST 2 PUZZLES RIGHT NOW
		puzzleType = 0;
		matchesToMove = UnityEngine.Random.Range(1, 4);
		if (puzzleType == 0) {
			texts[0].text = "ADD " + matchesToMove;
			bigString = sevenSegmentDigits[numA] + sevenSegmentDigits[numB] + sevenSegmentDigits[numC];
			var sb = new System.Text.StringBuilder(bigString);
			for (int i = 0; i < matchesToMove * 100; i++) {
				int index = UnityEngine.Random.Range(0,21);
				sb[index] = '1';
			}
			bigString = sb.ToString();

		}
		stringA = bigString.Substring(0, 7);
		stringB = bigString.Substring(7, 7);
		stringC = bigString.Substring(14, 7);
		for (int l = 0; l > 10; l++) {
			if (stringA == sevenSegmentDigits[l]) {
				confirmation += 1;
			}
			if (stringB == sevenSegmentDigits[l]) {
				confirmation += 1;
			}
			if (stringC == sevenSegmentDigits[l]) {
				confirmation += 1;
			}
		}
		if (confirmation != 3) {
			Generate();
		}
	}

	//Retuns you a shuffled, super random factorization of a digit!
	//USE THIS
	private static int[] GetDigitFactorTupleRandom(int digit) {
		var tuple = GetDigitFactorTupleSemiRandom(digit);
		if (RandomBool()) {
			int temp = tuple[0];
			tuple[0] = tuple[1];
			tuple[1] = temp;
		}
		return tuple;
	}

	//Returns a random factorization of a digit, always ordered from smallest to largest
	private static int[] GetDigitFactorTupleSemiRandom(int digit) {
		switch (digit) {
			case 0: return new int[] {0, UnityEngine.Random.Range(0, 10)};
			case 1: return new int[] {1, 1};
			case 2: return new int[] {1, 2};
			case 3: return new int[] {1, 3};
			case 4: return RandomBool()?new int[]{2, 2}:new int[]{1,4};
			case 5: return new int[] {1, 5};
			case 6: return new int[] {2, 3};
			case 7: return new int[] {1, 7};
			case 8: return RandomBool()?new int[]{2, 4}:new int[]{1,8};
			case 9: return RandomBool()?new int[]{3, 3}:new int[]{1,9};
			default: throw new System.ArgumentOutOfRangeException("A digit is a number between 0-9 (inclusive) dummy");
		}
	}

	private static bool RandomBool() {
		return (UnityEngine.Random.Range(0, 2)==0);
	}

	private enum Operation : byte {
		ADD = 0,
		SUB = 1,
		MUL = 2,
		DIV = 3,
		MOD = 4
	}
}
