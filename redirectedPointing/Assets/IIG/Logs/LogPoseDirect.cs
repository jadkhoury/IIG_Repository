/**************************************************************************
 * LogPose.cs allows to log a timestamp and the pose and position of
 * the avatar for that given isntant into a pose.csv file
 * Written by Henrique Galvan Debarba
 * Last update: 28/02/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;
using System.IO;


public class LogPoseDirect {

	// list of log entries
	//private List<CsvRow> markerLogList = new List<CsvRow>();

	public bool logIsOpen {get; set;}

	string fileFormat = ".pose.csv";

	bool header=true;

	CsvFileWriter m_writer;

	string currentFileName = "";

	public LogPoseDirect(){
		logIsOpen = false;
	}

	// log current pose if log is open
	public void LogAvatarPoseFrame(List<Transform> listOfJoints){
		LogAvatarPoseFrame (listOfJoints, Time.time);
	}
	public void LogAvatarPoseFrame(List<Transform> listOfJoints, float recordingTime, int samplingRate = 0){
		if (!logIsOpen)
			return;
	
		//m_writer = new CsvFileWriter(currentFileName);

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
			if (samplingRate != 0)
				headerLog.Add("Sampling Rate");
			m_writer.WriteRow(headerLog);
			//markerLogList.Add(headerLog);
			header=false;
		}

		CsvRow currentLog = new ReadWriteCsv.CsvRow();
		currentLog.Add (recordingTime.ToString("F6"));
		//currentLog.Add(Time.time.ToString("F6"));

		for(int i = 0; i <listOfJoints.Count; ++i)
		{
			//currentLog.Add(listOfJoints[i].name);
			Transform joint = listOfJoints[i];
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
		if (samplingRate != 0)
			currentLog.Add (samplingRate.ToString ());
		//markerLogList.Add(currentLog);

		m_writer.WriteRow(currentLog);
		currentLog.Clear ();
//		System.GC.Collect();
		//m_writer.Close();

	}
	
	// openg log
	public string OpenLog(string markerLogName){	
		currentFileName = NameLog (markerLogName);
		header = true;
		logIsOpen = true;
		m_writer = new CsvFileWriter(currentFileName);
		Debug.Log("avatar pose log is open as " + currentFileName);
		return currentFileName;
	}
	
	// close log
	public string NameLog(string logName){

		int i=0;
		logName += i.ToString();
		while (File.Exists(logName + fileFormat)){
			i++;
			logName = logName.Remove(logName.Length - 1);
			logName += i.ToString();
		}
		string poseLogName = logName + fileFormat;
		//SaveLog(poseLogName);

		return poseLogName;
	}	
	
	public void SaveLog(){
		logIsOpen = false;
		// write log to CSV file
		Debug.Log("saving avatar pose log " + currentFileName);
	//	using (CsvFileWriter writer = new CsvFileWriter(markerLogName))
	//	{
	//		foreach (CsvRow logEntry in markerLogList)
	//		{
	//			writer.WriteRow(logEntry);
	//		}
	//		writer.Close();
	//	}
		m_writer.Close();

		// clear log data
		//markerLogList.Clear();
		System.GC.Collect();
		Debug.Log("avatar pose log saved " + currentFileName);
	}	

}