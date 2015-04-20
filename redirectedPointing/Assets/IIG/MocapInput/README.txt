Henrique Galvan Debarba - 05/03/14

MocapInput controls the source of mocap data (VRPN streaming, .c3d file or .mkr.csv file)
and whether to debug this data
It contains

scripts:

-MocapPlayer.cs: controls the reproduction of preloaded mocap data

-C3DPlayer.cs: extends MocapPlayer.cs loading C3D files to memory

-CSVPlayer.cs: extends MocapPlayer.cs loading mkr.csv files to memory

-MarkersDebug.cs: draws current frame of markers position and visibility

-VRPN_Client.cs: bridge the VRPN client DLL to get stream from phasespace

-MocapInputController.cs: controls the data input and allows adding a delay
currently it sets the avatar posture too (which I might modify in the future)
