/**************************************************************************
 * MocapPlayer.cs controls the reproduction of preloaded mocap data
 * Written by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class MocapPlayer{
	// sampling delta time in seconds and loaded time, used to sync recorded with reproduction frequency
	public float samplingDTime {get; protected set;} // = 0.0f;
	
	// for internal control of frame cycling
	public float currentTime {get; set;} //= 0.0f;
	public int startFrame {get; protected set;} //= 0;
	public int stopFrame {get; protected set;} //= 0;
	public float repSpeed {get; set;} //= 1.0f;
	public bool repeat {get; set;}
	// recording attributes
	public float frameRate {get; protected set;} //= 0;
	public int totalPoints {get; protected set;} //= 0;
	public int totalFrames {get; protected set;} //= 0;
	
	// is there a loaded mocap?
	public bool mocapIsLoaded {get; protected set;} //= false;

	// keeps the mocap data
	protected List<double[,]> mocapData = new List<double[,]>();
	protected List<bool[]> mocapVisibility = new List<bool[]>();
	
	public MocapPlayer(){
		DumpMocap();
	}

	public virtual bool LoadMocap(string fileName)
	{
		return false;
	}
	
	// clear store mocap data
	public void DumpMocap(){
		// reset all variables
		samplingDTime = currentTime = startFrame = stopFrame = 0;
		frameRate = totalPoints = totalFrames = 0;
		repSpeed = 1.0f;
		repeat = false;
		mocapIsLoaded = false;
		
		// clear list
		mocapData.Clear();
		mocapVisibility.Clear();
	}
	
	// return points data at specific time
	public bool GetFrameAt(float timeSec, ref double[,] frameRef, ref bool[] visibRef){
		int frame = TestFrame(timeSec);
		if (frame==-1)
			return false;
		frameRef = mocapData[frame];
		visibRef = mocapVisibility[frame];
		return true;		
	}
	
	// return points data at specific frame
	public bool GetFrameAt(int frame, ref double[,] frameRef, ref bool[] visibRef){
		if (!mocapIsLoaded){
			Debug.Log ("There is no mocap loaded!");
			return false;
		}
		// frame is within bounds?
		if (frame >= stopFrame || frame <0){
			Debug.Log ("Loaded motion capture has less than " + frame + " frames");
			return false;
		}
		frameRef = mocapData[frame];
		visibRef = mocapVisibility[frame];
		return true;		
	}
	

	// return points data at specific frame
	public bool CycleFrames(float deltaTime, ref double[,] frameRef, ref bool[] visibRef){
		// rescale delta time according to reproduction speed, thus 1 is normal speed
		
		if (!mocapIsLoaded){
			Debug.Log ("There is no mocap loaded!");
			return false;
		}

		int frame = (int) (currentTime/samplingDTime);
		if (frame >= stopFrame  || frame <0){
			int resetFrame = (repSpeed>0) ? startFrame : stopFrame-1;
			if (!repeat){
				resetFrame = (repSpeed<0) ? startFrame : stopFrame-1;
			}
			currentTime = samplingDTime*resetFrame;
			frame = resetFrame;
		} else {
			deltaTime = deltaTime * repSpeed;
			currentTime += deltaTime;
		}

		frameRef = mocapData[frame];
		visibRef = mocapVisibility[frame];
		return true;		
	}
	
	// is the mocapLoaded?
	public bool IsLoaded(){
		return mocapIsLoaded;
	}
	
	// set the start and stop time of reproduction within the motion capture data
	public void SetStartStopTime(float nStartTime, float nStopTime){
		SetStartTime (nStartTime);
		SetStopTime (nStopTime);
	}
	// set the start time
	public void SetStartTime(float nStartTime){
		int frame = TestFrame (nStartTime);
		if (frame!=-1 && frame<stopFrame){
			startFrame = frame;
			// set current reproduction time
			if (currentTime<nStartTime)
				currentTime = nStartTime;
		}
	}
	// set the stop time
	public void SetStopTime(float nStopTime){
		int frame = TestFrame (nStopTime);
		if (frame!=-1 && frame>startFrame){
			stopFrame = frame;		
			// set current reproduction time
			if (currentTime>nStopTime)
				currentTime = nStopTime;
		}
	}

	public int GetCurrentFrameNbr(){
		return (int) (currentTime/samplingDTime);
	}

	public bool SetCurrentFrameNbr(int nFrame){
		if (nFrame >= startFrame  && nFrame < stopFrame){
			currentTime = samplingDTime * nFrame;
			return true;
		} else {
			return false;
		}
	}

	// test if there is a frame at a given time, return the frame number if that is the case
	int TestFrame(float time2test){
		if (!mocapIsLoaded){
			Debug.Log ("There is no mocap loaded!");
			return -1;
		}
		int frame = (int) (time2test/samplingDTime);
		if (frame >= totalFrames || frame <0){
			Debug.Log ("Loaded motion capture is shorter than " + time2test + " seconds");
			return -1;
		}
		return frame;
	}

}