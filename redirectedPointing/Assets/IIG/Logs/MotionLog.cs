// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;


public class MotionLog : MonoBehaviour {
/**************************************************************************
 * MotionLog.js allows to log position, rotation and collisions of game 
 * objects. It works in pair with MotionLogAttached.js, that serves as a 
 * message passer in case of Collision. It might be possible, if everything 
 * is logged into one file, that the collisions are not synchronised with 
 * rotation and position. So a mixed log might not be totally chronological.
 * It require CSVSupport.cs
 * Written by Sami Perrin
 * Last update: 25/11/14
 * *************************************************************************/


    public enum CallBackAt {update, lateUpdate, fixedUpdate};
    public enum LogType {
        allInSameFileDifferentRow, 
        gameObjectInDifferentFile,
        posRotCollisionInDifferentFile,
        allInDifferentFile,
        allInSameRowButCollisionInDifferentFile //Ignores recordPosition and recordRotation, and logs every object position and rotation in a row.
    };

	[System.Serializable]
    public class GoToRecord : System.Object {
		public string gameObjectName;

        public GameObject orProvideGameObject;
		public CallBackAt callBackTime;
		public bool recordPosition = true;
		public bool recordRotation = true;
		public bool recordCollision;
        
        [HideInInspector]
        public CsvFileWriter csvWriterPosition;
        [HideInInspector]
		public CsvFileWriter csvWriterRotation;
        [HideInInspector]
		public CsvFileWriter csvWriterCollision;
        [HideInInspector]
		public MotionLogAttached colliderMessage;
    }

	public LogType logType;
    public string filePrefix="motionLogRecord";
    public string fileSuffix=".csv";
    public bool  paused = false;
    public string switchPausedShortcut = "space";
    public bool  useGlobalCallbackTime = false;
    public CallBackAt globalCallbackTime;
    public bool  startOnlyWhenAllGOReady = false;
    public string stringFormatingTime = "F7";
    public string stringFormatingFloat = "F4";
    public List<GoToRecord> gameObjectToRecord = new List<GoToRecord>();

	private List<GoToRecord> notReadyGO         = new List<GoToRecord>();
	private List<GoToRecord> updateReadyGO      = new List<GoToRecord>();
	private List<GoToRecord> lateUpdateReadyGO  = new List<GoToRecord>();
	private List<GoToRecord> fixedUpdateReadyGO = new List<GoToRecord>();
	private List<CsvFileWriter> allCsvWriter       = new List<CsvFileWriter>();
    private CsvFileWriter csvWriter;
    private CsvFileWriter csvWriterCollision;
    private bool ready = false;
    private CsvFileWriter customCsvWriter;

    void  InitializeWriters (){
        
        customCsvWriter = new CsvFileWriter(filePrefix + "_custom" + fileSuffix);
        allCsvWriter.Add(customCsvWriter);

        if (logType == LogType.allInSameFileDifferentRow){
            csvWriter = new CsvFileWriter(filePrefix + fileSuffix);
            allCsvWriter.Add(csvWriter);
            foreach (GoToRecord goToRec in gameObjectToRecord){
                goToRec.csvWriterPosition = csvWriter;
                goToRec.csvWriterRotation = csvWriter;
                goToRec.csvWriterCollision = csvWriter;
            }
        }
        else if (logType == LogType.gameObjectInDifferentFile){
            foreach (GoToRecord goToRec in gameObjectToRecord){
                string name;
                if (goToRec.orProvideGameObject != null){
                    name = goToRec.orProvideGameObject.name;
                }
                else{
                    name=goToRec.gameObjectName;
                }
                CsvFileWriter writer = new CsvFileWriter(filePrefix + "_" + name + fileSuffix);
                goToRec.csvWriterPosition  = writer;
                goToRec.csvWriterRotation  = writer;
                goToRec.csvWriterCollision = writer;
                allCsvWriter.Add(goToRec.csvWriterPosition);
            }
        }
        else if (logType == LogType.posRotCollisionInDifferentFile){
            CsvFileWriter csvWriterPos = new CsvFileWriter(filePrefix + "_position" + fileSuffix);
            CsvFileWriter csvWriterRot = new CsvFileWriter(filePrefix + "_rotation" + fileSuffix);
            CsvFileWriter csvWriterCol = new CsvFileWriter(filePrefix + "_collision" + fileSuffix);
            allCsvWriter.Add(csvWriterCol);
            allCsvWriter.Add(csvWriterPos);
            allCsvWriter.Add(csvWriterRot);
            foreach (GoToRecord goToRec in gameObjectToRecord){
                goToRec.csvWriterPosition=csvWriterPos;
                goToRec.csvWriterRotation=csvWriterRot;
                goToRec.csvWriterCollision=csvWriterCol;
            }
        }
        else if (logType == LogType.allInDifferentFile){
            foreach (GoToRecord goToRec in gameObjectToRecord){
                string name_;
                if (goToRec.orProvideGameObject != null){
                    name_ = goToRec.orProvideGameObject.name;
                }
                else{
                    name_ = goToRec.gameObjectName;
                }
                goToRec.csvWriterCollision = new CsvFileWriter(filePrefix + "_collision_" + name_ + fileSuffix);
                goToRec.csvWriterPosition  = new CsvFileWriter(filePrefix + "_position_"  + name_ + fileSuffix);
                goToRec.csvWriterRotation  = new CsvFileWriter(filePrefix + "_rotation_"  + name_ + fileSuffix);
                allCsvWriter.Add(goToRec.csvWriterCollision);
                allCsvWriter.Add(goToRec.csvWriterPosition);
                allCsvWriter.Add(goToRec.csvWriterRotation);
            }
        }
        else if (logType == LogType.allInSameRowButCollisionInDifferentFile){
            csvWriter = new CsvFileWriter(filePrefix + "_positionAndRotation" + fileSuffix);
            csvWriterCollision = new CsvFileWriter(filePrefix + "_collision" + fileSuffix);
            allCsvWriter.Add(csvWriter);
            allCsvWriter.Add(csvWriterCollision);
            foreach (GoToRecord goToRec in gameObjectToRecord){
                goToRec.csvWriterCollision = csvWriterCollision;
            }
        }
    }

    void  LogCustomRow (ReadWriteCsv.CsvRow row){
        customCsvWriter.WriteRow(row);
    }

    void  FindGO (){
        if (gameObjectToRecord != null){
			foreach (GoToRecord goToRec in gameObjectToRecord){
                notReadyGO.Add(goToRec);
            }
            
            gameObjectToRecord = null;
        }
        
        int length = notReadyGO.Count;
        
        for (int i=0; i<length; i++){
            GoToRecord goToRec = notReadyGO[i];
            
            if (goToRec.orProvideGameObject == null){
                goToRec.orProvideGameObject = GameObject.Find(goToRec.gameObjectName);

                if (goToRec.orProvideGameObject == null){
                    Debug.LogWarning("MotionLog.cs - GameObject " + goToRec.gameObjectName + " not Found.");
                    continue;
                }

            }
            else{
                goToRec.gameObjectName = goToRec.orProvideGameObject.name;
            }
            
            if (goToRec.recordCollision){
				goToRec.colliderMessage = goToRec.orProvideGameObject.AddComponent("MotionLogAttached") as MotionLogAttached;
            }
            
            if (useGlobalCallbackTime){
                if (globalCallbackTime == CallBackAt.update){
                    updateReadyGO.Add(goToRec);
                }
                else if (globalCallbackTime == CallBackAt.fixedUpdate){
                    fixedUpdateReadyGO.Add(goToRec);
                }
                else if (globalCallbackTime == CallBackAt.lateUpdate){
                    lateUpdateReadyGO.Add(goToRec);
                }
            }
            else{   
                if (goToRec.callBackTime == CallBackAt.update){
                    updateReadyGO.Add(goToRec);
                }
                else if (goToRec.callBackTime == CallBackAt.fixedUpdate){
                    fixedUpdateReadyGO.Add(goToRec);
                }
                else if (goToRec.callBackTime == CallBackAt.lateUpdate){
                    lateUpdateReadyGO.Add(goToRec);
                }
            }
            
            notReadyGO.RemoveAt(i);
            length--;
            
        }
        
        if (notReadyGO.Count == 0){
            ready = true;
        }
    }

	void  LogCurrentFrame (List<GoToRecord> goToLog ){
        if (!ready) {
            FindGO();
            if (startOnlyWhenAllGOReady && !ready){
                return;
            }
        }
        
        if (paused){
            return;
        }
        
        string timestamp = Time.time.ToString(stringFormatingTime);
        
        
        if (logType == LogType.allInSameRowButCollisionInDifferentFile){
            if (goToLog.Count!=0){
                CsvRow row = new CsvRow();
                row.Add(timestamp);
                foreach (GoToRecord goToRec in goToLog){
                    row.Add(goToRec.gameObjectName);
                    if (goToRec.recordPosition){
                        LogPositionToRow(goToRec, timestamp, row);
                    }
                    if (goToRec.recordRotation){
                        LogRotationToRow(goToRec, timestamp, row);
                    }
                }
                csvWriter.WriteRow(row);
            }
        }
        else{
            foreach (GoToRecord goToRec in goToLog){
                if (goToRec.recordPosition){
                    LogPosition(goToRec, timestamp);
                }
                if (goToRec.recordRotation){
                    LogRotation(goToRec, timestamp);
                }
            }
        }
        
        //In all case log collision
        foreach (GoToRecord goToRec in goToLog){
            if (goToRec.recordCollision && goToRec.colliderMessage.newMessage){
                foreach (CollisionMessage message in goToRec.colliderMessage.messages){
                    LogCollision(goToRec, message);
                }
                goToRec.colliderMessage.reset();
            }
        }
    }

    void  LogPosition (GoToRecord goToRec, string timestamp){
        CsvRow row = new CsvRow();
        row.Add(timestamp);
        row.Add("position");
        row.Add(goToRec.gameObjectName);
        LogPositionToRow(goToRec, timestamp, row);
        goToRec.csvWriterPosition.WriteRow(row);
    }

    void  LogRotation (GoToRecord goToRec, string timestamp){
        CsvRow row = new CsvRow();
        row.Add(timestamp);
        row.Add("Rotation");
        row.Add(goToRec.gameObjectName);
        LogRotationToRow(goToRec, timestamp, row);
        goToRec.csvWriterRotation.WriteRow(row);
    }

    void  LogPositionToRow (GoToRecord goToRec, string timestamp, ReadWriteCsv.CsvRow row){
        row.Add(goToRec.orProvideGameObject.transform.position.x.ToString(stringFormatingFloat));
        row.Add(goToRec.orProvideGameObject.transform.position.y.ToString(stringFormatingFloat));
        row.Add(goToRec.orProvideGameObject.transform.position.z.ToString(stringFormatingFloat));
    }

    void  LogRotationToRow ( GoToRecord goToRec, string timestamp, ReadWriteCsv.CsvRow row){
        row.Add(goToRec.orProvideGameObject.transform.rotation.x.ToString(stringFormatingFloat));
        row.Add(goToRec.orProvideGameObject.transform.rotation.y.ToString(stringFormatingFloat));
        row.Add(goToRec.orProvideGameObject.transform.rotation.z.ToString(stringFormatingFloat));
        row.Add(goToRec.orProvideGameObject.transform.rotation.w.ToString(stringFormatingFloat));
    }

    void  LogCollision (GoToRecord goToRec, CollisionMessage message){
        CsvRow row = new ReadWriteCsv.CsvRow();
        row.Add(message.timestamp.ToString(stringFormatingTime));
        row.Add(message.collisionOrTrigger);
        row.Add(message.type);
        row.Add(message.from.name);
        row.Add(message.to.name);
        row.Add(message.from.transform.position.x.ToString(stringFormatingFloat));
        row.Add(message.from.transform.position.y.ToString(stringFormatingFloat));
        row.Add(message.from.transform.position.z.ToString(stringFormatingFloat));
        goToRec.csvWriterCollision.WriteRow(row);
    }

    void  FlushAllCsvWriter (){
        foreach (CsvFileWriter writer in allCsvWriter){
            writer.Flush();
        }
    }

    void  Start (){
        InitializeWriters();
        FindGO();
    }

    void  Update (){
        LogCurrentFrame(updateReadyGO);
    }

    void  LateUpdate (){
        LogCurrentFrame(lateUpdateReadyGO);
    }

    void  FixedUpdate (){
        LogCurrentFrame(fixedUpdateReadyGO);
    }

    void  OnApplicationQuit (){
        FlushAllCsvWriter();
    }

    void  OnGUI (){
        if (Event.current.Equals(Event.KeyboardEvent(switchPausedShortcut))){
            paused =! paused;
        }
    }

}