/**************************************************************************
 * C3DManager.cs especialization of MocapPlayer.cs which load mocaps in CSV
 * Written by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// C3D library plugin in C#
using Vub.Etro.IO;

public class C3DPlayer : MocapPlayer{
	private Vub.Etro.IO.C3dReader reader = new Vub.Etro.IO.C3dReader();

	// load c3d file to memory, a list structure is used so we can access it at any position
	public override bool LoadMocap(string fileName){
		// check if there is any loaded mocap, dump it if that is the case
		if (mocapIsLoaded)
			DumpMocap();
		
		// open C3D file
		Debug.Log("Opening file " + fileName);
		if (!reader.Open(fileName)){
        	Debug.Log("Error: Unable to open file " + fileName);
            return false;
        }
		
		// load basic information about the recording
		frameRate = reader.Header.FrameRate;
		totalPoints = reader.Header.NumberOfPoints;
		//scaleFactor =reader.Header.ScaleFactor;
		float scaleFactor = 0.001f; // TODO  HARD CODED !!!!!! because C3D file does not match with received VRPN LIVE!
		Debug.Log (reader.Header.FrameRate);
		// compute sampling delta time (in seconds)
		samplingDTime = 1.0f/frameRate;
		
		
		totalFrames = reader.GetParameter<Int16>("POINT:FRAMES");
		stopFrame = totalFrames;
		
		for (int i = 0; i < totalFrames ; i++)
        {
			// returns an array of all points, it is necessary to call this method in each cycle
			Vub.Etro.IO.Vector3[] mkrArray = reader.ReadFrame();
			// structure to hold markers of one capture frame
			double[,] newFrame = new double[reader.Header.NumberOfPoints,3];
			bool[] newVisibFrame = new bool[reader.Header.NumberOfPoints];
			// conversion
			int j=0;
			foreach (var mkr in mkrArray){
				newVisibFrame[j] = true;
				newFrame[j,0] = -mkr.X * scaleFactor; // TODO - HARD CODED !!!!
				newFrame[j,1] = mkr.Z * scaleFactor; // TODO axis inversion HARD CODED !!!! because C3D file does not match with received VRPN LIVE!
				newFrame[j,2] = mkr.Y * scaleFactor; // TODO axis inversion HARD CODED !!!!
					
				if (newFrame[j,0] == 0.0f && newFrame[j,1] == 0.0f && newFrame[j,2] == 0.0f)
					newVisibFrame[j] = false;
				
				j++;
			}
			// add this frame to the list
			mocapData.Add(newFrame);
			mocapVisibility.Add(newVisibFrame);
        }
		// close file
		Debug.Log("Closing file " + fileName);
		reader.Close();
		CleanData();
		mocapIsLoaded = true;
		
		return true;
	}

	
	// markers that were temporary lost during motion capture are set to position (0,0,0)
	// this generates a big error during mocap reproduction, thus these need to be treated
	public void CleanData (){
		for (int i =1; i<totalFrames; i++){
			for (int j =0; j<totalPoints; j++){
				// as the data is give in a double floating point, it is very unlikely to have any 0,0,0 read position
				// thus this is the criterium for cleaning
				if (mocapData[i][j,0] == 0 && mocapData[i][j,1] == 0 && mocapData[i][j,2] == 0){
					// simply uses the past reading, a filter could do a better job at the cost of delay
					for (int k=0; k<3; k++)
						mocapData[i][j,k] = mocapData[i-1][j,k];
				}
				
			}			
		}
		
	}
	

}