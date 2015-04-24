//author: Jad Khoury
//Script controling the experiment

using UnityEngine;
using System.Collections;

public class controlScript : MonoBehaviour {

	public float actionRange = 0.125;
	public float targetRadius = 1.5;
	public float collisionRadius;
	public float circleRadius = 0.125;
	public float nbTarget = 12;
	public Transform objectToRedirect;
	public Transform defaultTarget;
	public bool negativeDistortion;
	public bool isDistortting;
	public GameObject triggerObject;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
