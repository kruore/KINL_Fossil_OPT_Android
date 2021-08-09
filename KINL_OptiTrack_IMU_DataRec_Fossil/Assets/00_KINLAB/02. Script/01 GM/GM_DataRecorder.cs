using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TMPro;
using _KINLAB._KINLAB_IMU;

namespace _KINLAB
{
    [HideInInspector]
    public enum IMU_SensorData_Type { acceleration, angularVelocity, magneticField };

    public class S_MakerData
    {
        public int ID = 123456;
        public Vector3 Pos = Vector3.zero;
        public bool isUpdated = false;

        public S_MakerData(int _ID, Vector3 _pos, bool _upd)
        {
            ID = _ID; Pos = _pos; isUpdated = _upd;
        }
    }

    public class GM_DataRecorder : MonoBehaviour
    {

        public static GM_DataRecorder instance = null;

        private string rootpath = string.Empty;
        private string mainfolder_Path = string.Empty;

        private string mainFolderName = "KINLAB_MoCap_DataFolder";


        private string str_DataCategory = string.Empty;

        [HideInInspector]
        public enum Experiment_Type { EX_01, EX_02, EX_03, EX_04, EX_05, EX_06 };

        public Experiment_Type Curren_EX_Type;

        private double startTime = 0.0f;
        private double currentTime = 0.0f;

        [HideInInspector]
        public enum Warning_Type
        {
            Ex_Start, Ex_End, AutoStart, AutoStop, RecordStart, RecordStop, Score
        }

        [HideInInspector]
        public Queue<string> Queue_ex;

        public Queue<string> Queue_SmartWatch_ex;

        private bool isCategoryPrinted;

        private List<bool> isCategoryPrinted_acc;
        private List<bool> isCategoryPrinted_gyr;
        private List<bool> isCategoryPrinted_mag;

        private int IMU_DataFile_SavedCount = 0;
        private int DownLoad_GroupCount = 0;

        [SerializeField]
        private OptitrackStreamingClient Client;

        private List<Vector3> OptiTrack_markers_Pos = new List<Vector3>();

        private int OptiTrack_MarkerCount = 4;

        private bool isOptiTrackStreamingMode = false;

        private Dictionary<int, S_MakerData> m_MarkerDic = new Dictionary<int, S_MakerData>();
        private List<int> m_markerIDList = new List<int>();

        [SerializeField]
        private List<GameObject> List_Current_Kinect_Joints = new List<GameObject>();
        [SerializeField]
        private List<GameObject> List_Current_OptiTrack_Joints = new List<GameObject>();


        private List<Vector3> List_prev_Kinect_Joints_pos = new List<Vector3>(3);
        private List<Vector3> List_prev_OptiTrack_Joints_pos = new List<Vector3>(3);

        bool tempToggle_Automatic = false;
        bool bAutoproceLock = false;

        [SerializeField]
        private TextMeshProUGUI Text_Queue_OptiTrack;

        [SerializeField]
        private Text text_MsgDP;
        [SerializeField]
        private bool bRecording = false;
        public bool BRecording { get { return bRecording; }}
        //-------------------------------

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            mainfolder_Path = rootpath = Directory.GetCurrentDirectory();
            Debug.Log(rootpath);
            mainfolder_Path = MakeFolder(mainFolderName);
            Debug.Log(mainfolder_Path);

            isCategoryPrinted_acc = new List<bool>();
            isCategoryPrinted_gyr = new List<bool>();
            isCategoryPrinted_mag = new List<bool>();

            for (int i = 0; i < 3; i++)
            {
                isCategoryPrinted_acc.Add(false);
                isCategoryPrinted_gyr.Add(false);
                isCategoryPrinted_mag.Add(false);
            }

            //OptiTrack_MarkerCount = Opti_IMU_SettingPage_Management.instance.optiTrack_MarkerCount;

            //Debug.Log("Count : " + OptiTrack_MarkerCount);
        }

        private void Start()
        {
            Queue_ex = new Queue<string>();
            Queue_SmartWatch_ex = new Queue<string>();

            //startTime = currentTime = Time.time;
            startTime = currentTime = DateTime.Now.Ticks;

            for (int i = 0; i < 3; i++)
            {
                List_prev_Kinect_Joints_pos.Add(Vector3.zero);
                List_prev_OptiTrack_Joints_pos.Add(Vector3.zero);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Write_Warning(Warning_Type.Ex_Start, "");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Write_Warning(Warning_Type.Ex_End, "");
            }

            if (Input.GetKeyDown(KeyCode.Slash))
            {
                Start_SaveSteamingData_Batch();

            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Coru_AutoStartStop());
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (isOptiTrackStreamingMode)
                {
                    isOptiTrackStreamingMode = false;
                    Debug.Log("OptiTrack Streaming Mode is Deactivated");
                    TCPTestClient.instance.Send_Message_Index("\n" + "OptiTrack Streaming Mode is Deactivated.", 0);
                }
                else
                {
                    isOptiTrackStreamingMode = true;
                    Debug.Log("OptiTrack Streaming Mode is Activated");
                    TCPTestClient.instance.Send_Message_Index("\n" + "OptiTrack Streaming Mode is Activated.", 0);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                Write_Warning(Warning_Type.Score, "0");
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Write_Warning(Warning_Type.Score, "1");
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Write_Warning(Warning_Type.Score, "2");
            }

            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                Write_Warning(Warning_Type.Score, "3");
            }

            //Record_Data_OptitrackMarker_IMU();

            //foreach (var item in Client.m_latestMarkerStates)
            //{
            //    Debug.Log(item.Value.Id + "  " + item.Value.Position.x);
            //}

            //Record_Data();

            //Temp_0811_Record_Data();
        }

        private void FixedUpdate()
        {
            //for fingershoot
            //Record_Data_IMU_OptiTrackMarker();


            //for imu test
            //Record_Data_IMU_OptiTrackSkel("");
        }


        public string MakeFolder(string _WantedfolderName)
        {
            string finalFolderPath = string.Empty;

            finalFolderPath = System.IO.Path.Combine(mainfolder_Path, _WantedfolderName);

            if (!Directory.Exists(finalFolderPath))
            {
                Directory.CreateDirectory(finalFolderPath);
            }

            return finalFolderPath;

        }

        public void Enequeue_Data(string _data)
        {
            //Debug.Log(DateTime.Now.Ticks.ToString());

            //currentTime = Time.time;
            currentTime = DateTime.Now.Ticks;

            //Debug.Log((currentTime - startTime).ToString());

            double timestemp = (currentTime - startTime) / 10000000.0d;

            string stringRelativeTime = string.Format("{0:F4}", timestemp);

            //string refined_Data = DateTime.Now.ToString("yyyyMMddHHmmss.fff") + "," + stringRelativeTime + "," + _data;
            string refined_Data = DateTime.Now.ToString("HHmmssfff") + "," + stringRelativeTime + "," + _data;

            Queue_ex.Enqueue(refined_Data);

            Text_Queue_OptiTrack.text = "OptiTrack + mbient Count : " + Queue_ex.Count;
        }

        //public void Enequeue_SmartWatchData(string _ACC_data, string _Gyro_data, long _offset)
        public void Enequeue_SmartWatchData(string _ACC_data, string _Gyro_data, TimeSpan _offset)
        {
            try
            {
                string[] splitted_ACC_Data = _ACC_data.Split('^');
                string[] splitted_Gyro_Data = _Gyro_data.Split('^');

                if (splitted_ACC_Data.Length < 4 || splitted_Gyro_Data.Length < 4)
                    return;

                //long acc_time_offsetapplied = long.Parse(splitted_ACC_Data[0]) - _offset;
                //long gyro_time_offsetapplied = long.Parse(splitted_Gyro_Data[0]) - _offset;

                DateTime acc_dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(splitted_ACC_Data[0].Substring(0, 2)), int.Parse(splitted_ACC_Data[0].Substring(2, 2))
                    , int.Parse(splitted_ACC_Data[0].Substring(4, 2)), int.Parse(splitted_ACC_Data[0].Substring(6, 3)));

                DateTime gyro_dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(splitted_Gyro_Data[0].Substring(0, 2)), int.Parse(splitted_Gyro_Data[0].Substring(2, 2))
                    , int.Parse(splitted_Gyro_Data[0].Substring(4, 2)), int.Parse(splitted_Gyro_Data[0].Substring(6, 3)));

                acc_dateTime -= _offset;
                gyro_dateTime -= _offset;

                string acc_time_offsetapplied = string.Format("{0:D2}{1:D2}{2:D2}{3:D3}", acc_dateTime.Hour, acc_dateTime.Minute, acc_dateTime.Second, acc_dateTime.Millisecond);
                string gyro_time_offsetapplied = string.Format("{0:D2}{1:D2}{2:D2}{3:D3}", gyro_dateTime.Hour, gyro_dateTime.Minute, gyro_dateTime.Second, gyro_dateTime.Millisecond);

                ////ex 13 59 56 123   -
                //int HH, mm, SS, sss = 0;
                //HH = int.Parse(splitted_ACC_Data[0].Substring(0, 2));
                //mm = int.Parse(splitted_ACC_Data[0].Substring(2, 2));
                //SS = int.Parse(splitted_ACC_Data[0].Substring(4, 2));
                //sss = int.Parse(splitted_ACC_Data[0].Substring(6, 3));




                string refined_Data = acc_time_offsetapplied.ToString() + "," + gyro_time_offsetapplied.ToString() + "," + splitted_ACC_Data[1] + "," + splitted_ACC_Data[2] + "," + splitted_ACC_Data[3] + "," + splitted_Gyro_Data[1] + "," + splitted_Gyro_Data[2] + "," + splitted_Gyro_Data[3] + ",,," + splitted_ACC_Data[0] + "," + splitted_Gyro_Data[0];

                Queue_SmartWatch_ex.Enqueue(refined_Data);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                throw;
            }

           
        }

        public void Start_SaveSteamingData_Batch()
        {
            switch (Curren_EX_Type)
            {
                case Experiment_Type.EX_01:
                    WriteSteamingData_Batch(ref Queue_ex, "");
                    break;

                case Experiment_Type.EX_02:
                    WriteSteamingData_Batch(ref Queue_ex, "");
                    break;

                case Experiment_Type.EX_03:
                    //SceneManager.LoadScene("04 KINL Software 01 Terminated");
                    //WriteSteamingData_Batch(ref Queue_ex, "");

                    WriteSteamingData_Batch_SmartWatch(ref Queue_SmartWatch_ex);
                    WriteSteamingData_Batch(ref Queue_ex, "OptiTrackSkeleton");
                    //SceneManager.LoadScene("04 KINL Software 01 Terminated");
                    break;

                case Experiment_Type.EX_04:
                    SceneManager.LoadScene("04 KINL Software 01 Terminated");
                    WriteSteamingData_Batch(ref Queue_ex, "");
                    break;

                case Experiment_Type.EX_05:
                    WriteSteamingData_Batch_SmartWatch(ref Queue_SmartWatch_ex);
                    WriteSteamingData_Batch(ref Queue_ex, "OptiTrackSkeleton");
                   // SceneManager.LoadScene("04 KINL Software 01 Terminated");
                    break;

                case Experiment_Type.EX_06:
                    CuttingSteamingData_Batch(Queue_SmartWatch_ex);
                    WriteSteamingData_Batch_SmartWatch(ref Queue_SmartWatch_ex);
                    WriteSteamingData_Batch(ref Queue_ex, "OptiTrackSkeleton");
                    //SceneManager.LoadScene("04 KINL Software 01 Terminated");
                    break;
            }
        }

        int startMotion_index = 0;
        int endMotion_index = 0;
        public void CuttingSteamingData_Batch(Queue<string> _Queue_ex)
        {
            int maxCount = _Queue_ex.Count;

            for (int i = 0; i < maxCount; i++)
            {
                string[] splitData = _Queue_ex.Dequeue().Split(',');

                if(splitData[2].Contains("Recording Starts"))
                {
                    startMotion_index = i;
                }
                else if (splitData[2].Contains("Recording Stops"))
                {
                    endMotion_index = i;
                }
            }

        }

        public bool WriteSteamingData_Batch(ref Queue<string> _Queue_ex, string _additionalFileName)
        {
            bool tempb = false;

            try
            {
                string tempFileName = Curren_EX_Type.ToString() + "_" + _additionalFileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = _Queue_ex.Count;

                Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    while (_Queue_ex.Count != 0)
                    {
                        for (int i = 0; i < totalCountoftheQueue; i++)
                        {
                            string stringData = _Queue_ex.Dequeue();

                            if (stringData.Length > 0)
                            {
                                if (!isCategoryPrinted)
                                {
                                    switch (Curren_EX_Type)
                                    {
                                        case Experiment_Type.EX_01:
                                            str_DataCategory = "Date,Timestamp,"
                                                + "IMU_0_Acc_x_axis(g),IMU_0_Acc_y_axis(g),IMU_0_Acc_z_axis(g),"
                                                + "IMU_0_AngularVel_x_axis(deg/sec),IMU_0_AngularVel_y_axis(deg/sec),IMU_0_AngularVel_z_axis(deg/sec),"
                                                + "IMU_1_Acc_x_axis(g),IMU_1_Acc_y_axis(g),IMU_1_Acc_z_axis(g),"
                                                + "IMU_1_AngularVel_x_axis(deg/sec),IMU_1_AngularVel_y_axis(deg/sec),IMU_1_AngularVel_z_axis(deg/sec)"
                                                + ",Total Entry Count," + totalCountoftheQueue;
                                            break;

                                        case Experiment_Type.EX_02:
                                            str_DataCategory = "Date,Timestamp,"
                                                + "Hip_pos_x,Hip_pos_y,Hip_pos_z,Hip_orientation_Quat_w,Hip_orientation_Quat_x,Hip_orientation_Quat_y,Hip_orientation_Quat_z,Hip_orientation_Euler_x,Hip_orientation_Euler_y,Hip_orientation_Euler_z," +
                                   "Spine_pos_x,Spine_pos_y,Spine_pos_z,Spine_orientation_Quat_w,Spine_orientation_Quat_x,Spine_orientation_Quat_y,Spine_orientation_Quat_z,Spine_orientation_Euler_x,Spine_orientation_Euler_y,Spine_orientation_Euler_z," +
                                   "Chest_pos_x,Chest_pos_y,Chest_pos_z,Chest_orientation_Quat_w,Chest_orientation_Quat_x,Chest_orientation_Quat_y,Chest_orientation_Quat_z,Chest_orientation_Euler_x,Chest_orientation_Euler_y,Chest_orientation_Euler_z," +
                                   "Neck_pos_x,Neck_pos_y,Neck_pos_z,Neck_orientation_Quat_w,Neck_orientation_Quat_x,Neck_orientation_Quat_y,Neck_orientation_Quat_z,Neck_orientation_Euler_x,Neck_orientation_Euler_y,Neck_orientation_Euler_z," +
                                   "Head_pos_x,Head_pos_y,Head_pos_z,Head_orientation_Quat_w,Head_orientation_Quat_x,Head_orientation_Quat_y,Head_orientation_Quat_z,Head_orientation_Euler_x,Head_orientation_Euler_y,Head_orientation_Euler_z," +
                                   "LeftShoulder_pos_x,LeftShoulder_pos_y,LeftShoulder_pos_z,LeftShoulder_orientation_Quat_w,LeftShoulder_orientation_Quat_x,LeftShoulder_orientation_Quat_y,LeftShoulder_orientation_Quat_z,LeftShoulder_orientation_Euler_x,LeftShoulder_orientation_Euler_y,LeftShoulder_orientation_Euler_z," +
                                   "LeftUpperArm_pos_x,LeftUpperArm_pos_y,LeftUpperArm_pos_z,LeftUpperArm_orientation_Quat_w,LeftUpperArm_orientation_Quat_x,LeftUpperArm_orientation_Quat_y,LeftUpperArm_orientation_Quat_z,LeftUpperArm_orientation_Euler_x,LeftUpperArm_orientation_Euler_y,LeftUpperArm_orientation_Euler_z," +
                                   "LeftLowerArm_pos_x,LeftLowerArm_pos_y,LeftLowerArm_pos_z,LeftLowerArm_orientation_Quat_w,LeftLowerArm_orientation_Quat_x,LeftLowerArm_orientation_Quat_y,LeftLowerArm_orientation_Quat_z,LeftLowerArm_orientation_Euler_x,LeftLowerArm_orientation_Euler_y,LeftLowerArm_orientation_Euler_z," +
                                   "LeftHand_pos_x,LeftHand_pos_y,LeftHand_pos_z,LeftHand_orientation_Quat_w,LeftHand_orientation_Quat_x,LeftHand_orientation_Quat_y,LeftHand_orientation_Quat_z,LeftHand_orientation_Euler_x,LeftHand_orientation_Euler_y,LeftHand_orientation_Euler_z," +
                                   "RightShoulder_pos_x,RightShoulder_pos_y,RightShoulder_pos_z,RightShoulder_orientation_Quat_w,RightShoulder_orientation_Quat_x,RightShoulder_orientation_Quat_y,RightShoulder_orientation_Quat_z,RightShoulder_orientation_Euler_x,RightShoulder_orientation_Euler_y,RightShoulder_orientation_Euler_z," +
                                   "RightUpperArm_pos_x,RightUpperArm_pos_y,RightUpperArm_pos_z,RightUpperArm_orientation_Quat_w,RightUpperArm_orientation_Quat_x,RightUpperArm_orientation_Quat_y,RightUpperArm_orientation_Quat_z,RightUpperArm_orientation_Euler_x,RightUpperArm_orientation_Euler_y,RightUpperArm_orientation_Euler_z," +
                                   "RightLowerArm_pos_x,RightLowerArm_pos_y,RightLowerArm_pos_z,RightLowerArm_orientation_Quat_w,RightLowerArm_orientation_Quat_x,RightLowerArm_orientation_Quat_y,RightLowerArm_orientation_Quat_z,RightLowerArm_orientation_Euler_x,RightLowerArm_orientation_Euler_y,RightLowerArm_orientation_Euler_z," +
                                   "RightHand_pos_x,RightHand_pos_y,RightHand_pos_z,RightHand_orientation_Quat_w,RightHand_orientation_Quat_x,RightHand_orientation_Quat_y,RightHand_orientation_Quat_z,RightHand_orientation_Euler_x,RightHand_orientation_Euler_y,RightHand_orientation_Euler_z," +
                                   "LeftUpperLeg_pos_x,LeftUpperLeg_pos_y,LeftUpperLeg_pos_z,LeftUpperLeg_orientation_Quat_w,LeftUpperLeg_orientation_Quat_x,LeftUpperLeg_orientation_Quat_y,LeftUpperLeg_orientation_Quat_z,LeftUpperLeg_orientation_Euler_x,LeftUpperLeg_orientation_Euler_y,LeftUpperLeg_orientation_Euler_z," +
                                   "LeftLowerLeg_pos_x,LeftLowerLeg_pos_y,LeftLowerLeg_pos_z,LeftLowerLeg_orientation_Quat_w,LeftLowerLeg_orientation_Quat_x,LeftLowerLeg_orientation_Quat_y,LeftLowerLeg_orientation_Quat_z,LeftLowerLeg_orientation_Euler_x,LeftLowerLeg_orientation_Euler_y,LeftLowerLeg_orientation_Euler_z," +
                                   "LeftFoot_pos_x,LeftFoot_pos_y,LeftFoot_pos_z,LeftFoot_orientation_Quat_w,LeftFoot_orientation_Quat_x,LeftFoot_orientation_Quat_y,LeftFoot_orientation_Quat_z,LeftFoot_orientation_Euler_x,LeftFoot_orientation_Euler_y,LeftFoot_orientation_Euler_z," +
                                   "RightUpperLeg_pos_x,RightUpperLeg_pos_y,RightUpperLeg_pos_z,RightUpperLeg_orientation_Quat_w,RightUpperLeg_orientation_Quat_x,RightUpperLeg_orientation_Quat_y,RightUpperLeg_orientation_Quat_z,RightUpperLeg_orientation_Euler_x,RightUpperLeg_orientation_Euler_y,RightUpperLeg_orientation_Euler_z," +
                                   "RightLowerLeg_pos_x,RightLowerLeg_pos_y,RightLowerLeg_pos_z,RightLowerLeg_orientation_Quat_w,RightLowerLeg_orientation_Quat_x,RightLowerLeg_orientation_Quat_y,RightLowerLeg_orientation_Quat_z,RightLowerLeg_orientation_Euler_x,RightLowerLeg_orientation_Euler_y,RightLowerLeg_orientation_Euler_z," +
                                   "RightFoot_pos_x,RightFoot_pos_y,RightFoot_pos_z,RightFoot_orientation_Quat_w,RightFoot_orientation_Quat_x,RightFoot_orientation_Quat_y,RightFoot_orientation_Quat_z,RightFoot_orientation_Euler_x,RightFoot_orientation_Euler_y,RightFoot_orientation_Euler_z," +
                                   "LeftToeBase_pos_x,LeftToeBase_pos_y,LeftToeBase_pos_z,LeftToeBase_orientation_Quat_w,LeftToeBase_orientation_Quat_x,LeftToeBase_orientation_Quat_y,LeftToeBase_orientation_Quat_z,LeftToeBase_orientation_Euler_x,LeftToeBase_orientation_Euler_y,LeftToeBase_orientation_Euler_z," +
                                   "RightToeBase_pos_x,RightToeBase_pos_y,RightToeBase_pos_z,RightToeBase_orientation_Quat_w,RightToeBase_orientation_Quat_x,RightToeBase_orientation_Quat_y,RightToeBase_orientation_Quat_z,RightToeBase_orientation_Euler_x,RightToeBase_orientation_Euler_y,RightToeBase_orientation_Euler_z" + ",Total Entry Count," + totalCountoftheQueue;
                                            break;

                                        case Experiment_Type.EX_03:
                                            string catestr = string.Empty;

                                            for (int j = 0; j < OptiTrack_MarkerCount; j++)
                                            {
                                                catestr += "Marker_" + j + "_Pos_x,Marker_" + j + "_Pos_y,Marker_" + j + "_Pos_z,";
                                            }

                                            str_DataCategory = "Date,Timestamp,"
                                                + "IMU_0_Acc_x_axis(g),IMU_0_Acc_y_axis(g),IMU_0_Acc_z_axis(g),"
                                                + "IMU_0_AngularVel_x_axis(deg/sec),IMU_0_AngularVel_y_axis(deg/sec),IMU_0_AngularVel_z_axis(deg/sec),"
                                                + "IMU_1_Acc_x_axis(g),IMU_1_Acc_y_axis(g),IMU_1_Acc_z_axis(g),"
                                                + "IMU_1_AngularVel_x_axis(deg/sec),IMU_1_AngularVel_y_axis(deg/sec),IMU_1_AngularVel_z_axis(deg/sec),"
                                                + "||"
                                                + catestr
                                                + "Total Entry Count," + totalCountoftheQueue;
                                            //Debug.Log(OptiTrack_MarkerCount + "  " + catestr);
                                            break;


                                        case Experiment_Type.EX_04:
                                            str_DataCategory = "Date,Timestamp,"
                                               + "AK_Shoulder_pos_x,AK_Shoulder_pos_y,AK_Shoulder_pos_z,"
                                               + "AK_Shoulder_rot_x,AK_Shoulder_rot_y,AK_Shoulder_rot_z,"
                                               + "AK_Shoulder_trajectory_x,AK_Shoulder_trajectory_y,AK_Shoulder_trajectory_z,"
                                               + "AK_Elbow_pos_x,AK_Elbow_pos_y,AK_Elbow_pos_z,"
                                               + "AK_Elbow_rot_x,AK_Elbow_rot_y,AK_Elbow_rot_z,"
                                               + "AK_Elbow_trajectory_x,AK_Elbow_trajectory_y,AK_Elbow_trajectory_z,"
                                               + "AK_Wrist_pos_x,AK_Wrist_pos_y,AK_Wrist_pos_z,"
                                               + "AK_Wrist_rot_x,AK_Wrist_rot_y,AK_Wrist_rot_z,"
                                               + "AK_Wrist_trajectory_x,AK_Wrist_trajectory_y,AK_Wrist_trajectory_z,"

                                               + "OT_Shoulder_pos_x,OT_Shoulder_pos_y,OT_Shoulder_pos_z,"
                                               + "OT_Shoulder_rot_x,OT_Shoulder_rot_y,OT_Shoulder_rot_z,"
                                               + "OT_Shoulder_trajectory_x,OT_Shoulder_trajectory_y,OT_Shoulder_trajectory_z,"
                                               + "OT_Elbow_pos_x,OT_Elbow_pos_y,OT_Elbow_pos_z,"
                                               + "OT_Elbow_rot_x,OT_Elbow_rot_y,OT_Elbow_rot_z,"
                                               + "OT_Elbow_trajectory_x,OT_Elbow_trajectory_y,OT_Elbow_trajectory_z,"
                                               + "OT_Wrist_pos_x,OT_Wrist_pos_y,OT_Wrist_pos_z,"
                                               + "OT_Wrist_rot_x,OT_Wrist_rot_y,OT_Wrist_rot_z,"
                                               + "OT_Wrist_trajectory_x,OT_Wrist_trajectory_y,OT_Wrist_trajectory_z,"
                                               + "Total Entry Count," + totalCountoftheQueue;
                                            break;


                                        case Experiment_Type.EX_05:
                                        case Experiment_Type.EX_06:
                                            str_DataCategory = "Date,Timestamp,"
                                                + "EventLog,"
                                                + "IMU_0_Acc_x_axis(g),IMU_0_Acc_y_axis(g),IMU_0_Acc_z_axis(g),"
                                                + "IMU_0_AngularVel_x_axis(deg/sec),IMU_0_AngularVel_y_axis(deg/sec),IMU_0_AngularVel_z_axis(deg/sec),"
                                                + "IMU_1_Acc_x_axis(g),IMU_1_Acc_y_axis(g),IMU_1_Acc_z_axis(g),"
                                                + "IMU_1_AngularVel_x_axis(deg/sec),IMU_1_AngularVel_y_axis(deg/sec),IMU_1_AngularVel_z_axis(deg/sec),"
                                                + "Hip_pos_x,Hip_pos_y,Hip_pos_z,Hip_orientation_Quat_w,Hip_orientation_Quat_x,Hip_orientation_Quat_y,Hip_orientation_Quat_z,Hip_orientation_Euler_x,Hip_orientation_Euler_y,Hip_orientation_Euler_z," +
                                   "Spine_pos_x,Spine_pos_y,Spine_pos_z,Spine_orientation_Quat_w,Spine_orientation_Quat_x,Spine_orientation_Quat_y,Spine_orientation_Quat_z,Spine_orientation_Euler_x,Spine_orientation_Euler_y,Spine_orientation_Euler_z," +
                                   "Chest_pos_x,Chest_pos_y,Chest_pos_z,Chest_orientation_Quat_w,Chest_orientation_Quat_x,Chest_orientation_Quat_y,Chest_orientation_Quat_z,Chest_orientation_Euler_x,Chest_orientation_Euler_y,Chest_orientation_Euler_z," +
                                   "Neck_pos_x,Neck_pos_y,Neck_pos_z,Neck_orientation_Quat_w,Neck_orientation_Quat_x,Neck_orientation_Quat_y,Neck_orientation_Quat_z,Neck_orientation_Euler_x,Neck_orientation_Euler_y,Neck_orientation_Euler_z," +
                                   "Head_pos_x,Head_pos_y,Head_pos_z,Head_orientation_Quat_w,Head_orientation_Quat_x,Head_orientation_Quat_y,Head_orientation_Quat_z,Head_orientation_Euler_x,Head_orientation_Euler_y,Head_orientation_Euler_z," +
                                   "LeftShoulder_pos_x,LeftShoulder_pos_y,LeftShoulder_pos_z,LeftShoulder_orientation_Quat_w,LeftShoulder_orientation_Quat_x,LeftShoulder_orientation_Quat_y,LeftShoulder_orientation_Quat_z,LeftShoulder_orientation_Euler_x,LeftShoulder_orientation_Euler_y,LeftShoulder_orientation_Euler_z," +
                                   "LeftUpperArm_pos_x,LeftUpperArm_pos_y,LeftUpperArm_pos_z,LeftUpperArm_orientation_Quat_w,LeftUpperArm_orientation_Quat_x,LeftUpperArm_orientation_Quat_y,LeftUpperArm_orientation_Quat_z,LeftUpperArm_orientation_Euler_x,LeftUpperArm_orientation_Euler_y,LeftUpperArm_orientation_Euler_z," +
                                   "LeftLowerArm_pos_x,LeftLowerArm_pos_y,LeftLowerArm_pos_z,LeftLowerArm_orientation_Quat_w,LeftLowerArm_orientation_Quat_x,LeftLowerArm_orientation_Quat_y,LeftLowerArm_orientation_Quat_z,LeftLowerArm_orientation_Euler_x,LeftLowerArm_orientation_Euler_y,LeftLowerArm_orientation_Euler_z," +
                                   "LeftHand_pos_x,LeftHand_pos_y,LeftHand_pos_z,LeftHand_orientation_Quat_w,LeftHand_orientation_Quat_x,LeftHand_orientation_Quat_y,LeftHand_orientation_Quat_z,LeftHand_orientation_Euler_x,LeftHand_orientation_Euler_y,LeftHand_orientation_Euler_z," +
                                   "RightShoulder_pos_x,RightShoulder_pos_y,RightShoulder_pos_z,RightShoulder_orientation_Quat_w,RightShoulder_orientation_Quat_x,RightShoulder_orientation_Quat_y,RightShoulder_orientation_Quat_z,RightShoulder_orientation_Euler_x,RightShoulder_orientation_Euler_y,RightShoulder_orientation_Euler_z," +
                                   "RightUpperArm_pos_x,RightUpperArm_pos_y,RightUpperArm_pos_z,RightUpperArm_orientation_Quat_w,RightUpperArm_orientation_Quat_x,RightUpperArm_orientation_Quat_y,RightUpperArm_orientation_Quat_z,RightUpperArm_orientation_Euler_x,RightUpperArm_orientation_Euler_y,RightUpperArm_orientation_Euler_z," +
                                   "RightLowerArm_pos_x,RightLowerArm_pos_y,RightLowerArm_pos_z,RightLowerArm_orientation_Quat_w,RightLowerArm_orientation_Quat_x,RightLowerArm_orientation_Quat_y,RightLowerArm_orientation_Quat_z,RightLowerArm_orientation_Euler_x,RightLowerArm_orientation_Euler_y,RightLowerArm_orientation_Euler_z," +
                                   "RightHand_pos_x,RightHand_pos_y,RightHand_pos_z,RightHand_orientation_Quat_w,RightHand_orientation_Quat_x,RightHand_orientation_Quat_y,RightHand_orientation_Quat_z,RightHand_orientation_Euler_x,RightHand_orientation_Euler_y,RightHand_orientation_Euler_z," +
                                   "LeftUpperLeg_pos_x,LeftUpperLeg_pos_y,LeftUpperLeg_pos_z,LeftUpperLeg_orientation_Quat_w,LeftUpperLeg_orientation_Quat_x,LeftUpperLeg_orientation_Quat_y,LeftUpperLeg_orientation_Quat_z,LeftUpperLeg_orientation_Euler_x,LeftUpperLeg_orientation_Euler_y,LeftUpperLeg_orientation_Euler_z," +
                                   "LeftLowerLeg_pos_x,LeftLowerLeg_pos_y,LeftLowerLeg_pos_z,LeftLowerLeg_orientation_Quat_w,LeftLowerLeg_orientation_Quat_x,LeftLowerLeg_orientation_Quat_y,LeftLowerLeg_orientation_Quat_z,LeftLowerLeg_orientation_Euler_x,LeftLowerLeg_orientation_Euler_y,LeftLowerLeg_orientation_Euler_z," +
                                   "LeftFoot_pos_x,LeftFoot_pos_y,LeftFoot_pos_z,LeftFoot_orientation_Quat_w,LeftFoot_orientation_Quat_x,LeftFoot_orientation_Quat_y,LeftFoot_orientation_Quat_z,LeftFoot_orientation_Euler_x,LeftFoot_orientation_Euler_y,LeftFoot_orientation_Euler_z," +
                                   "RightUpperLeg_pos_x,RightUpperLeg_pos_y,RightUpperLeg_pos_z,RightUpperLeg_orientation_Quat_w,RightUpperLeg_orientation_Quat_x,RightUpperLeg_orientation_Quat_y,RightUpperLeg_orientation_Quat_z,RightUpperLeg_orientation_Euler_x,RightUpperLeg_orientation_Euler_y,RightUpperLeg_orientation_Euler_z," +
                                   "RightLowerLeg_pos_x,RightLowerLeg_pos_y,RightLowerLeg_pos_z,RightLowerLeg_orientation_Quat_w,RightLowerLeg_orientation_Quat_x,RightLowerLeg_orientation_Quat_y,RightLowerLeg_orientation_Quat_z,RightLowerLeg_orientation_Euler_x,RightLowerLeg_orientation_Euler_y,RightLowerLeg_orientation_Euler_z," +
                                   "RightFoot_pos_x,RightFoot_pos_y,RightFoot_pos_z,RightFoot_orientation_Quat_w,RightFoot_orientation_Quat_x,RightFoot_orientation_Quat_y,RightFoot_orientation_Quat_z,RightFoot_orientation_Euler_x,RightFoot_orientation_Euler_y,RightFoot_orientation_Euler_z," +
                                   "LeftToeBase_pos_x,LeftToeBase_pos_y,LeftToeBase_pos_z,LeftToeBase_orientation_Quat_w,LeftToeBase_orientation_Quat_x,LeftToeBase_orientation_Quat_y,LeftToeBase_orientation_Quat_z,LeftToeBase_orientation_Euler_x,LeftToeBase_orientation_Euler_y,LeftToeBase_orientation_Euler_z," +
                                   "RightToeBase_pos_x,RightToeBase_pos_y,RightToeBase_pos_z,RightToeBase_orientation_Quat_w,RightToeBase_orientation_Quat_x,RightToeBase_orientation_Quat_y,RightToeBase_orientation_Quat_z,RightToeBase_orientation_Euler_x,RightToeBase_orientation_Euler_y,RightToeBase_orientation_Euler_z" + ",Total Entry Count," + totalCountoftheQueue;
                                            break;
                                    }
                                    streamWriter.WriteLine(str_DataCategory);
                                    isCategoryPrinted = true;
                                }

                                streamWriter.WriteLine(stringData);
                            }
                        }
                    }
                }

                tempb = true;

                text_MsgDP.text += "\n" + "Saved OptiTrack + mbient Data. Count : " + totalCountoftheQueue;

                StartCoroutine(CheckSavingDataCompleted());
            }
            catch (Exception e)
            {
                Debug.Log("WriteSteamingData_BatchProcessing ERROR : " + e);
                TCPTestClient.instance.Send_Message_Index("\n" + "WriteSteamingData_BatchProcessing ERROR : " + e, 0);
            }

            return tempb;
        }

        bool isCategoryPrinted_SW = false;

        public bool WriteSteamingData_Batch_SmartWatch(ref Queue<string> _Queue_ex)
        {
            bool tempb = false;

            try
            {
                TCP_PTP_srv.instance.Pull_Data();


                string tempFileName = Curren_EX_Type.ToString() + "_" + "SmartWatch" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = _Queue_ex.Count;

                Debug.Log("Saving Watch Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    while (_Queue_ex.Count != 0)
                    {
                        Debug.Log("Queue Count + + + " +totalCountoftheQueue);
                        for (int i = 0; i < totalCountoftheQueue; i++)
                        {
                            string stringData = _Queue_ex.Dequeue();
                            if (stringData.Length > 0)
                            {
                                if (!isCategoryPrinted_SW)
                                {
                                    switch (Curren_EX_Type)
                                    {
                                        case Experiment_Type.EX_03:
                                            str_DataCategory =
                                                "TimeStamp_SmartWatch_Acc,TimeStamp_SmartWatch_Gyro,"
                                                + "Acc_X,Acc_Y,Acc_Z,"
                                                + "Gyro_X,Gyro_Y,Gyro_Z,"
                                                + "Total Entry Count," + totalCountoftheQueue;
                                            break;

                                        case Experiment_Type.EX_05:
                                            str_DataCategory =
                                           "TimeStamp_SmartWatch_Acc,TimeStamp_SmartWatch_Gyro,"
                                               + "Acc_X,Acc_Y,Acc_Z,"
                                               + "Gyro_X,Gyro_Y,Gyro_Z,"
                                               + "Total Entry Count," + totalCountoftheQueue;
                                            break;
                                        case Experiment_Type.EX_06:
                                            str_DataCategory =
                                                "TimeStamp_SmartWatch_Acc,TimeStamp_SmartWatch_Gyro,"
                                                + "Acc_X,Acc_Y,Acc_Z,"
                                                + "Gyro_X,Gyro_Y,Gyro_Z,"
                                                + "Total Entry Count," + totalCountoftheQueue;
                                            break;
                                    }
                                    streamWriter.WriteLine(str_DataCategory);
                                    isCategoryPrinted_SW = true;
                                }

                                streamWriter.WriteLine(stringData);
                            }
                        }
                    }
                }

                tempb = true;

                text_MsgDP.text += "\n" + "Saved Watch Data. Count : " + totalCountoftheQueue;

                StartCoroutine(CheckSavingDataCompleted());
            }
            catch (Exception e)
            {
                Debug.Log("WriteSteamingData_BatchProcessing ERROR : " + e);
                TCPTestClient.instance.Send_Message_Index("\n" + "WriteSteamingData_BatchProcessing ERROR : " + e, 0);
            }

            return tempb;
        }

        //public bool WriteSteamingData_Batch_IMU(IMU_SensorData_Type sensorData_Type, ref Queue<string> _SensorDataqueue, string MAC_Address, int _sensorIndex)
        //{
        //    bool tempb = false;

        //    try
        //    {
        //        string file_Location = string.Empty;

        //        string tempFileName_acc = fileName + "_" + _sensorIndex + "_ACC.txt";
        //        string tempFileName_gyr = fileName + "_" + _sensorIndex + "_GYR.txt";
        //        string tempFileName_mag = fileName + "_" + _sensorIndex + "_MAG.txt";

        //        switch (sensorData_Type)
        //        {
        //            case IMU_SensorData_Type.acceleration:
        //                file_Location = System.IO.Path.Combine(folder_Path, tempFileName_acc);
        //                break;
        //            case IMU_SensorData_Type.angularVelocity:
        //                file_Location = System.IO.Path.Combine(folder_Path, tempFileName_gyr);
        //                break;
        //            case IMU_SensorData_Type.magneticField:
        //                file_Location = System.IO.Path.Combine(folder_Path, tempFileName_mag);
        //                break;
        //        }

        //        int totalCountoftheQueue = _SensorDataqueue.Count;


        //        using (StreamWriter streamWriter = File.AppendText(file_Location))
        //        {
        //            while (_SensorDataqueue.Count != 0)
        //            {
        //                for (int i = 0; i < totalCountoftheQueue; i++)
        //                {
        //                    string stringData = _SensorDataqueue.Dequeue();

        //                    if (stringData.Length > 0)
        //                    {
        //                        switch (sensorData_Type)
        //                        {
        //                            case IMU_SensorData_Type.acceleration:
        //                                if (!isCategoryPrinted_acc[_sensorIndex])
        //                                {
        //                                    str_DataCategory = "Date,x-axis(g),y-axis(g),z-axis(g)," + MAC_Address + ",Total Entry Count," + totalCountoftheQueue;
        //                                    isCategoryPrinted_acc[_sensorIndex] = true;
        //                                    streamWriter.WriteLine(str_DataCategory);
        //                                }
        //                                break;
        //                            case IMU_SensorData_Type.angularVelocity:
        //                                if (!isCategoryPrinted_gyr[_sensorIndex])
        //                                {
        //                                    str_DataCategory = "Date,x-axis(deg/sec),y-axis(deg/sec),z-axis(deg/sec)," + MAC_Address + ",Total Entry Count," + totalCountoftheQueue;
        //                                    isCategoryPrinted_gyr[_sensorIndex] = true;
        //                                    streamWriter.WriteLine(str_DataCategory);
        //                                }
        //                                break;
        //                            case IMU_SensorData_Type.magneticField:
        //                                if (!isCategoryPrinted_mag[_sensorIndex])
        //                                {
        //                                    str_DataCategory = "Date,x-axis(deg/sec),y-axis(deg/sec),z-axis(deg/sec)," + MAC_Address + ",Total Entry Count," + totalCountoftheQueue;
        //                                    isCategoryPrinted_mag[_sensorIndex] = true;
        //                                    streamWriter.WriteLine(str_DataCategory);
        //                                }
        //                                break;
        //                        }
        //                        streamWriter.WriteLine(stringData);
        //                    }
        //                }
        //            }
        //        }


        //        tempb = true;
        //        //Debug.Log("File Path : " + file_Location); TextCtrl.instance.AddText("\n" + "BRP_MetaMotion : " + "File Path - " + file_Location);
        //        Debug.Log(sensorData_Type.ToString() + " - FILEWRITE is finished");
        //        //TextCtrl.instance.AddText("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + "BRP_MetaMotion : " + MAC_Address + " " + sensorData_Type.ToString() + "  FILE_WRITE is finished." + " (Total Data Entry Count : " + totalCountoftheQueue + " / ");
        //        TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + "BRP_MetaMotion : " + MAC_Address + " (Sensor Index : " + _sensorIndex + ") / Saving "
        //            + sensorData_Type.ToString() + " data is finished. (Entry Count >> " + totalCountoftheQueue + ")", 0);

        //        //ex04_CheckSavedData.Add("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " : " + "BRP_MetaMotion : " + MAC_Address + " / " + "Sensor Index : " + _sensorIndex + " / Saving "
        //        //    + sensorData_Type.ToString() + " data is finished.");

        //        IMU_DataFile_SavedCount++;

        //        if (IMU_DataFile_SavedCount / 3 == _KINLAB_IMU.IMU_Management.instance.SensorCount_Max)
        //        {
        //            StartCoroutine(CheckSavingDataCompleted());
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log("WriteSteamingData_BatchProcessing ERROR : " + e);
        //        TCPTestClient.instance.Send_Message_Index("\n" + "WriteSteamingData_BatchProcessing ERROR : " + e, 0);
        //    }

        //    return tempb;
        //}


        //public void WriteLoggedData_IMU(IMU_SensorData_Type sensorData_Type, ref Queue<string> queue, string MAC_Address, int _sensorIndex)
        //{
        //    if(Curren_EX_Type == Experiment_Type.EX_02)
        //    {
        //        string file_Location = string.Empty;

        //        string tempFileName_acc = fileName + "_" + _sensorIndex + "_ACC.txt";
        //        string tempFileName_gyr = fileName + "_" + _sensorIndex + "_GYR.txt";
        //        string tempFileName_mag = fileName + "_" + _sensorIndex + "_MAG.txt";

        //        switch (sensorData_Type)
        //        {
        //            case IMU_SensorData_Type.acceleration:
        //                file_Location = System.IO.Path.Combine(folder_Path, tempFileName_acc);
        //                break;
        //            case IMU_SensorData_Type.angularVelocity:
        //                file_Location = System.IO.Path.Combine(folder_Path, tempFileName_gyr);
        //                break;
        //            case IMU_SensorData_Type.magneticField:
        //                file_Location = System.IO.Path.Combine(folder_Path, tempFileName_mag);
        //                break;
        //        }

        //        for (int i = 0; i < queue.Count; i++)
        //        {
        //            string stringData = queue.Dequeue();

        //            if (stringData.Length > 0)
        //            {
        //                using (StreamWriter streamWriter = File.AppendText(file_Location))
        //                {
        //                    switch (sensorData_Type)
        //                    {
        //                        case IMU_SensorData_Type.acceleration:
        //                            if (!isCategoryPrinted_acc[_sensorIndex])
        //                            {
        //                                str_DataCategory = "Date,x-axis(g),y-axis(g),z-axis(g)," + MAC_Address;
        //                                isCategoryPrinted_acc[_sensorIndex] = true;
        //                                streamWriter.WriteLine(str_DataCategory);
        //                            }
        //                            break;
        //                        case IMU_SensorData_Type.angularVelocity:
        //                            if (!isCategoryPrinted_gyr[_sensorIndex])
        //                            {
        //                                str_DataCategory = "Date,x-axis(deg/sec),y-axis(deg/sec),z-axis(deg/sec)," + MAC_Address;
        //                                isCategoryPrinted_gyr[_sensorIndex] = true;
        //                                streamWriter.WriteLine(str_DataCategory);
        //                            }
        //                            break;
        //                        case IMU_SensorData_Type.magneticField:
        //                            if (!isCategoryPrinted_mag[_sensorIndex])
        //                            {
        //                                str_DataCategory = "Date,x-axis(deg/sec),y-axis(deg/sec),z-axis(deg/sec)," + MAC_Address;
        //                                isCategoryPrinted_mag[_sensorIndex] = true;
        //                                streamWriter.WriteLine(str_DataCategory);
        //                            }
        //                            break;
        //                    }
        //                    streamWriter.WriteLine(stringData);
        //                }
        //            }
        //        }

        //        Debug.Log("File Path : " + file_Location); //TextCtrl.instance.AddText("\n" + "BRP_MetaMotion : " + "File Path - " + file_Location);
        //        Debug.Log("FILEWRITE is finished"); //TextCtrl.instance.AddText("\n" + "BRP_MetaMotion : " + "FILE_WRITE is finished");
        //    }
        //}

        public void Write_Warning(Warning_Type _warning, string _additionalMessage)
        {
            string warning_Message = string.Empty;

            switch (_warning)
            {
                case Warning_Type.Ex_Start:
                    warning_Message = "[Motion Starts Now]";
                    Debug.Log("[Sqaut Motion Starts Now]");
                    TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " : " + warning_Message, 0);
                    break;

                case Warning_Type.Ex_End:
                    warning_Message = "[Motion Ends Now]";
                    Debug.Log("[Sqaut Motion Ends Now]");
                    TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " : " + warning_Message, 0);
                    break;

                case Warning_Type.AutoStart:
                    warning_Message = "[Auto Starting]";
                    Debug.Log(warning_Message);
                    text_MsgDP.text += "\n" + warning_Message;
                    break;

                case Warning_Type.AutoStop:
                    warning_Message = "[Auto Stopping]";
                    Debug.Log(warning_Message);
                    text_MsgDP.text += "\n" + warning_Message;
                    break;

                case Warning_Type.RecordStart:
                    warning_Message = "[Recording Starts Now !!]";
                    Debug.Log(warning_Message);
                    text_MsgDP.text += "\n" + warning_Message;
                    break;

                case Warning_Type.RecordStop:
                    warning_Message = "[Recording Stops Now !!]";
                    Debug.Log(warning_Message);
                    text_MsgDP.text += "\n" + warning_Message;
                    break;

                case Warning_Type.Score:
                    warning_Message = "[Score : " + _additionalMessage  +" point]";
                    Debug.Log(warning_Message);
                    text_MsgDP.text += "\n" + warning_Message;
                    break;
            }

            Enequeue_Data(warning_Message);
        }

        private IEnumerator CheckSavingDataCompleted()
        {

            switch (Curren_EX_Type)
            {
                case Experiment_Type.EX_03:

                    for (int i = 0; i < 10; i++)
                    {
                        _KINLAB_IMU.IMU_Management.instance.Disconnect_IMU_Sensors();

                        yield return new WaitForSeconds(3.0f);
                        TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " : " + "All the data are saved.)", 0);
                        //TempDF.instance.Func01();
                    }

                    break;
            }



            yield break;
        }

       


        public void EnDic_MarkerDic(int _markerID, Vector3 _makerPos)
        {
            if(m_MarkerDic.ContainsKey(_markerID))
            {
                m_MarkerDic[_markerID].Pos = _makerPos;
                m_MarkerDic[_markerID].isUpdated = true;

            }
            else
            {
                S_MakerData markerData = new S_MakerData(_markerID, _makerPos, true);
                m_MarkerDic.Add(_markerID, markerData);
            }
        }

        public void Check_MarkerDataState()
        {
            foreach (var value in m_MarkerDic.Values)
            {
                if(value.isUpdated)
                {
                    value.isUpdated = false;
                }
                else
                {
                    value.isUpdated = false;
                    value.Pos = Vector3.zero;

                }
            }

            //SortBubble_MarkerDic();
            //Record_Data();
        }

        public void SortBubble_MarkerDic()
        {
            m_markerIDList.Clear();

            foreach (var key in m_MarkerDic.Keys)
            {
                m_markerIDList.Add(key);
            }

            BubbleSort(ref m_markerIDList);
        }

        private void BubbleSort(ref List<int> _dataArray)
        {
            int temp_bs;

            for (int i = 0; i < (_dataArray.Count - 1); i++)
            {
                for (int j = 0; j < (_dataArray.Count - 1) - i; j++)
                {
                    if (_dataArray[j] > _dataArray[j + 1])
                    {
                        temp_bs = _dataArray[j];
                        _dataArray[j] = _dataArray[j + 1];
                        _dataArray[j + 1] = temp_bs;
                    }

                }
            }
        }

        public void Debug_MarkerMisfire(string _markerArrange)
        {
            Enequeue_Data(_markerArrange);
        }


        public void Record_Data_IMU_OptiTrackMarker()
        {
            if (!bRecording)
                return;

            StringBuilder sb = new StringBuilder();

            sb.Append(',');

            OptiTrack_markers_Pos.Clear();

            if (isOptiTrackStreamingMode)
            {
                SortBubble_MarkerDic();

                for (int i = 0; i < m_markerIDList.Count; i++)
                {
                    OptiTrack_markers_Pos.Add(m_MarkerDic[m_markerIDList[i]].Pos);
                }

                OptiTrack_MarkerCount = OptiTrack_markers_Pos.Count;


                //if (markerCount == 20 && IMU_Manager.instance.sensors.Count == 2 && IMU_Manager.instance.sensors[0].isStreaming == true && IMU_Manager.instance.sensors[1].isStreaming == true)
                if (true)
                {
                    //Debug.Log("B : " );

                    Vector3 IMU_Sensor_0_Acc = Vector3.zero;
                    Vector3 IMU_Sensor_0_AngularVec = Vector3.zero;

                    Vector3 IMU_Sensor_1_Acc = Vector3.zero;
                    Vector3 IMU_Sensor_1_AngularVec = Vector3.zero;

                    if (_KINLAB_IMU.IMU_Management.instance.IsStreamingMode)
                    {
                        IMU_Sensor_0_Acc = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(0, IMU_SensorData_Type.acceleration);
                        IMU_Sensor_0_AngularVec = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(0, IMU_SensorData_Type.angularVelocity);

                        IMU_Sensor_1_Acc = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(1, IMU_SensorData_Type.acceleration);
                        IMU_Sensor_1_AngularVec = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(1, IMU_SensorData_Type.angularVelocity);
                    }


                    sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.x).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.y).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.z).Append(',');

                    sb.AppendFormat("{0:F4}", IMU_Sensor_0_AngularVec.x).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_0_AngularVec.y).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_0_AngularVec.z).Append(',');

                    sb.AppendFormat("{0:F4}", IMU_Sensor_1_Acc.x).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_1_Acc.y).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_1_Acc.z).Append(',');

                    sb.AppendFormat("{0:F4}", IMU_Sensor_1_AngularVec.x).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_1_AngularVec.y).Append(',');
                    sb.AppendFormat("{0:F4}", IMU_Sensor_1_AngularVec.z).Append(',');
                }

                ///off
                //sb.Append("||");

                for (int i = 0; i < OptiTrack_MarkerCount; i++)
                {
                    sb.AppendFormat("{0:F6}", -OptiTrack_markers_Pos[i].x).Append(',');
                    sb.AppendFormat("{0:F6}", OptiTrack_markers_Pos[i].y).Append(',');
                    sb.AppendFormat("{0:F6}", OptiTrack_markers_Pos[i].z).Append(',');
                }

                if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                Enequeue_Data(sb.ToString());

            }
        }

        public void Record_Data_IMU_OptiTrackSkel(string sb_SkeltonData)
        {
            if (!bRecording)
                return;

            StringBuilder sb = new StringBuilder();

            sb.Append(',');

            //if (isOptiTrackStreamingMode)
            {

                Vector3 IMU_Sensor_0_Acc = Vector3.zero;
                Vector3 IMU_Sensor_0_AngularVec = Vector3.zero;

                Vector3 IMU_Sensor_1_Acc = Vector3.zero;
                Vector3 IMU_Sensor_1_AngularVec = Vector3.zero;

                if (_KINLAB_IMU.IMU_Management.instance.IsStreamingMode)
                {
                    IMU_Sensor_0_Acc = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(0, IMU_SensorData_Type.acceleration);
                    IMU_Sensor_0_AngularVec = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(0, IMU_SensorData_Type.angularVelocity);

                    IMU_Sensor_1_Acc = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(1, IMU_SensorData_Type.acceleration);
                    IMU_Sensor_1_AngularVec = _KINLAB_IMU.IMU_Management.instance.Get_IMU_Current_SensorData_by_Index(1, IMU_SensorData_Type.angularVelocity);
                }


                sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.x).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.y).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.z).Append(',');

                sb.AppendFormat("{0:F4}", IMU_Sensor_0_AngularVec.x).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_0_AngularVec.y).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_0_AngularVec.z).Append(',');

                sb.AppendFormat("{0:F4}", IMU_Sensor_1_Acc.x).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_1_Acc.y).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_1_Acc.z).Append(',');

                sb.AppendFormat("{0:F4}", IMU_Sensor_1_AngularVec.x).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_1_AngularVec.y).Append(',');
                sb.AppendFormat("{0:F4}", IMU_Sensor_1_AngularVec.z).Append(',');


                sb.Append(sb_SkeltonData);


                if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                Enequeue_Data(sb.ToString());

            }
        }

        public void Record_KinectSkel_OptiTrackSkel()
        {
            if (!bRecording)
                return;

            StringBuilder sb = new StringBuilder();

            sb.Append(',');

            for (int i = 0; i < List_Current_Kinect_Joints.Count; i++)
            {
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.position.x).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.position.y).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.position.z).Append(',');

                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.rotation.eulerAngles.x).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.rotation.eulerAngles.y).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.rotation.eulerAngles.z).Append(',');

                //Debug.Log(List_prev_Kinect_Joints_pos[i].x.ToString("F4"));

                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.position.x - List_prev_Kinect_Joints_pos[i].x).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.position.y - List_prev_Kinect_Joints_pos[i].y).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_Kinect_Joints[i].transform.position.z - List_prev_Kinect_Joints_pos[i].z).Append(',');

                List_prev_Kinect_Joints_pos[i] = List_Current_Kinect_Joints[i].transform.position;
            }

            for (int i = 0; i < List_Current_OptiTrack_Joints.Count; i++)
            {
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.position.x).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.position.y).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.position.z).Append(',');

                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.rotation.eulerAngles.x).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.rotation.eulerAngles.y).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.rotation.eulerAngles.z).Append(',');

                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.position.x - List_prev_OptiTrack_Joints_pos[i].x).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.position.y - List_prev_OptiTrack_Joints_pos[i].y).Append(',');
                sb.AppendFormat("{0:F4}", List_Current_OptiTrack_Joints[i].transform.position.z - List_prev_OptiTrack_Joints_pos[i].z).Append(',');

                List_prev_OptiTrack_Joints_pos[i] = List_Current_OptiTrack_Joints[i].transform.position;
            }



            if (sb.Length > 0 && sb[sb.Length - 1] == ',')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            Enequeue_Data(sb.ToString());
        }

        private IEnumerator Coru_AutoStartStop()
        {
            if (bAutoproceLock)
                yield break;

            bAutoproceLock = true;

            if (tempToggle_Automatic)
            {
                tempToggle_Automatic = false;
                Write_Warning(Warning_Type.AutoStop, "");

                Debug.Log("OptiTrack/Tizen Streaming Mode is Deactivated");

                TCP_PTP_srv.instance.Send_Message_to_slaveClock(Enum_IPC_Message.Stop_TizenData);
                TCP_PTP_srv.instance.Send_Message_to_slaveClock(Enum_IPC_Message.Stop_FossilData);
                Write_Warning(Warning_Type.RecordStop, "");
          //      _KINLAB_IMU.IMU_Management.instance.Start_Streaming_IMU_Sensors();
                bRecording = false;
                isOptiTrackStreamingMode = false;
             //   GM_Sound.instance.SoundEffect_by_Index(transform.position, 1);

                yield return new WaitForSeconds(2.0f);

                Start_SaveSteamingData_Batch();

                isCategoryPrinted = false;
                isCategoryPrinted_SW = false;
            }
            else
            {
                tempToggle_Automatic = true;
                Write_Warning(Warning_Type.AutoStart, "");

                TCP_PTP_srv.instance.Send_Message_to_slaveClock(Enum_IPC_Message.SyncMasterSlaveClockTime);
                GM_Sound.instance.SoundEffect_by_Index(transform.position, 2);
                yield return new WaitForSeconds(4.0f);

                if(TCP_PTP_srv.instance.TimeOffset.TotalSeconds != 0.0f)
                {
                    TCP_PTP_srv.instance.Send_Message_to_slaveClock(Enum_IPC_Message.Start_TizenData);
                    TCP_PTP_srv.instance.Send_Message_to_slaveClock(Enum_IPC_Message.Start_FossilData);
                    //    _KINLAB_IMU.IMU_Management.instance.Start_Streaming_IMU_Sensors();
                    yield return new WaitForSeconds(1.0f);

                    Write_Warning(Warning_Type.RecordStart, "");
                    isOptiTrackStreamingMode = true;
                    bRecording = true;
                //    GM_Sound.instance.SoundEffect_by_Index(transform.position, 0);
                    Debug.Log("OptiTrack/SmartWatch Mode is Activated");
                }
                else
                {
                    Debug.LogError("***** Please start again by press 'S' Button");
                }


            }

            bAutoproceLock = false;

            yield break;
        }

        public void Update_ConsoleText(string _text)
        {
            text_MsgDP.text += "\n" + _text;
        }
    }
}