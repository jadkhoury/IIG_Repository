//author: Jad Khoury
//Script managing the target
using UnityEngine;

public class TargetManager : MonoBehaviour
{
	public GameObject targetPrefab;
	private bool running = false; //Made public to test. Should eventually become private.
	private bool triggered = false;
	private int activationCounter = 0;
	private ControlScript control;
	private int nbTargets;
	private float circleRadius;
	private float tR;
	private float aR;
	private GameObject activeTarget = null;
	private int[] order;
	private GameObject hand;
	private RedirectSin distortionScript;
	private bool waitingToLeaveAR = false;
	//Tests variable
	private float nextActionTime = 1.0f;
	private float period = 1f;
	private GameObject[] targetsArray;

	void Awake ()
	{
		control = GameObject.FindGameObjectWithTag ("GameController").GetComponent<ControlScript> ();
		nbTargets = control.nbTargets;
		circleRadius = control.circleRadius;
		aR = control.actionRange;
		tR = control.targetRadius;
		hand = control.triggerObject;
		distortionScript = hand.GetComponent<RedirectSin> ();
		order = ComputeOrder ();
		targetsArray = new GameObject[nbTargets + 1];
	}

	void Update ()
	{
		//For testing
		//if (running && Time.time > nextActionTime) {
		//	nextActionTime += period;
		//	Trigger ();
		//}ttttt

		if (running && triggered) {
			Debug.Log("Trigger loop");
			triggered = false;
			if (activationCounter < nbTargets) {
				EnableTarget (order [activationCounter]);
			} else {
				End ();
			}
		}

		if (running && waitingToLeaveAR && distortionScript.GetDistanceToTarget () > control.actionRange) {
			waitingToLeaveAR = false;
			distortionScript.SetTarget (activeTarget.transform);
			Debug.Log (Time.time + ": Switched distorion target: " + activeTarget.name);
		}

	}

	public void Run ()
	{
		if (running)
			return;
		CreateTargets ();
		running = true;
		control.isDistorting = true;
		activationCounter = 0;
		EnableTarget (order [activationCounter]);

	}

	private void End ()
	{
		//We should be able to stop the exerience at all time.
		running = false;
		activationCounter = 0;
		triggered = false;
		if (activeTarget != null) {
			activeTarget.GetComponent<TargetScript> ().Disable ();
			activeTarget = null;
		}
		control.isDistorting = false;
		control.TrialCompleted ();

	}

	public void Restart ()
	{
		End ();
		Run ();

	}

	public void DestroyTargets ()
	{
		foreach (GameObject obj in targetsArray) {
			Object.Destroy (obj);
		}
	}

	public void Trigger ()
	{
		this.triggered = true;

	}

	private void EnableTarget (int index)
	{
		//there will always be only one target active at a time
		if (activeTarget != null) {
			activeTarget.GetComponent<TargetScript> ().Disable ();
		}
		activeTarget = targetsArray [index];
		Debug.Log (Time.time + ": " + activeTarget.name);
		activeTarget.GetComponent<TargetScript> ().Enable ();
		if (activationCounter == 0)
			distortionScript.SetTarget (activeTarget.transform);
		++activationCounter;
		waitingToLeaveAR = true;
	}

	private int[] ComputeOrder ()
	{
		int half = Mathf.CeilToInt (nbTargets / 2.0f);
		int index = 0;
		int[] order = new int[nbTargets];
		for (int i = 1; i <= half; ++i) {
			order [index] = i;
			index++;
			if (index < nbTargets) {
				order [index] = i + half;
				index++;
			}
		}
		return order;
	}

	private void CreateTargets ()
	{
		this.transform.rotation = Quaternion.identity; //during the time the targets are created the display is straight
		targetsArray [0] = null;
		for (int i = 1; i<= nbTargets; ++i) {
			float angle = i * 2 * Mathf.PI / nbTargets;
			float x = circleRadius * Mathf.Cos (angle);
			float y = circleRadius * Mathf.Sin (angle);//dependant to the size of the mesh cube
			float z = 0.021f;
			GameObject targetClone = (GameObject)Instantiate (targetPrefab, transform.position + new Vector3 (x, y, z), Quaternion.identity);
			targetClone.transform.parent = transform;
			targetClone.transform.rotation = targetClone.transform.parent.rotation;
			targetClone.transform.Rotate (new Vector3 (0, 0, angle * 180 / Mathf.PI));
			targetClone.transform.localScale = new Vector3 (2 * tR, 2 * tR, 0.005f); //because the scale rep the diameter not the radius
			BoxCollider box = targetClone.AddComponent<BoxCollider> ();
			box.size = (new Vector3 (aR / tR, aR / tR, 0.3f));
			targetClone.name = "Target_" + i;
			targetsArray [i] = targetClone;
			targetClone.GetComponent<TargetScript> ().Init ();
		}

	}
}

