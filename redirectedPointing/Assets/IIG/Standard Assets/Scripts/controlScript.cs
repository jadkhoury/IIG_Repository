//author: Jad Khoury
//Script controling the experiment
using UnityEngine;
using System.Collections;

public class controlScript : MonoBehaviour
{
	
	public float actionRange = 0.125f;
	public float targetRadius = 1.5f;
	public float virtualTargetRadius;
	public float circleRadius = 0.125f;
	public int nbTargets = 12;
	public Transform objectToRedirect;
	public Transform defaultTarget;
	public bool negativeDistortion;
	public bool isDistorting;
	public GameObject triggerObject;
	[SerializeField]
	private GameObject display;
	

	public void Awake ()
	{
		actionRange = 0.125f;
		actionRange = 0.125f;
		targetRadius = 0.015f;
		virtualTargetRadius = 0.03f;
		circleRadius = 0.125f;
		display = GameObject.FindGameObjectWithTag("Display");
		
	}
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{

	}
	
	public void BlockCompleted (GameObject caller)
	{
		TargetManager manager = display.GetComponent<TargetManager>();
		manager.Destroy();
	}
	
	
}
