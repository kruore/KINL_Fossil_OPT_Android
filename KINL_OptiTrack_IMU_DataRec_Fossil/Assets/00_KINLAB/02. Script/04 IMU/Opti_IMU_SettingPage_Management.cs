using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace _KINLAB
{
    public class Opti_IMU_SettingPage_Management : MonoBehaviour
    {
        public static Opti_IMU_SettingPage_Management instance = null;

        private List<string> IMU_Sensor_MAC_Address;
        [HideInInspector]
        public Dictionary<int, string> dic_Selected_IMU_Sensor_MAC_Address;
        private List<Button> IMU_Sensor_Buttons;
        private List<Text> list_Selected_IMU_Sensors_Name;

        private int selected_IMU_Sensor_MAC_Address_Count;
        private int selected_IMU_Sensor_MAC_Address_Count_MAX;



        [HideInInspector]
        public int optiTrack_MarkerCount;
        [SerializeField]
        private int optiTrack_MarkerCount_default;



        [SerializeField]
        private Button IMU_Sensor01_Button;
        [SerializeField]
        private Button IMU_Sensor02_Button;
        [SerializeField]
        private Button IMU_Sensor03_Button;

        [SerializeField]
        private Button IMU_Sensor04_Button;
        [SerializeField]
        private Button IMU_Sensor05_Button;
        [SerializeField]
        private Button IMU_Sensor06_Button;
        [SerializeField]
        private Button IMU_Sensor07_Button;
        [SerializeField]
        private Button IMU_Sensor08_Button;

        [SerializeField]
        private Button IMU_Sensor09_Button;

        private ColorBlock original_button_color;
        private ColorBlock selected_button_color;

        [SerializeField]
        private Text selected_Sensor01_Name;

        [SerializeField]
        private Text selected_Sensor02_Name;

        [SerializeField]
        private Text selected_Sensor03_Name;

        [SerializeField]
        private InputField InputField_OptiTrack_MarkerCount;


        [SerializeField]
        private GameObject panel01;

        [SerializeField]
        private GameObject panel02;
        //---------------------------------------------

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            IMU_Sensor_MAC_Address = new List<string>();
            dic_Selected_IMU_Sensor_MAC_Address = new Dictionary<int, string>();
            IMU_Sensor_Buttons = new List<Button>();
            list_Selected_IMU_Sensors_Name = new List<Text>();

            selected_IMU_Sensor_MAC_Address_Count = 0;
            selected_IMU_Sensor_MAC_Address_Count_MAX = 3;

            optiTrack_MarkerCount = optiTrack_MarkerCount_default;

        }

        private void Start()
        {
            selected_button_color = original_button_color = IMU_Sensor01_Button.colors;
            Color newColor = new Color(0.0f, 25.0f, 34.0f);
            selected_button_color.normalColor = newColor;

            string IMU_Sensor_MAC_Addr01 = "CA:36:BD:74:C5:A6"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr01);
            string IMU_Sensor_MAC_Addr02 = "C3:DA:74:4B:3C:0B"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr02);
            string IMU_Sensor_MAC_Addr03 = "C3:DD:26:84:9D:C7"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr03);
            string IMU_Sensor_MAC_Addr04 = "C9:6A:41:07:77:A3"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr04);
            string IMU_Sensor_MAC_Addr05 = "D9:F4:56:D7:F2:97"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr05);
            string IMU_Sensor_MAC_Addr06 = "E7:C8:23:6D:9E:A5"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr06);
            string IMU_Sensor_MAC_Addr07 = "E1:BB:1B:D7:E9:41"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr07);
            //string IMU_Sensor_MAC_Addr08 = "FE:97:1B:5A:F8:02"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr08);
            string IMU_Sensor_MAC_Addr08 = "E0:7E:13:A9:E7:7B"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr08);
            string IMU_Sensor_MAC_Addr09 = "C0:A2:D9:59:94:49"; IMU_Sensor_MAC_Address.Add(IMU_Sensor_MAC_Addr09);

            IMU_Sensor_Buttons.Add(IMU_Sensor01_Button); IMU_Sensor_Buttons.Add(IMU_Sensor02_Button); IMU_Sensor_Buttons.Add(IMU_Sensor03_Button); IMU_Sensor_Buttons.Add(IMU_Sensor04_Button);
            IMU_Sensor_Buttons.Add(IMU_Sensor05_Button); IMU_Sensor_Buttons.Add(IMU_Sensor06_Button); IMU_Sensor_Buttons.Add(IMU_Sensor07_Button); IMU_Sensor_Buttons.Add(IMU_Sensor08_Button);
            IMU_Sensor_Buttons.Add(IMU_Sensor09_Button);

            list_Selected_IMU_Sensors_Name.Add(selected_Sensor01_Name); list_Selected_IMU_Sensors_Name.Add(selected_Sensor02_Name); list_Selected_IMU_Sensors_Name.Add(selected_Sensor03_Name);


        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ///Type 02
                if (panel01.activeSelf)
                {
                    EventSystem eventSystem = EventSystem.current;

                    eventSystem.SetSelectedGameObject(InputField_OptiTrack_MarkerCount.gameObject, null);
                    InputField_OptiTrack_MarkerCount.OnPointerClick(new PointerEventData(EventSystem.current));

                    panel01.SetActive(false);
                    panel02.SetActive(true);
                }
                else if (panel02.activeSelf)
                {
                    panel02.SetActive(false);
                    panel01.SetActive(true);
                }
            }

        }

        public string Get_Selected_IMU_Sensor_MAC_Address(int _index)
        {
            string MAC_Address = string.Empty;

            if (_index < dic_Selected_IMU_Sensor_MAC_Address.Count)
            {
                MAC_Address = dic_Selected_IMU_Sensor_MAC_Address[_index];
            }

            return MAC_Address;
        }

        public int Calc_Enable_index_Selected_IMU_Sensor()
        {
            int index = 0;

            if (selected_IMU_Sensor_MAC_Address_Count == selected_IMU_Sensor_MAC_Address_Count_MAX)
            {
                Reset_Selected_IMU_Sensors();
            }

            index = selected_IMU_Sensor_MAC_Address_Count;
            Debug.Log("index" + index);
            selected_IMU_Sensor_MAC_Address_Count++;
            return index;
        }

        public int Get_Selected_IMU_Sensor_MAC_Address_Count()
        {
            return selected_IMU_Sensor_MAC_Address_Count;
        }

        #region Area : Press the Buttons of IMU Sensor MAC Addr

        public void Press_Button_IMU_Sensor_MAC_Addr(int _index)
        {
            if (!dic_Selected_IMU_Sensor_MAC_Address.ContainsValue(IMU_Sensor_MAC_Address[_index]))
            {
                int dic_Key = Calc_Enable_index_Selected_IMU_Sensor();
                dic_Selected_IMU_Sensor_MAC_Address.Add(dic_Key, IMU_Sensor_MAC_Address[_index]);
                IMU_Sensor_Buttons[_index].colors = selected_button_color;

                //list_Selected_IMU_Sensors_Name[dic_Key].text = dic_Selected_IMU_Sensor_MAC_Address[dic_Key];
                for (int i = 0; i < dic_Selected_IMU_Sensor_MAC_Address.Count; i++)
                {
                    list_Selected_IMU_Sensors_Name[i].text = dic_Selected_IMU_Sensor_MAC_Address[i];
                }
            }
        }

        public void Button_IMU_Sensor_MAC_Addr01()
        {
            Press_Button_IMU_Sensor_MAC_Addr(0);
        }

        public void Button_IMU_Sensor_MAC_Addr02()
        {
            Press_Button_IMU_Sensor_MAC_Addr(1);
        }

        public void Button_IMU_Sensor_MAC_Addr03()
        {
            Press_Button_IMU_Sensor_MAC_Addr(2);
        }

        public void Button_IMU_Sensor_MAC_Addr04()
        {
            Press_Button_IMU_Sensor_MAC_Addr(3);
        }

        public void Button_IMU_Sensor_MAC_Addr05()
        {
            Press_Button_IMU_Sensor_MAC_Addr(4);
        }

        public void Button_IMU_Sensor_MAC_Addr06()
        {
            Press_Button_IMU_Sensor_MAC_Addr(5);
        }

        public void Button_IMU_Sensor_MAC_Addr07()
        {
            Press_Button_IMU_Sensor_MAC_Addr(6);
        }

        public void Button_IMU_Sensor_MAC_Addr08()
        {
            Press_Button_IMU_Sensor_MAC_Addr(7);
        }

        public void Button_IMU_Sensor_MAC_Addr09()
        {
            Press_Button_IMU_Sensor_MAC_Addr(8);
        }

        #endregion

        #region Area : Button_Selected_IMU_Sensor_MAC_Address_Count_MAX_UP_Down
        public void Up_Selected_IMU_Sensor_MAC_Address_Count_MAX()
        {
            selected_IMU_Sensor_MAC_Address_Count_MAX++;
        }

        public void Down_Selected_IMU_Sensor_MAC_Address_Count_MAX()
        {
            selected_IMU_Sensor_MAC_Address_Count_MAX--;
        }
        #endregion

        #region Area : Button_Reset_Selected_IMU_Sensors
        public void Reset_Selected_IMU_Sensors()
        {
            selected_IMU_Sensor_MAC_Address_Count = 0;
            dic_Selected_IMU_Sensor_MAC_Address.Clear();

            foreach (var item in IMU_Sensor_Buttons)
            {
                item.colors = original_button_color;
            }

            foreach (var item in list_Selected_IMU_Sensors_Name)
            {
                item.text = string.Empty;
            }
        }
        #endregion

        #region Area : OptiTrack
        public void GetInput_Panel02_01(string inputStr)
        {
            InputField_OptiTrack_MarkerCount.transform.GetChild(2).gameObject.SetActive(false);
            InputField_OptiTrack_MarkerCount.transform.GetChild(3).gameObject.SetActive(false);

            int tempI;

            if (int.TryParse(inputStr, out tempI) && tempI >= 1)
            {
                optiTrack_MarkerCount = tempI;
                InputField_OptiTrack_MarkerCount.transform.GetChild(2).gameObject.SetActive(true);
                Debug.Log("aaa");
            }
            else
            {
                optiTrack_MarkerCount = optiTrack_MarkerCount_default;
                InputField_OptiTrack_MarkerCount.transform.GetChild(3).gameObject.SetActive(true);
            }
        }
        #endregion
    }
}