/**************************************************************************
 * MarkersDebug.cs debug markers positions
 * Written by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;


public class MarkersDebug {

	Dictionary<int, GameObject> m_markersDebug;

	public GameObject [] markerPosArray;
//	public bool [] visibilityArray;
	
	// materials that will be used in visible/notvisible markers
	private Material matVisible;
	private Material matNotVisible;
	
	// sphere primitive has by standard the size of 1 
	float scaleFactor = 0.025f;
	
	// to store the total number of markers
	int totalMarkers =0;
	
	// list of log entries
	//private List<CsvRow> markerLogList = new List<CsvRow>();
	// log name prefix
	//public string logPrefix = "MarkerDebugLog_";
	//private bool logIsOpen = false;
	public GameObject markerDebugParent = new GameObject();
	//bool isVisible = true;

	public void Init(List<Tuple<int, string>> markers){
		markerDebugParent.name = "Markers debug";
		totalMarkers = markers.Count;		
		// create materials to be used on marker representations
		matVisible = new Material (Shader.Find ("Diffuse"));
		matVisible.color = Color.green;
		matNotVisible = new Material (Shader.Find ("Diffuse"));
		matNotVisible.color = Color.red;

		markerPosArray = new GameObject [markers.Count];
		m_markersDebug = new Dictionary<int, GameObject> ();

		GameObject markerPrefab = Resources.Load("MarkerDebugPrefab", typeof(GameObject)) as GameObject;
//		Font debugFont = Resources.Load("consola", typeof(Font)) as Font;
//		Material fontMaterial = Resources.Load("consola", typeof(Material)) as Material;



		// assign a sphere primitive to gameobj, rescale it, set material, name and hash key
		for (int i=0; i<markers.Count; i++) {
			GameObject newMarker = GameObject.Instantiate(markerPrefab) as GameObject;// Instantiate(prefab, pos, Quaternion.identity);
		//	GameObject newMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//	newMarker.transform.localScale = new UnityEngine.Vector3(scaleFactor,scaleFactor,scaleFactor);
			
		//	newMarker.renderer.castShadows = false;
		//	newMarker.renderer.useLightProbes = true;
			
			newMarker.renderer.material =  matNotVisible;
			newMarker.transform.parent = markerDebugParent.transform;
			newMarker.name = markers[i].Second;

		//	GameObject newText = new GameObject();
		//	newText.AddComponent("TextMesh");
			
		//	TextMesh editTextMesh = newText.GetComponent<TextMesh>();

		//	editTextMesh.text = newMarker.name;
			//editTextMesh.font = debugFont;

			//newText.renderer.castShadows = false;
			//newText.renderer.useLightProbes = true;
			//newText.renderer.material = fontMaterial;;

			//newText.transform.parent = newMarker.transform;
			//newText.transform.localScale = new Vector3(.5f, .5f, .5f);
			m_markersDebug.Add(markers[i].First, newMarker);
			markerPosArray[i] = newMarker;
		}
	}

	// make all markers (in)visible
	public void SetVisible(bool isVisible){
		for (int i =0; i<markerPosArray.Length; i++){
			markerPosArray[i].renderer.enabled = isVisible;
			for (int j=0; j < markerPosArray[i].transform.childCount; j++){
				markerPosArray[i].transform.GetChild(j).renderer.enabled = isVisible;
			}
		}
	}


	
	// set markers attributes at once
	public void SetMarkersHash(Vector3 [] markPosArray, int [] indexes, bool [] visibArray){
		for (int i =0; i<markPosArray.Length; i++){
			if (m_markersDebug[indexes[i]] != null){
				m_markersDebug[indexes[i]].transform.position = markPosArray[i];
				if (markerPosArray[i].renderer.enabled){
					if (visibArray[i])
						m_markersDebug[indexes[i]].renderer.material =  matVisible;
					else
						m_markersDebug[indexes[i]].renderer.material =  matNotVisible;
				}
				if (visibArray[i])
					markerPosArray[i].tag = "markerVisible";
				else
					markerPosArray[i].tag = "markerNotVisible";
			}
		}
	}
	
	
	// set marker position and material (occlusion) attributes for an individual marker
	public void SetMarkerHash(int index, Vector3 mkrPos){
		if (m_markersDebug[index] != null){
			Debug.Log("index " + index + " is not a valid marker");
			return;
		}
		if (markerPosArray[index].renderer.enabled)
			m_markersDebug[index].renderer.material =  matVisible;

		markerPosArray[index].tag = "markerVisible";
		m_markersDebug[index].transform.position = mkrPos;
	}
	
	public void MarkerNotVisibleHash(int index){
		if (m_markersDebug[index] != null){
			Debug.Log("index " + index + " is not a valid marker");
			return;
		}
		if (markerPosArray[index].renderer.enabled)
			m_markersDebug[index].renderer.material =  matNotVisible;
		markerPosArray[index].tag = "markerNotVisible";
	}
/*	public void Init(int numbMarkers){
		totalMarkers = numbMarkers;		
		// create materials to be used on marker representations
		matVisible = new Material (Shader.Find ("Diffuse"));
		matVisible.color = Color.green;
		matNotVisible = new Material (Shader.Find ("Diffuse"));
		matNotVisible.color = Color.red;
		
		// CAN I RESET THIS? probably with a clear on game objects array
		markerPosArray = new GameObject [totalMarkers];

		// assign a sphere primitive to gameobj, rescale it, set material
		for (int i =0; i<totalMarkers; i++){
			
			markerPosArray[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			markerPosArray[i].transform.localScale = new UnityEngine.Vector3(scaleFactor,scaleFactor,scaleFactor);

			markerPosArray[i].renderer.castShadows = false;
			markerPosArray[i].renderer.useLightProbes = true;

			markerPosArray[i].renderer.material =  matNotVisible;
			markerPosArray[i].transform.parent = markerDebugParent.transform;
		}
	}

	// make all markers (in)visible
	public void SetVisible(bool isVisible){
		for (int i =0; i<markerPosArray.Length; i++){
			markerPosArray[i].renderer.enabled = isVisible;
		}
	}
*/
	// set markers attributes at once
	public void SetMarkers(Vector3 [] markPosArray, bool [] visibArray){
		if (markPosArray.Length != markerPosArray.Length || markPosArray.Length != visibArray.Length){
			Debug.Log("array lenghts doesn't match");
			return;
		}

		// update markers debug visibility and pos

		for (int i =0; i<markPosArray.Length; i++){
			markerPosArray[i].transform.position = markPosArray[i];
			if (markerPosArray[i].renderer.enabled){
				if (visibArray[i])
					markerPosArray[i].renderer.material =  matVisible;
				else
					markerPosArray[i].renderer.material =  matNotVisible;
			}
			if (visibArray[i])
				markerPosArray[i].tag = "markerVisible";
			else
				markerPosArray[i].tag = "markerNotVisible";
		}

	}
	

	// set marker position and material (occlusion) attributes for an individual marker
	public void SetMarker(int index, UnityEngine.Vector3 mkrPos){
		if (index>=totalMarkers){
			Debug.Log("index " + index + " is bigger than total markers: " + totalMarkers);
			return;
		}
		if (markerPosArray[index].renderer.enabled)
			markerPosArray[index].renderer.material =  matVisible;
		markerPosArray[index].tag = "markerVisible";
		markerPosArray[index].transform.position = mkrPos;
	}
	
	public void MarkerNotVisible(int index){
		if (index>=totalMarkers){
			Debug.Log("index " + index + " is bigger than total markers: " + totalMarkers);
			return;
		}
		if (markerPosArray[index].renderer.enabled)
			markerPosArray[index].renderer.material =  matNotVisible;
		markerPosArray[index].tag = "markerNotVisible";
	}

}
