/**************************************************************************
 * CSVPlayer.cs especialization of MocapPlayer.cs which load mocaps in CSV
 * Written by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using ReadWriteCsv;

public class CSVPlayer : MocapPlayer {

	// parse CSV file with prerecorded mocap
	public override bool LoadMocap(string fileName){
		// check if there is any loaded mocap, dump it if that is the case
		if (mocapIsLoaded)
			DumpMocap();
		
		// open C3D file
		Debug.Log("Opening file " + fileName);

		CsvFileReader reader = new CsvFileReader(fileName);
		// TODO check if the file was loaded successfully
		//Debug.Log("Error: Unable to open file " + fileName);
		//return false;

		string fileContent = reader.ReadToEnd();
		reader.Close();

		float startTime = 0, endTime = 0;

		using (StringReader strReader = new StringReader(fileContent)) 
        { 
            string line; 
			int countFrame = 0;
			// skip header
			line = strReader.ReadLine();
		
            while ((line = strReader.ReadLine()) != null) 
            { 
				// list of positions for this frame
				List<Vector3> mocapLine = new List<Vector3>();

				string[] mocapLineArray = line.Split(new string[] { "," }, StringSplitOptions.None);
				if (((mocapLineArray.Length-3)%4)!=0)
					return false;

				totalPoints = (mocapLineArray.Length-3)/4;
				double [,]mocLine = new double[totalPoints,3];
				bool []visibLine = new bool[totalPoints];
				for (int i=0; i<totalPoints; i++){
					if (mocapLineArray[4*i+3] == "NA"){
						mocapLineArray[4*i+3] = "NaN";
						mocapLineArray[4*i+4] = "NaN";
						mocapLineArray[4*i+5] = "NaN";
					}

					Vector3 posEntry = new Vector3(float.Parse(mocapLineArray[4*i+3]),
					                               float.Parse(mocapLineArray[4*i+4]),
					                               float.Parse(mocapLineArray[4*i+5]));	
					visibLine[i] = (1==float.Parse(mocapLineArray[4*i+2]));
					//new position entry
					mocapLine.Add(posEntry);
					mocLine[i,0] = posEntry.x;
					mocLine[i,1] = posEntry.y;	
					mocLine[i,2] = posEntry.z;	
				}

				if (countFrame==0)
					startTime = float.Parse(mocapLineArray[0]);
				countFrame++;
				endTime = float.Parse(mocapLineArray[0]);

				// new frame of positions and visibility
				mocapData.Add(mocLine);
				mocapVisibility.Add(visibLine);
        	}
    	}

		// set reproduction parameters
		totalFrames = mocapData.Count;
		stopFrame = totalFrames;
		frameRate = totalFrames/(endTime-startTime);
		// sampling delta time (in seconds)
		samplingDTime = 1.0f/frameRate;

		mocapIsLoaded = true;

		return true;
	}
}
