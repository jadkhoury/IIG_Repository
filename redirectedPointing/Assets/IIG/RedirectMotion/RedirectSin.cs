//author: Jad Khoury
//Class used to compute and apply the distortion to the object this script is attached to
//from the reference object in regard to the target 
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RedirectSin : MonoBehaviour
{
	
	
	public bool isOn = true;
	public Transform reference;
	[SerializeField] 
	private float distanceToTarget;
	private controlScript control;
	private float tR;
	private float vTR;
	private Transform target;
	private bool negativeDistortion;
	private bool isDistorting;
	private float B;
	private float A;
	private float actionRange; // Range in which the distortion happens, in m
	[SerializeField] 
	private float strength; // Amplitude of the sinusoid. Between O and 100.
	
	void Awake ()
	{
		control = GameObject.FindGameObjectWithTag ("GameController").GetComponent<controlScript> (); 
		tR = control.targetRadius;
		vTR = control.virtualTargetRadius;
		target = control.defaultTarget;
		reference = control.objectToRedirect;
		actionRange = control.actionRange;
		B = Mathf.PI / actionRange;
		A = 1f / B;
		//computing of the strength param in term of the radius
		float tmp = A * Mathf.Sin (B * vTR);
		strength = 100 * (vTR - tR) / tmp;
		if (strength >= 100)
			strength = 100;
		
		
		
	}
	
	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.V))
			isDistorting = !isDistorting;
		if (Input.GetKeyUp (KeyCode.B))
			isOn = !isOn; 
		if (Input.GetKeyUp (KeyCode.C))
			negativeDistortion = !negativeDistortion;
	}
	
	
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		this.isDistorting = control.isDistorting;
		this.negativeDistortion = control.negativeDistortion;
		this.transform.rotation = reference.transform.rotation;
		
		if (!isDistorting || !isOn) {
			this.transform.position = reference.position;
			return;		
		}
		distanceToTarget = Mathf.Abs (Vector3.Distance (target.position, reference.position));
		float distortion = ComputeDistortion (distanceToTarget);
		Vector3 direction = target.position - reference.position;
		direction.Normalize ();
		Vector3 distortionVector = direction * distortion;
		distortionVector = ProjectOnTarget (distortionVector);
		if (negativeDistortion)
			this.transform.position = reference.position - distortionVector;
		else
			this.transform.position = reference.position + distortionVector;
		//Debug.Log("distortion value: " + distortion);
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
			float B = Mathf.PI / actionRange;
			float A = 1f / B;
			float distortion = strength / 100f * A * Mathf.Sin (B * d);
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
