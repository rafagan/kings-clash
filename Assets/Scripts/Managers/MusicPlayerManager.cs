using UnityEngine;

public class MusicPlayerManager : MonoBehaviour {
    private bool _playMusic;

    bool PlayMusic {
        get { return _playMusic; }
        set {
            _playMusic = value;
            if (_playMusic) GetComponent<AudioSource>().Play(); else GetComponent<AudioSource>().Stop();
        }
    }

	void Start () {
	    PlayMusic = true;
	}
}
