using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[RequireComponent(typeof(KMBombModule))]
[RequireComponent(typeof(KMAudio))]
public class matchematicsScript : MonoBehaviour {

	private new KMAudio audio;
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
	private static int moduleIdCounter = 1;
	private int moduleId;

	private void Log(string message) {
		Debug.Log("[Matchematics #"+moduleId+"] "+message);
	}

	private bool moduleSolved = false;

	private void Awake() {
		moduleId = moduleIdCounter++;
		module = this.GetComponent<KMBombModule>();
		audio = this.GetComponent<KMAudio>();
		for (int i = 0; i < 3; i++) {
			this.digits[i].SetAudio(this.audio);
		}
	}

	// Use this for initialization
	private void Start() {
		Generate();
		confirmButton.OnInteract += delegate() {
			this.audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
			this.confirmButton.AddInteractionPunch();
			if (moduleSolved) return false;
			if (TestInput()) {
				module.HandlePass();
				moduleSolved = true;
			} else {
				module.HandleStrike();
			}
			return false;
		};
		resetButton.OnInteract += delegate() {
			ResetDigits();
			this.audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
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
		for (int i = 0; i < 3; i++) {
			numbers[i] = int.Parse(""+puzzle[i]);
		}
		Log("LEVEL GENERATED: Level type:"+texts[0].text+" Initial digits:"+numbers[0]+numbers[1]+numbers[2]+" Operation:"+texts[1].text);
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
		Log("SUBMITED:"+Convert.ToString(A,2)+" ORIGINAL:"+Convert.ToString(B,2));
		int add = Convert.ToString( A & ~B, 2).Replace("0","").Length;
		int rem = Convert.ToString(~A &  B, 2).Replace("0","").Length;
		Log("Added:"+add+" matches Removed:"+rem+" matches, Matches to change:"+matchesToMove);
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
		Log("Remove/Add match test passed");
		//Check if matches form real numbers and get the values
		var inputNum = new int[3];
		for (int i = 0; i < 3; i++) {
			inputNum[i] = Array.IndexOf(sevenSegmentDigits, digits[i].MatchConfiguration);
			if (inputNum[i] == -1) {
				Log("Invalid digit at position "+i);
				return false;
			}
		}
		Log("All digits valid");
		//Check if the numbers fit the equation
		Log("Testing Equation "+(inputNum[0]+texts[1].text+inputNum[1]+"="+inputNum[2]));
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

	[HideInInspector]
	public string TwitchHelpMessage = "To write digits into the module. Use \"type/submit [3 digits]\" (submit presses the CONFIRM button). use \"reset\" to press the reset button.";
	public List<KMSelectable> ProcessTwitchCommand(string command) {
		var output = new List<KMSelectable>();
		var nums = new int[3];
		command = command.ToLowerInvariant();
		var match = Regex.Match(command, "reset.*");
		if (command.Contains("reset")) {
			output.Add(this.resetButton);
			return output;
		}
		bool submit = command.Contains("submit");
		bool type = submit || command.Contains("type");
		//Check for exactly 3 digits in the command
		match = Regex.Match(command, "^(?:[^0-9])*([0-9])(?:[^0-9])*([0-9])(?:[^0-9])*([0-9])(?:[^0-9])*$");
		if (match.Success) {
			for (int i = 0; i < 3; i++) {
				nums[i] = int.Parse(match.Groups[i+1].Value);
			}
			if (type) {
				for (int i = 0; i < 3; i++) {
					int difference = sevenSegmentDigits[nums[i]] ^ this.digits[i].MatchConfiguration;
					for (int j = 0; j < 7; j++) {
						if ((difference & 1 << j) != 0) output.Add(this.digits[i][j].Selectable);
					}
				}
			}
		}
		//Press submit only if there is 3 or 0 digits
		if (submit && (match.Success || !Regex.Match(command, "[0-9]").Success)) output.Add(this.confirmButton);
		return output;
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
