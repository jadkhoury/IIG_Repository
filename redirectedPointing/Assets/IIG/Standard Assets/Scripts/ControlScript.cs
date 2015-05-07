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
	public bool blockRunning = false;


	private GameObject display;
	private float currentTime;
	private bool trialCompleted = false;
	private bool questionAnswered = false;
	private List<List<float>> blocks = new List<List<float>> ();
	[SerializeField]
	private int currentTrial = 0;
	[SerializeField]
	private int currentBlock = 0;
	private float blockLength;
	private CsvFileWriter customCsvWriter;
	CsvRow row;
	private string stringFormatingTime;
	private GameObject question;
	private Animator anim;

	void Awake ()
	{
		blockLength = conditions.Length * repetitionsPerBlock; 
		display = GameObject.FindGameObjectWithTag ("Display");
		anim = GameObject.FindGameObjectWithTag ("GUI").GetComponent<Animator>();
		question = GameObject.FindGameObjectWithTag ("Text");
		question.SetActive (false);
		stringFormatingTime = GetComponent<MotionLog> ().stringFormatingTime;
		for (int i = 0; i < nbBlocks; i++) {
			blocks.Add (new List<float> ());
		}
		fillBlocks ();
	}


	// Use this for initialization
	void Start ()
	{
		customCsvWriter = new CsvFileWriter (nameOfSubject + ".csv");
		row = new CsvRow ();
		row.Add ("Block nb");
		row.Add ("Trial nb");
		row.Add ("Start time");
		row.Add ("End time");
		row.Add ("Target Radius");
		row.Add ("Virtual Target Radius");
		row.Add ("Distortion percieved");
		customCsvWriter.WriteRow (row);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.Space)) {
			StartBlock ();
		}
		//For testing
		if (Input.GetKeyUp (KeyCode.T)) {
			display.GetComponent<TargetManager> ().Trigger ();
		}
		//For testing
		if (Input.GetKeyUp (KeyCode.Y)) {
			QuestionAnswered ("YES");
		}
		//For testing
		if (Input.GetKeyUp (KeyCode.N)) {
			QuestionAnswered ("NO");
		}
		//For testing
		if (Input.GetKeyUp (KeyCode.R)) {
			anim.SetTrigger ("FadeOut");
		}
		//For testing
		if (Input.GetKeyUp (KeyCode.E)) {
			anim.SetTrigger ("End");
		}
		//For testing
		if (Input.GetKeyUp (KeyCode.P)) {
			anim.SetTrigger ("Pause");
		}
		//********FLLUX IMPLEMENTATION********//
		if (trialCompleted) {
			trialCompleted = false;
			EndTrial ();
			Invoke ("AskQuestion", 1);
		}
		if (questionAnswered) {
			questionAnswered = false;
			++currentTrial;
			if (currentTrial < blockLength) {
				Invoke ("StartNextTrial", 1);
			} else {
				if (currentBlock < nbBlocks) {
					currentBlock++;
					pauseBetweenBlocks ();
				} else {
					EndExperiment ();
				}
			}
		}
	}

	void OnDestroy ()
	{
		customCsvWriter.Close ();
	}

	private void StartNextTrial ()
	{
		//LOG
		row = new CsvRow ();
		string timestamp = Time.time.ToString (stringFormatingTime);
		row.Add (currentBlock.ToString ());
		row.Add (currentTrial.ToString ());
		row.Add (timestamp);
		//Begin
		this.virtualTargetRadius = blocks [currentBlock] [currentTrial];
		Debug.Log ("tR =" + targetRadius + ", vTR =" + virtualTargetRadius);
		display.GetComponent<TargetManager> ().Run ();
	}

	public void TrialCompleted ()
	{
		trialCompleted = true;
		
	}

	private void EndTrial ()
	{
		TargetManager tgtManager = display.GetComponent<TargetManager> ();
		tgtManager.DestroyTargets ();
		string timestamp = Time.time.ToString (stringFormatingTime);
		row.Add (timestamp);
		row.Add (targetRadius.ToString ());
		row.Add (virtualTargetRadius.ToString ());
	}

	private void AskQuestion ()
	{
		question.SetActive (true);
		AnswerManager aManager = display.GetComponent<AnswerManager> ();
		aManager.Run ();
	}

	public void QuestionAnswered (string s)
	{
		display.GetComponent<AnswerManager> ().Destroy ();
		;
		Debug.Log ("Question Aswered: " + s);
		questionAnswered = true;
		question.SetActive (false);
		row.Add (s);
		customCsvWriter.WriteRow (row);
	}

	private void StartBlock ()
	{
		if (blockRunning)
			return;
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("PauseClip"))
			anim.SetTrigger("FadeOut");
		blockRunning = true;
		currentTrial = 0;
		StartNextTrial ();
	}

	private void EndExperiment ()
	{
		customCsvWriter.Close ();
		anim.SetTrigger("End");
	}
	
	private void pauseBetweenBlocks ()
	{
		blockRunning = false;
		anim.SetTrigger ("Pause");
	}

	private void fillBlocks ()
	{
		foreach (float param in conditions) {
			foreach (List<float> block in blocks) {
				for (int j = 0; j < repetitionsPerBlock; j++) {
					block.Add (param);
				}
			}
		}
		foreach (List<float> block in blocks) {
			block.Shuffle ();			
		}
	}
}
