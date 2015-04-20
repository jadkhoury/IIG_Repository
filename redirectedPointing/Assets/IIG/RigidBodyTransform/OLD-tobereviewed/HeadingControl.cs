using UnityEngine;
using System.Collections;

// this class is ment to set an acurate heading diretion based on
// position of three markers, to build a 6DoF absolute reference
// heading computed by oculus sensors
// orientation alignment

public class HeadingControl : MonoBehaviour {

	public GameObject _OVRCamContObj;
	public GameObject _OVRattachedTo;

//	OVRCameraController _OVRCamCont;
	RBTransform _RBTransf;

	float headingOffset = 0;

	float correctionQ = 0;

	Quaternion lastDeviceQ;

	// Use this for initialization
	void Start () {
		if (_OVRCamContObj!=null){
//			_OVRCamCont = _OVRCamContObj.GetComponent<OVRCameraController>();
//			if (_OVRCamCont==null)
//				Debug.Log("HeadingControl: OVRCameraController component not found");
		}
		else
			Debug.Log("HeadingControl: OVR camera controller GO not set");

		if (_OVRattachedTo!=null){
			_RBTransf = _OVRattachedTo.GetComponent<RBTransform>();
			if (_RBTransf==null)
				Debug.Log("HeadingControl: RBTransform component not found");
		}
		else
			Debug.Log("HeadingControl: OVRattachedTo GO not set");
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
		// get 
		Quaternion dQ = Quaternion.identity;
		Vector3 dV = new Vector3();
//		OVRDevice.GetCameraPositionOrientation (ref dV, ref dQ, 0.03f);
//			.GetPredictedOrientation(0, ref dQ);
		// oculus heading + allignemnt - marker allignement = 0
		// new pos update
		if (_RBTransf.justUpdated){
//			Quaternion aQ = /*_RBTransf.transform.rotation;*/_OVRCamContObj.transform.rotation;
			//_OVRCamCont.GetOrientationOffset(ref aOffset);
			Vector3 b =_OVRattachedTo.transform.rotation.eulerAngles;
//			b.x = 0;
//			b.z = 0;
			Quaternion bQ = Quaternion.Euler(b);
//			aQ = dQ;
			Quaternion abDiff = Quaternion.Inverse(dQ) * bQ;
			lastDeviceQ = Quaternion.Slerp(lastDeviceQ,abDiff,0.1f);

			//_OVRCamCont.SetYRotation(abDiff.eulerAngles.y);
			this.transform.rotation = bQ ;//bQ;//Quaternion.Inverse(aQ);//aQ * abDiff;
			Quaternion finalRot = dQ * lastDeviceQ;
	//		finalRot.x = 0;
	//		finalRot.z = 0;
			this.transform.rotation = finalRot;
			//this.transform.rotation = Quaternion.Slerp(dQ,bQ,0.1f);
		}else
		{
		//	Quaternion diffDQ = Quaternion.Inverse(lastDeviceQ)*dQ;
		//	Vector3 diffOnY = diffDQ.eulerAngles;
		//	this.transform.rotation = Quaternion.Euler(diffOnY) * this.transform.rotation;
			this.transform.rotation = dQ ;
		}
		//lastDeviceQ = dQ;
	}
}
