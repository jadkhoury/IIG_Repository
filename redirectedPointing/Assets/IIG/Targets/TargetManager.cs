//author: Jad Khoury
//Script managing 
using UnityEngine;

public class TargetManager : MonoBehaviour
{
	public GameObject targetPrefab;
	public int nbTargets;
	public float radius;

	private bool running = false; //Made public to test. Should eventually become private.

	private GameObject activeTarget;
	private int activationCounter = 0;
	private bool triggered = false; 
	private int[] order;
	private GameObject hand;
	private RedirectSin distortionScript;
	//Tests variable
	private float nextActionTime = 0.0f;
	private float period = 3f;


	void Start(){
		hand = GameObject.FindGameObjectWithTag("hand");
		distortionScript = hand.GetComponent<RedirectSin>();
		order = computeOrder();
		createTargets();
		Run();
	}

	void Update(){
		//For testing
		//if (Time.time > nextActionTime ) {
		//	nextActionTime += period;
		//	EnableBall(order[activationCounter]);
		//}
			
		if (running && triggered){
			EnableBall(order[activationCounter]);
			triggered = false;
		}
		if (activationCounter >= order.Length){
			Restart();
			//End();

		}
		
	}

	public void Run(){
		if (running == true)
			return;
		running = true;
		EnableBall(order[activationCounter]);
		Debug.Log("Hey");
	}
	private void End(){ 
		//We should be able to stop the exerience at all time.
		running = false;
		activationCounter = 0;
		if (activeTarget != null){
			activeTarget.GetComponent<TargetScript>().Disable();
			activeTarget = null;
		}
	}

	public void Restart(){
		End ();
		Run ();
	}


	public void Trigger(){
		this.triggered = true;

	}

	private void EnableBall(int index){
		//there will always be only one target active at a time
		if (activeTarget != null){
			activeTarget.GetComponent<TargetScript>().Disable();
		}
		activeTarget = GameObject.Find("Target_" + index);
		activeTarget.GetComponent<TargetScript>().Enable();
		Debug.Log ("Target " + activationCounter + " activated");
		++activationCounter;
		distortionScript.setTarget(activeTarget.transform);
	}

	private int[] computeOrder(){
		int half = Mathf.CeilToInt(nbTargets/2.0f);
		int index = 0;
		int[] order = new int[nbTargets];
		for(int i = 1; i <= half; ++i){
			order[index] = i;
			index++;
			if (index < nbTargets){
				order[index] = i + half;
				index++;
			}
		}
		return order;
	}

	private void createTargets(){
		for (int i = 1; i<= nbTargets; ++i) {
			float angle = i * 2 * Mathf.PI / nbTargets;
			float x = radius * Mathf.Cos(angle);
			float y = 0.005f;
			float z = radius * Mathf.Sin(angle);
			GameObject targetClone = (GameObject) Instantiate(targetPrefab, transform.position + new Vector3(x, y, z), Quaternion.identity);
			targetClone.transform.parent = transform;
			targetClone.name = "Target_" + i;
			targetClone.GetComponent<TargetScript>().Init();

		}

	}

}

