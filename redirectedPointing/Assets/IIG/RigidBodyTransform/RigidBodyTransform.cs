/**************************************************************************
 * RBTransform.cs defines a coordinate system based on at least 3 tracked 
 * points attached to a rigid body
 * Written by Samuel Gruner and Henrique Galvan Debarba
 * Last update: 05/03/14
 * *************************************************************************/


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class CoordinateSystem {

	public Transform [] markers { get; private set;}

	public Quaternion rotOffset;

	public Vector3 posOffset;

	public void InitCoordSystem (Transform mkr1, Transform mkr2, Transform mkr3, Vector3 pOffset, Quaternion rOffset){
		markers = new Transform[]{mkr1, mkr2, mkr3};
		rotOffset = rOffset;
		posOffset = pOffset;
	}

	public void CreateCoordSystem (Transform mkr1, Transform mkr2, Transform mkr3, Vector3 reference, Quaternion refQuat){
		markers = new Transform[]{mkr1, mkr2, mkr3};
		rotOffset = Quaternion.Inverse (refQuat) * ComputeRotation ();
		Vector3 meanPos = (mkr1.position + mkr2.position + mkr3.position) / 3;
		posOffset = reference - meanPos;
		posOffset = Quaternion.Inverse (refQuat) * posOffset;
	}

	Quaternion ComputeRotation(){
		
		// 3 perpendicular vectors constructed using 3 points 
		Vector3 vec1 = (markers[2].position - markers[0].position).normalized;
		Vector3 vec3 = Vector3.Cross(vec1,(markers[1].position - markers[0].position)).normalized;
		Vector3 vec2 = Vector3.Cross(vec3,vec1).normalized;
		
		// orientation matrix based on coord system - it express the rotation from world to the above coord system		
		Matrix4x4 trayOrientationMat = Matrix4x4.identity;
		trayOrientationMat.m00 = vec1.x;
		trayOrientationMat.m01 = vec1.y;
		trayOrientationMat.m02 = vec1.z;
		trayOrientationMat.m10 = vec2.x;
		trayOrientationMat.m11 = vec2.y;
		trayOrientationMat.m12 = vec2.z;
		trayOrientationMat.m20 = vec3.x;
		trayOrientationMat.m21 = vec3.y;
		trayOrientationMat.m22 = vec3.z;
		
		// inverse of the orientation TODO ?? inverse should be == to transpose
		return Quaternion.Inverse (Extensions.QuaternionFromMatrix(trayOrientationMat));
	}

	bool MarkersAreVisible(){
		if (markers [0].tag == "markerNotVisible" || markers [1].tag == "markerNotVisible" || markers [2].tag == "markerNotVisible")
			return false;
		return true;
	}

	public bool GetPosAndRot (out Vector3 position ,out Quaternion rotation){
		rotation = ComputeRotation () * Quaternion.Inverse (rotOffset);
		Vector3 meanPos = (markers[0].position + markers[1].position + markers[2].position) / 3;
		position = meanPos + (rotation * posOffset);
		//	position = markers[0].position + (ComputeRotation () * Quaternion.Inverse (rotOffset)) * posOffset;
		return MarkersAreVisible();
	}
	
}


public class RigidBodyTransform : MonoBehaviour {

	public enum updateCallback {UPDATE, LATE_UPDATE, FIXED_UPDATE};
	public updateCallback callback = updateCallback.UPDATE;
	public bool updateRotation = true;


	// requires initialization
	public bool initialized {get{return _INIT;} private set{_INIT = value;}}

	bool _INIT = false;

	public Transform controlledTr;



	List<Transform> markersTr = new List<Transform>();
	public string [] markerNames;

	List<CoordinateSystem> coordSystems = new List<CoordinateSystem>();


	public string _fileNameForPersistentData = "";

	public bool loadSavedCalibration = false;



	public void Load()
	{
		if( !File.Exists( Application.persistentDataPath
		                 + _fileNameForPersistentData ) )
		{
			Debug.Log ("file " + Application.persistentDataPath
			           + _fileNameForPersistentData + " could not be found");
			return;
//			InitializePersistentData();
		}
		
		if( File.Exists( Application.persistentDataPath
		                + _fileNameForPersistentData ) )
		{
			BinaryFormatter bf = new BinaryFormatter();
			
			FileStream file = File.Open( Application.persistentDataPath
			                            + _fileNameForPersistentData, FileMode.Open );
			
//			GameData data = ( GameData )bf.Deserialize( file );

			CalibrationData data = ( CalibrationData )bf.Deserialize( file );

			file.Close();

			FindMarkers();
			InitializeCS(data);
//			num = data.numToSerialize;              
		}
	}

	public void Save()
	{
		BinaryFormatter bf = new BinaryFormatter();

		FileStream file = File.Create( Application.persistentDataPath
		                              + _fileNameForPersistentData );
		
		CalibrationData data = new CalibrationData();



		string[,] CSNames = new string[coordSystems.Count,3];
		float[,] CSPosOffset = new float[coordSystems.Count,3];
		float[,] CSRotOffset = new float[coordSystems.Count,4];
		for (int i = 0; i<coordSystems.Count; i++){
			for (int j = 0; j<3; j++)
				CSNames[i,j] = coordSystems[i].markers[j].name;
			CSPosOffset[i,0] = coordSystems[i].posOffset.x;
			CSPosOffset[i,1] = coordSystems[i].posOffset.y;
			CSPosOffset[i,2] = coordSystems[i].posOffset.z;
			CSRotOffset[i,0] = coordSystems[i].rotOffset.x;
			CSRotOffset[i,1] = coordSystems[i].rotOffset.y;
			CSRotOffset[i,2] = coordSystems[i].rotOffset.z;
			CSRotOffset[i,3] = coordSystems[i].rotOffset.w;
		}
		data.markerNames = markerNames;
		data.CSNames = CSNames;
		data.CSPosOffsets = CSPosOffset;
		data.CSRotOffsets = CSRotOffset;
//		data.numToSerialize = num;
		
		bf.Serialize( file, data );
		
		file.Close();
	}

	void FindMarkers(){
		foreach (string mkrName in markerNames) {
			GameObject markerRef = GameObject.Find(mkrName);
			if (markerRef != null){
				markersTr.Add (markerRef.transform);
			}else
				Debug.LogWarning ("marker " + mkrName + " could not be found");
		}
		if (markersTr.Count < 3)
			Debug.LogError ("RigidBody.cs: not enough points to define a coordinate system");
	}

	void Start() {
		if (_fileNameForPersistentData=="")
			_fileNameForPersistentData = this.gameObject.name;
		_fileNameForPersistentData = "/"+_fileNameForPersistentData+".dat";
		if (loadSavedCalibration)
			Load();
		if (markersTr.Count == 0)
			FindMarkers ();
	}

	void InitializeCS(CalibrationData data){
		int totalCS = data.CSNames.GetLength (0);
		for (int i = 0; i<totalCS; i++) {
			Transform marker1 = GameObject.Find(data.CSNames[i,0]).transform;
			Transform marker2 = GameObject.Find(data.CSNames[i,1]).transform;
			Transform marker3 = GameObject.Find(data.CSNames[i,2]).transform;
			Vector3 posOffset = new Vector3(data.CSPosOffsets[i,0],data.CSPosOffsets[i,1],data.CSPosOffsets[i,2]);
			Quaternion rotOffset = new Quaternion(data.CSRotOffsets[i,0],data.CSRotOffsets[i,1],data.CSRotOffsets[i,2],data.CSRotOffsets[i,3]);
			CoordinateSystem newCS = new CoordinateSystem();
			newCS.InitCoordSystem(marker1, marker2, marker3, posOffset, rotOffset);
			coordSystems.Add(newCS);
		}
		initialized = true;
	}

	void InitializeCS(){
		coordSystems.Clear();
		for (int i=0; i<markersTr.Count-2; i++){
			for (int j=i+1; j<markersTr.Count-1; j++){
				for (int k=j+1; k<markersTr.Count; k++){
					if (i != j && i != k && j != k){
						CoordinateSystem newCS = new CoordinateSystem();
						newCS.CreateCoordSystem(markersTr[i], markersTr[j], markersTr[k], controlledTr.position, controlledTr.rotation);
						coordSystems.Add(newCS);
					}
				}
			}
		}
		if (coordSystems.Count > 0)
			initialized = true;
	}

	void UpdateTransform(){
		if (Input.GetKeyUp (KeyCode.C)) {
			InitializeCS ();
		}
		if (Input.GetKeyUp (KeyCode.S)) 
			Save ();

		if (!initialized)
			return;

		for (int i = 0; i < coordSystems.Count; i++) {
			Quaternion ithCSRot;
			Vector3 ithCSPos;
			bool allVisible = coordSystems[i].GetPosAndRot(out ithCSPos, out ithCSRot);	
			if (allVisible){
				controlledTr.position = ithCSPos;
				if (updateRotation)
					controlledTr.rotation = ithCSRot;

				i = coordSystems.Count;
			}
		}
	}

	void FixedUpdate(){
		if (callback == updateCallback.FIXED_UPDATE)
			UpdateTransform ();
	}
	void LateUpdate(){
		if (callback == updateCallback.LATE_UPDATE)
			UpdateTransform ();
	}
	void Update(){
		if (callback == updateCallback.UPDATE)
			UpdateTransform ();
	}
	

}

[Serializable]
class CalibrationData {
	public string [] markerNames;
	public string [,] CSNames;
	public float [,] CSPosOffsets;
	public float [,] CSRotOffsets;
}
