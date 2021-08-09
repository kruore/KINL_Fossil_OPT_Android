using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace _KINLAB
{
    public class DataEdit_SettingPage_Management : MonoBehaviour
    {
        public static DataEdit_SettingPage_Management instance = null;

        [SerializeField]
        private GameObject panel01;

        [SerializeField]
        private GameObject panel02;

        [SerializeField]
        private InputField InputField_OptiTrack_MarkerCount;

        [SerializeField]
        private InputField InputField_OptiTrack_MarkerNickname;

        [HideInInspector]
        public string fileNameforLoading = string.Empty;

        [SerializeField]
        private GameObject marker_Set_DropDown;

        [HideInInspector]
        public List<string> list_MarkerSet_Nickname = new List<string>();
        //---------------------------------------------

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            list_MarkerSet_Nickname.Add("NONE");

        }

        public void GetInput_Panel01_01(string inputStr)
        {
            InputField_OptiTrack_MarkerCount.transform.GetChild(2).gameObject.SetActive(false);
            InputField_OptiTrack_MarkerCount.transform.GetChild(3).gameObject.SetActive(false);

            if (inputStr != string.Empty)
            {
                fileNameforLoading = inputStr;
                InputField_OptiTrack_MarkerCount.transform.GetChild(2).gameObject.SetActive(true);
                Debug.Log("aaa");
            }
            else
            {
                fileNameforLoading = string.Empty;
                InputField_OptiTrack_MarkerCount.transform.GetChild(3).gameObject.SetActive(true);
            }
        }

        public void GetInput_Panel01_02(string inputStr)
        {
            InputField_OptiTrack_MarkerNickname.transform.GetChild(2).gameObject.SetActive(false);
            InputField_OptiTrack_MarkerNickname.transform.GetChild(3).gameObject.SetActive(false);

            if (inputStr != string.Empty)
            {
                list_MarkerSet_Nickname.Add(inputStr);
                InputField_OptiTrack_MarkerNickname.transform.GetChild(2).gameObject.SetActive(true);
                Debug.Log("aaa");
            }
            else
            {
                InputField_OptiTrack_MarkerNickname.transform.GetChild(3).gameObject.SetActive(true);
            }


            marker_Set_DropDown.GetComponent<TMP_Dropdown>().ClearOptions();
            marker_Set_DropDown.GetComponent<TMP_Dropdown>().AddOptions(list_MarkerSet_Nickname);
            marker_Set_DropDown.GetComponent<TMP_Dropdown>().RefreshShownValue();

           
        }
    }
}