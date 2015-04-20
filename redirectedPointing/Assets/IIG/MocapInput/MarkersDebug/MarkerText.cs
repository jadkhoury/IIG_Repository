using UnityEngine;
using System.Collections;

public class MarkerText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		TextMesh editTextMesh = this.gameObject.GetComponent<TextMesh>();
		editTextMesh.text = this.transform.parent.name;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.rotation = Quaternion.LookRotation( this.transform.position - Camera.main.transform.position );
	}
}
