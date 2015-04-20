/**************************************************************************
 * AppController.cs is a sample on how to manage log and targets classes, 
 * key input and GUI
 * Written by Henrique Galvan Debarba
 * Last update: 05/03/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class AppController : MonoBehaviour {

	// log markers position and visibility into CSV
	LogMarkers m_logMarkers = new LogMarkers();
	// log avatar pose and root position
	//LogPose m_logPose = new LogPose();
	LogPoseDirect m_logPose= new LogPoseDirect();

	LogTargets m_logTargets = new LogTargets();
	// body parts names for relevant contact logs
	public string relevantContacts;
	public SimpleTargetManager m_tgtManager;

	
	// set standard log path and name
	public string folderPath =  "experiment/LOG/";
	public string logName =  "logName";

	public  MocapInputController m_mocapInput;
	//public InputHandler m_mocapInput;
	//public CustomizedAvatarController m_avatarCtrl;

	// markers and avatar poses files may be very big!
	public bool keepMotionLogs = false;
	
	public bool blockIsRunning {get; private set;}

	public Camera OrbitCamera;
	public GameObject OculusCameras;

	public bool useOculus = false;


	private string startTime;
	
	public bool useConfigFile = true;
	string configPath;

	void Awake(){
		configPath = Application.dataPath;
		if (Application.platform == RuntimePlatform.OSXPlayer) {
			configPath += "/../../";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer) {
			configPath += "/../";
		}
		configPath+="config.txt";

		if (useConfigFile) {
			useOculus = Convert.ToBoolean (ConfigFile.Search (MemberInfoGetting.GetMemberName(() => useOculus), configPath));
			m_mocapInput.fileToLoad = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.fileToLoad), configPath);
			m_mocapInput.mocap_marker_alias_file_path = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.mocap_marker_alias_file_path), configPath);
			m_mocapInput.avtController.mocap_config_file_path = 
				ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.mocap_config_file_path), configPath);
			m_mocapInput.avtController.skeleton_calibration_file_path = 
				ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.skeleton_calibration_file_path), configPath);

		}
	}

	void SaveConfig(){
		string[] currentState = {
			MemberInfoGetting.GetMemberName(() => useOculus), useOculus.ToString(),
			MemberInfoGetting.GetMemberName(() => m_mocapInput.fileToLoad), m_mocapInput.fileToLoad,
			MemberInfoGetting.GetMemberName(() => m_mocapInput.mocap_marker_alias_file_path), m_mocapInput.mocap_marker_alias_file_path,
			MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.mocap_config_file_path), m_mocapInput.avtController.mocap_config_file_path,
			MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.skeleton_calibration_file_path), m_mocapInput.avtController.skeleton_calibration_file_path

		};
		ConfigFile.SaveFile(ref currentState, configPath);
	}


	// Use this for initialization
	void Start () {

		startTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
		// make sure the log folder exists!
		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);
		blockIsRunning = false;

		if (OculusCameras != null && OrbitCamera != null){
			if (!useOculus) {
				OrbitCamera.enabled = true;
				OculusCameras.SetActive(false);
			} else {
				OrbitCamera.enabled = false;
				OculusCameras.SetActive(true);
			}
		}

	}
	

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.M)) {
			m_mocapInput.EnableMarkerDebug (!m_mocapInput.debugMarkersPos);
		
		}
		// start and close block
		if (Input.GetKeyUp(KeyCode.Alpha1) && !blockIsRunning)
			StartBlock();
		else if (Input.GetKeyUp(KeyCode.Alpha2) && blockIsRunning)
			CloseBlock();

		if (Input.GetKeyUp(KeyCode.Alpha0))
		    SaveConfig();

		// 
		if (Input.GetKeyUp(KeyCode.P)){
			useOculus = !useOculus;
			if (OculusCameras != null && OrbitCamera != null){
				if (!useOculus) {
					OrbitCamera.enabled = true;
					OculusCameras.SetActive(false);
				} else {
					OrbitCamera.enabled = false;
					OculusCameras.SetActive(true);
				}
			}
		}

	}

	public void StartBlock(){
		StartBlock(logName);
	}

	// start logs and start showing targets
	public void StartBlock(string newLogName){
		Debug.Log("AppController: openning logs");
		string blockName = folderPath + newLogName + startTime;
		if (keepMotionLogs){
			m_logPose.OpenLog(blockName);
			m_logMarkers.OpenLog(m_mocapInput.GetNamesList());
			if (m_tgtManager != null)
				m_tgtManager.ClearContactLogs();
		}
		blockIsRunning = true;
	}

	public void CloseBlock(){
		CloseBlock(logName);
	}
	// save logs and stop showing targets
	public void CloseBlock(string newLogName){
		Debug.Log("AppController: closing logs");
		string blockName = folderPath + newLogName + startTime;
		if (keepMotionLogs){
			if (m_tgtManager != null)
				m_logTargets.SaveLog(blockName, m_tgtManager.GetContactsLogs(relevantContacts));
			m_logMarkers.CloseLog(blockName);
			m_logPose.SaveLog();
		}
		blockIsRunning = false;
	}

	// fixedupdate is called every physics update, interval may be defined manually
	void FixedUpdate(){
		if (keepMotionLogs){
			if (m_logMarkers.logIsOpen)
				m_logMarkers.LogMarkersFrame(m_mocapInput.frame, m_mocapInput.visibFrame);

			if (m_logPose.logIsOpen){
				m_logPose.LogAvatarPoseFrame(m_mocapInput.avtController.jointTF);
			}
		}

	}
	


}

