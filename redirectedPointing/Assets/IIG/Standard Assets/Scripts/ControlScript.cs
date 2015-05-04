//author: Jad Khoury
//Script controling the experiment
using UnityEngine;
using System.Collections.Generic;

public class ControlScript : MonoBehaviour
{
	public Transform objectToRedirect;
	public GameObject triggerObject;
	public Transform defaultTarget;
	public bool isDistorting;
	public bool negativeDistortion;

	public float actionRange = 0.125f;
	public float targetRadius = 0.015f;
	public float virtualTargetRadius = 0.03f;
	public float circleRadius = 0.125f;
	public int nbTargets = 12;
	public int nbBlocks;
	public int repetitionsPerBlock;
	public float[] conditions;


	[SerializeField]
	private GameObject display;
	private float currentTime;
	private bool trialCompleted = false;
	private bool questionAnswered = false;
	private List<List<float>> blocks = new List<List<float>>();
	[SerializeField]
	private int currentTrial = 0;
	[SerializeField]
	private int currentBlock = 0;
	private float blockLength;

	void Awake ()
	{
		display = GameObject.FindGameObjectWithTag("Display");
		blockLength = conditions.Length * repetitionsPerBlock; 
		for (int i = 0; i < nbBlocks; i++) {
			blocks.Add(new List<float>());
		}
		fillBlocks();
	}
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.Space)) {
			startBlock();
		}
		if (Input.GetKeyUp (KeyCode.T)) {
			display.GetComponent<TargetManager>().Trigger();
		}
		if (trialCompleted) {
			trialCompleted = false;
			EndTrial();
			AskQuestion();
		}
		if (questionAnswered){
			questionAnswered = false;
			++currentTrial;
			if (currentTrial < blockLength) {
				startNextTrial();
			} else {
				if (currentBlock < nbBlocks) {
					currentBlock++;
					pauseBetweenBlocks();

				} else {
					EndExperiment();
				}
			}
		}
	}


	private void EndTrial(){
		TargetManager tgtManager = display.GetComponent<TargetManager>();
		tgtManager.DestroyTargets();
	}

	private void AskQuestion(){
		AnswerManager aManager = display.GetComponent<AnswerManager>();
		aManager.Run();

	}	
	public void TrialCompleted (){
		trialCompleted = true;

	}

	public void QuestionAnswered(){
		questionAnswered = true;
	}

	private void startNextTrial(){
		this.virtualTargetRadius = blocks[currentBlock][currentTrial];
		display.GetComponent<TargetManager>().Run();

	}
	

	private void startBlock(){
		currentTrial = 0;
		Debug.Log("startblock");
		virtualTargetRadius = blocks[currentBlock][currentTrial];
		display.GetComponent<TargetManager>().Run();
	}

	private void EndExperiment(){
		// TODO
	}
	
	private void pauseBetweenBlocks(){
		//TODO
	}

	private void fillBlocks(){
		foreach (float param in conditions) {
			foreach (List<float> block in blocks) {
				for (int j = 0; j < repetitionsPerBlock; j++) {
					block.Add(param);
				}
			}
		}
		foreach (List<float> block in blocks) {
			block.Shuffle();			
		}
	}
}
