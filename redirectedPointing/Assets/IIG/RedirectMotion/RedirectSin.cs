﻿//author: Jad Khoury
//Class used to compute and apply the distortion to the object this script is attached to
//from the reference object in regard to the target 

using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class RedirectSin : MonoBehaviour {
//lelele

	public bool isOn = false;

	private Transform target; 
	private Transform reference;
	private bool negativeDistortion;
	private bool isDistorting;
	private controlScript control = GameObject.FindGameObjectWithTag("GameController"); 
	private float B;
	private float A;
	private float actionRange; // Range in which the distortion happens, in m
	private float strength; // Amplitude of the sinusoid. Between O and 100.

	void Awake(){
		target = control.defaultTarget;
		reference = control.objectToRedirect;
		actionRange = control.actionRange;
		B = Mathf.PI / actionRange;
		A = 1f/B;
		float tmp = A * Mathf.Sin(B*control.collisionRadius);
		strength = 100 * (control.collisionRadius / control.targetRadius) /  tmp;


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
		if (!isOn)
			return;

		float distanceToTarget = Vector3.Distance (target.position, reference.position);
		distanceToTarget = Mathf.Abs(distanceToTarget);
		if (!isDistorting) {
			this.transform.position = reference.position;
			return;		
		}

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
		d = Mathf.Abs (d); //Just to make sure.
		if (d > actionRange)
			return 0;
		else if (Mathf.Abs(d) < 0.001)
			return 0;
		else
		{
		//if (strength > 100)
		//		strength = 100; //max is 100%
			
			float B = Mathf.PI / actionRange;
			float A = 1f/B;
			float distortion = strength/100f * A * Mathf.Sin(B*d);
			return distortion;
		}
	}

	public void setTarget(Transform newTarget){
		this.target = newTarget;
	}
}
