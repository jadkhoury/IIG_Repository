/**************************************************************************
 * RBTransform.cs defines a coordinate system based on at least 3 tracked 
 * points attached to a rigid body
 * Written by Samuel Gruner and Henrique Galvan Debarba
 * Last update: 05/03/14
 * *************************************************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RBTransform : MonoBehaviour {
	
	// 
	Vector3 centerRealPosition = Vector3.zero;

	private Quaternion alignmentOffset = new Quaternion();
	
	List<Tuple<Vector3,bool>> markers = new List<Tuple<Vector3,bool>>();
	List<Tuple<Quaternion,bool>> m_rotations = new List<Tuple<Quaternion,bool>>();

	List<int[]> m_CSIndex = new List<int[]>();
	Quaternion currentRotation = new Quaternion();

	bool initialized = false;

	Transform OculusRefObject;
	public Vector3 offset = new Vector3();

	public bool justUpdated {get; private set;}

	public bool isInitialized(){
		return initialized;
	}

	void Start(){

		justUpdated = false;
	}

	Quaternion ComputeRotation(int[] mkrIndex){

		//	markers[mkrIndex[0]].First - markers[mkrIndex[1]].First
		//01 21
		//Vector3 vec1 = (markers[mkrIndex[2]].First - markers[mkrIndex[0]].First).normalized;
		//Vector3 vec2 = Vector3.Cross(vec1, (markers[mkrIndex[1]].First - markers[mkrIndex[0]].First)).normalized;
		//Vector3 vec3 = -Vector3.Cross(vec2,vec1).normalized;
		Vector3 vec1 = (markers[mkrIndex[2]].First - markers[mkrIndex[0]].First).normalized;
		Vector3 vec3 = Vector3.Cross(vec1,(markers[mkrIndex[1]].First - markers[mkrIndex[0]].First)).normalized;
		Vector3 vec2 = Vector3.Cross(vec3,vec1).normalized;
		//Vector3 traySide = (frontRightMarkerPos - frontLeftMarkerPos).normalized;
		//Vector3 trayNormal = Vector3.Cross(traySide, (rearLeftMarkerPos - frontLeftMarkerPos)).normalized;
		//Vector3 trayFront = Vector3.Cross(traySide,trayNormal).normalized;


		
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
/*		// transpose
		trayOrientationMat.m00 = vec1.x;
		trayOrientationMat.m10 = vec1.y;
		trayOrientationMat.m20 = vec1.z;
		trayOrientationMat.m01 = vec2.x;
		trayOrientationMat.m11 = vec2.y;
		trayOrientationMat.m21 = vec2.z;
		trayOrientationMat.m02 = vec3.x;
		trayOrientationMat.m12 = vec3.y;
		trayOrientationMat.m22 = vec3.z;
*/

		return Quaternion.Inverse (QuaternionFromMatrix(trayOrientationMat));
	//	return QuaternionFromMatrix(trayOrientationMat);
	}

	void UpdateRotations(){
		for (int i=0; i<m_CSIndex.Count; i++){
			if (markers[m_CSIndex[i][0]].Second && markers[m_CSIndex[i][1]].Second && markers[m_CSIndex[i][2]].Second){
				m_rotations[i].First=ComputeRotation(m_CSIndex[i]);
				m_rotations[i].Second = true;
			}
			else
				m_rotations[i].Second = false;
		}
	}
	void UpdateCoordSystems(){
		m_CSIndex.Clear();
		m_rotations.Clear();
		for (int i=0; i<markers.Count-2; i++){
			for (int j=1; j<markers.Count-1; j++){
				for (int k=2; k<markers.Count; k++){
					int[] newIndex={i,j,k};
					m_CSIndex.Add(newIndex);
					m_rotations.Add (new Tuple<Quaternion,bool>(new Quaternion(),false));
				}
			}
		}
		UpdateRotations();
	}

	// notice that this should be called every marker position update
	// this function manages a list, adding and erasing at index, 
	// this may rend it inneficient if the amount of markers sent changes very often
	public bool InitMarker(ref Vector3[] positions,ref bool[] visibilty){
		if (positions.Length!= visibilty.Length)
			return false;
		// set markers position and visibility
		for (int i=0; i<positions.Length; i++){
			if (markers.Count<=i)
				markers.Add(new Tuple<Vector3, bool>(new Vector3(),false));

			markers[i].First = positions[i];
			markers[i].Second = visibilty[i];
		}
		// erases markers that are no longer used
		if (positions.Length<markers.Count)
			for (int i = positions.Length; i<markers.Count; i++)
				markers.RemoveAt(i);
		// generate CD indexes and compute possible rotations
		UpdateCoordSystems();
		initialized = true;
		return true;
	}

	public bool UpdateMarkers(ref Vector3[] positions,ref bool[] visibilty){
		if (!initialized || positions.Length!= visibilty.Length || positions.Length!= markers.Count)
			return false;

		for (int i=0; i<positions.Length; i++){
			markers[i].First = positions[i];
			markers[i].Second = visibilty[i];
		}
		// compute possible rotations
		UpdateRotations();
		UpdateTransform ();
		return true;
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}

	void UpdateTransform(){


			
		int validRotations = 0;
		for (int i=0; i<m_rotations.Count; i++){
			if (m_rotations[i].Second){
				validRotations++;
				currentRotation = m_rotations[i].First;

			}
		}
		if (markers[0].Second)
			centerRealPosition = markers[0].First;
		//break;
		justUpdated = (validRotations==0) ? false : true;

		// TODO alignment will occour here  - alignmentOffset * rbRotation
		transform.rotation = currentRotation;
		transform.position = centerRealPosition + currentRotation*offset;
	}


}
