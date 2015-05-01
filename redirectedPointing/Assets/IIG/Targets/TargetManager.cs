//author: Jad Khoury
//Script managing the target
using UnityEngine;

public class TargetManager : MonoBehaviour
{
	public GameObject targetPrefab;
	
	private bool running = false; //Made public to test. Should eventually become private.
	private bool triggered = false; 
	private int activationCounter = 0;
	private controlScript control;
	private int nbTargets;
	private float circleRadius;
	private float tR;
	private float aR;
	private GameObject activeTarget;
	private int[] order;
	private GameObject hand;
	private RedirectSin distortionScript;
	private bool waitingToLeaveAR = false;
	//Tests variable
	//private float nextActionTime = 0.0f;
	//private float period = 3f;

	void Awake(){
		control = GameObject.FindGameObjectWithTag("GameController").GetComponent<controlScript>(); 
		nbTargets = control.nbTargets;
		circleRadius = control.circleRadius;
		aR = control.actionRange;
		tR = control.targetRadius; 
		hand = control.triggerObject;
		distortionScript = hand.GetComponent<RedirectSin>();
		order = computeOrder();
	}


	void Start(){
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
		if (waitingToLeaveAR && distortionScript.getDistanceToTarget() > control.actionRange){
			waitingToLeaveAR = false;
			distortionScript.setTarget(activeTarget.transform);
			Debug.Log("Switched distorion target: " + activeTarget.name);
		}
		
	}

	public void Run(){
		if (running == true)
			return;
		running = true;
		EnableBall(order[activationCounter]);
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
		++activationCounter;
		waitingToLeaveAR = true;
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
			float x = circleRadius * Mathf.Cos(angle);
			float y = 0.021f; //dependant to the size of the mesh cube
			float z = circleRadius * Mathf.Sin(angle);
			GameObject targetClone = (GameObject) Instantiate(targetPrefab, transform.position + new Vector3(x, y, z), Quaternion.identity);
			targetClone.transform.parent = transform;
			targetClone.transform.rotation = targetClone.transform.parent.rotation;
			targetClone.transform.Rotate(new Vector3(0, 0,-angle*180/Mathf.PI));
			targetClone.transform.localScale = new Vector3(2*tR, 2*tR, 0.005f); //because the scale rep the diameter not the radius
			BoxCollider box = targetClone.AddComponent<BoxCollider>();
			box.size = (new Vector3(aR/tR, aR/tR, 0.3f));
			targetClone.name = "Target_" + i;
			targetClone.GetComponent<TargetScript>().Init();


		}

	}

}


