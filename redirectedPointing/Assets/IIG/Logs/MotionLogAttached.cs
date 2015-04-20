// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionMessage : System.Object {
	public float timestamp;
	public string collisionOrTrigger;
	public string type;
	public GameObject from;
	public GameObject to;
}

public class MotionLogAttached : MonoBehaviour {




	[HideInInspector]
	public List<CollisionMessage> messages = new List<CollisionMessage>();
	[HideInInspector]
	public bool newMessage = false;

	void  Start (){
		reset();
	}

	public void  reset (){
		messages.Clear ();
		newMessage = false;
	}

	public void  createMessage (string collisionOrTrigger, string type, GameObject to){
		CollisionMessage message = new CollisionMessage();
		message.timestamp = Time.time;
		message.collisionOrTrigger = collisionOrTrigger;
		message.type = type;
		message.from = gameObject;
		message.to = to;
		messages.Add(message);
		newMessage = true;
	}

	void  OnCollisionEnter ( Collision collision  ){
		createMessage("collision", "enter", collision.gameObject);
	}

	void  OnCollisionExit ( Collision collision  ){
		createMessage("collision", "exit", collision.gameObject);
	}

	void  OnTriggerEnter ( Collider collider  ){
		createMessage("trigger", "enter", collider.gameObject);
	}

	void  OnTriggerExit ( Collider collider  ){
		createMessage("trigger", "exit", collider.gameObject);
	}
}