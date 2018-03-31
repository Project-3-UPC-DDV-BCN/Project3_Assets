using TheEngine;

public class Music {
	
	TheAudioSource audio_source;

	void Start () {
		audio_source = GetComponent<TheAudioSource>();

		audio_source.Play("Play_Calm_song");
	}
	
	void Update () {
		
		
	}
}