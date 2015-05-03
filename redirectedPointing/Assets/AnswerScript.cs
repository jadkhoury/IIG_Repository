using UnityEngine;
using System.Collections;

public class AnswerScript : MonoBehaviour {

	private ControlScript control;
	private GameObject hand;
	private AnswerManager manager;

	public void Awake ()
	{
		control = GameObject.FindGameObjectWithTag ("GameController").GetComponent<ControlScript> (); 
		hand = control.triggerObject;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init ()
	{
		this.collider.isTrigger = true;
		manager = GetComponentInParent<AnswerManager> ();
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject == hand) {
			manager.Answer(this.name);
		}
	}
	
}
