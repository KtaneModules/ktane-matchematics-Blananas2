using System;
using UnityEngine;

[RequireComponent(typeof(KMBombModule))]
public class matchematicsScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	private KMBombModule module;

	[SerializeField]
	private TextMesh[] texts; //0 = screenText, 1 = operation
	[SerializeField]
	private DigitManager[] digits;
	[SerializeField]
	private KMSelectable confirmButton;
	[SerializeField]
	private KMSelectable resetButton;

	private int[] numbers = new int[3];

	private Operation operation = Operation.ADD;
	private PuzzleType puzzleType = PuzzleType.ADD;
	private int matchesToMove = 0;
	private string operationSymbols = "+-*/%";
	private int[] sevenSegmentDigits = new int[] {
		Convert.ToInt32("1110111".Reverse(), 2),
		Convert.ToInt32("0010010".Reverse(), 2),
		Convert.ToInt32("1011101".Reverse(), 2),
		Convert.ToInt32("1011011".Reverse(), 2),
		Convert.ToInt32("0111010".Reverse(), 2),
		Convert.ToInt32("1101011".Reverse(), 2),
		Convert.ToInt32("1101111".Reverse(), 2),
		Convert.ToInt32("1010010".Reverse(), 2),
		Convert.ToInt32("1111111".Reverse(), 2),
		Convert.ToInt32("1111011".Reverse(), 2)
	};

	//Logging
	private static int moduleIdCounter = 0;
	private int moduleId;
	private bool moduleSolved = false;

	private void Awake() {
		moduleId = moduleIdCounter++;
		module = this.GetComponent<KMBombModule>();
	}

	// Use this for initialization
	private void Start() {
		Generate();
		confirmButton.OnInteract += delegate() {
			if (moduleSolved) return false;
			if (TestInput()) {
				module.HandlePass();
				moduleSolved = true;
			} else {
				module.HandleStrike();
			}
			confirmButton.AddInteractionPunch();
			return false;
		};
		resetButton.OnInteract += delegate() {
			ResetDigits();
			resetButton.AddInteractionPunch();
			return false;
		};
	}

	private void Generate() {
		puzzleType = (PuzzleType) UnityEngine.Random.Range(0, 3);
		matchesToMove = UnityEngine.Random.Range(1, 4);
		int nPuzzles  = MatcheMaticsData.puzzles[(int) puzzleType].Length;
		string puzzle = MatcheMaticsData.puzzles[(int) puzzleType][UnityEngine.Random.Range(0, nPuzzles)];
		matchesToMove = int.Parse(""+puzzle[4]);
		texts[0].text = puzzleType+" "+matchesToMove;
		texts[1].text = ""+puzzle[3];
		operation = (Operation) operationSymbols.IndexOf(puzzle[3]);
		Debug.Log(puzzle+" "+puzzleType);
		for (int i = 0; i < 3; i++) {
			numbers[i] = int.Parse(""+puzzle[i]);
		}
		ResetDigits();
	}

	private void ResetDigits() {
		for (int i = 0; i < 3; i++) {
			digits[i].MatchConfiguration = sevenSegmentDigits[numbers[i]];
		}
	}

	private bool TestInput() {
		int A = digits[0].MatchConfiguration << 14 | digits[1].MatchConfiguration << 7 | digits[2].MatchConfiguration;
		int B = sevenSegmentDigits[numbers[0]] << 14 | sevenSegmentDigits[numbers[1]] << 7 | sevenSegmentDigits[numbers[2]];
		Debug.Log(Convert.ToString( A & ~B, 2));
		Debug.Log(Convert.ToString(~A &  B, 2));
		int add = Convert.ToString( A & ~B, 2).Replace("0","").Length;
		int rem = Convert.ToString(~A &  B, 2).Replace("0","").Length;
		Debug.Log("Added:"+add+" Removed:"+rem);
		//Check if correct amount of matches changed
		switch (puzzleType) {
			case PuzzleType.ADD: {
				if (rem > 0 || add != matchesToMove) return false; 
				break;
			}
			case PuzzleType.REMOVE: {
				if (add > 0 || rem != matchesToMove) return false; 
				break;
			}
			case PuzzleType.MOVE: {
				if (add != rem || add != matchesToMove) return false; 
				break;
			}
			default: return true;
		}
		//Check if matches form real numbers and get the values
		var inputNum = new int[3];
		for (int i = 0; i < 3; i++) {
			inputNum[i] = Array.IndexOf(sevenSegmentDigits, digits[i].MatchConfiguration);
			Debug.Log("NUM"+i+":"+inputNum[i]);
			if (inputNum[i] == -1) return false;
		}

		//Check if the numbers fit the equation
		Debug.Log("Equation");
		return SatisfyEquation(inputNum[0], inputNum[1], inputNum[2], this.operation);
	}

	private static bool SatisfyEquation(int a, int b, int c, Operation op) {
		switch (op) {
			case Operation.ADD: return a + b == c;
			case Operation.SUB: return a - b == c;
			case Operation.MUL: return a * b == c;
			case Operation.DIV: return b != 0 && a == b * c;
			case Operation.MOD: return b != 0 && a % b == c;
			default: return true;
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
	private enum PuzzleType : byte {
		REMOVE = 0,
		ADD = 1,
		MOVE = 2
	}
}

public static class Extensions {
	public static string Reverse(this string input) {
		char[] chars = input.ToCharArray();
		Array.Reverse(chars);
		return new String(chars);
	}
}
