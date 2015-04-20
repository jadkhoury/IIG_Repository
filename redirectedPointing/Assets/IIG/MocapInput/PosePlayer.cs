/**************************************************************************
 * PosePlayer.cs scale body segments according to a calibration file,
 * and reproduces a pose.csv file containing a set positions and orientations
 * into an avatar (assumed to have the same rigging as the model from which
 * the animation was acquired) 
 * Henrique Galvan Debarba 2014
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PosePlayer:MonoBehaviour {


	// callibration files
	public TextAsset model_joint_positions_file;
	public TextAsset skeleton_calibration_file;
	public string skeleton_calibration_file_path;
	public TextAsset pose_recording;
	public string pose_recording_path;
	public float reproductionSpeed = 1.0f;
	int firstFrame = 1;
	int lastFrame = 1;
	int currentFrame = 1;

	// skinned mesh of the 3D model
	public SkinnedMeshRenderer skin;
	float sampleInterval = 0.016f;
	float sampleError = 0.0f;
	//string[] poseData;

	List<float[]> poseData = new List<float[]>();
	// this keeps a poitner to all body joints
	public List<GameObject> jointGO {get; protected set;}

	public bool load_whole_pose_file = false;

	System.IO.StreamReader reader;



	bool ReadPoseFrame (out float [] newFrame) {
		if (reader.EndOfStream) {
			reader.Close();
			reader = new System.IO.StreamReader(pose_recording_path);
			if (reader == null){
				newFrame = null;
				return false;			
			}
			reader.ReadLine();
		}

		string[] frameStr = reader.ReadLine ().Split (',');
		newFrame = new float[frameStr.Length];
		for (int i = 0; i < frameStr.Length; i++) {
			newFrame [i] = float.Parse (frameStr [i]);
		}

		return true;
	}

	bool DiscardFrame(){
		if (!reader.EndOfStream){
			reader.ReadLine ();
		
		}else{
			reader.Close();
			reader = new System.IO.StreamReader(pose_recording_path);
			if (reader == null){
				return false;			
			}
			reader.ReadLine();
		}
		return true;
	}

	// Initialization
	void Start () {
		Debug.Log ("starting");
		ScaleMesh();
		jointGO = new List<GameObject>();
		jointGO.Clear();
		// TODO open file, read joints name, keep pointer to these joints
		// jointGO

		//string fileContent = pose_recording.text.TrimStart().TrimEnd();
		reader = new System.IO.StreamReader(pose_recording_path);

		string[] jointNames = reader.ReadLine().Split(',');

		for (int i = 1; i < jointNames.Length; i+=7) {
			Transform thisJointTr = Extensions.Search(this.transform, jointNames [i].Split('.')[0]);

				//GameObject.Find (jointNames [i].Split('.')[0]);
			if (thisJointTr != null){
				jointGO.Add(thisJointTr.gameObject);
				Debug.Log (jointGO[jointGO.Count-1].name);
			} else {
				Debug.Log(jointNames [i].Split('.')[0] + " NOT FOUND");
			}
		}

		float [] newFrame;
		if (ReadPoseFrame (out newFrame)) {
			sampleInterval = 1.0f / newFrame[newFrame.Length-1];
		} else {
			Debug.LogError(pose_recording_path + " file is empty");
			return;
		}
		if (load_whole_pose_file) {
			while (ReadPoseFrame (out newFrame)) {
				poseData.Add (newFrame);
			}
		} 

		//reader.ReadLine();
	/*	string fileContent = reader.ReadToEnd ();


		poseData = fileContent.Split('\n');
		lastFrame = poseData.Length - 1;
		string[] jointNames = poseData[0].Split(',');
		Debug.Log ("string ready");
		for (int i = 1; i < jointNames.Length; i+=7) {
			GameObject thisJointGO = GameObject.Find (jointNames [i].Split('.')[0]);
			if (thisJointGO != null){
				jointGO.Add(thisJointGO);
				Debug.Log (jointGO[jointGO.Count-1].name);
			} else {
				Debug.Log(jointNames [i].Split('.')[0] + " NOT FOUND");
			}


		}*/
		Debug.Log ("total joints: " + jointGO.Count);
		/*using (StringReader strReader = new StringReader(fileContent)) {
			string line; 
			int countFrame = 0;
			// skip header
			line = strReader.ReadLine();
			
			while ((line = strReader.ReadLine()) != null) 
			{

			}
		}*/
	}
		
		// scale body segments accoding to calibration
	void ScaleMesh(){
		//SetInitialJointValues();
		
		Transform lowest_joint = GetLowestJoint();
		float lowest_y_before_scale = lowest_joint.position.y;

		string text;
		if (skeleton_calibration_file_path != null){
			System.IO.StreamReader reader = new System.IO.StreamReader(skeleton_calibration_file_path);
			reader.ReadLine();
			text = reader.ReadToEnd ();
		} else
			text = skeleton_calibration_file.text.TrimStart().Remove(0, 2).TrimStart().TrimEnd();

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
		
//		Extensions.Search(this.transform, "HumanoidRoot").Translate(0.0F, -y_dif, 0.0F, Space.World);
		//GameObject.Find("HumanoidRoot").transform.Translate(0.0F, -y_dif, 0.0F, Space.World);
		
		skin.sharedMesh.RecalculateNormals();
		skin.sharedMesh.RecalculateBounds();
	}
	
	void Update(){
		Set_joint_values(Time.deltaTime);
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
		Transform l_hip = Extensions.Search(this.transform, "l_hip");
		//Transform l_hip = GameObject.Find("l_hip").transform;
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
			Transform joint_to_be_set =  Extensions.Search(this.transform, joint_name);//GameObject.Find(joint_name).transform;
			joint_to_be_set.position = bone_length;
			
		}
		
	}
	
	
	// 
	void set_bone_length(string joint_name, double bone_length)
	{
		Transform joint_to_be_translated = Extensions.Search(this.transform, joint_name);//GameObject.Find(joint_name).transform;
		
		float initial_len = joint_to_be_translated.localPosition.magnitude;
		
		float scale_factor = (float)bone_length / initial_len;
		joint_to_be_translated.localPosition *= scale_factor;
	}

	public Vector3 getVector3(string rString){
		string[] temp = rString.Substring(1,rString.Length-2).Split(',');
		float x = float.Parse(temp[0]);
		float y = float.Parse(temp[1]);
		float z = float.Parse(temp[2]);
		Vector3 rValue = new Vector3(x,y,z);
		return rValue;
	}

	public Quaternion getQuaternion(string rString){
		string[] temp = rString.Substring(1,rString.Length-2).Split(',');
		float x = float.Parse(temp[0]);
		float y = float.Parse(temp[1]);
		float z = float.Parse(temp[2]);
		float w = float.Parse(temp[3]);
		Quaternion rValue = new Quaternion(x,y,z,w);
		return rValue;
	}



	// update root position and joint values
	void Set_joint_values(float deltaTime)
	{
		//sampleError += deltaTime;
		deltaTime *= reproductionSpeed;
		deltaTime += sampleError;
		float frameIncrement = Mathf.Round (deltaTime / sampleInterval);
		sampleError = deltaTime - frameIncrement * sampleInterval;
		currentFrame+= (int) frameIncrement;



		float[] frame;
		if (load_whole_pose_file){
			if (currentFrame >= poseData.Count)
						currentFrame = 1;
			//string[] jointsData = poseData [currentFrame].Split ('"');//new char[] {'"', ','});
			frame = poseData [currentFrame];
		} else if ((int) frameIncrement>0){
			for (int i=0; i < (int) frameIncrement -1; i++)
				if (! DiscardFrame())
					return;

			if (! ReadPoseFrame (out frame)) 
				return;
		} else
			return;
		//Debug.Log (jointsData[1] + "-" + jointsData[2] + "-" + jointsData[3] + "-" + jointsData[4]);
		int jointGOCount = 0;
		for(int i = 1; i < frame.Length; i+=7){
			if (i == 1){

				Transform root = Extensions.Search(this.transform, "root");//GameObject.Find("root").transform;
				//Vector3 rootPos =  getVector3(jointsData[i]);
				Vector3 rootPos = new Vector3(frame[i],
				                              frame[i+1],
				                              frame[i+2]);
				root.localPosition = new Vector3(rootPos.x,-rootPos.z,rootPos.y);
				//root.rotation = getQuaternion(jointsData[i+2]);
			} //else 
			{
				if (jointGOCount < jointGO.Count){
					Transform joint = jointGO[jointGOCount].transform;
					if (joint != null)
						//joint.rotation = getQuaternion(jointsData[i+2]);
						joint.rotation = new Quaternion(frame[i+3],
						                                frame[i+4],
						                                frame[i+5],
						            					frame[i+6]);
				}
			}
			jointGOCount++;
		}
	


		
		
	}
}
