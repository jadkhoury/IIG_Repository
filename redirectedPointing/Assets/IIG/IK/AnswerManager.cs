using UnityEngine;
using System.Collections;

public class AnswerManager : MonoBehaviour
{

	public Material yesMaterial;
	public Material noMaterial;
	public GameObject answerPrefab;
	private float circleRadius;
	private ControlScript control;
	private float answerRadius;
	private GameObject[] buttons;

	void Awake ()
	{
		control = GameObject.FindGameObjectWithTag ("GameController").GetComponent<ControlScript> ();
		circleRadius = control.circleRadius;
		answerRadius = 0.05f;
		buttons = new GameObject[2];
	}

	public void Run ()
	{
		createAnswers ();
	}
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Answer (string s)
	{
		Debug.Log (s);
		foreach (GameObject obj in buttons)
			Object.Destroy (obj);
	}

	void createAnswers ()
	{
		for (int i = 0; i <2; ++i) {
			float angle = i * 2 * Mathf.PI / 2;
			float x = circleRadius * Mathf.Cos (angle);
			float y = 0.021f; //dependant to the size of the mesh cube
			float z = circleRadius * Mathf.Sin (angle);
			GameObject answerClone = (GameObject)Instantiate (answerPrefab, transform.position + new Vector3 (x, y, z), Quaternion.identity);
			answerClone.transform.parent = transform;
			answerClone.transform.rotation = answerClone.transform.parent.rotation;
			answerClone.transform.localScale = new Vector3 (2 * answerRadius, 2 * answerRadius, 0.005f); //because the scale rep the diameter not the radius
			answerClone.renderer.material = (i == 0) ? noMaterial : yesMaterial;
			answerClone.name = (i == 0) ? "NO" : "YES";
			buttons [i] = answerClone;
			answerClone.GetComponent<AnswerScript> ().Init ();
		}

	}
}