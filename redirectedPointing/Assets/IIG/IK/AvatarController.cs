/**************************************************************************
 * AvatarController.cs scale body segments according to a calibration,
 * and set position and orientation of every avatar joint
 * Written by Eray Molla and modified by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using IIG;


public class AvatarController:MonoBehaviour {

	// callibration files
	public TextAsset model_joint_positions_file;
//	public TextAsset skeleton_calibration_file;
	public string skeleton_calibration_file_path;

	// skinned mesh of the 3D model
	public SkinnedMeshRenderer skin;

	// location and name of config files
	public string mocap_config_file_path;

	// Mocap is the wrapper to the IK DLL
	private Mocap mocap;
	
	// this keeps a poitner to all body joints
	//public List<GameObject> jointTF {get; protected set;}
	public List<Transform> jointTF {get; protected set;}
//	public List<List<Transform>> jointTF {get; protected set;}


	// Initialization
	void Start () {

		ScaleMesh();
		jointTF = new List<Transform>();
		//jointTF = new List<List<Transform>>();
		//jointTF = new List<GameObject>();
		mocap = new Mocap(mocap_config_file_path);
		jointTF.Clear();
		for(int i = 0; i < mocap.no_of_joints; ++i){
			//Transform childJoint = transform.GetComponentsInChildren().FirstOrDefault(t => t.name == mocap.joint_names[i]);
//			Transform childJoint = this.transform.Find(mocap.joint_names[i]);
			Transform childJoint = Extensions.Search(this.transform, mocap.joint_names[i]);
//			List<Transform> childJoint = new List<Transform>();
//			Extensions.SearchAll(this.transform, mocap.joint_names[i], ref childJoint);

			if (childJoint != null){
				jointTF.Add(childJoint);
			}
		}
			//jointTF.Add(GameObject.Find(mocap.joint_names[i]));
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

		GameObject.Find("HumanoidRoot").transform.Translate(0.0F, -y_dif, 0.0F, Space.World);
		
		skin.sharedMesh.RecalculateNormals();
		skin.sharedMesh.RecalculateBounds();
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
		Transform joint_to_be_translated = GameObject.Find(joint_name).transform;

		float initial_len = joint_to_be_translated.localPosition.magnitude;
		
		float scale_factor = (float)bone_length / initial_len;
		joint_to_be_translated.localPosition *= scale_factor;
	}


	// update root position and joint values
	void Set_joint_values()
	{
	//	Debug.Log (jointTF.Count);
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
		for(int i = 0; i < jointTF.Count; ++i)
		{
	//		for(int j = 0; j < jointTF[i].Count; ++j){
			//Transform joint = GameObject.Find(mocap.joint_names[i]).transform;
				Transform joint = jointTF[i];//.transform; // more efficient, uses a predefined list of pointers instead of name search
				Quaternion q = new Quaternion((float) mocap.orientations[i, 0], 
											  (float) mocap.orientations[i, 2],
											  (float) -mocap.orientations[i, 1], 
											  (float) mocap.orientations[i, 3]);
				if (q.x != q.x){ Debug.Log("quaternion NaN!");}
				else if (joint != null)
					joint.localRotation = q;
		//	}
		}


	}
}
