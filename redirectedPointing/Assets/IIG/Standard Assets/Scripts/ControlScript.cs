//author: Jad Khoury
//Script controling the experiment
using UnityEngine;
using System.Collections.Generic;
using ReadWriteCsv;

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
	public bool log = true;
	public string nameOfSubject = "Subject0";
	public float timeBetweenBlocks = 60;



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
	private CsvFileWriter customCsvWriter;
	CsvRow row;
	private string stringFormatingTime;


	void Awake ()
	{
		display = GameObject.FindGameObjectWithTag("Display");
		blockLength = conditions.Length * repetitionsPerBlock; 
		stringFormatingTime = GetComponent<MotionLog>().stringFormatingTime;
		for (int i = 0; i < nbBlocks; i++) {
			blocks.Add(new List<float>());
		}
		fillBlocks();
	}


	// Use this for initialization
	void Start ()
	{
		customCsvWriter = new CsvFileWriter(nameOfSubject + ".csv");
		row = new CsvRow();
		row.Add("Block nb");
		row.Add("Start time");
		row.Add("End time");
		row.Add("Target Radius");
		row.Add("Virtual Target Radius");
		row.Add("Distortion percieved");
		customCsvWriter.WriteRow(row);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.Space)) {
			StartBlock();
		}
		if (Input.GetKeyUp (KeyCode.T)) {
			display.GetComponent<TargetManager>().Trigger();
		}
		if (trialCompleted) {
			trialCompleted = false;
			EndTrial();
			Invoke("AskQuestion", 2);
			//AskQuestion();
		}
		if (questionAnswered){
			questionAnswered = false;
			++currentTrial;
			if (currentTrial < blockLength) {
				Invoke("StartNextTrial", 2);
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

	void OnDestroy(){
		customCsvWriter.Close();
	}


	private void StartNextTrial(){
		row = new CsvRow();
		string timestamp = Time.time.ToString(stringFormatingTime);
		row.Add(currentBlock.ToString ());
		row.Add(timestamp);
		this.virtualTargetRadius = blocks[currentBlock][currentTrial];
		Debug.Log ("tR =" + targetRadius + ", vTR =" + virtualTargetRadius);
		display.GetComponent<TargetManager>().Run();
	}
	public void TrialCompleted (){
		trialCompleted = true;
		
	}
	private void EndTrial(){
		TargetManager tgtManager = display.GetComponent<TargetManager>();
		tgtManager.DestroyTargets();
		string timestamp = Time.time.ToString(stringFormatingTime);
		row.Add(timestamp);
		row.Add(targetRadius.ToString());
		row.Add(virtualTargetRadius.ToString());
	}

	private void AskQuestion(){
		//TODO display gui text for question
		AnswerManager aManager = display.GetComponent<AnswerManager>();
		aManager.Run();
	}

	public void QuestionAnswered(string s){
		Debug.Log("Question Aswered: "+s);
		questionAnswered = true;
		row.Add(s);
		customCsvWriter.WriteRow(row);
	}


	

	private void StartBlock(){
		currentTrial = 0;
		StartNextTrial();
	}

	private void EndExperiment(){
		// TODO
	}
	
	private void pauseBetweenBlocks(){
		//TODO: display something
		Invoke("StartBlock", timeBetweenBlocks); 
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
