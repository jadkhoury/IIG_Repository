using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Question : MonoBehaviour {

	public string[] questionText;
	List<GameObject> questionTextGOs = new List<GameObject>();

	public Font textFont;
	TextMesh optionTextMesh;


	public float distance = 0.5f;
	public float spreadAngle = 120.0f;

	public GameObject[] optionsGO;


	void SetOptionsPose (){
		float angleInterval = spreadAngle / optionsGO.Length;
		float rescale = 2 * distance * Mathf.Tan ((Mathf.Deg2Rad * angleInterval) / 2);
		for (int i = 0; i<optionsGO.Length; i++) {
			Vector3 adjustedScale = optionsGO[i].transform.lossyScale;
			adjustedScale.x = 1*rescale/adjustedScale.x;
			adjustedScale.y = 1*rescale/adjustedScale.y;
			adjustedScale.z = 1*rescale/adjustedScale.z;
			adjustedScale *= 0.9f;
			optionsGO[i].transform.localScale = Vector3.Scale(adjustedScale, optionsGO[i].transform.lossyScale);

			float optionAngle = spreadAngle/2 - angleInterval/2 - angleInterval*i;
			Vector3 newPosition = Vector3.RotateTowards (this.transform.forward, this.transform.right, optionAngle*Mathf.Deg2Rad, 0.0f);

			optionsGO[i].transform.localPosition = newPosition * distance + new Vector3(0,-rescale,0);
			optionsGO[i].transform.rotation = Quaternion.identity;
			optionsGO[i].transform.Rotate(this.transform.up, optionAngle);
		}

	//	optionTextMesh.fontSize = (int)(100.0f * rescale);



	//	optionTextMesh.
	}

	// Use this for initialization
	void Start () {
		float angleInterval = spreadAngle / optionsGO.Length;
		float rescale = 2 * distance * Mathf.Tan ((Mathf.Deg2Rad * angleInterval) / 2);

		if (textFont == null)
			Debug.LogWarning ("missing font");
		
		
		for (int i =0; i < questionText.Length; i++){
			GameObject questionLine;
			//if (questionTextGOs.Count < questionText.Length){
				questionLine = new GameObject();
				questionTextGOs.Add(questionLine);
			//} else {
			//	questionLine = questionTextGOs[i];
			//}
			
			
			questionLine.transform.parent = this.transform;
			questionLine.transform.localPosition = new Vector3(0,rescale * 0.25f * (questionText.Length-i-1),distance);
			questionLine.AddComponent<MeshRenderer>();
			
			optionTextMesh = questionLine.AddComponent<TextMesh>();	
			
			questionLine.renderer.material = textFont.material;
			optionTextMesh.text = questionText[i];
			optionTextMesh.font = textFont;
			optionTextMesh.fontStyle = FontStyle.Normal;
			optionTextMesh.characterSize = 0.01f;
			optionTextMesh.fontSize = (int) (250.0f * rescale);
			optionTextMesh.anchor = TextAnchor.MiddleCenter;
			
			
		}
		
	//	this.gameObject.layer = menuLayer;


	//	optionsGO = new GameObject[options.Length];
	//	for (int i = 0; i<optionsGO.Length; i++) {
			//optionsGO.		
	//	}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.A)){
			foreach (GameObject option in optionsGO){
				RaySelectable componentPointer = (RaySelectable) option.GetComponent <RaySelectable>();
				if (componentPointer != null)
					componentPointer.Restart();
			}
			SetOptionsPose ();

		}
	}
}
