/**************************************************************************
 * LogMarkers.cs allows to log a time stamp, marker positions, and whether
 * they were visible at that given instant into a mkr.csv file
 * Written by Henrique Galvan Debarba
 * Last update: 28/02/14
 * *************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;
using System.IO;

public class LogMarkers {

	// list of log entries
	private List<CsvRow> markerLogList = new List<CsvRow>();

	public bool logIsOpen {get; set;}

	string fileFormat = ".mkr.csv";

	float startTime;

	public LogMarkers(){
		logIsOpen = false;
	}

	// log current pose if log is open
	public void LogMarkersFrame(double[,] listOfMarkers, bool[] markersVisibility){
		if (!logIsOpen || listOfMarkers==null || listOfMarkers.GetLength(0)!=markersVisibility.Length)
			return;
		
		CsvRow currentLog = new ReadWriteCsv.CsvRow();
		currentLog.Add((Time.time-startTime).ToString("F7"));
		currentLog.Add(Time.time.ToString("F7"));

		for(int i = 0; i <listOfMarkers.GetLength(0); ++i)
		{		
			currentLog.Add(((markersVisibility[i])?1:0).ToString());
			for(int j = 0; j <listOfMarkers.GetLength(1); ++j)
			{
				currentLog.Add(listOfMarkers[i,j].ToString("F4"));
			}
		}
		markerLogList.Add(currentLog);
	}
	
	// openg log
	public void OpenLog(List<Tuple<int, string>> nameList){	

		logIsOpen = true;
		Debug.Log("markers log is open");

		CsvRow currentLog = new ReadWriteCsv.CsvRow();
		currentLog.Add("Time (s)");
		currentLog.Add("SystemTime (s)");

		for(int i = 0; i <nameList.Count; ++i)
		{		
			currentLog.Add(nameList[i].Second);
			currentLog.Add(nameList[i].Second + ".X");
			currentLog.Add(nameList[i].Second + ".Y");
			currentLog.Add(nameList[i].Second + ".Z");
		}
		markerLogList.Add(currentLog);

		startTime = Time.time;
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
		string markerLogName = logName + fileFormat;
		SaveLog(markerLogName);
		Debug.Log("avatar pose log is closed");
		
		return markerLogName;
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
		}
		
		// clear log data
		markerLogList.Clear();
		System.GC.Collect ();
		Debug.Log("avatar pose log saved " + markerLogName);
	}	

}