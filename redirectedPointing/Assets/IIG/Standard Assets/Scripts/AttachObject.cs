/**************************************************************************
 * AttachObject.cs allows to selectively control a transform using another 
 * one in the scene (pos and/or orient in/from local/global space)
 * Written by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;

public class AttachObject : MonoBehaviour {
	public enum updateCallback {UPDATE, LATEUPDATE, FIXEDUPDATE, ONPRERENDER}
	public updateCallback callback = updateCallback.UPDATE;

	// attach this transform to: 
	public Transform attachTo;
	public string attachToName;
	// update position
	public bool usePosition = true;
	// update orientation
	public bool useRotation = false;
	// set this transform in local space?
	public bool setLocalTransform = false;
	// get orient/pos from local space?
	public bool getLocalTransform = false;

	// use initial transform as a localTransform
	public bool useOffset = true;
	// keep initial transform offset
	Quaternion rotationOffset;
	Vector3 positionOffset;

	void Start () {
		if (attachTo == null) {
			GameObject attachToGO =  GameObject.Find(attachToName);
			if (attachToGO != null)
				attachTo = attachToGO.transform;
			else
				Debug.LogError("AttachObject: no attach transform defined");
		}

		if (attachTo!=null){
			if (useOffset){
				// keep difference in position
				positionOffset = transform.position - attachTo.position;
				// keep difference in orientation
				rotationOffset = Quaternion.Inverse(attachTo.rotation) * transform.rotation;
				// transform diff in distance into attachTo coord system
				positionOffset = attachTo.rotation * positionOffset;
			}
		}else
			Debug.Log("AttachObject: transform not set");
	}

	void OnPreRender(){
		if (callback == updateCallback.ONPRERENDER)
			UpdatePosition();
	}
	void Update(){
		if (callback == updateCallback.UPDATE)
			UpdatePosition();
	}
	void LateUpdate(){
		if (callback == updateCallback.LATEUPDATE)
			UpdatePosition();
	}
	void FixedUpdate(){
		if (callback == updateCallback.FIXEDUPDATE)
			UpdatePosition();
	}

	
	void  UpdatePosition() {
		if (attachTo!=null){
			if (usePosition){
				if (setLocalTransform)
					transform.localPosition = (getLocalTransform) ? attachTo.localPosition : attachTo.position;
				else
					transform.position = (getLocalTransform) ? attachTo.localPosition : attachTo.position;
			}
			if (useRotation){
				if (setLocalTransform)
					transform.localRotation = (getLocalTransform) ? attachTo.localRotation : attachTo.rotation;
				else
					transform.rotation = (getLocalTransform) ? attachTo.localRotation : attachTo.rotation;
			}
			// apply the offset to  the transform (as if a local transform were kept)
			if (useOffset){
				transform.position += transform.rotation * positionOffset;
				//transform.rotation *= rotationOffset;
			}
		} else
			Debug.Log("AttachObject: transform not set");
	}
}
