// mimic transformations of one to another transformation hiearchy.
// e.g. from one animated character to a second one with no animation.
// it assumes the names of GOs on each transformation hierarchy are equal
// Henrique Galvan Debarba - 2014

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MimicTransform : MonoBehaviour {

	public Transform mimicFrom;
	public Transform mimicTo;

	public bool mimicHierarchy = true;

	public bool mimicPosition = true;
	public bool mimicRotation = true;
	public bool mimicScale = true;

	List<Tuple<Transform,Transform>> mimicFromTo;


	void FindMatches(Transform rootNode){
		Transform thisMatch = Extensions.Search (mimicFrom, rootNode.name);
		if (thisMatch != null) {
			mimicFromTo.Add(new Tuple<Transform, Transform>(thisMatch,rootNode));
		}
		if (mimicHierarchy) {
			for (int i=0; i < rootNode.childCount; i++) {
				FindMatches (rootNode.GetChild (i));
			}
		}
	}


	// Use this for initialization
	void Start () {
		mimicFromTo = new List<Tuple<Transform, Transform>> ();
		FindMatches (mimicTo);
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < mimicFromTo.Count; i++) {
			if (mimicPosition)
				mimicFromTo[i].Second.localPosition = mimicFromTo[i].First.localPosition;		
			if (mimicRotation)
				mimicFromTo[i].Second.localRotation = mimicFromTo[i].First.localRotation;
			if (mimicScale)
				mimicFromTo[i].Second.localScale = mimicFromTo[i].First.localScale;
		}
	}
}
