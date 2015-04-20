using UnityEngine;
using System.Collections;

public class MagneticTarget : MonoBehaviour {

	public float distThreshould = 0.5f;
	public Transform intendedTarget;
	// THIS HAS TO BE CHANGED BY THE LIMB OUTPUT OF THE FIRST IK PHASE
	public string markerName;
	public float maxDeviationPercent = 0.5f;
	float maxDeviationMagnitude = 0;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		maxDeviationMagnitude = distThreshould * maxDeviationPercent;
		GameObject actualEndEffectorGO = GameObject.Find (markerName);
		Transform actualEndEffector = actualEndEffectorGO.transform;
		Vector3 intendedDirection = intendedTarget.position - actualEndEffector.position;
		float distance = Mathf.Abs (intendedDirection.magnitude);
		if (distance < distThreshould) {
			float scalar = 1.0f - distance/distThreshould;
			this.transform.position = actualEndEffector.position + Vector3.ClampMagnitude(intendedDirection, scalar * maxDeviationMagnitude) ;
		} else {
			this.transform.position = actualEndEffector.position;
		}
		this.transform.rotation = actualEndEffector.rotation;
	}
}
