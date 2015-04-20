using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;
using System.IO;

public class LogTargets {

	// list of log entries
	private List<CsvRow> markerLogList = new List<CsvRow>();
	string fileFormat = ".tgt.csv";

	public string SaveLog(string logName, List<Tuple<string[], Tuple<string, float>>>  targetsLog){
		int i=0;
		logName += i.ToString();
		while (File.Exists(logName + fileFormat)){
			i++;
			logName = logName.Remove(logName.Length - 1);
			logName += i.ToString();
		}
		logName += fileFormat;

		// write log to CSV file
		Debug.Log("LogTargets.cs: saving targets log " + logName);
		using (CsvFileWriter writer = new CsvFileWriter(logName))
		{
			foreach (Tuple<string[], Tuple<string, float>> logEntry in targetsLog){
				CsvRow currentLog = new ReadWriteCsv.CsvRow();
				for (int j = 0; j < logEntry.First.Length; j++){
					currentLog.Add(logEntry.First[j]);
				}
				currentLog.Add(logEntry.Second.First);
				currentLog.Add(logEntry.Second.Second.ToString());

				writer.WriteRow(currentLog);
			}
		}

		Debug.Log("LogTargets.cs: avatar pose log saved as " + logName);

		return logName;
	}	

}
