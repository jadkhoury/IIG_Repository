using UnityEngine;
using System.Collections;

public class RotationCorrection : MonoBehaviour {

	public Transform correct;
	public Transform drifting;

	//public Transform debug1;
	//public Transform debug2;
	Quaternion lastDelta;
	Quaternion correction = Quaternion.identity;

	// Use this for initialization
	void Start () {
	//	lastDelta = correct.rotation * Quaternion.Inverse (drifting.rotation);

	}
	
	// Update is called once per frame
	void LateUpdate () {
		lastDelta = correction;
		//Quaternion currentDelta = (correct.rotation) * Quaternion.Inverse (drifting.localRotation);// * Quaternion.Inverse(lastDelta));
		Quaternion currentDelta = (correct.rotation) * Quaternion.Inverse (Quaternion.Inverse(lastDelta) * drifting.localRotation);// * );
		//Quaternion currentDelta = Quaternion.Inverse (Quaternion.Inverse(lastDelta) * drifting.localRotation) * (correct.rotation);// * );
		//Quaternion currentDelta = Quaternion.Inverse (drifting.localRotation) * (correct.rotation);// * );

		//Quaternion currentDelta = drifting.rotation * Quaternion.Inverse (correct.rotation);
		correction = Quaternion.Slerp (correction, currentDelta, 0.005f);

		this.transform.rotation =   correction;

		//debug1.rotation = correction * (drifting.rotation * Quaternion.Inverse(lastDelta));
		//debug2.rotation =  correction;
	}
}
