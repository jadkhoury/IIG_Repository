using UnityEngine;
using System.Collections;
using System;

public class PosePlayerControl : MonoBehaviour {
	
	// config variables
	public bool useConfigFile = true;
	public string configFileName = "configPose.txt";
	public PosePlayer m_posePlayer;
	public PosePlayer m_poseShadow;


	// Use this for initialization
	void Awake () {
		string configPath = Application.dataPath;
		if (Application.platform == RuntimePlatform.OSXPlayer) {
			configPath += "/../../";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer) {
			configPath += "/../";
		} else if (Application.platform == RuntimePlatform.WindowsEditor) {
			configPath += "/";
		}
		configPath+=configFileName;


		if (useConfigFile) {
			bool shadow_avatar = Convert.ToBoolean (ConfigFile.Search ("shadow_avatar", configPath));

			m_posePlayer.load_whole_pose_file = Convert.ToBoolean (ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_posePlayer.load_whole_pose_file), configPath));
			m_posePlayer.pose_recording_path = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_posePlayer.pose_recording_path), configPath);
			m_posePlayer.skeleton_calibration_file_path = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_posePlayer.skeleton_calibration_file_path), configPath);
			m_posePlayer.reproductionSpeed = Convert.ToInt32 (ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_posePlayer.reproductionSpeed), configPath));

			if (shadow_avatar) {
				m_poseShadow.load_whole_pose_file = m_posePlayer.load_whole_pose_file;
				m_poseShadow.pose_recording_path = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_poseShadow.pose_recording_path)+"_shadow", configPath);
				m_poseShadow.skeleton_calibration_file_path = m_posePlayer.skeleton_calibration_file_path;
				m_poseShadow.reproductionSpeed = m_posePlayer.reproductionSpeed;
			} else {
				Destroy(m_poseShadow);
			}
		//	m_posePlayer[0].pos
	/*		useOculus = Convert.ToBoolean (ConfigFile.Search (MemberInfoGetting.GetMemberName(() => useOculus), configPath));
			m_mocapInput.fileToLoad = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.fileToLoad), configPath);
			m_mocapInput.mocap_marker_alias_file_path = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.mocap_marker_alias_file_path), configPath);
			m_mocapInput.avtController.mocap_config_file_path = 
				ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.mocap_config_file_path), configPath);
			m_mocapInput.avtController.skeleton_calibration_file_path = 
				ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.skeleton_calibration_file_path), configPath);
	*/		
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
