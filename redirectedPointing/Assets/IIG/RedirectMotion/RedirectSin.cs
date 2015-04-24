//author: Jad Khoury
//Class used to compute and apply the distortion to the object this script is attached to
//from the reference object in regard to the target 

using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class RedirectSin : MonoBehaviour {


	public bool isOn = false;

	private controlScript control;
	private float tR;
	private float cR;
	private Transform target; 
	private Transform reference;
	private bool negativeDistortion;
	private bool isDistorting;
	private float B;
	private float A;
	private float actionRange; // Range in which the distortion happens, in m
	private float strength; // Amplitude of the sinusoid. Between O and 100.

	void Awake(){
		control = GameObject.FindGameObjectWithTag("GameController").GetComponent<controlScript>(); 
		tR = control.targetRadius;
		cR = control.collisionRadius;
		target = control.defaultTarget;
		reference = control.objectToRedirect;
		actionRange = control.actionRange;
		B = Mathf.PI / actionRange;
		A = 1f/B;
		//computing of the strength param in term of the radius
		float tmp = A * Mathf.Sin(B*cR);
		strength = 100 * (cR - tR) /  tmp;


	}

	void Update(){
		if (Input.GetKeyUp (KeyCode.V))
			isDistorting = !isDistorting;
		if (Input.GetKeyUp (KeyCode.B))
			isOn = !isOn; 
		if (Input.GetKeyUp (KeyCode.C))
			negativeDistortion = !negativeDistortion;
	}



	// Update is called once per frame
	void FixedUpdate () {
		this.transform.rotation = reference.transform.rotation;

		if (!isDistorting || !isOn) {
			this.transform.position = reference.position;
			return;		
		}

		float distanceToTarget = Mathf.Abs(Vector3.Distance (target.position, reference.position));
		float distortion = ComputeDistortion(distanceToTarget);
		Vector3 direction = target.position - reference.position;
		direction.Normalize ();
		Vector3 distortionVector = direction * distortion;
		if (negativeDistortion)
			this.transform.position = reference.position - distortionVector;
		else
			this.transform.position = reference.position + distortionVector;
		return;


	}

	private float ComputeDistortion (float d)
	{
		if (d > actionRange)
			return 0;
		//else if (d < 0.001)
		else if (d < tR) // to test
			return 0;
		else
		{			
			float B = Mathf.PI / actionRange;
			float A = 1f/B;
			float distortion = strength/100f * A * Mathf.Sin(B*(d-tR);
			return distortion;
		}
	}

	public void setTarget(Transform newTarget){
		this.target = newTarget;
	}

	public float distanceToTarget(){
		return this.distanceToTarget
	}
}
