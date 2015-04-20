/**************************************************************************
 * Mocap.cs bridges IK DLL with Unity
 * Written by Eray Molla and modified by Henrique Galvan Debarba
 * Last update: 03/03/14
 * *************************************************************************/

using System;
using System.Runtime.InteropServices;     // DLL support

namespace IIG
{
	public class Mocap
	{
		public int no_of_joints;
		public double[] root_pos;
		public double[,] orientations;
		public string[] joint_names;
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern void send_marker_values(int[] marker_ids, double[,] marker_positions);
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern int read_joint_values([Out] double[] root_pos, [Out] double[,] orientation);
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern int init(string mocap_config_file_path);
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern void update();
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern int get_no_of_joints();
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern IntPtr get_joint_names();
		
		[DllImport("IIGMocap", CallingConvention=CallingConvention.Cdecl)]
		private static extern int get_max_joint_name_length();
		
		
		public Mocap (string mocap_config_file_path)
		{
			init(mocap_config_file_path);
			no_of_joints = get_no_of_joints();
			joint_names = new string[no_of_joints];
			root_pos = new double[3];
			
			for(int i = 0; i < no_of_joints; ++i)
			{
				joint_names[i] = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(get_joint_names(), 4 * i));
			}
			
			// length is 4 because it is a quaternion
			orientations = new double[no_of_joints, 4];
			
			
		}
		
		public void UpdateIK(int[] marker_ids, double[,] marker_positions)
		{
			update ();
			send_marker_values(marker_ids, marker_positions);
			read_joint_values(root_pos, orientations);
		}
	}
}

