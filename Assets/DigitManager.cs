using UnityEngine;

public class DigitManager : MonoBehaviour {
	[SerializeField]
	private MatchManager[] matches = null;
	public int MatchConfiguration {
		get {
			var config = 0;
			for (int i = 0; i < 7; i++) {
				if (matches[i].state) {
					config |= 1 << i;
				}
			}
			return config;
		}
		set {
			for (int i = 0; i < 7; i++) {
				matches[i].state = (value & 1 << i) != 0;
			}
		}
	}

	public int GetMatchConfiguration() {
		return 0;
	}

	public void SetAudio(KMAudio a) {
		for (int i = 0; i < 7; i++) {
			this.matches[i].SetAudio(a);
		}
	}
}
