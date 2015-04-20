using UnityEngine;
using System.Collections;

public class RaySelectable : MonoBehaviour {

	public string optionText = "YES";
	public Transform rayTransform;
	public float selectableDiameter = 0.5f;
	public int menuLayer = 8;
	public float timeToSelect = 2;
	public Font textFont;

	private float fixationTime = 0;

	public bool selected { get; private set;}

	TextMesh optionTextMesh;

	GameObject confirmBarS;
	GameObject confirmBar;

	GameObject backgroundPlane;

	public Material background;
	public Material backgroundHighlight;
	public Material bgBar;
	public Material bgBarFilling;
	// has to create texture render

	// Use this for initialization
	void Start () {
		selected = false;

		if (textFont == null)
						Debug.LogWarning ("missing font");

		// create/replace collider with a sphere collider
		if (this.collider == null || (this.collider.GetType () != typeof (SphereCollider) && this.collider.GetType () != typeof (BoxCollider))) {
			Destroy(this.gameObject.GetComponent("Collider"));
			this.gameObject.AddComponent<SphereCollider>();	
		}
		if (this.collider.GetType () == typeof(SphereCollider)) {
			SphereCollider sphereCol = (SphereCollider)this.gameObject.GetComponent ("SphereCollider");
			sphereCol.radius = selectableDiameter / 2.0f;
		}
		if (this.collider.GetType () == typeof(BoxCollider)) {
			BoxCollider boxCol = (BoxCollider)this.gameObject.GetComponent ("BoxCollider");
			boxCol.size = new Vector3(selectableDiameter, selectableDiameter, 0.01f);
		}

		// text renderer
		if (this.renderer == null)
			this.gameObject.AddComponent<MeshRenderer>();

		//if (this.gameObject.GetComponent("Mesh") != null)
		//	Destroy(this.gameObject.GetComponent("Mesh"));
		optionTextMesh = this.gameObject.AddComponent<TextMesh>();	

		this.renderer.material = textFont.material;
		optionTextMesh.text = optionText;
		optionTextMesh.font = textFont;
		optionTextMesh.fontStyle = FontStyle.Normal;
		optionTextMesh.characterSize = 0.05f;
		optionTextMesh.fontSize = (int) (100.0f * selectableDiameter);
		optionTextMesh.anchor = TextAnchor.MiddleCenter;

		this.gameObject.layer = menuLayer;

		if (rayTransform == null)
			rayTransform = Camera.main.transform;

		if (background != null ) {
			GameObject backgroundGO = GameObject.CreatePrimitive (PrimitiveType.Quad);
			backgroundGO.renderer.material = background;
			backgroundGO.transform.parent = this.transform;
			backgroundGO.transform.localPosition = new Vector3(0, 0, 0.01f / this.transform.localScale.z);
			backgroundGO.transform.localScale = new Vector3(selectableDiameter, selectableDiameter, selectableDiameter);
			backgroundGO.layer = menuLayer;
			backgroundPlane = backgroundGO;
		}

		confirmBar = GameObject.CreatePrimitive (PrimitiveType.Quad);
		confirmBarS = GameObject.CreatePrimitive (PrimitiveType.Quad);
		confirmBarS.transform.parent = this.transform;
		confirmBar.transform.parent = confirmBarS.transform;
		confirmBarS.transform.localPosition = new Vector3 (0, -selectableDiameter/2 + selectableDiameter * .15f, 0);
		confirmBarS.transform.localScale = new Vector3 (selectableDiameter * .85f, selectableDiameter/5 * .75f , 1);
		//confirmBarS.renderer.material.color = new Color (0.2f, 0.2f, 0.2f, 0.2f);
		confirmBarS.renderer.material = bgBar;
		confirmBar.transform.localScale = new Vector3 (0, 1, 1);
		confirmBar.transform.localPosition = new Vector3 (-selectableDiameter, 0, 0);
		//confirmBar.renderer.material.color = new Color (0.9f, 0.9f, 0.9f, 1);
		confirmBar.renderer.material = bgBarFilling;
		confirmBar.layer = menuLayer;
		confirmBarS.layer = menuLayer;
	
	}

/*	public void SetOption (string newText, Font newFont, float newTimeToConfirm = 2, float newDiameter = 0.5, int newLayer = 8){
		optionText = newText;
		timeToSelect = newTimeToConfirm;
		menuLayer = newLayer;
		textFont = newFont;
		selectableDiameter = newDiameter;
		SphereCollider sphereCol = (SphereCollider) this.gameObject.GetComponent("SphereCollider");
		sphereCol.radius = selectableDiameter/2.0f;
			
		optionTextMesh = this.gameObject.AddComponent<TextMesh>();	
		
		this.renderer.material = textFont.material;
		optionTextMesh.text = optionText;
		optionTextMesh.font = textFont;
		optionTextMesh.fontStyle = FontStyle.Normal;
		optionTextMesh.characterSize = 0.05f;
		optionTextMesh.fontSize = (int) (100.0f * selectableDiameter);
		optionTextMesh.anchor = TextAnchor.MiddleCenter;
		
		this.gameObject.layer = menuLayer;
		
		if (rayTransform == null)
			rayTransform = Camera.main.transform;
		
		confirmBar = GameObject.CreatePrimitive (PrimitiveType.Quad);
		confirmBarS = GameObject.CreatePrimitive (PrimitiveType.Quad);
		confirmBarS.transform.parent = this.transform;
		confirmBar.transform.parent = confirmBarS.transform;
		confirmBarS.transform.localPosition = new Vector3 (0, -selectableDiameter/2, 0);
		confirmBarS.transform.localScale = new Vector3 (selectableDiameter, selectableDiameter/5, 1);
		confirmBarS.renderer.material.color = new Color (0.2f, 0.2f, 0.2f, 0.2f);
		confirmBar.transform.localScale = new Vector3 (0, 1, 1);
		confirmBar.transform.localPosition = new Vector3 (-selectableDiameter, 0, 0);
		confirmBar.renderer.material.color = new Color (0.9f, 0.9f, 0.9f, 1);
	}*/

	// Update is called once per frame
	void Update () {
		Debug.DrawLine (rayTransform.position, rayTransform.position + rayTransform.forward * 1000, Color.red);
		Debug.DrawLine (rayTransform.position, this.transform.position, Color.green);

		if (selected)
						return;

		Debug.DrawLine (rayTransform.position, this.transform.position, Color.yellow);



		int layerMask = 1 << menuLayer;
		RaycastHit rayHit;
		if (Physics.Raycast (rayTransform.position, rayTransform.forward, out rayHit, 1000, layerMask) && rayHit.collider == this.collider) {
			fixationTime += Time.deltaTime;
			backgroundPlane.renderer.material = backgroundHighlight;
			optionTextMesh.color = Color.white;
		}else{
			fixationTime = 0;
			optionTextMesh.color = Color.grey;
			backgroundPlane.renderer.material = background;
			
		}

		confirmBar.transform.localScale = new Vector3 (fixationTime/timeToSelect, 1, 1);
		confirmBar.transform.localPosition = new Vector3 (-0.5f + 0.5f * (fixationTime/timeToSelect), 0, -0.001f);
	//	confirmBar.transform.localPosition = new Vector3 ((-selectableDiameter/2 + (selectableDiameter/2) * (fixationTime/timeToSelect))*(1/this.transform.localScale.x), 0, -0.001f);
//		confirmBar.transform.localPosition = new Vector3 (-selectableDiameter/2 + (selectableDiameter/2) * (fixationTime/timeToSelect), 0, -0.001f);

		if (fixationTime >= timeToSelect)
			selected = true;


	}

	public void Restart () {
		fixationTime = 0;
		selected = false;
	}



}
