// this script collects the events from each SimpleTarget.cs component on GOs
// under "targetsParent" hierarchy.
// Henrique Galvan Debarba - 2014


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleTargetManager : MonoBehaviour {
	SimpleTarget[] targets;// = new List<SimpleTarget>():
	public Transform targetsParent;
	
	void Start () {
		if (targetsParent == null)
			targetsParent = this.transform;

		targets = targetsParent.GetComponentsInChildren<SimpleTarget> ();
	}
	
	// collect only triggers with GO names specified in objNamesStr
	// if objNamesStr is empty it collects all the triggers.
	public List<Tuple<string[], Tuple<string, float>>> GetContactsLogs(string objNamesStr){
		char[] sep = new char[]{ ' ', ',', ';' };
		string[] objNames = objNamesStr.Split (sep);
		List<Tuple<string[], Tuple<string, float>>> selectContacts = 
			new List<Tuple<string[], Tuple<string, float>>> ();
		// Get all relevant contacts (if any)
		for (int i=0; i<targets.Length; i++){
			for (int j=0; j<targets[i].contactLog.Count; j++) {
				for (int objID = 0; objID<objNames.Length; objID++){
					for (int strID = 0; strID<targets[i].contactLog[j].First.Length; strID++){
						if (objNames == null && objNames[objID] == targets[i].contactLog[j].First[strID]){
							selectContacts.Add(targets[i].contactLog[j]);
						}
					}
				}
			}
		}
		// sort events by time
		selectContacts.Sort((x, y) => x.Second.Second.CompareTo(y.Second.Second));

		return selectContacts;
	}
	
	public void ClearContactLogs(){
		for (int i=0; i<targets.Length; i++){
			for (int j=0; j<targets[i].contactLog.Count; j++) {
				targets[i].contactLog.Clear();
			}
		}
	}
}
