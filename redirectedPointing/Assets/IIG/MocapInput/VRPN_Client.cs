/*
 * AUTHOR: 
 * Eray Molla
 * 
 * DATE: 
 * 19/08/2014
 * 
 * DESCRIPTION: 
 * This source file is responsible for accessing the functionalities of
 * VRPN_Client_DLL file. It encapsulates the the .dll file so
 * that the other modules don't see the details.
 * */

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;     // DLL support

namespace VRPN
{
	public class Client
	{
		public double[,] marker_positions;
		public int[] marker_ids;
		public int no_of_markers;
		public GameObject[] marker_spheres;
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern int create_VRPN_Tracker(string marker_alias_file_name, string tracker_name);
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void init();
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void stop_streaming();
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void destroy();
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void update();
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void get_marker_positions([Out] double[,] marker_positions);
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern int test();
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern int get_marker_ids([Out] int[] marker_ids);
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern int get_no_of_markers();
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void set_frame_no(int frame_no);
		
		[DllImport("VRPN_Client_DLL", CallingConvention=CallingConvention.Cdecl)]
		private static extern void set_frame_increment(int frame_no);
		
		public Client (string marker_alias_file_name, string tracker_name)
		{
			if(create_VRPN_Tracker(marker_alias_file_name, tracker_name) == 1)
			{
				no_of_markers = get_no_of_markers();
				marker_ids = new int[no_of_markers];
				get_marker_ids(marker_ids);
				marker_positions = new double[no_of_markers, 3];
			}
			else
				throw new System.ArgumentException("VRPN Client could not be created. Make sure that there is no other client");
		}
		
		~Client()
		{
			destroy();
		}
		
		public void Init()
		{
			init();
		}
		
		public void Stop_streaming()
		{
			stop_streaming();
		}
		
		public void Update()
		{
			update();
			get_marker_positions(marker_positions);
		}
		
		// sets the the frame no that will be read from the c3d file
		public void Set_Frame_No(int frame_no)
		{
			set_frame_no(frame_no);
		}
		
		// sets the frame_no increment where frame_no += increment
		public void Set_Frame_Increment(int frame_increment)
		{
			set_frame_increment(frame_increment);
		}

		public void Render_Markers()
		{
			for(int i = 0; i < no_of_markers; ++i)
			{
				if(marker_positions[i,0] == marker_positions[i,0])
					marker_spheres[i].transform.position = new Vector3((float)(-marker_positions[i,0]), 
															 		   (float)(marker_positions[i,1]), 
															           (float)(marker_positions[i,2]));
			}
		}

		public void InitMarkerRendering()
		{
			marker_spheres = new GameObject[no_of_markers];
			
			for(int i = 0; i < no_of_markers; ++i)
			{
				marker_spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				marker_spheres[i].transform.localScale = new Vector3((float)0.02, (float)0.02, (float)0.02);
				marker_spheres[i].renderer.material.color = new Color(0, 0, 1);
			}
		}

		public void StopMarkerRendering()
		{
			for(int i = 0; i < no_of_markers; ++i)
				GameObject.Destroy(marker_spheres[i]);
			marker_spheres = null;
		}
	}
	
	
}

