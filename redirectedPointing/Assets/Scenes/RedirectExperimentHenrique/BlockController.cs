using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockController : MonoBehaviour {

	public int trialRepetitions = 10;
	public float [] distortionProportions;
	public GameObject [] targets;
	public List<int> encodedOrder = new List<int> ();

	int currentTrial = 0;

	//public DistortionScript distortionControl;



	void Start () {
		// TODO check validity of class arguments


		// generate SimpleTargetManager

		// generates random order of factors for a block
		int mX = trialRepetitions, mY = distortionProportions.Length, mZ = targets.Length;
		for (int i=0; i<mX; i++) {
			for (int j=0; j<mY; j++) {
				for (int k=0; k<mZ; k++) {
					encodedOrder.Add(i*mY*mZ + j * mZ + k);
				}	
			}		
		}
		encodedOrder.Shuffle ();


	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartBlock () {
		StartTrial ();
	}

	public void CloseBlock () {
		StartTrial ();
	}

	void AdvanceTrial (){
		StopTrial ();
		currentTrial ++;
		if (encodedOrder.Count <= currentTrial)
			CloseBlock ();
		else
			StartTrial ();

	}

	void StartTrial () {
		int rep, dist, tgt;
		GetIndexes (out rep, out dist, out tgt);

		// activate trial
		//distortionControl.distMagnitude = distortionProportions[dist] * tgtDistance;
		targets [tgt].SetActive (true);


	}

	void StopTrial () {
		int rep, dist, tgt;
		GetIndexes (out rep, out dist, out tgt);

		// logEntry
		// vector of values separated by comas

		// deactivate tgt
		targets [tgt].SetActive (false);
	}




	/* from encoded indexes back to the original indexes */
	bool GetIndexes(out int rep, out int dist, out int tgt){
		int mX = trialRepetitions, mY = distortionProportions.Length, mZ = targets.Length;

		if (encodedOrder.Count <= currentTrial) {
			rep = dist = tgt = -1;
			return false;
		}
		int encoded = encodedOrder [currentTrial];
		rep = encoded / (mZ * mY);
		dist = (encoded - rep * mZ * mY) / mZ;
		tgt = encoded - rep * mZ * mY - dist * mZ;

		return true;
	}
}
