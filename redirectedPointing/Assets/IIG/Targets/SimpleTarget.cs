// this script keeps track of trigger contacts the GO with this component received. 
// It records time of trigger, action (start/end of contact) and the name of GOs involved
// Henrique Galvan Debarba - 2014

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleTarget : MonoBehaviour {
	public List<Tuple<string[], Tuple<string, float>>> contactLog { get; private set; }
	
	void Start () {
		// initialize 
		contactLog = new List<Tuple<string[], Tuple<string, float>>> ();

		// make sure there is a collider, and that it is set as a trigger
		if (this.collider == null) {
			this.gameObject.AddComponent<SphereCollider>();		
		}
		this.collider.isTrigger = true;

	}

	void OnTriggerEnter(Collider otherObj){
		ContactEntry (otherObj, "start");
	}
	void OnTriggerStay(Collider otherObj){

	}
	void OnTriggerExit(Collider otherObj){
		ContactEntry (otherObj, "end");
	}

	// add and entry to contactLog
	void ContactEntry (Collider otherObj, string action){
		Tuple<string, float> contact = new Tuple<string, float>(action, Time.time);
		string[] names = new string[] {otherObj.name, this.name};
		contactLog.Add (new Tuple<string[], Tuple<string, float>>(names, contact));
	}
}