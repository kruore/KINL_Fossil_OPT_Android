using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace _KINLAB
{
    public enum Enum_IPC_Message
    {
        Connect = 1,
        Disconnect = 2,
        ConnectFossil = 3,
        DisconnectFossil = 4,


        SyncMasterSlaveClockTime = 13,
        Start_TizenData = 12,
        Stop_TizenData = 32,

        Start_FossilData = 14,
        Stop_FossilData = 15,

        Transfer_ACC_SampleData = 22,
        Transfer_GYRO_SampleData = 222,
        Fossil_Transfer_ACC_SampleData = 33,
        Fossil_Transfer_GYRO_SampleData = 333

    }

    public class TCP_PTP_srv : MonoBehaviour
    {

        public static TCP_PTP_srv instance = null;

        //private int timeOffset = 0;
        //public int TimeOffset { get { return timeOffset; } }

        private TimeSpan timeOffset;
        public TimeSpan TimeOffset { get { return timeOffset; } }


        [SerializeField]
        private TextMeshProUGUI Text_Queue_Tizen;
        [SerializeField]
        private TextMeshProUGUI Text_Queue_Fossil;


        public enum Enum_Client_Category
        {
            SW = 1, //SmartWatch
        }

        public struct ClientProfile
        {
            public Socket clientSocket;
            public bool bBeingConnectedNow;
        }

        public class AsyncObject
        {
            public byte[] Buffer;
            public Socket WorkingSocket;
            public AsyncObject(int bufferSize)
            {
                this.Buffer = new byte[bufferSize];
            }
        }

        private Socket m_ServerSocket = null;
        // 클라이언트 소켓을 리스트로 변경
        private List<Socket> list_ClientSocket = new List<Socket>();

        //private Dictionary<string, ClientProfile> dic_SP_ClientProfile = new Dictionary<string, ClientProfile>();
        //private Dictionary<string, ClientProfile> dic_SW_ClientProfile = new Dictionary<string, ClientProfile>();

        private AsyncCallback m_fnReceiveHandler;
        private AsyncCallback m_fnSendHandler;
        private AsyncCallback m_fnAcceptHandler;

        //private int portNum = 1031;
        //private int portNum = 4519;
        private int portNum = 4545;

        private Queue<string> queue_Clients_Msg = new Queue<string>();

        //------------------------------------------------

        // 이부분은 유티티에서 처리할 부분때문에 만들어 놓은 부분.
        public static string msginfo = "";
        // 삽질의 영속성.


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
            StartServer(portNum);
        }

        void Update()
        {
            //if (queue_Clients_Msg.Count > 0)
            //{
            //    text_MsgDP.text += "\n" + queue_Clients_Msg.Dequeue();
            //}

            DecodeClientMsg();

            if (Input.GetKeyDown(KeyCode.F1))
            {
                Send_Message_to_slaveClock(Enum_IPC_Message.SyncMasterSlaveClockTime);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                Send_Message_to_slaveClock(Enum_IPC_Message.Start_TizenData);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                Send_Message_to_slaveClock(Enum_IPC_Message.Stop_TizenData);
            }


            if (Input.GetKeyDown(KeyCode.F4))
            {
                Send_Message_to_slaveClock(Enum_IPC_Message.Start_FossilData);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Send_Message_to_slaveClock(Enum_IPC_Message.Stop_FossilData);
            }

            //if (Input.GetKeyDown(KeyCode.F4))
            //{
            //    int quecount = acclist.Count > gyrolist.Count ? gyrolist.Count : acclist.Count;

            //    Debug.LogError(quecount.ToString() + " // " + acclist.Count + " // " + gyrolist.Count);

            //    for (int i = 0; i < quecount; i++)
            //    {
            //        //GM_DataRecorder.instance.Enequeue_Data(acclist[i] + "," + gyrolist[i]);
            //        GM_DataRecorder.instance.Enequeue_SmartWatchData(acclist[i], gyrolist[i], TimeOffset);
            //    }
            //}

        }
        private void OnDisable()
        {
            StopServer();
        }

        public void get_msg(string str)
        {
            msginfo = str;
        }
        public TCP_PTP_srv()
        {
            // 비동기 작업에 사용될 대리자를 초기화합니다.
            m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
            m_fnSendHandler = new AsyncCallback(handleDataSend);
            m_fnAcceptHandler = new AsyncCallback(handleClientConnectionRequest);
        }

        public void StartServer(int PortNum)
        {
            // TCP 통신을 위한 소켓을 생성합니다.
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //m_ServerSocket.NoDelay = true;

            // 특정 포트에서 모든 주소로부터 들어오는 연결을 받기 위해 포트를 바인딩합니다.
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, PortNum));
            // 연결 요청을 받기 시작합니다.
            get_msg("server start");

            //최대접속자 20
            m_ServerSocket.Listen(20);
            // null부분에 m_ServerSocket
            m_ServerSocket.BeginAccept(m_fnAcceptHandler, m_ServerSocket);
        }

        public void StopServer()
        {
            foreach (var item in list_ClientSocket)
            {
                item.Close();
            }

            m_ServerSocket.Close();
        }

        public void SendMessage(string message)
        {
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            // 다중 클라이언트에 한꺼번에 메세지 보내는 것으로 변경, 인덱스 따로 두면 개별로 보낼수 있음.
            foreach (Socket sk in list_ClientSocket)
            {
                AsyncObject ao = new AsyncObject(1);
                // 문자열을 바이트 배열으로 변환
                ao.Buffer = Encoding.UTF8.GetBytes(message);
                // 사용된 소켓을 저장
                ao.WorkingSocket = sk;
                // 전송 시작!
                try
                {
                    ao.WorkingSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
                }
                catch (SocketException e)
                {
                    get_msg(e.ToString());
                }
                catch (NullReferenceException ae)
                {
                    get_msg(ae.ToString());
                }
            }
        }

        private void handleClientConnectionRequest(IAsyncResult ar)
        {
            Debug.Log("aaaa");

            // 클라이언트의 연결 요청을 수락합니다.
            //Socket sockClientq = (Socket)ar.AsyncState;
            Socket sockClient = m_ServerSocket.EndAccept(ar);
            get_msg("new client : " + sockClient.RemoteEndPoint.ToString());
            //        // array[0] 는 IP 이고, array[1] 은 Port 입니다.


            // 새 클라이언트 추가
            list_ClientSocket.Add(sockClient);
            //Socket sockClient = sockClientq.EndAccept(ar);
            // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
            //AsyncObject ao = new AsyncObject(4096);
            AsyncObject ao = new AsyncObject(8096);
            // 작업 중인 소켓을 저장하기 위해 sockClient 할당
            ao.WorkingSocket = sockClient;
            // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
            sockClient.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            // 클라인언트 접속완료후 새 클라이언트 접속을 위해 다시 대기
            m_ServerSocket.BeginAccept(m_fnAcceptHandler, m_ServerSocket);
        }
        private void handleDataReceive(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            //string revTime = DateTime.Now.ToString(",HHmmssfff");

            // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
            int recvBytes = ao.WorkingSocket.EndReceive(ar);


            string revTime = DateTime.Now.ToString(",HHmmssfff");

            // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
            if (recvBytes > 0)
            {
                // 필요없는 공백 .Trim()으로 날림
                string tmpmsg = Encoding.UTF8.GetString(ao.Buffer).Trim('\0');
                get_msg("Rec-" + tmpmsg);


                string[] splitted_Tuple_loaded_Data = tmpmsg.Split(',');
                if (splitted_Tuple_loaded_Data.Length > 2)
                {
                    switch (splitted_Tuple_loaded_Data[1])
                    {
                        case "13":
                            tmpmsg += revTime;
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            Debug.Log(tmpmsg);
                            break;
                        case "1":
                            Debug.Log("Watch Was Connected!");
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            break;
                        case "3":
                            Debug.Log("Fossil Watch Was Connected!");
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            break;
                        case "22":
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            break;
                        case "222":
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            break;
                        case "33":
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            break;
                        case "333":
                            queue_Clients_Msg.Enqueue(tmpmsg);
                            break;
                    }

                    //if(splitted_Tuple_loaded_Data[0] == "11")
                    //{


                    //}
                    //else if (splitted_Tuple_loaded_Data[0] == "22"
                    //    || splitted_Tuple_loaded_Data[0] == "222")
                    //{

                    //}
                    //else
                    //{
                    //    Debug.Log("RawMsg : " + tmpmsg);
                    //}

                    //string[] splitted_Tuple_loaded_Data = tmpmsg.Split(';');

                    //foreach (string item in splitted_Tuple_loaded_Data)
                    //{
                    //    if (item.Length > 0)
                    //        queue_Clients_Msg.Enqueue(ao.WorkingSocket.RemoteEndPoint.ToString() + "," + item);
                    //}

                    //SendMessage("I Received It !");

                    // 버퍼처리 후 리셑
                    ao.Buffer = new byte[8096];
                }

                // 자료 처리가 끝났으면~
                // 이제 다시 데이터를 수신받기 위해서 수신 대기를 해야 합니다.
                // Begin~~ 메서드를 이용해 비동기적으로 작업을 대기했다면
                // 반드시 대리자 함수에서 End~~ 메서드를 이용해 비동기 작업이 끝났다고 알려줘야 합니다!
                ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
        }
        private void handleDataSend(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            SocketError error;
            // 자료를 전송하고, 전송한 바이트를 가져옵니다.
            int sentBytes = ao.WorkingSocket.EndSend(ar, out error);



            if (sentBytes > 0)
            {
                string tmpmsg = Encoding.UTF8.GetString(ao.Buffer).Trim('\0');
                //get_msg("Send-" + tmpmsg);

                Debug.Log(error.ToString() + " / " + tmpmsg);
            }
        }

        private void DecodeClientMsg()
        {
            if (queue_Clients_Msg.Count > 0)
            {
                //11,   165138021      ,165141266,    165138766    ,165138027
                // [0]    [1]               [2]         [3]          [4]


                // 유니티 서버가 이만큼의 데이터를 보낸다. 서버시각->타이젠이 받은 다음에 타이젠에 PTP_revTime에 넣고 받자마자 바로 리시브 하는데.
                // 받았던 서버 시간 + 리시브 타임 4+1 해서 [5] 까지를 SendTime에 붙인다
                // 클라이언트가 [6]까지 붙여서 보내서 서버가 [6]까지 확인한다.
                // 서버가 그 뒤 [7]까지 붙인다.
                // [4], [5].[6],[7]은 시간. 숭신시간, 받은시간, 보낸시간 ,다시 받은 시간
                // 이를 PTP로 계산한다. c의 void * recv_thread(Client client)를 통해
                //Connect,   13,    SW_Tizen,    JSB,   165138021      ,165141266,    165138766    ,165138027
                //  [0]      [1]     [2]        [3]       [4]             [5]           [6]           [7]

                try
                {
                    string tuple = queue_Clients_Msg.Dequeue();
                    Debug.Log(tuple);


                    string[] splitted_Tuple_loaded_Data_C = tuple.Split(';');

                    for (int j = 0; j < splitted_Tuple_loaded_Data_C.Length; j++)
                    {
                        string[] splitted_Tuple_loaded_Data3 = splitted_Tuple_loaded_Data_C[j].Split(',');

                        switch (splitted_Tuple_loaded_Data3[1])
                        {
                            case "1":
                                Debug.Log("Tizen Client Connect");
                                GM_DataRecorder.instance.Update_ConsoleText("Tizen Watch -> Connected");
                                break;

                            case "3":
                                Debug.Log("Fossil Client Connect");
                                GM_DataRecorder.instance.Update_ConsoleText("Fossil Watch -> Connected");
                                break;

                            case "33":
                                if (!GM_DataRecorder.instance.BRecording)
                                    break;

                                if (splitted_Tuple_loaded_Data3[1] == "33")
                                {
                                    acclist_Fossil.Add(splitted_Tuple_loaded_Data3[4]);

                                    Text_Queue_Fossil.text = "Fossil Count : " + acclist_Fossil.Count;
                                }
                                else
                                {
                                    gyrolist_Fossil.Add(splitted_Tuple_loaded_Data3[4]);
                                }
                                break;
                            case "333":
                                if (!GM_DataRecorder.instance.BRecording)
                                    break;

                                if (splitted_Tuple_loaded_Data3[1] == "33")
                                {
                                    acclist_Fossil.Add(splitted_Tuple_loaded_Data3[4]);

                                    Text_Queue_Fossil.text = "Fossil Count : " + gyrolist_Fossil.Count;
                                }
                                else
                                {
                                    gyrolist_Fossil.Add(splitted_Tuple_loaded_Data3[4]);
                                }
                                break;
                        }

                    }


                    string[] splitted_Tuple_loaded_Data = tuple.Split(',');

                    switch (splitted_Tuple_loaded_Data[1])
                    {
                        case "13":
                            //long t1 = long.Parse(splitted_Tuple_loaded_Data[5]) - long.Parse(splitted_Tuple_loaded_Data[4]);
                            ////Debug.LogError("t1 " + t1);
                            //long t2 = long.Parse(splitted_Tuple_loaded_Data[7]) - long.Parse(splitted_Tuple_loaded_Data[6]);
                            ////Debug.LogError("t2 " + t2);
                            //long offset = (t1 - t2) / 2;
                            //long delay = (t1 + t2) / 2;

                            Tuple<TimeSpan, TimeSpan> bbb = Calculate_PTP_OffsetDelay(splitted_Tuple_loaded_Data[4], splitted_Tuple_loaded_Data[5]
                            , splitted_Tuple_loaded_Data[7], splitted_Tuple_loaded_Data[6]);

                            timeOffset = bbb.Item1;

                            if (bbb.Item2.TotalSeconds <= 0.04f)
                            {
                                Debug.Log(string.Format("Result - Offset : {0} // Delay : {1}", bbb.Item1.TotalSeconds, bbb.Item2.TotalSeconds));

                                timeOffset = bbb.Item1;
                            }
                            else
                            {
                                Send_Message_to_slaveClock(Enum_IPC_Message.SyncMasterSlaveClockTime);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }


                //if (splitted_Tuple_loaded_Data.Length <5)
                //    return;

                //switch (splitted_Tuple_loaded_Data[1])
                //{

                //    //case "111":
                //    //    GM_DataRecorder.instance.Update_ConsoleText("Tizen Watch -> Connected");
                //    //    break;

                //    case "22":
                //    case "222":
                //        splitted_Tuple_loaded_Data = tuple.Split(';');

                //        for (int i = 0; i < splitted_Tuple_loaded_Data.Length; i++)
                //        {
                //            string[] splitted_Tuple_loaded_Data2 = splitted_Tuple_loaded_Data[i].Split(',');
                //            //GM_DataRecorder.instance.Enequeue_Data(splitted_Tuple_loaded_Data2[1]);

                //            //if (splitted_Tuple_loaded_Data2.Length != 5)
                //            //    return;

                //            if (!GM_DataRecorder.instance.BRecording)
                //                break;

                //            if (splitted_Tuple_loaded_Data2[1] == "22")
                //            {
                //                acclist.Add(splitted_Tuple_loaded_Data2[4]);

                //                Text_Queue_Tizen.text = "Tizen Count : " + acclist.Count;
                //            }
                //            else
                //            {
                //                gyrolist.Add(splitted_Tuple_loaded_Data2[4]);
                //            }

                //        }
                //        break;
                //    case "33":
                //        for (int i = 0; i < splitted_Tuple_loaded_Data.Length; i++)
                //        {
                //            string[] splitted_Tuple_loaded_Data2 = splitted_Tuple_loaded_Data[i].Split(',');
                //            //GM_DataRecorder.instance.Enequeue_Data(splitted_Tuple_loaded_Data2[1]);

                //            //if (splitted_Tuple_loaded_Data2.Length != 5)
                //            //    return;

                //            if (!GM_DataRecorder.instance.BRecording)
                //                break;

                //            if (splitted_Tuple_loaded_Data2[1] == "33")
                //            {
                //                acclist.Add(splitted_Tuple_loaded_Data2[4]);

                //                Text_Queue_Fossil.text = "Fossil Count : " + acclist_Fossil.Count;
                //            }
                //            else
                //            {
                //                gyrolist.Add(splitted_Tuple_loaded_Data2[4]);
                //            }

                //        }
                //        break;
                //    case "333":
                //        for (int i = 0; i < splitted_Tuple_loaded_Data.Length; i++)
                //        {
                //            string[] splitted_Tuple_loaded_Data2 = splitted_Tuple_loaded_Data[i].Split(',');
                //            //GM_DataRecorder.instance.Enequeue_Data(splitted_Tuple_loaded_Data2[1]);

                //            //if (splitted_Tuple_loaded_Data2.Length != 5)
                //            //    return;

                //            if (!GM_DataRecorder.instance.BRecording)
                //                break;

                //            if (splitted_Tuple_loaded_Data2[1] == "333")
                //            {
                //                acclist.Add(splitted_Tuple_loaded_Data2[4]);

                //                //                                    Text_Queue_Tizen.text = "Tizen Count : " + acclist.Count;
                //            }
                //            else
                //            {
                //                gyrolist.Add(splitted_Tuple_loaded_Data2[4]);
                //            }

                //        }
                //        break;
                //}

                //if (splitted_Tuple_loaded_Data[0] == "11")
                //{
                //    long t1 = long.Parse(splitted_Tuple_loaded_Data[2]) - long.Parse(splitted_Tuple_loaded_Data[1]);
                //    //Debug.LogError("t1 " + t1);
                //    long t2 = long.Parse(splitted_Tuple_loaded_Data[4]) - long.Parse(splitted_Tuple_loaded_Data[3]);
                //    //Debug.LogError("t2 " + t2);
                //    long offset = (t1 - t2) / 2;
                //    long delay = (t1 + t2) / 2;

                //    if (delay <= 4)
                //    {
                //        Debug.Log(string.Format("Result - Offset : {0} // Delay : {1}", offset, delay));

                //        timeOffset = (int)offset;
                //    }
                //    else
                //    {
                //        Send_Message_to_slaveClock(Enum_IPC_Message.SyncMasterSlaveClockTime);
                //    }


                //    //long intv = long.Parse(aaa) - long.Parse(splitted_Tuple_loaded_Data[1]);
                //    //Debug.LogError(intv);
                //}
                ////else if (splitted_Tuple_loaded_Data[0] == "22")
                ////{
                ////    splitted_Tuple_loaded_Data = tuple.Split(';');

                ////    for (int i = 0; i < splitted_Tuple_loaded_Data.Length; i++)
                ////    {
                ////        string[] splitted_Tuple_loaded_Data2 = splitted_Tuple_loaded_Data[i].Split(',');
                ////        //GM_DataRecorder.instance.Enequeue_Data(splitted_Tuple_loaded_Data2[1]);
                ////        acclist.Add(splitted_Tuple_loaded_Data2[1]);
                ////    }
                ////}
                ////else if (splitted_Tuple_loaded_Data[0] == "222")
                ////{
                ////    splitted_Tuple_loaded_Data = tuple.Split(';');

                ////    for (int i = 0; i < splitted_Tuple_loaded_Data.Length; i++)
                ////    {
                ////        string[] splitted_Tuple_loaded_Data2 = splitted_Tuple_loaded_Data[i].Split(',');
                ////        //GM_DataRecorder.instance.Enequeue_Data(splitted_Tuple_loaded_Data2[1]);
                ////        gyrolist.Add(splitted_Tuple_loaded_Data2[1]);
                ////    }
                ////}
                //else if (splitted_Tuple_loaded_Data[0] == "22" || splitted_Tuple_loaded_Data[0] == "222")
                //{
                //    splitted_Tuple_loaded_Data = tuple.Split(';');

                //    for (int i = 0; i < splitted_Tuple_loaded_Data.Length; i++)
                //    {
                //        string[] splitted_Tuple_loaded_Data2 = splitted_Tuple_loaded_Data[i].Split(',');
                //        //GM_DataRecorder.instance.Enequeue_Data(splitted_Tuple_loaded_Data2[1]);

                //        if (splitted_Tuple_loaded_Data2.Length != 2)
                //            return;

                //        if (!GM_DataRecorder.instance.BRecording)
                //            break;

                //        if(splitted_Tuple_loaded_Data2[0] == "22")
                //        {
                //            acclist.Add(splitted_Tuple_loaded_Data2[1]);

                //            Text_Queue_Tizen.text = "Tizen Count : " + acclist.Count;
                //        }
                //        else
                //        {
                //            gyrolist.Add(splitted_Tuple_loaded_Data2[1]);
                //        }

                //    }
                //}

            }
        }

        List<string> acclist = new List<string>();
        List<string> gyrolist = new List<string>();
        List<string> acclist_Fossil = new List<string>();
        List<string> gyrolist_Fossil = new List<string>();
        //public void Update_ConsoleText(string _text)
        //{
        //    //text_MsgDP.text += "\n" + _text;
        //}


        public void Send_Message_to_slaveClock(Enum_IPC_Message _IPC_Message)
        {

            foreach (Socket sk in list_ClientSocket)
            {
                AsyncObject ao = new AsyncObject(1);

                // 사용된 소켓을 저장
                ao.WorkingSocket = sk;
                //ao.WorkingSocket.NoDelay = true;


                // 문자열을 바이트 배열으로 변환
                ao.Buffer = Encoding.UTF8.GetBytes(_IPC_Message.ToString() + "," + (int)_IPC_Message + "," + "SV," + "JSB" + DateTime.Now.ToString(",HHmmssfff") + "\n");

                // 전송 시작!
                try
                {
                    ao.WorkingSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
                    //aaa = DateTime.Now.ToString("HHmmssfff");
                }
                catch (SocketException e)
                {
                    get_msg(e.ToString());
                }
                catch (NullReferenceException ae)
                {
                    get_msg(ae.ToString());
                }
            }

        }

        public void Pull_Data()
        {
            try
            {
                int quecount = acclist_Fossil.Count > gyrolist_Fossil.Count ? gyrolist_Fossil.Count : acclist_Fossil.Count;
                //int quecount = acclist.Count > gyrolist.Count ? gyrolist.Count : acclist.Count;
                Debug.Log(acclist.Count);
                Debug.Log(gyrolist.Count);
                Debug.Log(acclist_Fossil.Count);
                Debug.Log(gyrolist_Fossil.Count);
                Debug.LogError(quecount.ToString() + " // " + acclist_Fossil.Count + " // " + gyrolist_Fossil.Count);

                for (int i = 0; i < quecount; i++)
                {
                    //GM_DataRecorder.instance.Enequeue_Data(acclist[i] + "," + gyrolist[i]);
                    //TIZEN
                    //     GM_DataRecorder.instance.Enequeue_SmartWatchData(acclist[i], gyrolist[i], TimeOffset);


                    GM_DataRecorder.instance.Enequeue_SmartWatchData(acclist_Fossil[i], gyrolist_Fossil[i], TimeOffset);
                }
                //    acclist.Clear();
                //    gyrolist.Clear();
                acclist_Fossil.Clear();
                gyrolist_Fossil.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                throw;
            }


        }

        public Tuple<TimeSpan, TimeSpan> Calculate_PTP_OffsetDelay(string t1, string t2, string t3, string t4)
        {
            //ex 175959111 -> 17 59 59 111
            TimeSpan offset, delay;

            try
            {

                List<DateTime> list_date = new List<DateTime>();

                list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t1.Substring(0, 2)), int.Parse(t1.Substring(2, 2))
                    , int.Parse(t1.Substring(4, 2)), int.Parse(t1.Substring(6, 3))));

                list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t2.Substring(0, 2)), int.Parse(t2.Substring(2, 2))
                    , int.Parse(t2.Substring(4, 2)), int.Parse(t2.Substring(6, 3))));

                list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t3.Substring(0, 2)), int.Parse(t3.Substring(2, 2))
                    , int.Parse(t3.Substring(4, 2)), int.Parse(t3.Substring(6, 3))));

                list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t4.Substring(0, 2)), int.Parse(t4.Substring(2, 2))
                    , int.Parse(t4.Substring(4, 2)), int.Parse(t4.Substring(6, 3))));


                TimeSpan timeSpan_t2t1 = list_date[1] - list_date[0];
                TimeSpan timeSpan_t4t3 = list_date[3] - list_date[2];

                offset = new TimeSpan((timeSpan_t2t1.Ticks - timeSpan_t4t3.Ticks) / 2);

                delay = new TimeSpan((timeSpan_t2t1.Ticks + timeSpan_t4t3.Ticks) / 2);

                Debug.Log("offset : " + offset.TotalSeconds.ToString());

            }
            catch (Exception)
            {

                throw;
            }

            return Tuple.Create(offset, delay);
        }


    }
}