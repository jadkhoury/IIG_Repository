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
using System.Collections.Generic;
using System.Linq;

using IIG;


public class mkr2pose : MonoBehaviour {
	

	// log avatar pose and root position
	LogPoseDirect m_logPose= new LogPoseDirect();
	
	// set standard log path and name
	public string folderPath =  "experiment/LOG/";
	public string logName =  "logName"; // TODO same as input file
	
	public  MocapInputController2 m_mocapInput = new MocapInputController2();

	public bool useConfigFile = true;
	string configPath;

	string record_joints = "";

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
			m_mocapInput.fileToLoad = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.fileToLoad), configPath);
			m_mocapInput.mocap_marker_alias_file_path = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.mocap_marker_alias_file_path), configPath);
			m_mocapInput.avtController.mocap_config_file_path = 
				ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.mocap_config_file_path), configPath);
			m_mocapInput.avtController.skeleton_calibration_file_path = 
				ConfigFile.Search (MemberInfoGetting.GetMemberName(() => m_mocapInput.avtController.skeleton_calibration_file_path), configPath);
			record_joints = ConfigFile.Search (MemberInfoGetting.GetMemberName(() => record_joints), configPath);

		}
	}
	
	int currentFrame =0;
	// Use this for initialization
	void Start () {
		
		// make sure the log folder exists!
		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);
		logName = m_mocapInput.fileToLoad;

		m_logPose.OpenLog (logName);

		m_mocapInput.avtController.StartAvatarController (this.transform, record_joints);
		m_mocapInput.StartMocapInput ();

		currentFrame = m_mocapInput.m_mocapPlayer.startFrame;
	}
	
	
	// Update is called once per frame
	void Update () {


		while (currentFrame <= m_mocapInput.m_mocapPlayer.stopFrame) {

			m_mocapInput.m_mocapPlayer.SetCurrentFrameNbr (currentFrame);
			m_mocapInput.FrameUpdate (currentFrame);
			float repTime = m_mocapInput.m_mocapPlayer.currentTime;
			int samplingFreq = (int)  m_mocapInput.m_mocapPlayer.frameRate;
			if (m_logPose.logIsOpen) {
				m_logPose.LogAvatarPoseFrame (m_mocapInput.avtController.jointGO, repTime, samplingFreq);
			} else
					Debug.LogError ("error");
			currentFrame += 1;
		}
		m_logPose.SaveLog ();
		Application.Quit ();
	}
	// fixedupdate is called every physics update, interval may be defined manually
	void FixedUpdate(){

	}

}

public class MocapInputController2 {
	
	// VRPN settings
	public string VRPN_connection_string = "Tracker0@localhost";	
	private VRPN.Client vrpn_client;
	
	// mocap recording path and name, it loads C3D and .mkr.csv
	// .mkr.csv is a simple ASCII that can be created using LogMarker.cs
	public string mocap_marker_alias_file_path;
	
	public string fileToLoad;
	public MocapPlayer m_mocapPlayer  {get; protected set;} // = new CSVPlayer();
	
	// input source
	public enum MocapSource {LIVE=0, FILE=1};
	public MocapSource m_mocapSource = MocapSource.FILE;


	
	// debug markers (draw them as sphere in the screen)
	public MarkersDebug mkrDebug;
	public bool debugMarkersPos = true;
	
	public double[,] frame;// {get; protected set;}//= null;
	public bool [] visibFrame;// {get; protected set;}// = null;
	
	// avatar controller, which receives mocap input from this class
	public AvatarController2 avtController = new AvatarController2();
	
	List<Tuple<int,string>> m_mkrNames = new  List<Tuple<int,string>> ();
	//public LogMocap m_logMocap = new LogMocap();
	
	// Initialization
	public void StartMocapInput () {
		
		// init Markers debug
		mkrDebug = new MarkersDebug();
		
		// open c3D file
		if (fileToLoad.EndsWith(".c3d"))
			m_mocapPlayer = new C3DPlayer();
		else
			m_mocapPlayer = new CSVPlayer();
		
		m_mocapPlayer.LoadMocap(fileToLoad);
		
		//c3dMan.SetStartStopTime(10.0f,25.0f);
		
		vrpn_client = new VRPN.Client(mocap_marker_alias_file_path, VRPN_connection_string);
		vrpn_client.Init();
		
		
		LoadMakerNames (mocap_marker_alias_file_path, out m_mkrNames);
		
		if (m_mocapSource == MocapSource.FILE) {
			mkrDebug.Init (m_mkrNames);
			//mkrDebug.Init (m_mocapPlayer.totalPoints);
		} else {
			mkrDebug.Init (m_mkrNames);
			//mkrDebug.Init (vrpn_client.no_of_markers);
		}
		mkrDebug.SetVisible(debugMarkersPos);
	}
	
	
	void LoadMakerNames(string inFilePath, out List<Tuple<int,string>> mkrnames){
		mkrnames = new List<Tuple<int, string>>();
		System.IO.StreamReader reader = new System.IO.StreamReader(inFilePath);
		string line;
		
		while((line = reader.ReadLine()) != null)
		{
			string[] id_value = line.Split(';');
			if (id_value[0] != null){
				string[] mkrNameID = id_value[0].Split(' ');
				if (mkrNameID.Length >= 2){
					Tuple<int, string> mkrNameIDTuple = new Tuple<int, string>(System.Convert.ToInt32(mkrNameID[0]), mkrNameID[1]);
					mkrnames.Add(mkrNameIDTuple);
				}
			}
			
		}
		reader.Close();
	}
	
	public void FrameUpdate(int currentFrame){
		//double [,] frame = null;
		//bool [] visibFrame = null;
		bool validFrame = false;
		int totalMarkers = 0;
		
		// MOCAP from file
		if (m_mocapSource==MocapSource.FILE){
			//validFrame = m_mocapPlayer.CycleFrames(deltaT, ref frame, ref visibFrame);
			validFrame = m_mocapPlayer.GetFrameAt(currentFrame, ref frame, ref visibFrame);
			totalMarkers = m_mocapPlayer.totalPoints;
		}
		// VRPN stream
		else if (m_mocapSource==MocapSource.LIVE){
			
			vrpn_client.Update();
			
			// copy positions (so a buffer can be kept)
			//double[,] currentFrame
			frame = new double[vrpn_client.marker_positions.GetLength(0),vrpn_client.marker_positions.GetLength(1)];
			visibFrame = new bool[vrpn_client.marker_positions.GetLength(0)];
			totalMarkers = vrpn_client.marker_positions.GetLength(0);
			for (int i = 0; i < vrpn_client.marker_positions.GetLength(0); i++){
				visibFrame[i]= true;
				for (int j = 0; j < vrpn_client.marker_positions.GetLength(1); j++)
					frame[i,j] = vrpn_client.marker_positions[i,j];
				if (frame[i,0]!=frame[i,0])
					visibFrame[i] = false;
			}
			
			// I do not know what could be used to define a valid frame input from VRPN (yet)
			validFrame = true;
		}
		
		
		if (validFrame){
			if (avtController!= null)
				avtController.MocapUpdateIK(vrpn_client.marker_ids, frame);
			else
				Application.Quit();
			
		} 
		
	}
	
	//void MocapUpdate(){
	//	FrameUpdate (Time.fixedDeltaTime);
	//}
	public void EnableMarkerDebug(bool enable){
		mkrDebug.SetVisible(enable);
		debugMarkersPos = enable;
	}
	public double[,] GetLatestFrame(){
		return frame;
	}
	public bool[] GetLatestFrameVisibility(){
		return visibFrame;
	}
	public List<Tuple<int,string>> GetNamesList(){
		return m_mkrNames;
	}
}





//public static class Extensions
//{
//	public static Transform Search(this Transform target, string name){
//		if (target.name == name) return target;
//		
//		for (int i = 0; i < target.childCount; ++i){
//			var result = Search(target.GetChild(i), name);
//			
//			if (result != null) return result;
//		}
//		return null;
//	}
//	public static void SearchAll(this Transform target, string name, ref List<Transform> result){
//		if (target.name == name) result.Add(target);
//		
//		for (int i = 0; i < target.childCount; ++i)
//		{	
//			SearchAll(target.GetChild(i), name, ref result);	
//		}
//	}
//}

public class AvatarController2 {
	
	// callibration files
	public TextAsset model_joint_positions_file;
	//	public TextAsset skeleton_calibration_file;
	public string skeleton_calibration_file_path;
	
	// skinned mesh of the 3D model
	//public SkinnedMeshRenderer skin;
	
	// location and name of config files
	public string mocap_config_file_path;
	
	// Mocap is the wrapper to the IK DLL
	private Mocap mocap;
	
	// this keeps a poitner to all body joints
	//public List<GameObject> jointGO {get; protected set;}
	public List<Transform> jointGO {get; protected set;}
	//	public List<List<Transform>> jointGO {get; protected set;}

	Transform thisBody;

	string controlledJoints;
	List<int> controlledJointsIdx = new List<int>();

	// Initialization
	public void StartAvatarController (Transform thisTr, string _controlledJoints) {
		controlledJoints = _controlledJoints;
		thisBody = thisTr;
		ScaleMesh();
		jointGO = new List<Transform>();
		//jointGO = new List<List<Transform>>();
		//jointGO = new List<GameObject>();
		mocap = new Mocap(mocap_config_file_path);
		jointGO.Clear();
		for(int i = 0; i < mocap.no_of_joints; ++i){
			//Transform childJoint = transform.GetComponentsInChildren().FirstOrDefault(t => t.name == mocap.joint_names[i]);
			//			Transform childJoint = this.transform.Find(mocap.joint_names[i]);
			if (controlledJoints.Contains(mocap.joint_names[i])){
				Transform childJoint = Extensions.Search(thisBody.transform, mocap.joint_names[i]);
			//			List<Transform> childJoint = new List<Transform>();
			//			Extensions.SearchAll(this.transform, mocap.joint_names[i], ref childJoint);
			
				if (childJoint != null){
					jointGO.Add(childJoint);
					controlledJointsIdx.Add(i);
				}
			}
		}
		//jointGO.Add(GameObject.Find(mocap.joint_names[i]));
	}
	
	// scale body segments accoding to calibration
	void ScaleMesh(){
		//SetInitialJointValues();
		
		Transform lowest_joint = GetLowestJoint();
		float lowest_y_before_scale = lowest_joint.position.y;
		
		//string text = skeleton_calibration_file.text.TrimStart().Remove(0, 2).TrimStart().TrimEnd();
		System.IO.StreamReader reader = new System.IO.StreamReader(skeleton_calibration_file_path);
		reader.ReadLine();
		string text = reader.ReadToEnd ();
		string[] lines = text.Split('\n');
		
		
		foreach (string line in lines)
		{
			if (line.Length > 2){
				string[] joint_names_and_length = line.Split(' ');
				string joint_name = joint_names_and_length[0].TrimStart().TrimEnd();
				double bone_length = System.Convert.ToDouble(joint_names_and_length[1]);
				if(joint_name != "HumanoidRoot")
					set_bone_length(joint_name, bone_length);
			}
		}
		
		float lowest_y_after_scale = lowest_joint.position.y;
		float y_dif = lowest_y_after_scale - lowest_y_before_scale;
		
		//GameObject.Find("root").transform.Translate(0.0F, -y_dif, 0.0F, Space.World);
		GameObject.Find("HumanoidRoot").transform.Translate(0.0F, -y_dif, 0.0F, Space.World);
		
		//skin.sharedMesh.RecalculateNormals();
		//skin.sharedMesh.RecalculateBounds();
	}
	
	public void MocapUpdateIK(int[] marker_ids, double[,] marker_positions){
		mocap.UpdateIK(marker_ids, marker_positions);
		Set_joint_values();
	}
	
	Transform GetLowestJointUnderGivenJoint(Transform j)
	{
		Transform return_val = j;
		float lowest_y = j.position.y;
		foreach (Transform child in j)
		{
			Transform best_descendant = GetLowestJointUnderGivenJoint(child);
			float best_descendant_y = best_descendant.position.y;
			if(best_descendant_y < lowest_y)
			{
				lowest_y = best_descendant_y;
				return_val = best_descendant;
			}
		}
		return return_val;
	}
	
	Transform GetLowestJoint()
	{
		Transform l_hip = GameObject.Find("l_hip").transform;
		return GetLowestJointUnderGivenJoint(l_hip);
	}
	
	void SetInitialJointValues(){
		
		
		string text = model_joint_positions_file.text.TrimStart().TrimEnd();
		string[] lines = text.Split('\n');
		
		for (int i=0; i < lines.Length*0.5-1 ; i+=2)
		{
			string[] joint_name_string = lines[i].Split(' ');
			string[] joint_position_string = lines[i+1].Split(' ');
			
			string joint_name = joint_name_string[0].TrimStart().TrimEnd();
			Vector3 bone_length = new Vector3(float.Parse (joint_position_string[1]),
			                                  float.Parse (joint_position_string[2]),
			                                  -float.Parse (joint_position_string[3]));
			Transform joint_to_be_set = GameObject.Find(joint_name).transform;
			joint_to_be_set.position = bone_length;
			
		}
		
	}
	
	
	// 
	void set_bone_length(string joint_name, double bone_length)
	{
		Transform joint_to_be_translated = Extensions.Search(thisBody.transform, joint_name);//GameObject.Find(joint_name).transform;
		
		float initial_len = joint_to_be_translated.localPosition.magnitude;
		
		float scale_factor = (float)bone_length / initial_len;
		joint_to_be_translated.localPosition *= scale_factor;
	}
	
	
	// update root position and joint values
	void Set_joint_values()
	{
		//	Debug.Log (jointGO.Count);
		//Transform root = Extensions.Search(this.transform, "root");
		Transform root = GameObject.Find("root").transform;
		if (root != null){
			if (mocap.root_pos[0] != mocap.root_pos[0]){ Debug.Log("root pos NaN!");}else
				root.localPosition = new Vector3((float) mocap.root_pos[0], 
				                                 (float) mocap.root_pos[1],
				                                 -(float) mocap.root_pos[2]);
			root.localPosition = Quaternion.Inverse(root.rotation) * root.localPosition;
		}
		
		//for(int i = 0; i < mocap.no_of_joints; ++i)
		for(int i = 0; i < jointGO.Count; ++i)
		{
			//		for(int j = 0; j < jointGO[i].Count; ++j){
			//Transform joint = GameObject.Find(mocap.joint_names[i]).transform;
			Transform joint = jointGO[i];//.transform; // more efficient, uses a predefined list of pointers instead of name search
			Quaternion q = new Quaternion((float) mocap.orientations[controlledJointsIdx[i], 0], 
			                              (float) mocap.orientations[controlledJointsIdx[i], 2],
			                              (float) -mocap.orientations[controlledJointsIdx[i], 1], 
			                              (float) mocap.orientations[controlledJointsIdx[i], 3]);
			if (q.x != q.x){ Debug.Log("quaternion NaN!");}
			else if (joint != null)
				joint.localRotation = q;
			//	}
		}
		
		
	}
}
