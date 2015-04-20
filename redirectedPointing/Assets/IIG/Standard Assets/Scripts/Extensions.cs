﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
	public static Transform Search(Transform target, string name){
		if (target.name == name) return target;
		
		for (int i = 0; i < target.childCount; ++i){
			var result = Search(target.GetChild(i), name);
			
			if (result != null) return result;
		}
		return null;
	}
	public static void SearchAll(Transform target, string name, ref List<Transform> result){
		if (target.name == name) result.Add(target);
		
		for (int i = 0; i < target.childCount; ++i)
		{	
			SearchAll(target.GetChild(i), name, ref result);	
		}
	}


	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}
}