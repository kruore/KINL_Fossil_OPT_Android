using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MbientLab.MetaWear;
using MbientLab.MetaWear.NetStandard;
using MbientLab.MetaWear.Data;
using MbientLab.MetaWear.Sensor;
using MbientLab.MetaWear.Core;

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace _KINLAB
{
    namespace _KINLAB_IMU
    {
        public class IMU_Sensor
        {
            private IMetaWearBoard metawear = null;
            private IAccelerometer accelerometer = null;
            private IMagnetometerBmm150 magnetometer = null;
            private IGyroBmi160 gyro = null;
            private ILogging logger = null;

            public string p_MAc_Address { get { return MAC_Address; } }
            private string MAC_Address = string.Empty;

            public Queue<string> accelerometer_Log_Queue;
            public Queue<string> gyro_Log_Queue;
            public Queue<string> magnetometer_Log_Queue;

            public Queue<string> accelerometer_Steamming_Queue;
            public Queue<string> gyro_Steamming_Queue;
            public Queue<string> magnetometer_Steamming_Queue;

            public bool isConnected = false;
            public bool islogging = false;
            public bool isDownloadReady = false;
            public bool isStreaming = false;

            static private int sensorCount_Max;
            static public int sensorCount_Current;
            static private string _3DCL_string = "BRP_MetaMotion : ";

            static readonly object divisionlocker = new object();

            private int indexInSensors;

            private string str_batterycheck = string.Empty;

            private Vector3 current_AngulVeloc = Vector3.zero;
            private Vector3 current_Acceleration = Vector3.zero;
            //--------------------------------------------------------------------------------------------------------

            public IMU_Sensor(string _MAC_Address, int _indexInSensors)
            {
                MAC_Address = _MAC_Address;
                indexInSensors = _indexInSensors;

                accelerometer_Log_Queue = new Queue<string>();
                gyro_Log_Queue = new Queue<string>();
                magnetometer_Log_Queue = new Queue<string>();

                accelerometer_Steamming_Queue = new Queue<string>();
                gyro_Steamming_Queue = new Queue<string>();
                magnetometer_Steamming_Queue = new Queue<string>();

                isConnected = false;
                islogging = false;
                isDownloadReady = false;
                isStreaming = false;

                sensorCount_Max = 3;
                sensorCount_Current = 0;
            }

            public async Task<string> Connect_IMU_Sensor()
            {
                //bool lockTaken = false;
                //Monitor.Enter(divisionlocker, ref lockTaken);

                StringBuilder sb = new StringBuilder();

                try
                {
                    Debug.Log(_3DCL_string + MAC_Address + " <-- ...Trying Connecting...");
                    //TextCtrl.instance.AddText("\n" + _3DCL_string + MAC_Address + " <-- ...Trying Connecting..."); 
                    //sb.AppendLine(" <-- ...Trying Connecting...");

                    if (isConnected)
                    {
                        Debug.Log(_3DCL_string + MAC_Address + " is already connected");
                        //TextCtrl.instance.AddText("\n" + _3DCL_string + MAC_Address + " is already connected");
                        //sb.AppendLine(_3DCL_string + MAC_Address + " is already connected");
                    }
                    else
                    {
                        metawear = MbientLab.MetaWear.NetStandard.Application.GetMetaWearBoard(MAC_Address);
                        await metawear.InitializeAsync();

                        if (metawear.IsConnected)
                        {
                            isConnected = true;

                            ///
                            metawear.OnUnexpectedDisconnect = () =>
                            {
                                Debug.Log(_3DCL_string + metawear.MacAddress + " Unexpectedly lost connection");
                            };

                            //var deviceInfo = await sensors[index].metawear.ReadDeviceInformationAsync();
                            //Debug.Log(_3DCL_string + "MetaWear Device Information: " + deviceInfo); //mainText.text += "\n" + _3DCL_string + "MetaWear Device Information: " + deviceInfo;


                            ///
                            //ILed led;
                            //if ((led = metawear.GetModule<ILed>()) == null)
                            //{
                            //    Debug.Log(_3DCL_string + "LED module is present on this board");
                            //}
                            //led.Play();


                            ///
                            //Debug.Log(_3DCL_string + "MetaWear Device MAC Address: " + metawear.MacAddress);


                            ///
                            byte temp01 = await metawear.ReadBatteryLevelAsync();
                            Debug.Log(_3DCL_string + MAC_Address + " Device Battery Level: " + temp01 + "%");
                            str_batterycheck = MAC_Address + " is connected. / Device Battery Level " + temp01 + "%";


                            ///
                            accelerometer = metawear.GetModule<IAccelerometer>();
                            accelerometer.Configure(odr: 200f, range: 4f);
                            //Debug.Log(_3DCL_string + "Accelerometer is Ready.");


                            ///
                            gyro = metawear.GetModule<IGyroBmi160>();
                            gyro.Configure(odr: MbientLab.MetaWear.Sensor.GyroBmi160.OutputDataRate._200Hz, range: MbientLab.MetaWear.Sensor.GyroBmi160.DataRange._1000dps);
                            //Debug.Log(_3DCL_string + "Gyroscope is Ready."); //TextCtrl.instance.AddText(_3DCL_string + "Gyroscope is Ready.");


                            ///
                            magnetometer = metawear.GetModule<IMagnetometerBmm150>();
                            magnetometer.Configure(preset: MbientLab.MetaWear.Sensor.MagnetometerBmm150.Preset.HighAccuracy);
                            //Debug.Log(_3DCL_string + "Magnetometer is Ready.");


                            ///
                            //metawear.GetModule<ISettings>().EditBleConnParams(maxConnInterval: 7.5f);


                            ///
                            //logger = metawear.GetModule<ILogging>();
                            //logger.ClearEntries();
                            //Debug.Log(_3DCL_string + "Data Logger is Ready.");


                            ++sensorCount_Current;
                            Debug.Log(_3DCL_string + "Current Sensor Count : " + sensorCount_Current);

                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    //TextCtrl.instance.AddText("\n" + MAc_Address + " <-- Failed to connect it.");
                    //sb.AppendLine(_3DCL_string + MAC_Address + " <-- Failed to connect it.");
                }
                finally
                {
                    //if (lockTaken)
                    //    Monitor.Exit(divisionlocker);
                }


                return sb.ToString();
            }

            public string Check_State_IMU_Sensor()
            {
                return str_batterycheck;
            }


            public async Task<string> Disconnect_IMU_Sensor()
            {
                StringBuilder sb = new StringBuilder();

                try
                {
                    if (isConnected)
                    {
                        metawear.TearDown();
                        Debug.Log(MAC_Address + " <-- Teared Down.");
                        TCPTestClient.instance.Send_Message_Index("\n" + _3DCL_string + MAC_Address + " <-- Teared Down.", 0);

                        Debug.Log(MAC_Address + " <-- Trying Disconnecting...");
                        TCPTestClient.instance.Send_Message_Index("\n" + _3DCL_string + MAC_Address + " <-- Trying Disconnecting...", 0);

                        //await metawear.DisconnectAsync();
                        await metawear.GetModule<IDebug>().DisconnectAsync();

                        if (!metawear.IsConnected)
                        {
                            isConnected = false;
                            Debug.Log(MAC_Address + " <-- MetaWear Board is Disconnected ***");
                            TCPTestClient.instance.Send_Message_Index("\n" + _3DCL_string + MAC_Address + " <-- MetaWear Board is Disconnected ***", 0);

                            sensorCount_Current--;
                        }
                        else
                        {
                            Debug.Log(MAC_Address + "<-- Disconnecting is failed");
                            TCPTestClient.instance.Send_Message_Index("\n" + _3DCL_string + MAC_Address + "<-- Disconnecting is failed", 0);

                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    TCPTestClient.instance.Send_Message_Index("\n" + _3DCL_string + MAC_Address + " <-- Failed to Disconnect it.", 0);

                }
                finally
                {

                }

                return sb.ToString();
            }

            public void TearDown_IMU_Sensor()
            {
                try
                {
                    metawear.TearDown();
                    Debug.Log(MAC_Address + " <-- Teared Down.");
                    TCPTestClient.instance.Send_Message_Index("\n" + _3DCL_string + MAC_Address + " <-- Teared Down.", 0);
                    //TextCtrl.instance.AddText("\n" + _3DCL_string + MAC_Address + " <-- Teared Down.");
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                finally
                {

                }
            }

            public async Task Activate_StreamingMode_IMU()
            {
                //Monitor.Enter(divisionlocker);
                try
                {
                    if (isConnected)
                    {
                        if (!isStreaming)
                        {
                            metawear.GetModule<ISettings>().EditBleConnParams(maxConnInterval: 7.5f);

                            await accelerometer.PackedAcceleration.AddRouteAsync(source => source.Stream(data =>
                            {
                            //Debug.Log("Acceleration = " + data.Value<Acceleration>());
                            accelerometer_Steamming_Queue.Enqueue(string.Format("{0},{1},{2},{3}", data.FormattedTimestamp, data.Value<Acceleration>().X, data.Value<Acceleration>().Y, data.Value<Acceleration>().Z));
                                current_Acceleration.Set(data.Value<Acceleration>().X, data.Value<Acceleration>().Y, data.Value<Acceleration>().Z);
                            }
                            ));



                            await gyro.PackedAngularVelocity.AddRouteAsync(source => source.Stream(data =>
                            {
                                gyro_Steamming_Queue.Enqueue(string.Format("{0},{1},{2},{3}", data.FormattedTimestamp, data.Value<AngularVelocity>().X, data.Value<AngularVelocity>().Y, data.Value<AngularVelocity>().Z));
                                current_AngulVeloc.Set(data.Value<AngularVelocity>().X, data.Value<AngularVelocity>().Y, data.Value<AngularVelocity>().Z);
                            }
                            ));



                            await magnetometer.PackedMagneticField.AddRouteAsync(source => source.Stream(data =>
                            {
                                magnetometer_Steamming_Queue.Enqueue(string.Format("{0},{1},{2},{3}", data.FormattedTimestamp, data.Value<MagneticField>().X, data.Value<MagneticField>().Y, data.Value<MagneticField>().Z));
                            }
                            ));

                            accelerometer.PackedAcceleration.Start();
                            gyro.PackedAngularVelocity.Start();
                            magnetometer.PackedMagneticField.Start();

                            accelerometer.Start();
                            gyro.Start();
                            magnetometer.Start();

                            isStreaming = true;
                            TCPTestClient.instance.Send_Message_Index("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " activates streaming mode now.", 0);
                            //TextCtrl.instance.AddText("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " activates streaming mode now.");
                        }
                        else
                        {
                            TCPTestClient.instance.Send_Message_Index("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " is already on streaming mode.", 0);
                            //TextCtrl.instance.AddText("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " is already on streaming mode.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                finally
                {
                    //Monitor.Exit(divisionlocker);

                }
            }

            public async Task Deactivate_StreamingMode()
            {
                try
                {
                    if (isConnected)
                    {
                        if (isStreaming)
                        {
                            accelerometer.PackedAcceleration.Stop();
                            accelerometer.Stop();

                            gyro.PackedAngularVelocity.Stop();
                            gyro.Stop();

                            magnetometer.PackedMagneticField.Stop();
                            magnetometer.Stop();

                            isStreaming = false;
                            TCPTestClient.instance.Send_Message_Index("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " deactivates streaming mode now.", 0);
                            //TextCtrl.instance.AddText("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " deactivates streaming mode now.");
                        }
                        else
                        {
                            TCPTestClient.instance.Send_Message_Index("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " is not on streaming mode now.", 0);
                            //TextCtrl.instance.AddText("\n* BRP_MetaMotion : " + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " is not on streaming mode now.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                finally
                {

                }
            }

            public void Save_Streaming_Data_On_Batch()
            {
                if(GM_DataRecorder.instance != null)
                {
                    TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " ->> Start Downloading the Streaming-Data(IMU).", 0);

                    //off
                    //GM_DataRecorder.instance.WriteSteamingData_Batch_IMU(IMU_SensorData_Type.acceleration, ref accelerometer_Steamming_Queue, MAC_Address, indexInSensors);
                    //GM_DataRecorder.instance.WriteSteamingData_Batch_IMU(IMU_SensorData_Type.angularVelocity, ref gyro_Steamming_Queue, MAC_Address, indexInSensors);
                    //GM_DataRecorder.instance.WriteSteamingData_Batch_IMU(IMU_SensorData_Type.magneticField, ref magnetometer_Steamming_Queue, MAC_Address, indexInSensors);
                    TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " " + MAC_Address + " ->> Download the Streaming-Data(IMU) is completed.", 0);

                }
            }

            public Vector3 Get_Current_AngulVeloc()
            {
                return current_AngulVeloc;
            }

            public Vector3 Get_Current_Acceleration()
            {
                return current_Acceleration;
            }
        }
    }
}