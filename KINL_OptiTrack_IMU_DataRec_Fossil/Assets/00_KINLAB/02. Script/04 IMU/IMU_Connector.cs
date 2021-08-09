using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using System.Threading;



namespace _KINLAB
{
    namespace _KINLAB_IMU
    {
        public class IMU_Connector : MonoBehaviour
        {
            private string tempstr = string.Empty;

            private List<Thread> IMU_connect_threads;

            Thread IMU_connect_thread01;
            Thread IMU_connect_thread02;
            Thread IMU_connect_thread03;


            //-------------------------------------

            private void Awake()
            {
                IMU_connect_threads = new List<Thread>();

                IMU_connect_threads.Add(IMU_connect_thread01);
                IMU_connect_threads.Add(IMU_connect_thread02);
                IMU_connect_threads.Add(IMU_connect_thread03);
            }

            private void Start()
            {
                #region 20200114 1551
                StartCoroutine(Corou_Run_Thread_for_IMU_Connection());
                #endregion
            }

            private void OnDisable()
            {
                //foreach (var item in IMU_connect_threads)
                //{
                //    if (item.IsAlive)
                //    {
                //        item.Abort();
                //    }
                //}

                if (IMU_connect_thread01.IsAlive)
                {
                    IMU_connect_thread01.Abort();
                }

                if (IMU_connect_thread02.IsAlive)
                {
                    IMU_connect_thread02.Abort();
                }

                if (IMU_connect_thread03.IsAlive)
                {
                    IMU_connect_thread03.Abort();
                }
            }

            private IEnumerator Corou_Run_Thread_for_IMU_Connection()
            {
                for (int i = 0; i < Opti_IMU_SettingPage_Management.instance.dic_Selected_IMU_Sensor_MAC_Address.Count; i++)
                {
                    if (!IMU_Management.instance.Check_IMU_Connection_State(i))
                    {
                        Run_Thread_for_IMU_Connection_by_Index(i);
                        //yield return new WaitForSeconds(1.0f);

                        if (!IMU_Management.instance.Check_IMU_Connection_State(i))
                        {

                            Run_Thread_for_IMU_Connection_by_Index(i);
                            //yield return new WaitForSeconds(1.0f);

                            if (!IMU_Management.instance.Check_IMU_Connection_State(i))
                            {
                                Run_Thread_for_IMU_Connection_by_Index(i);
                            }
                        }
                    }
                }

                yield break;
            }

            private void Run_Thread_for_IMU_Connection_by_Index(int _index)
            {
                IMU_connect_threads[_index] = new Thread(new ParameterizedThreadStart(Initialize_IMU_Sensor_by_Index));
                IMU_connect_threads[_index].Priority = System.Threading.ThreadPriority.Highest;
                IMU_connect_threads[_index].Start(_index);
                IMU_connect_threads[_index].Join();
            }

            private void Initialize_IMU_Sensor_by_Index(object _index)
            {
                string mpstr = IMU_Management.instance.sensors[(int)_index].Connect_IMU_Sensor().Result;
            }
        }
    }
}