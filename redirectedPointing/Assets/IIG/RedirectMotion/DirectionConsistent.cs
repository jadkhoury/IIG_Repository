using UnityEngine;
using System.Collections;

public enum Interpolation{
	linear = 0,
	logistic = 1
};

public class DirectionConsistent : MonoBehaviour {
	public float deviationMagnitude= 0.2f;
	public Vector3 direction=Vector3.left;

	public string refMkrName = "RSHO\t";
	public string midMkrName = "RELB\t";
	public string EEMkrName = "RWRI\t";

	public Interpolation interpolationMethod = Interpolation.linear;
	public float startInterpolationAt = 0.2f, stopInterpolationAt = 0.6f; 

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

		Vector3 refP = refMkr.transform.position; 
		Vector3 midV = midMkr.transform.position - refP;
		Vector3 endV = EEMkr.transform.position - refP;

		float dist = Vector3.Magnitude(new Vector3(endV.x, 0, endV.z));
		float coef = (dist - startInterpolationAt) / (stopInterpolationAt - startInterpolationAt);
		//float coef = 0.0f;
		if (interpolationMethod == Interpolation.linear) {
			coef = Mathf.Clamp (coef, 0.0f, 1.0f);
		} else {
			coef = coef * 10 -5;
			coef = 1/(1+Mathf.Exp(-coef)); // logistic
		}
		this.transform.position = refP + endV + direction.normalized * coef * deviationMagnitude;


	}
}
