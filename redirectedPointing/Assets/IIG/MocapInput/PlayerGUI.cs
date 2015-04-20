using UnityEngine;
using System.Collections;


public class PlayerGUI : MonoBehaviour {
	Rect repControlRect = new Rect(20, 20, 300, 90);
	bool pause = false;
	float currentFrame = 0.0F;
	public MocapInputController m_inputController;
	MocapPlayer m_mocapPlayer;
	// Use this for initialization
	void Start () {
		m_mocapPlayer = m_inputController.m_mocapPlayer;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//void OnGUI() {
		//hSliderValue = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), hSliderValue, 0.0F, 10.0F);
	void OnGUI(){
		if (m_mocapPlayer == null){
			m_mocapPlayer = m_inputController.m_mocapPlayer;
		}
		// dragable window
		// play set repspeed to 1
		// pause set repspeed to 0
		// fastforward duplicate repspeed
		// backward half repspeed
		// repeat button reset time to 0 when done
		// 
		repControlRect = GUI.Window(0, repControlRect, ReproductionWindow, "reproduction");



	}


	//int keepCurrentFrame = 0;
	void ReproductionWindow(int windowID) {
		if (GUI.Button(new Rect(10, 25, 60, 30), "RW")){
			if (m_mocapPlayer.repSpeed > 0.0f){
				m_mocapPlayer.repSpeed = -2;
			} else {
				m_mocapPlayer.repSpeed *= 2;
			}
		}
		if (pause){
			if (GUI.Button(new Rect(80, 25, 60, 30), "Play")){
				m_mocapPlayer.repSpeed = 1;
				pause = false;
			}
		} else {
			if (GUI.Button(new Rect(80, 25, 60, 30), "Pause")){
				m_mocapPlayer.repSpeed = 0;
				pause = true;
			}
		}
		if (GUI.Button(new Rect(150, 25, 60, 30), "FF")){
			if (m_mocapPlayer.repSpeed < 0.0f){
				m_mocapPlayer.repSpeed = 2;
			} else {
				m_mocapPlayer.repSpeed *= 2;
			}
		}

		m_mocapPlayer.repeat = GUI.Toggle(new Rect(230, 30, 60, 25), m_mocapPlayer.repeat, "Repeat");

		currentFrame = GUI.HorizontalSlider(new Rect(10, 65, 280, 25), (float)m_mocapPlayer.GetCurrentFrameNbr(), (float)m_mocapPlayer.startFrame, (float)m_mocapPlayer.stopFrame);
		if ((int)currentFrame != (int)m_mocapPlayer.GetCurrentFrameNbr()) {
			m_mocapPlayer.SetCurrentFrameNbr((int)currentFrame);
			//keepCurrentFrame = (int)currentFrame;
		}



		GUI.DragWindow();
	}
}
