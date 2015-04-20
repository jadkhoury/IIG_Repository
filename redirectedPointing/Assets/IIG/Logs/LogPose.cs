/**************************************************************************
 * LogPoseDirect.cs allows to log a timestamp and the pose and position of
 * the avatar for that given isntant into a pose.csv file
 * it saves each frame directly to the file ta save memory
 * Written by Henrique Galvan Debarba
 * Last update: 28/02/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;
using System.IO;


public class LogPose {

	// list of log entries
	private List<CsvRow> markerLogList = new List<CsvRow>();

	public bool logIsOpen {get; set;}

	string fileFormat = ".pose.csv";

	bool header=true;

	public LogPose(){
		logIsOpen = false;
	}

	// log current pose if log is open
	public void LogAvatarPoseFrame(List<GameObject> listOfJoints){
		if (!logIsOpen)
			return;
	
		if (header){
			CsvRow headerLog = new ReadWriteCsv.CsvRow();
			headerLog.Add("Time (s)");
			string [] sufixes = new string[] {".X", ".Y", ".Z", ".QX", ".QY", ".QZ",".QW"};
			for(int i = 0; i <listOfJoints.Count; ++i)
			{
				foreach(string sufix in sufixes){
					headerLog.Add(listOfJoints[i].name + sufix);
				}
			}
			markerLogList.Add(headerLog);
			header=false;
		}

		CsvRow currentLog = new ReadWriteCsv.CsvRow();
		currentLog.Add(Time.time.ToString("F6"));

		for(int i = 0; i <listOfJoints.Count; ++i)
		{
			//currentLog.Add(listOfJoints[i].name);
			Transform joint = listOfJoints[i].transform;
			currentLog.Add(joint.position.x.ToString("F4"));
			currentLog.Add(joint.position.y.ToString("F4"));
			currentLog.Add(joint.position.z.ToString("F4"));
			currentLog.Add(joint.rotation.x.ToString("F4"));
			currentLog.Add(joint.rotation.y.ToString("F4"));
			currentLog.Add(joint.rotation.z.ToString("F4"));
			currentLog.Add(joint.rotation.w.ToString("F4"));
//			currentLog.Add(joint.position.ToString("F4"));
//			currentLog.Add(joint.rotation.ToString("F4"));
		}
		markerLogList.Add(currentLog);
	}
	
	// openg log
	public void OpenLog(){	
		header = true;
		logIsOpen = true;
		Debug.Log("avatar pose log is open");
	}
	
	// close log
	public string CloseLog(string logName){
		logIsOpen = false;
		int i=0;
		logName += i.ToString();
		while (File.Exists(logName + fileFormat)){
			i++;
			logName = logName.Remove(logName.Length - 1);
			logName += i.ToString();
		}
		string poseLogName = logName + fileFormat;
		SaveLog(poseLogName);
		Debug.Log("avatar pose log is closed");
		
		return poseLogName;
	}	
	
	public void SaveLog(string markerLogName){
		
		// write log to CSV file
		Debug.Log("saving avatar pose log " + markerLogName);
		using (CsvFileWriter writer = new CsvFileWriter(markerLogName))
		{
			foreach (CsvRow logEntry in markerLogList)
			{
				writer.WriteRow(logEntry);
			}
			writer.Close();
			//writer.Flush();

		}

		// clear log data
		markerLogList.Clear();
		System.GC.Collect();
		Debug.Log("avatar pose log saved " + markerLogName);
	}	

}