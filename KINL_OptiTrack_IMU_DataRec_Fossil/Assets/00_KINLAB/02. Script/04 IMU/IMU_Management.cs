using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MbientLab.MetaWear;
using MbientLab.MetaWear.NetStandard;
using System.Threading.Tasks;

using System;
using System.IO;

using MbientLab.MetaWear.Peripheral;

using MbientLab.MetaWear.Data;
using MbientLab.MetaWear.Sensor;
using MbientLab.MetaWear.Core;
using MbientLab.MetaWear.Impl;
using MbientLab.MetaWear.Builder;

using System.Threading;

//using MbientLab.Warble;
//using UnityEngine.UI;

namespace _KINLAB
{
    namespace _KINLAB_IMU
    {
        public class IMU_Management : MonoBehaviour
        {
            public List<IMU_Sensor> sensors;

            public static IMU_Management instance = null;

            private int sensorCount_Max = 3;
            public int SensorCount_Max { get { return sensorCount_Max; } }

            private int connectedSensorCount = 0;

            [SerializeField]
            public string teststr = string.Empty;

            private bool isIMUDataSaved = false;

            private bool isStreamingMode = false;
            public bool IsStreamingMode { get { return isStreamingMode; } }


            [SerializeField]
            private GameObject waiting_Text_01;

            [SerializeField]
            private GameObject waiting_Text_02;

            [SerializeField]
            private GameObject allSensorConnected_Text_01;

            //[HideInInspector]
            //public enum IMU_SensorData_Type { acceleration, angularVelocity, magneticField };
            //--------------------------------------------------------

            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;
                    DontDestroyOnLoad(gameObject);
                }

                sensors = new List<IMU_Sensor>();

                Create_IMU_Sensor();
            }

            private void Start()
            {

            }

            private void OnDisable()
            {
                for (int i = 0; i < sensorCount_Max; i++)
                {
                    if (sensors[i].isConnected)
                    {
                        string temp = sensors[i].Disconnect_IMU_Sensor().Result;
                    }
                }
            }

            private void OnApplicationQuit()
            {
                StopAllCoroutines();
            }

            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    Connect_IMU_Sensors();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    Disconnect_IMU_Sensors();
                }

                if (Input.GetKeyDown(KeyCode.C))
                {
                    TearDown_IMU_Sensors();
                }

                if (Input.GetKeyDown(KeyCode.Comma))
                {
                    Start_Streaming_IMU_Sensors();
                }

                if (Input.GetKeyDown(KeyCode.Period))
                {
                    Stop_Streaming_IMU_Sensors();
                }

                //if (Input.GetKeyDown(KeyCode.Slash))
                //{
                //    Save_Data_IMU_Sensors();
                //}
            }

            private void FixedUpdate()
            {
                //if(isStreamingMode)
                //{
                //    for (int i = 0; i < sensorCount_Max; i++)
                //    {
                //        if (sensors[i].isConnected)
                //        {
                //            Debug.Log(sensors[i].Get_Current_AngulVeloc().magnitude.ToString());
                //        }
                //    }
                //}
            }

            private void Create_IMU_Sensor()
            {
                sensorCount_Max = Opti_IMU_SettingPage_Management.instance.dic_Selected_IMU_Sensor_MAC_Address.Count;

                for (int i = 0; i < sensorCount_Max; i++)
                {
                    IMU_Sensor sensor = new IMU_Sensor(Opti_IMU_SettingPage_Management.instance.dic_Selected_IMU_Sensor_MAC_Address[i], i);
                    sensors.Add(sensor);
                }
            }

            private void Connect_IMU_Sensors()
            {

            }

            public void Disconnect_IMU_Sensors()
            {
                for (int i = 0; i < sensorCount_Max; i++)
                {
                    if (sensors[i].isConnected)
                    {
                        sensors[i].Disconnect_IMU_Sensor();
                    }
                }

            }

            private void TearDown_IMU_Sensors()
            {
                Debug.Log("TearDown_IMU_Sensor");
                for (int i = 0; i < sensorCount_Max; i++)
                {
                    sensors[i].TearDown_IMU_Sensor();
                }
            }

            public void Start_Streaming_IMU_Sensors()
            {
                if (!isStreamingMode)
                {
                    isStreamingMode = true;
                    for (int i = 0; i < sensorCount_Max; i++)
                    {
                        sensors[i].Activate_StreamingMode_IMU();
                    }
                }

            }

            public void Stop_Streaming_IMU_Sensors()
            {
                if (isStreamingMode)
                {
                    isStreamingMode = false;
                    for (int i = 0; i < sensorCount_Max; i++)
                    {
                        sensors[i].Deactivate_StreamingMode();
                    }
                }
            }

            public void Save_Data_IMU_Sensors()
            {
                if (!isIMUDataSaved)
                {
                    isIMUDataSaved = true;
                    //SceneManager.LoadScene("04 3DCL EVA Terminated");

                    //IMU
                    StartCoroutine(Coru_Save_Streaming_Data_on_Batch());
                }
            }

            public bool Check_IMU_Connection_State(int _index)
            {
                if (sensors[_index].isConnected)
                {
                    TCPTestClient.instance.Send_Message_Index("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + sensors[_index].p_MAc_Address + " is successfully connected.", 0);
                    TCPTestClient.instance.Send_Message_Index("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + sensors[_index].Check_State_IMU_Sensor(), 0);
                    Check_Connected_IMU_Sensor_Count();
                    return true;
                }
                else
                {
                    TCPTestClient.instance.Send_Message_Index("\n* Failed to connect to " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + sensors[_index].p_MAc_Address, 0);
                }

                return false;
            }

            private void Check_Connected_IMU_Sensor_Count()
            {
                connectedSensorCount++;

                if(connectedSensorCount == sensorCount_Max)
                {
                    waiting_Text_01.SetActive(false);
                    waiting_Text_02.SetActive(false);
                    allSensorConnected_Text_01.SetActive(true);
                }
            }


            private IEnumerator Coru_Save_Streaming_Data_on_Batch()
            {
                yield return new WaitForSeconds(2.0f);

                for (int i = 0; i < sensorCount_Max; i++)
                {
                    sensors[i].Save_Streaming_Data_On_Batch();
                }


                yield break;
            }

            public IMU_Sensor Get_Sensor_byIndex(int _index)
            {
                if (sensors[_index].isConnected)
                {
                    return sensors[_index];
                }

                return null;
            }

            public Vector3 Get_IMU_Current_SensorData_by_Index(int _index, IMU_SensorData_Type _dataType)
            {
                Vector3 sensorData = Vector3.zero;

                if(sensors[_index].isConnected)
                {
                    switch (_dataType)
                    {
                        case IMU_SensorData_Type.acceleration:
                            sensorData = sensors[_index].Get_Current_Acceleration();
                            break;

                        case IMU_SensorData_Type.angularVelocity:
                            sensorData =  sensors[_index].Get_Current_AngulVeloc();
                            break;

                        case IMU_SensorData_Type.magneticField:
                            sensorData = Vector3.zero;
                            break;
                    }

                    return sensorData;
                }
                else
                {
                    return Vector3.zero;
                }

            }

        }
    }
}