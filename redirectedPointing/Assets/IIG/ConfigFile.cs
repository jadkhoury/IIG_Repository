// This script is used to manage a config file. Options are saved and loaded 
// from this configuration file so that they can be set without recompiling the program.
// e.g. calibration file used for mocap; motion capture file to be reproduced; ip address etc...
// Henrique Galvan Debarba - 2014

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

public static class MemberInfoGetting
{
	public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
	{
		MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
		return expressionBody.Member.Name;
	}
}

public class ConfigFile
{
	public static void SaveFile(ref string[] configArray, string ConfigPath)
	{
		if ((configArray.Length%2) != 0)
			return;
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i<configArray.Length; i+=2)
			sb.AppendLine( configArray[i] + "=" + configArray[i+1]);
		System.IO.StreamWriter writer = new System.IO.StreamWriter(ConfigPath);
		writer.Write(sb.ToString());
		writer.Close();
	}
	
	public static string Search(string propertyName, string ConfigPath)
	{
		System.IO.StreamReader reader = new System.IO.StreamReader(ConfigPath);
		string line;
		string value = "";
		while((line = reader.ReadLine()) != null)
		{
			string[] id_value = line.Split('=');
			if (id_value[0] == propertyName){
				value = id_value[1].ToString();
			}
		}
		reader.Close();
		return value;
	}
}