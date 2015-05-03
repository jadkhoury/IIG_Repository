//author: Jad Khoury
//Script managing the behavior of the targets.
using UnityEngine;
 
public class TargetScript : MonoBehaviour
{
	public Material activated;
	public Material idle;
	private bool isEnabled = false;
	private controlScript control;
	private GameObject hand;
	private TargetManager manager;

	public void Awake ()
	{
		control = GameObject.FindGameObjectWithTag ("GameController").GetComponent<controlScript> (); 
		hand = control.triggerObject;
	}

	public void Enable ()
	{
		this.isEnabled = true;
		this.renderer.material = activated;
		// Debug.Log("enable "+ this.gameObject.name);
	}

	public void Disable ()
	{
		this.isEnabled = false;
		this.renderer.material = idle;
		//Debug.Log("disable "+ this.gameObject.name);
	}

	public void Init ()
	{
		// make sure there is a collider, and that it is set as a trigger
		if (this.collider == null) {
			this.gameObject.AddComponent<MeshCollider> ();		
		}
		this.collider.isTrigger = true;
		this.renderer.material = idle;
		manager = GetComponentInParent<TargetManager> ();
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject == hand && isEnabled) {
			//Debug.Log(this.gameObject.name + " triggerEnter");
			manager.Trigger ();
		}
	}




}

