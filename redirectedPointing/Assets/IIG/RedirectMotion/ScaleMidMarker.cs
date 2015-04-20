using UnityEngine;
using System.Collections;

public class ScaleMidMarker : MonoBehaviour {
	public string midMkrName = "RELB\t";
	public string EEMkrName = "RWRI\t";
	public string refMkrName = "RSHO\t";
	public Transform EEretgt;
	GameObject midMkr, EEMkr, refMkr;

	bool FindMarkers(){
		if (midMkr == null || EEMkr == null || refMkr == null) {
			refMkr = GameObject.Find (refMkrName);
			midMkr = GameObject.Find (midMkrName);
			EEMkr = GameObject.Find (EEMkrName);	

			if (midMkr == null || EEMkr == null || refMkr == null){
				Debug.LogWarning ("ScaleMidMarker: " + midMkrName + " or " + 
				                  EEMkrName + " or " + refMkr + " not found");
				return false;
			}
		}
		return true;
	}

	// Use this for initialization
	void Start () {
		FindMarkers ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!FindMarkers ())
			return;

		// ref point
		Vector3 refP = refMkr.transform.position; 

		Vector3 midP = midMkr.transform.position - refP;

		// dist from ref point to redirected point
		Vector3 endPrtgt = EEretgt.position - refP;
		// dist from ref point to EE marker point
		Vector3 endP = EEMkr.transform.position - refP;
		// proportion in x y z between redir. point and marker point
		//Vector3 proportions = new Vector3 (EEretgtP.x/EEP.x, EEretgtP.y/EEP.y, EEretgtP.z/EEP.z);

		//this.transform.position = new Vector3 (midP.x*proportions.x, 
		//               midP.y*proportions.y, midP.z*proportions.z) + refP;


		float proport = endPrtgt.magnitude / endP.magnitude;
		Vector3 crossp = Vector3.Cross (endP, endPrtgt);
		//Vector3 crossp = Vector3.Cross (endPrtgt, endP);
		float rotAngle = Vector3.Angle(endP,endPrtgt);

		Vector3 midPrtgt = Quaternion.AngleAxis (rotAngle, crossp) * midP;
		midPrtgt =	midPrtgt * proport;
		this.transform.position = midPrtgt + refP;

	}
}
