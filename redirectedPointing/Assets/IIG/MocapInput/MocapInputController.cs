/**************************************************************************
 * MocapInputController.cs controls the motion capture source and reproduction
 * Written by Henrique Galvan Debarba
 * Last update: 28/02/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using IIG;


public class MocapInputController : MonoBehaviour {

	public enum updateCallback {UPDATE, LATE_UPDATE, FIXED_UPDATE};
	public updateCallback callback = updateCallback.UPDATE;

	// VRPN settings
	public string VRPN_connection_string;	
	private VRPN.Client vrpn_client;

	// mocap recording path and name, it loads C3D and .mkr.csv
	// .mkr.csv is a simple ASCII that can be created using LogMarker.cs
	public string mocap_marker_alias_file_path;
	
	public string fileToLoad;
	//C3DManager c3dMan = new C3DManager();
	//C3DPlayer c3dMan = new C3DPlayer();
	//CSVPlayer CSVMan = new CSVPlayer();
	public MocapPlayer m_mocapPlayer  {get; protected set;} // = new CSVPlayer();

	// input source
	public enum MocapSource {LIVE=0, FILE=1};
	public MocapSource m_mocapSource;
	
	// delay mocap? keep marker+deltatime buffers and time controllers
	public bool delayMocap = false;
	// buffer for delayed mocap
	public Queue<double[,]> mocapBuffer = new Queue<double[,]>();
	public Queue<float> deltaTimeBuffer = new Queue<float>();
	public float delayTime = 1.5f;
	private float keptInterval = 0;
	//public int bufferSizeLimit = 60;
	
	// debug markers (draw them as sphere in the screen)
	public MarkersDebug mkrDebug;
	public bool debugMarkersPos = true;

	public double[,] frame {get; protected set;}//= null;
	public bool [] visibFrame {get; protected set;}// = null;

	// avatar controller, which receives mocap input from this class
	public AvatarController avtController = null;

	List<Tuple<int,string>> m_mkrNames = new  List<Tuple<int,string>> ();
	//public LogMocap m_logMocap = new LogMocap();

	// Initialization
	void Start () {

		// init Markers debug
		mkrDebug = new MarkersDebug();
		
		// open c3D file
		if (fileToLoad.EndsWith(".c3d"))
			m_mocapPlayer = new C3DPlayer();
		else
			m_mocapPlayer = new CSVPlayer();

		m_mocapPlayer.LoadMocap(fileToLoad);

		//c3dMan.SetStartStopTime(10.0f,25.0f);

		if (m_mocapSource == MocapSource.LIVE) {
			vrpn_client = new VRPN.Client (mocap_marker_alias_file_path, VRPN_connection_string);
			vrpn_client.Init ();
		}
	
		LoadMakerNames (mocap_marker_alias_file_path, out m_mkrNames);

		if (m_mocapSource == MocapSource.FILE) {
			mkrDebug.Init (m_mkrNames);
			//mkrDebug.Init (m_mocapPlayer.totalPoints);
		} else {
			mkrDebug.Init (m_mkrNames);
			//mkrDebug.Init (vrpn_client.no_of_markers);
		}
		mkrDebug.SetVisible(debugMarkersPos);
	}


	void LoadMakerNames(string inFilePath, out List<Tuple<int,string>> mkrnames){
		mkrnames = new List<Tuple<int, string>>();
		System.IO.StreamReader reader = new System.IO.StreamReader(inFilePath);
		string line;

		while((line = reader.ReadLine()) != null)
		{
			string[] id_value = line.Split(';');
			if (id_value[0] != null){
				string[] mkrNameID = id_value[0].Split(' ');
				if (mkrNameID.Length >= 2){
					Tuple<int, string> mkrNameIDTuple = new Tuple<int, string>(System.Convert.ToInt32(mkrNameID[0]), mkrNameID[1]);
					mkrnames.Add(mkrNameIDTuple);
				}
			}

		}
		reader.Close();
	}

	public void FrameUpdate(float deltaT){
		//double [,] frame = null;
		//bool [] visibFrame = null;
		bool validFrame = false;
		int totalMarkers = 0;
		
		// MOCAP from file
		if (m_mocapSource==MocapSource.FILE){
			double [,] frameLocal = null;
			bool [] visibFrameLocal = null;
			validFrame = m_mocapPlayer.CycleFrames(deltaT, ref frameLocal, ref visibFrameLocal);
			frame = frameLocal;
			visibFrame = visibFrameLocal;
			totalMarkers = m_mocapPlayer.totalPoints;
		}
		// VRPN stream
		else if (m_mocapSource==MocapSource.LIVE){
			
			vrpn_client.Update();
			
			// copy positions (so a buffer can be kept)
			//double[,] currentFrame
			frame = new double[vrpn_client.marker_positions.GetLength(0),vrpn_client.marker_positions.GetLength(1)];
			visibFrame = new bool[vrpn_client.marker_positions.GetLength(0)];
			totalMarkers = vrpn_client.marker_positions.GetLength(0);
			for (int i = 0; i < vrpn_client.marker_positions.GetLength(0); i++){
				visibFrame[i]= true;
				for (int j = 0; j < vrpn_client.marker_positions.GetLength(1); j++)
					frame[i,j] = vrpn_client.marker_positions[i,j];
				if (frame[i,0]!=frame[i,0])
					visibFrame[i] = false;
			}
			
			// I do not know what could be used to define a valid frame input from VRPN (yet)
			validFrame = true;
		}
		
		
		if (validFrame){
			// manage delay buffer, it is updated always so it is not 
			// necessary to wait for it to be filled after enabling
			mocapBuffer.Enqueue(frame);
			deltaTimeBuffer.Enqueue(deltaT);
			keptInterval += deltaT;
			if (delayMocap){
				if (keptInterval > delayTime){
					frame = mocapBuffer.Dequeue();
					keptInterval -= deltaTimeBuffer.Dequeue();
				}
			}else{
				if (keptInterval > delayTime){
					mocapBuffer.Dequeue();
					keptInterval -= deltaTimeBuffer.Dequeue();
				}
			}	
			
			// convert pos to Vector3 format, used by MarkersDebug to set markers representation position
			//if (debugMarkersPos){
				for (int i = 0 ; i< totalMarkers; i++){
					//if marker is not visible its position is not updated
					if (!visibFrame[i]){
						mkrDebug.MarkerNotVisible(i);
					}else{
						// - on z parameter is HARD CODED !!!!!! so mocap and markers match
						// a 180 on the avatar was also needed to match ...
						// the reason has something to do with right/left handed coord system (mocap is right h, unity left h)
						Vector3 currentMkr = new Vector3((float)frame[i,0],(float)frame[i,1],-(float)frame[i,2]);
						mkrDebug.SetMarker(i,currentMkr);
					}
				}
				//mkrDebug.LogMocapFrame();
			//}
			
			if (avtController!= null)
				avtController.MocapUpdateIK(vrpn_client.marker_ids, frame);
			
		} 

	}

	void FixedUpdate(){
		if (callback == updateCallback.FIXED_UPDATE)
			FrameUpdate (Time.fixedDeltaTime);
	}
	void LateUpdate(){
		if (callback == updateCallback.LATE_UPDATE)
			FrameUpdate (Time.fixedDeltaTime);
	}
	void Update(){
		if (callback == updateCallback.UPDATE)
			FrameUpdate (Time.fixedDeltaTime);
	}


	public void EnableMarkerDebug(bool enable){
		mkrDebug.SetVisible(enable);
		debugMarkersPos = enable;
	}


	public List<Tuple<int,string>> GetNamesList(){
		return m_mkrNames;
	}
}
