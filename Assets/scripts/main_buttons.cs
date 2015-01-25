using UnityEngine;
using System.Collections;

public class main_buttons : MonoBehaviour {
	enum states {mainmenu, quit, play};
	states state;
	// Use this for initialization
	void Start () {
		state = states.mainmenu;
	}
	public void stateToPlay(){
		state = states.play;
	}
	public void stateToQuit(){
		state = states.quit;
	}
	public void onMouseDown(){
		switch (state) {
		case states.mainmenu :
			break;
		case states.play :
			Application.LoadLevel(1);
			break;
		case states.quit :
			Application.Quit();
			break;
				}
		}

}
