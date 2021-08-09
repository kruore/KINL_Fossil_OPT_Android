using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

namespace _KINLAB
{
    public class GM_DataLoader : MonoBehaviour
    {
        public static GM_DataLoader instance = null;

        private string rootpath = string.Empty;
        private string folder_Path = string.Empty;
        private string folderName = "3DCL_DataLog";

        //private string fileName = "Experiment_03_DataLog_20200406173146";
        private string fileName = string.Empty;
        private StreamReader fileReader = null;

        private bool isDataLoaded = false;

        private string string_loaded_Data = string.Empty;

        private Queue<string> queue_IMU_pre_data = new Queue<string>();

        private List<string> list_optiTrack_pre_CSV_data = new List<string>();
        private Queue<string> queue_optiTrack_data_copy = null;
        private string pre_raw_CSV_optiTrack_Category = string.Empty;

        private Dictionary<int, List<Vector3>> dic_optiTrack_data_origin = new Dictionary<int, List<Vector3>>();
        public Dictionary<int, List<Vector3>> Dic_optiTrack_data_origin { get { return dic_optiTrack_data_origin; } }
        //private Dictionary<int, List<Vector3>> dic_optiTrack_data_copy = null;

        private Dictionary<int, List<bool>> dic_optiTrack_data_bool_origin = new Dictionary<int, List<bool>>();
        public Dictionary<int, List<bool>> Dic_optiTrack_data_bool_origin { get { return dic_optiTrack_data_bool_origin; } }

        private int pre_raw_CSV_marker_Count = 0;
        public int Pre_raw_CSV_marker_Count { get { return pre_raw_CSV_marker_Count; } }

        private float startTime = 0.0f;
        private float currentTime = 0.0f;

        private bool temb = false;

        private int queue_optiTrack_data_copy_EntryCount = 0;
        public int Queue_optiTrack_data_copy_EntryCount { get { return queue_optiTrack_data_copy_EntryCount; } }

        [SerializeField]
        private GameObject marker_Render_Sphere_Pref;
        public GameObject Marker_Render_Sphere_Pref { get { return marker_Render_Sphere_Pref; } }

        [SerializeField]
        private GameObject nNmarker_Render_Sphere_Pref;
        public GameObject NNmarker_Render_Sphere_Pref { get { return nNmarker_Render_Sphere_Pref; } }

        [SerializeField]
        private GameObject time_Slider;
        public GameObject Time_Slider { get { return time_Slider; } }

        [SerializeField]
        private GameObject section_Slider01;
        public GameObject Section_Slider01 { get { return section_Slider01; } }

        [SerializeField]
        private GameObject section_Slider02;
        public GameObject Section_Slider02 { get { return section_Slider02; } }

        [SerializeField]
        private GameObject markerInfo_Display_Panel;
        public GameObject MarkerInfo_Display_Panel { get { return markerInfo_Display_Panel; } }

        [SerializeField]
        private GameObject markerInfo_Display_button_pref;
        public GameObject MarkerInfo_Display_button_pref { get { return markerInfo_Display_button_pref; } }


        [SerializeField]
        private GameObject each_MarkerInfopanel_pref;
        public GameObject Each_MarkerInfopanel_pref { get { return each_MarkerInfopanel_pref; } }

        [SerializeField]
        private GameObject each_MarkerInfoTitle_pref;
        public GameObject Each_MarkerInfoTitle_pref { get { return each_MarkerInfoTitle_pref; } }

        [SerializeField]
        private Material selectedMaterial;
        public Material SelectedMaterial { get { return selectedMaterial; } }

        [SerializeField]
        private GameObject marker_Set_DropDown;
        public GameObject Marker_Set_DropDown { get { return marker_Set_DropDown; } }

        [SerializeField]
        private GameObject marker_Nicknaming_Table;
        public GameObject Marker_Nicknaming_Table { get { return marker_Nicknaming_Table; } }

        [SerializeField]
        private List<GameObject> list_Nickname_Table_Set_button = new List<GameObject>();
        public List<GameObject> List_Nickname_Table_Set_button { get { return list_Nickname_Table_Set_button; } }



        private bool isCategoryPrinted = false;
        

        //------------------------------------------------------

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            
        }

        private void Start()
        {
            rootpath = Directory.GetCurrentDirectory();

            fileName = DataEdit_SettingPage_Management.instance.fileNameforLoading;

            //Debug.Log(Application.targetFrameRate.ToString());
            //Application.targetFrameRate = 250;
            //Debug.Log(Application.targetFrameRate.ToString());
        }

        private void OnDisable()
        {
            DisposeFileReader();
            Directory.SetCurrentDirectory(rootpath);
            Debug.Log("GM_DataEditor is Disable.");
        }

        private void Update()
        {


            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("Start loading csv data...");

                Start_Load_MarkerData();
            }
        }


        public void Start_Load_MarkerData()
        {
            if (!isDataLoaded && fileName != string.Empty)
            {
                string tempFileName = fileName + ".txt";
                //rootpath = Directory.GetCurrentDirectory();
                folder_Path = System.IO.Path.Combine(rootpath, folderName);
                Directory.SetCurrentDirectory(folder_Path);
                string file_Location = System.IO.Path.Combine(folder_Path, tempFileName);

                if (fileName.Length == 0 || !File.Exists(tempFileName))
                {
                    return;
                }

                fileReader = new StreamReader(tempFileName);



                //StartCoroutine(Read_CSV_Data_LbyL());
                Read_CSV_Data_LbyL();
                
            }

        }

        private void DisposeFileReader()
        {
            if (fileReader != null)
            {
                fileReader.Dispose();
                fileReader = null;
            }
        }



        //private IEnumerator Read_CSV_Data_LbyL()
        private void Read_CSV_Data_LbyL()
        {
            if (fileReader == null)
            {
                //yield break;
            }

            startTime = currentTime = Time.time;

            bool endOfFile = false;

            queue_IMU_pre_data.Clear();
            list_optiTrack_pre_CSV_data.Clear();

            while (!endOfFile)
            {

                string_loaded_Data = fileReader.ReadLine();

                if (string_loaded_Data == null)
                {
                    Debug.Log("endOfFile");
                    endOfFile = true;
                }
                else
                {
                    char[] delimiters = { '|' };
                    string[] splitted_string_loaded_Data = string_loaded_Data.Split(delimiters);

                    if (splitted_string_loaded_Data.Length >= 3)
                    {
                        queue_IMU_pre_data.Enqueue(splitted_string_loaded_Data[0]);
                        list_optiTrack_pre_CSV_data.Add(splitted_string_loaded_Data[2]);

                        if(!temb)
                        {
                            temb = true;
                            Debug.Log(splitted_string_loaded_Data[2]);
                        }

                    }
                }

                //yield return new WaitForEndOfFrame(); // Entry : 1067 -> 5.0632 sec
                //yield return new WaitForFixedUpdate(); // Entry : 1067 -> 21.4210 sec
                //yield return null;
            }

            queue_optiTrack_data_copy = new Queue<string>(list_optiTrack_pre_CSV_data);
            Debug.Log("queue_optiTrack_data_editing");

            pre_raw_CSV_optiTrack_Category = queue_optiTrack_data_copy.Dequeue();

            char[] dellimiters_comma = { ',' };
            string[] splitted_optiTrack_Data = pre_raw_CSV_optiTrack_Category.Split(dellimiters_comma);

            pre_raw_CSV_marker_Count = (splitted_optiTrack_Data.Length - 2) / 3;
            Debug.Log("pre_raw_CSV_marker_Count : " + pre_raw_CSV_marker_Count);

            queue_optiTrack_data_copy_EntryCount = queue_optiTrack_data_copy.Count;
            //markerInfo_Display_Panel.GetComponent<RectTransform>().right = new Vector3(-queue_optiTrack_data_copy_EntryCount, 10 * pre_raw_CSV_marker_Count);
            //markerInfo_Display_Panel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, queue_optiTrack_data_copy_EntryCount);

            for (int i = 0; i < queue_optiTrack_data_copy_EntryCount; i++)
            {
                splitted_optiTrack_Data = queue_optiTrack_data_copy.Dequeue().Split(dellimiters_comma);

                int current_DataArray_Marker_Count = (splitted_optiTrack_Data.Length / 3);
                //여기서 pre_raw_CSV_marker_Count & current_DataArray_Marker_Count 이친구 비교해서, current_DataArray_Marker_Count이 친구가 작으면 나머지에는 vector.zero 넣자
                //gm_editor쪽에서는 해당 벡터값이 제로벡터일 때, 구슬 렌더링 꺼주면 될듯
                int current_DataArray_Marker_Count_Comp = pre_raw_CSV_marker_Count - current_DataArray_Marker_Count;
                //Debug.Log("current_DataArray_Marker_Count_Comp : " + current_DataArray_Marker_Count_Comp);

                int markerIndex = 0;



                for (int j = 0; j < pre_raw_CSV_marker_Count; j++)
                {
                    List<Vector3> tempVec_List = new List<Vector3>();
                    List<bool> tempBool_List = new List<bool>();
                    

                    float pos_X = 0.0f; float pos_Y = 0.0f; float pos_Z = 0.0f;


                    if (current_DataArray_Marker_Count_Comp > 0 && j >= current_DataArray_Marker_Count)
                    {
                        if (dic_optiTrack_data_origin.ContainsKey(j))
                        {
                            dic_optiTrack_data_origin.TryGetValue(j, out tempVec_List);
                            tempVec_List.Add(Vector3.zero);

                            dic_optiTrack_data_bool_origin.TryGetValue(j, out tempBool_List);
                            tempBool_List.Add(false);
                        }
                        else
                        {
                            tempVec_List.Add(Vector3.zero);
                            dic_optiTrack_data_origin.Add(j, tempVec_List);

                            tempBool_List.Add(false);
                            dic_optiTrack_data_bool_origin.Add(j, tempBool_List);
                        }

                        continue;
                    }
                    else
                    {
                        if (splitted_optiTrack_Data[markerIndex] != null)
                            float.TryParse(splitted_optiTrack_Data[markerIndex], out pos_X);
                        if (splitted_optiTrack_Data[markerIndex] != null)
                            float.TryParse(splitted_optiTrack_Data[markerIndex + 1], out pos_Y);
                        if (splitted_optiTrack_Data[markerIndex] != null)
                            float.TryParse(splitted_optiTrack_Data[markerIndex + 2], out pos_Z);

                        //Debug.Log(j + " ||| " + pos_X + " " + pos_Y + " " + pos_Z);

                        Vector3 tempVV = new Vector3(-pos_X, pos_Y, pos_Z); /// Unity left-handed & Motive right-handed

                        if (dic_optiTrack_data_origin.ContainsKey(j))
                        {
                            dic_optiTrack_data_origin.TryGetValue(j, out tempVec_List);
                            tempVec_List.Add(tempVV);


                            dic_optiTrack_data_bool_origin.TryGetValue(j, out tempBool_List);
                            if (tempVV == Vector3.zero)
                            {
                                tempBool_List.Add(false);
                            }
                            else
                            {
                                tempBool_List.Add(true);
                            }
                        }
                        else
                        {
                            tempVec_List.Add(tempVV);
                            dic_optiTrack_data_origin.Add(j, tempVec_List);

                            if (tempVV == Vector3.zero)
                            {
                                tempBool_List.Add(false);
                            }
                            else
                            {
                                tempBool_List.Add(true);
                            }
                            dic_optiTrack_data_bool_origin.Add(j, tempBool_List);

                        }


                    }
                    markerIndex += 3;


                }
            }

            ///List in Dictionary - Data Unit Test
            //for (int i = 0; i < dic_optiTrack_data_origin.Count; i++)
            //{
            //    List<Vector3> temp_l = new List<Vector3>();

            //    dic_optiTrack_data_origin.TryGetValue(i, out temp_l);

            //    for (int j = 0; j < temp_l.Count; j++)
            //    {
            //        Debug.Log("MarkerIndex : " + i + " // SequenceNum : " + j + " // Vector : " + temp_l[j].x + " " + temp_l[j].y + " " + temp_l[j].z);
            //    }

            //}

            Directory.SetCurrentDirectory(rootpath);

            //dic_optiTrack_data_copy = new Dictionary<int, List<Vector3>>(dic_optiTrack_data_origin);
            gameObject.AddComponent<GM_DataEditor>();


            isDataLoaded = true;

            currentTime = Time.time;

            Debug.Log("END - Read_CSV_Data_LbyL // " + string.Format("{0:F4}", currentTime - startTime));
        }



        public void WriteEditedData_Batch(ref Queue<string> _EditedDataqueue, ref List<string> _NicknamedMarkerCount)
        {
            try
            {
                string savefileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_EditedData";

                string tempFileName = savefileName + ".txt";

                folder_Path = System.IO.Path.Combine(rootpath, folderName);
                Directory.SetCurrentDirectory(folder_Path);
                string file_Location = System.IO.Path.Combine(folder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = _EditedDataqueue.Count;

                queue_IMU_pre_data.Dequeue(); //delete category

                if (queue_IMU_pre_data.Count != totalCountoftheQueue)
                {
                    return;
                }

                Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    while (_EditedDataqueue.Count != 0)
                    {
                        for (int i = 0; i < totalCountoftheQueue; i++)
                        {
                            string editedData_inString = queue_IMU_pre_data.Dequeue() + _EditedDataqueue.Dequeue();

                            if (editedData_inString.Length > 0)
                            {
                                if (!isCategoryPrinted)
                                {


                                    string catestr = string.Empty;

                                    for (int j = 0; j < _NicknamedMarkerCount.Count; j++)
                                    {
                                        catestr += _NicknamedMarkerCount[j] + "_Pos_x," + _NicknamedMarkerCount[j] + "_Pos_y," + _NicknamedMarkerCount[j] + "_Pos_z,";
                                    }

                                    m_str_DataCategory = "Date,Timestamp,"
                                        + "IMU_0_Acc_x_axis(g),IMU_0_Acc_y_axis(g),IMU_0_Acc_z_axis(g),"
                                        + "IMU_0_AngularVel_x_axis(deg/sec),IMU_0_AngularVel_y_axis(deg/sec),IMU_0_AngularVel_z_axis(deg/sec),"
                                        + "IMU_1_Acc_x_axis(g),IMU_1_Acc_y_axis(g),IMU_1_Acc_z_axis(g),"
                                        + "IMU_1_AngularVel_x_axis(deg/sec),IMU_1_AngularVel_y_axis(deg/sec),IMU_1_AngularVel_z_axis(deg/sec),"
                                        + catestr
                                        + "Total Entry Count," + totalCountoftheQueue;
                                    //Debug.Log(OptiTrack_MarkerCount + "  " + catestr);

                                    streamWriter.WriteLine(m_str_DataCategory);
                                    isCategoryPrinted = true;
                                }

                            }

                            streamWriter.WriteLine(editedData_inString);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("WriteSteamingData_BatchProcessing ERROR : " + e);
                TCPTestClient.instance.Send_Message_Index("\n" + "WriteSteamingData_BatchProcessing ERROR : " + e, 0);
            }
            finally
            {
                Directory.SetCurrentDirectory(rootpath);
            }

        }

    }
}
