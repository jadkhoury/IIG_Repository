//author: Jad Khoury
//Class used to compute and apply the distortion to the object this script is attached to
//from the reference object in regard to the target 
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RedirectLog : MonoBehaviour
{
	public Transform reference;

	private float distanceToTarget;
	private ControlScript control;
	private float tR;
	private float vTR;
	private Transform target;
	private bool isDistorting;
	private float actionRange; // Range in which the distortion happens, in m

	
	void Awake ()
	{
		control = GameObject.FindGameObjectWithTag ("GameController").GetComponent<ControlScript> (); 
		tR = control.targetRadius;
		vTR = control.virtualTargetRadius;
		target = control.defaultTarget;
		reference = control.objectToRedirect;
		actionRange = control.actionRange;
	}
	
	void Update ()
	{
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		this.isDistorting = control.isDistorting;
		this.transform.rotation = reference.transform.rotation;
		this.vTR = control.virtualTargetRadius;
		this.tR = control.targetRadius;
		
		if (!isDistorting) {
			this.transform.position = reference.position;
			return;		
		}
		distanceToTarget = Mathf.Abs (Vector3.Distance (target.position, reference.position));
		float distortion = ComputeDistortion (distanceToTarget);
		Vector3 direction = target.position - reference.position;
		direction.Normalize ();
		Vector3 distortionVector = direction * distortion;
		distortionVector = ProjectOnTarget (distortionVector);
		this.transform.position = reference.position - distortionVector;
		return;
		
		
	}
	
	private Vector3 ProjectOnTarget (Vector3 vect)
	{
		Vector3 normal = target.transform.forward;
		normal.Normalize ();
		Vector3 newVect = new Vector3 ();
		newVect = vect - Vector3.Dot (vect, normal) * normal;
		return newVect;
	}
	
	private float ComputeDistortion (float d)
	{
		if (d > actionRange)
			return 0;
		else if (d < 0.01)
			return 0;
		else {			
			float tmp =  Mathf.Log (tR/actionRange,vTR/actionRange);
			float distortion = actionRange * Mathf.Pow(d/actionRange, tmp) - d; // -d because we add the vector to d
			Debug.Log("Distortion, TR, vTR =  " + tR + " , " + vTR + " , " + distortion); 
			return distortion;
		}
	}
	
	public void SetTarget (Transform newTarget)
	{
		this.target = newTarget;
	}
	
	public float GetDistanceToTarget ()
	{
		return this.distanceToTarget;
	}
}
