using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class MatchManager : MonoBehaviour {

	private bool _state = false;
	[SerializeField]
	private GameObject match;
	private KMSelectable selectable;
	private new KMAudio audio = null;

	public bool state {
		get {return _state;}
		set {
			_state = value;
			match.SetActive(value);
		}
	}

	public KMSelectable Selectable {get {return selectable;}}

	void Awake() {
		selectable = this.GetComponent<KMSelectable>();
		state = state;
		selectable.OnInteract += delegate() {
			state = !state;
			if (this.audio != null) this.audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
			selectable.AddInteractionPunch(0.5f);
			return false;
		};
	}

	public void SetAudio(KMAudio a) {
		this.audio = a;
	}
}
