using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchMaterialOnContact : MonoBehaviour {
	int contacts = 0;
	public Material OnContact;
	public Material NoContact;
	float timer = 0;
	bool timerFlag = false;
	public float timerThreshold = 1;
	void Start () {
		// make sure there is a collider, and that it is set as a trigger
		if (this.collider == null) {
			this.gameObject.AddComponent<SphereCollider>();		
		}
		this.collider.isTrigger = true;

		if (this.renderer.material == null) {
			this.renderer.material = NoContact;	
		}

	}
	void Update(){
		if (contacts == 0 || timer > timerThreshold) {
			this.renderer.material = NoContact;
			timerFlag = false;
			if (contacts == 0){
				timer = 0;
			}
		}else {		
			this.renderer.material = OnContact;
			if (timerFlag) {
				timer += Time.deltaTime;
			}
		}
		contacts = 0;
	

	}
	void OnTriggerEnter(Collider otherObj){
		contacts++;
		timerFlag = true;
	}
	void OnTriggerStay(Collider otherObj){
		contacts++;
	}

	void OnTriggerExit(Collider otherObj){
		//contacts--;

	}
}
