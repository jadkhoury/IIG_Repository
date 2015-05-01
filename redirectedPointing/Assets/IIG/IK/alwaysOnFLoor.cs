using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class alwaysOnFLoor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Vector3 scale = this.transform.lossyScale;
		Vector3 position = this.transform.position;
		this.transform.position += new Vector3(0, scale.y/2f - position.y , 0);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 scale = this.transform.lossyScale;
		Vector3 position = this.transform.position;
		this.transform.position += new Vector3(0, scale.y/2f - position.y , 0);
	}
}
