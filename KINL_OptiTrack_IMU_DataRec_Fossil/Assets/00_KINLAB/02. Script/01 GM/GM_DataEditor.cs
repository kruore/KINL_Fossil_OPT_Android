using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

using UnityEngine.Events;
using System.Text;

namespace _KINLAB
{
    public class GM_DataEditor : MonoBehaviour
    {
        public static GM_DataEditor instance = null;

        private bool isPlayingData = false;

        private Dictionary<int, List<Vector3>> dic_optiTrack_data_copy = null;
        private Dictionary<int, List<bool>> dic_optiTrack_data_bool_copy = null;

        private Dictionary<string, List<Vector3>> dic_Edited_optiTrack_data = null;
        private Dictionary<string, int> dic_Edited_NickName_Index = null;

        int totalEntryCount = 0;
        int currentEntryNum = 0;

        int totalmarkerCount = 0;

        private GameObject defaultmarker_Render_Sphere_Pref;
        private GameObject NNmarker_Render_Sphere_Pref;

        private List<GameObject> list_marker_Render_Sphere = new List<GameObject>();
        private List<GameObject> list_marker_InfoPanel_Button = new List<GameObject>();
        private List<GameObject> list_EachMarkerInfoPanel = new List<GameObject>();
        private List<GameObject> list_EachMarkerInfoPanel_Title = new List<GameObject>();

        private GameObject time_Slider;
        private GameObject section_Slider01;
        private GameObject section_Slider02;
        int edit_sectionLeft_currentEntryNum = 0;
        int edit_sectionRight_currentEntryNum = 0;

        [SerializeField]
        private GameObject markerInfo_Display_Panel;

        [SerializeField]
        private GameObject each_MarkerInfoButton_pref;

        [SerializeField]
        private GameObject each_MarkerInfopanel_pref;

        [SerializeField]
        private GameObject each_MarkerInfoTitle_pref;

        private List<GameObject> selected_two_buttons = new List<GameObject>();

        //private ColorBlock original_button_color;
        //private ColorBlock selected_button_color;

        [SerializeField]
        private Material selectedMarkerMaterial;

        [SerializeField]
        private Material originalMarkerMaterial;

        private Material originalNNMarkerMaterial;

        private GameObject marker_Set_DropDown;

        private List<string> list_MarkerSet_Nickname = new List<string>();
        private int current_MarkerSet_MarkerNickname_index = 0;
        private bool isSelectedNickname = false;

        private GameObject marker_Nicknaming_Table;
        private List<GameObject> list_Nickname_Table_Set_button;

        private MarkerNicknameTableInterface.Nickname_Table_Interface_Type current_button_type;

        private int selected_Nicknaming_First_Target_Index = -1;
        private int selected_Nicknaming_Second_Target_Index = -1;


        //----------------------------------

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
            InitData();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Toggle_Play_Video();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                Init_Video();
            }

            if (isPlayingData)
            {
                Update_Video_scene(false);
            }

            if (Input.GetKeyDown(KeyCode.Slash))
            {
                Save_Edited_Data();
            }


            //Debug.Log(current_button_type.ToString());
        }
        private void OnDisable()
        {
            isPlayingData = false;
        }

        private void InitData()
        {
            dic_optiTrack_data_copy = new Dictionary<int, List<Vector3>>(GM_DataLoader.instance.Dic_optiTrack_data_origin);
            dic_optiTrack_data_bool_copy = new Dictionary<int, List<bool>>(GM_DataLoader.instance.Dic_optiTrack_data_bool_origin);
            dic_Edited_optiTrack_data = new Dictionary<string, List<Vector3>>();
            dic_Edited_NickName_Index = new Dictionary<string, int>();

            defaultmarker_Render_Sphere_Pref = GM_DataLoader.instance.Marker_Render_Sphere_Pref;
            NNmarker_Render_Sphere_Pref = GM_DataLoader.instance.NNmarker_Render_Sphere_Pref;

            totalEntryCount = GM_DataLoader.instance.Queue_optiTrack_data_copy_EntryCount;

            markerInfo_Display_Panel = GM_DataLoader.instance.MarkerInfo_Display_Panel;
            each_MarkerInfoButton_pref = GM_DataLoader.instance.MarkerInfo_Display_button_pref;
            each_MarkerInfopanel_pref = GM_DataLoader.instance.Each_MarkerInfopanel_pref;
            each_MarkerInfoTitle_pref = GM_DataLoader.instance.Each_MarkerInfoTitle_pref;
            selectedMarkerMaterial = GM_DataLoader.instance.SelectedMaterial;
            marker_Set_DropDown = GM_DataLoader.instance.Marker_Set_DropDown;
            marker_Nicknaming_Table = GM_DataLoader.instance.Marker_Nicknaming_Table;
            list_Nickname_Table_Set_button = new List<GameObject>(GM_DataLoader.instance.List_Nickname_Table_Set_button);

            totalmarkerCount = GM_DataLoader.instance.Pre_raw_CSV_marker_Count;
            for (int i = 0; i < totalmarkerCount; i++)
            {
                GameObject tempGO = Instantiate(defaultmarker_Render_Sphere_Pref, Vector3.zero, Quaternion.identity);
                //tempGO.GetComponent<MeshRenderer>().enabled = false;
                tempGO.GetComponent<OptiTrackMarkerInterface>().Set_Marker_Info(i, string.Empty);
                tempGO.name = "Marker " + i;

                list_marker_Render_Sphere.Add(tempGO);

                Toggle_Render_Marker(i, false);
            }

            //Debug.Log("list_marker_Render_Sphere " + list_marker_Render_Sphere.Count);
            //Debug.Log(totalEntryCount);

            int panelMargin = 10;
            int panelWidth = totalEntryCount + (panelMargin * 2); //ex 3062 + 20 = 3082
            //int panelHeight = panelMargin * ((4 * totalmarkerCount) + 1); //ex 11 -> 450
            int panelHeight = (totalmarkerCount * 3 * panelMargin) + (panelMargin * 4); //panelMargin * ((3 * totalmarkerCount) + 2); //ex 11 -> 350

            markerInfo_Display_Panel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);

            markerInfo_Display_Panel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);




            List<bool> list_bool;


            float markerPanelFirstHeight = panelMargin + (1.5f * panelMargin);


            for (int i = 0; i < totalmarkerCount; i++)
            {
                GameObject tempGO = Instantiate(each_MarkerInfopanel_pref);
                //tempGO.transform.parent = markerInfo_Display_Panel.transform;
                tempGO.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
                tempGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth - (panelMargin * 2));
                tempGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));
                //tempGO.GetComponent<RectTransform>().localPosition = new Vector3(panelWidth / 2, -(panelHeight / 2), 0.0f);
                tempGO.GetComponent<RectTransform>().localPosition = new Vector3(panelWidth / 2, -(markerPanelFirstHeight + (i * (panelMargin * 3))), 0.0f);
                tempGO.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                //tempGO.GetComponent<RectTransform>().rect. = Vector2.zero;



                GameObject tempGO02 = Instantiate(each_MarkerInfoTitle_pref);
                tempGO02.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
                tempGO02.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);
                tempGO02.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));
                tempGO02.GetComponent<RectTransform>().localPosition = new Vector3(75, -(markerPanelFirstHeight + (i * (panelMargin * 3))), 0.0f);
                tempGO02.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                tempGO02.GetComponent<TextMeshProUGUI>().text = "Marker " + i;
                tempGO02.GetComponent<RectTransform>().SetParent(tempGO.transform);

                tempGO.name = "Marker " + i + " " + "DataFrame";
                tempGO02.name = "Marker " + i + " " + "Title";

                list_EachMarkerInfoPanel.Add(tempGO);
                list_EachMarkerInfoPanel_Title.Add(tempGO02);
            }



            for (int i = 0; i < totalmarkerCount; i++)
            {
                dic_optiTrack_data_bool_copy.TryGetValue(i, out list_bool);

                bool isMakingButton = false;
                int buttonSize = 0;

                for (int j = 0; j < totalEntryCount; j++) // current entry = j
                {

                    if (list_bool[j]) // true data
                    {
                        buttonSize++;
                    }
                    else
                    {
                        if (buttonSize > 0)
                        {
                            GameObject m_button = Instantiate(each_MarkerInfoButton_pref);
                            m_button.GetComponent<RectTransform>().SetParent(list_EachMarkerInfoPanel[i].transform);
                            m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSize);
                            m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));

                            float aaaaaaaaaa = (-(panelWidth / 2) + panelMargin) + (j - (buttonSize / 2));
                            m_button.GetComponent<RectTransform>().localPosition = new Vector3(aaaaaaaaaa, 0.0f, 0.0f);
                            m_button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            m_button.GetComponent<MarkerInfoPanelButton>().Set_Marker_Info(i);
                            list_marker_InfoPanel_Button.Add(m_button);

                            buttonSize = 0;
                        }
                        else
                        {

                        }
                    }


                }

                if (buttonSize > 0)
                {
                    GameObject m_button = Instantiate(each_MarkerInfoButton_pref);
                    m_button.GetComponent<RectTransform>().SetParent(list_EachMarkerInfoPanel[i].transform);
                    m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSize);
                    m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));
                    float aaaaaaaaaa = (-(panelWidth / 2) + panelMargin) + (totalEntryCount - (buttonSize / 2));
                    m_button.GetComponent<RectTransform>().localPosition = new Vector3(aaaaaaaaaa, 0.0f, 0.0f);
                    m_button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    m_button.GetComponent<MarkerInfoPanelButton>().Set_Marker_Info(i);
                    m_button.tag = "MarkerDataButton";
                    list_marker_InfoPanel_Button.Add(m_button);

                    buttonSize = 0;
                }
            }

            //selected_button_color = original_button_color = list_marker_InfoPanel_Button[0].GetComponent<Button>().colors;
            ////Color newColor = new Color(81.0f, 209.0f, 255.0f);
            //Color newColor = Color.green;
            //selected_button_color.selectedColor = newColor;

            time_Slider = GM_DataLoader.instance.Time_Slider;
            time_Slider.GetComponent<Slider>().onValueChanged.AddListener(delegate { OnTimeSliderValueChanged(); });
            time_Slider.GetComponent<Slider>().value = 0.0f;
            Update_Video_scene(true);

            section_Slider01 = GM_DataLoader.instance.Section_Slider01;
            section_Slider01.GetComponent<Slider>().onValueChanged.AddListener(
                delegate 
                {
                int currentEntryNum_Approximate = (int)(section_Slider01.GetComponent<Slider>().value * (float)totalEntryCount);
                edit_sectionLeft_currentEntryNum = currentEntryNum_Approximate;
                //Debug.Log("section_Slider01 :" + edit_sectionLeft_currentEntryNum);
                });
            section_Slider01.GetComponent<Slider>().value = 0.3f;

            section_Slider02 = GM_DataLoader.instance.Section_Slider02;
            section_Slider02.GetComponent<Slider>().onValueChanged.AddListener(
                delegate 
                {
                int currentEntryNum_Approximate = (int)(section_Slider02.GetComponent<Slider>().value * (float)totalEntryCount);
                edit_sectionRight_currentEntryNum = currentEntryNum_Approximate;
                //Debug.Log("section_Slider02 : " + edit_sectionRight_currentEntryNum);
                });
            section_Slider02.GetComponent<Slider>().value = 0.4f;



            section_Slider01.GetComponent<RectTransform>().SetParent(transform);
            section_Slider01.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
            section_Slider02.GetComponent<RectTransform>().SetParent(transform);
            section_Slider02.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);

            time_Slider.GetComponent<RectTransform>().SetParent(transform);
            time_Slider.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);

            section_Slider01.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
            section_Slider02.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
            time_Slider.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);

            originalMarkerMaterial = list_marker_Render_Sphere[0].GetComponent<MeshRenderer>().material;


            //list_MarkerSet_Nickname.Add("NONE");
            //list_MarkerSet_Nickname.Add("A_spine00_Occipital");
            //list_MarkerSet_Nickname.Add("A_spine01_C7");
            //list_MarkerSet_Nickname.Add("A_spine02_T3");

            list_MarkerSet_Nickname.AddRange(DataEdit_SettingPage_Management.instance.list_MarkerSet_Nickname);

            marker_Set_DropDown.GetComponent<TMP_Dropdown>().ClearOptions();
            marker_Set_DropDown.GetComponent<TMP_Dropdown>().AddOptions(list_MarkerSet_Nickname);
            marker_Set_DropDown.GetComponent<TMP_Dropdown>().RefreshShownValue();

            marker_Set_DropDown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener
                ((parameter) => 
                {
                    if (parameter == 0)
                    {
                        current_MarkerSet_MarkerNickname_index = 0;
                        isSelectedNickname = false;
                    }
                    else
                    {
                        current_MarkerSet_MarkerNickname_index = parameter;
                        isSelectedNickname = true;
                    }

                    marker_Nicknaming_Table.transform.GetChild(0).GetComponent<TMP_Text>().text = list_MarkerSet_Nickname[current_MarkerSet_MarkerNickname_index];
                }
                );

            //list_marker_Nicknaming_Target.Add()
        }

        private void Toggle_Play_Video()
        {
            if(isPlayingData)
            {
                isPlayingData = false;
                Debug.Log("*** Toogle VIDEO STOP");

            }
            else
            {

                Debug.Log("*** Toogle VIDEO START");
                isPlayingData = true;
            }
        }

        private void Init_Video()
        {
            isPlayingData = false;
            currentEntryNum = 0;
            Debug.Log("*** Init VIDEO Play");

            for (int i = 0; i < totalmarkerCount; i++)
            {
                Toggle_Render_Marker(i, false);
            }

            time_Slider.GetComponent<Slider>().value = 0.0f;
        }

        private void Update_Video_scene(bool _isDrag)
        {
            if(currentEntryNum >= totalEntryCount)
            {
                isPlayingData = false;
                Debug.Log("*** VIDEO END");

                //for (int i = 0; i < totalmarkerCount; i++)
                //{
                //    Toggle_Render_Marker(i, false);
                //}

                return;
            }

            //Debug.Log("Entry Num : " + currentEntryNum);

            List<Vector3> tempList;

            for (int i = 0; i < totalmarkerCount; i++)
            {
                dic_optiTrack_data_copy.TryGetValue(i, out tempList);

                list_marker_Render_Sphere[i].transform.position = tempList[currentEntryNum];

                if(list_marker_Render_Sphere[i].transform.position == Vector3.zero)
                {
                    //list_marker_Render_Sphere[i].GetComponent<MeshRenderer>().enabled = false;
                    Toggle_Render_Marker(i, false);
                }
                else
                {
                    //list_marker_Render_Sphere[i].GetComponent<MeshRenderer>().enabled = true;
                    Toggle_Render_Marker(i, true);
                }
            }


            if(_isDrag)
            {

            }
            else
            {
                currentEntryNum++;

                float sldier_percent = (float)currentEntryNum / (float)totalEntryCount;
                time_Slider.GetComponent<Slider>().SetValueWithoutNotify(sldier_percent);

                //Debug.Log(time_Slider.GetComponent<Slider>().value.ToString("F3"));
            }
        }

        private void Toggle_Render_Marker(int _index, bool _enable)
        {
            //foreach (var item in list_marker_Render_Sphere)
            //{
            //    item.GetComponent<MeshRenderer>().enabled = _enable;
            //    item.GetComponent<OptiTrackMarkerInterface>().Toggle_Render_MarkerName(_enable);
            //}

            list_marker_Render_Sphere[_index].GetComponent<MeshRenderer>().enabled = _enable;
            list_marker_Render_Sphere[_index].GetComponent<OptiTrackMarkerInterface>().Toggle_Render_MarkerName(_enable);
        }

        private void OnTimeSliderValueChanged()
        {
            //Debug.Log(time_Slider.GetComponent<Slider>().value.ToString("F3"));

            isPlayingData = false;

            int currentEntryNum_Approximate = (int)(time_Slider.GetComponent<Slider>().value * (float)totalEntryCount);

            currentEntryNum = currentEntryNum_Approximate;

            //Debug.Log(currentEntryNum_Approximate);

            Update_Video_scene(true);
        }

        public void OnSelectMarkerInfoPanelButton(int _index)
        {
            foreach (var item in list_marker_Render_Sphere)
            {
                if(item.gameObject.tag == "NickNamedMarker")
                {
                    item.GetComponent<MeshRenderer>().material = originalNNMarkerMaterial;
                }
                else
                {
                    item.GetComponent<MeshRenderer>().material = originalMarkerMaterial;
                }

            }

            list_marker_Render_Sphere[_index].GetComponent<MeshRenderer>().material = selectedMarkerMaterial; //%%%

            if(isSelectedNickname && current_button_type != MarkerNicknameTableInterface.Nickname_Table_Interface_Type.NONE)
            {

                int tempIndex = (int)current_button_type - 1;

                list_Nickname_Table_Set_button[tempIndex].transform.parent.GetComponent<TMP_Text>().text = list_EachMarkerInfoPanel_Title[_index].GetComponent<TMP_Text>().text;

                //이제 캐스팅 확인 버튼 만들고, 데이터 정리 들어가야함
                if(tempIndex == 0)
                {
                    selected_Nicknaming_First_Target_Index = _index;
                    //Debug.Log("selected_Nicknaming_First_Target_Index : " + selected_Nicknaming_First_Target_Index);
                }
                else if (tempIndex == 1)
                {
                    selected_Nicknaming_Second_Target_Index = _index;
                    //Debug.Log("selected_Nicknaming_Second_Target_Index : " + selected_Nicknaming_Second_Target_Index);
                }



            }
        }

        public void OnClick_MarkerNickSetButton(MarkerNicknameTableInterface.Nickname_Table_Interface_Type _buttonType, bool _isFocus)
        {
            if(_isFocus)
            {
                if (current_button_type != _buttonType)
                {
                    current_button_type = _buttonType;

                }

            }
            else
            {
                current_button_type = MarkerNicknameTableInterface.Nickname_Table_Interface_Type.NONE;
            }

            foreach (var item in list_Nickname_Table_Set_button)
            {
                item.GetComponent<MarkerNicknameTableInterface>().ResetButton();
            }
        }

        public void OnClick_MarkerNickCast()
        {
            if(selected_Nicknaming_First_Target_Index != -1 && selected_Nicknaming_Second_Target_Index != -1 && selected_Nicknaming_First_Target_Index != selected_Nicknaming_Second_Target_Index)
            {
                //Debug.Log(section_Slider01.GetComponent<Slider>().value + " /// " + section_Slider02.GetComponent<Slider>().value);
                string stringKey = marker_Nicknaming_Table.transform.GetChild(0).GetComponent<TMP_Text>().text;

                int distance = edit_sectionLeft_currentEntryNum - edit_sectionRight_currentEntryNum;

                int startEntry = -1;
                //int endEntry = -1;

                Debug.Log("selected_Nicknaming_First_Target_Index : " + selected_Nicknaming_First_Target_Index + " /// " + "selected_Nicknaming_Second_Target_Index : " + selected_Nicknaming_Second_Target_Index);

                if(distance < 0)
                {
                    startEntry = edit_sectionLeft_currentEntryNum;
                    //endEntry = edit_sectionRight_currentEntryNum;
                }
                else
                {
                    startEntry = edit_sectionRight_currentEntryNum;
                    //endEntry = edit_sectionLeft_currentEntryNum;
                }

                List<Vector3> list_first_target_marker_ref;
                List<Vector3> list_second_target_marker_ref;
                dic_optiTrack_data_copy.TryGetValue(selected_Nicknaming_First_Target_Index, out list_first_target_marker_ref);
                dic_optiTrack_data_copy.TryGetValue(selected_Nicknaming_Second_Target_Index, out list_second_target_marker_ref);

                List<Vector3> list_first_target_marker = new List<Vector3>(list_first_target_marker_ref);
                List<Vector3> list_second_target_marker = new List<Vector3>(list_second_target_marker_ref);


                Debug.Log("distance : " + Mathf.Abs(distance) + "  " + selected_Nicknaming_First_Target_Index + " to "+ selected_Nicknaming_Second_Target_Index);
                for (int i = startEntry; i < Mathf.Abs(distance) + startEntry; i++)
                {
                    
                    //Debug.Log("ENTRY :" + i + " Before : " + list_first_target_marker[i] + " /// " + list_second_target_marker[i]);
                    list_first_target_marker[i] = list_second_target_marker[i];
                    //list_first_target_marker[i].Set(list_second_target_marker[i].x, list_second_target_marker[i].y, list_second_target_marker[i].z);
                    //Debug.Log("ENTRY :" + i + " After : " + list_first_target_marker[i] + " /// " + list_second_target_marker[i]);
                }

                bool tempb_isnew = false;

                int intkey = -1;
                if (dic_Edited_NickName_Index.ContainsKey(stringKey))
                {
                    List<Vector3> tempVec_List01 = new List<Vector3>();
                    List<Vector3> tempVec_List02 = new List<Vector3>();

                    dic_Edited_NickName_Index.TryGetValue(stringKey, out intkey);

                    Debug.Log("BBBB " + intkey);
                    dic_optiTrack_data_copy.TryGetValue(intkey, out tempVec_List02);
                    //tempVec_List02 = list_first_target_marker;
                    tempVec_List02.Clear();
                    tempVec_List02.AddRange(list_first_target_marker);

                    Debug.Log("BBBB " + tempVec_List02.Count);

                    dic_Edited_optiTrack_data.TryGetValue(stringKey, out tempVec_List01);
                    //tempVec_List01 = list_first_target_marker;
                    tempVec_List01.Clear();
                    tempVec_List01.AddRange(list_first_target_marker);

                    dic_Edited_NickName_Index.TryGetValue(stringKey, out intkey);



                    //list_EachMarkerInfoPanel.RemoveAt(intkey);
                    //list_marker_InfoPanel_Button.RemoveAt(intkey);
                    //Destroy(list_EachMarkerInfoPanel_Title[intkey]);
                    //Destroy(list_marker_InfoPanel_Button[intkey]);

                    //int childDestIndex = 0;
                    //for (int i = 0; i < list_EachMarkerInfoPanel[intkey].transform.childCount; i++)
                    //{
                    //    if(list_EachMarkerInfoPanel[intkey].transform.GetChild(childDestIndex).tag == "MarkerDataButton")
                    //    {
                    //        Destroy(list_EachMarkerInfoPanel[intkey].transform.GetChild(childDestIndex).gameObject);
                    //    }
                    //    else
                    //    {
                    //        childDestIndex++;
                    //    }

                    //}

                    List<GameObject> list_destroyGO = new List<GameObject>(); 

                    for (int i = 0; i < list_EachMarkerInfoPanel[intkey].transform.childCount; i++)
                    {
                        if (list_EachMarkerInfoPanel[intkey].transform.GetChild(i).tag == "MarkerDataButton")
                        {
                            list_destroyGO.Add(list_EachMarkerInfoPanel[intkey].transform.GetChild(i).gameObject);
                        }

                    }

                    foreach (var item in list_destroyGO)
                    {
                        Destroy(item);
                    }

                    tempb_isnew = false;
                }
                else
                {
                    intkey = list_EachMarkerInfoPanel.Count;

                    dic_Edited_optiTrack_data.Add(stringKey, list_first_target_marker);
                    dic_Edited_NickName_Index.Add(stringKey, intkey);
                    dic_optiTrack_data_copy.Add(intkey, list_first_target_marker);

                    Debug.Log("AAAA " + intkey);

                    tempb_isnew = true;
                }

                foreach (var item in dic_Edited_optiTrack_data.Keys)
                {
                    Debug.Log(item.ToString());
                }



                int panelMargin = 10;
                int panelWidth = totalEntryCount + (panelMargin * 2);

                float markerPanelFirstHeight = panelMargin + (1.5f * panelMargin);



                int panelHeight = ((list_EachMarkerInfoPanel.Count+1) * 3 * panelMargin) + (panelMargin * 4);
                //Debug.Log("aaaaaaaaaaaa   " + list_EachMarkerInfoPanel.Count);

                for (int i = 0; i < list_EachMarkerInfoPanel.Count; i++)
                {
                    list_EachMarkerInfoPanel[i].GetComponent<RectTransform>().SetParent(transform);
                    list_EachMarkerInfoPanel_Title[i].GetComponent<RectTransform>().SetParent(transform);
                }

                markerInfo_Display_Panel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);

                for (int i = 0; i < list_EachMarkerInfoPanel.Count; i++)
                {
                    list_EachMarkerInfoPanel[i].GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
                    list_EachMarkerInfoPanel_Title[i].GetComponent<RectTransform>().SetParent(list_EachMarkerInfoPanel[i].transform);
                }

                if (tempb_isnew)
                {
                    GameObject tempGO = Instantiate(each_MarkerInfopanel_pref);
                    //tempGO.transform.parent = markerInfo_Display_Panel.transform;
                    tempGO.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
                    tempGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth - (panelMargin * 2));
                    tempGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));
                    //tempGO.GetComponent<RectTransform>().localPosition = new Vector3(panelWidth / 2, -(panelHeight / 2), 0.0f);
                    tempGO.GetComponent<RectTransform>().localPosition = new Vector3(panelWidth / 2, -(markerPanelFirstHeight + (intkey * (panelMargin * 3))), 0.0f);
                    tempGO.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    //tempGO.GetComponent<RectTransform>().rect. = Vector2.zero;
                    tempGO.name = stringKey + "DataFrame";



                    GameObject tempGO02 = Instantiate(each_MarkerInfoTitle_pref);
                    tempGO02.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
                    tempGO02.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);
                    tempGO02.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));
                    tempGO02.GetComponent<RectTransform>().localPosition = new Vector3(75, -(markerPanelFirstHeight + (intkey * (panelMargin * 3))), 0.0f);
                    tempGO02.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    tempGO02.GetComponent<TextMeshProUGUI>().text = marker_Nicknaming_Table.transform.GetChild(0).GetComponent<TMP_Text>().text;
                    tempGO02.GetComponent<RectTransform>().SetParent(tempGO.transform);
                    tempGO02.name = stringKey + "Title";

                    list_EachMarkerInfoPanel.Add(tempGO);
                    list_EachMarkerInfoPanel_Title.Add(tempGO02);

                    GameObject tempGO03 = Instantiate(NNmarker_Render_Sphere_Pref, Vector3.zero, Quaternion.identity);
                    //tempGO.GetComponent<MeshRenderer>().enabled = false;
                    tempGO03.GetComponent<OptiTrackMarkerInterface>().Set_Marker_Info(intkey, stringKey);
                    tempGO03.name = "Marker " + stringKey + " " + intkey;

                    originalNNMarkerMaterial = tempGO03.GetComponent<MeshRenderer>().material;
                    list_marker_Render_Sphere.Add(tempGO03);
                    totalmarkerCount++;

                    Toggle_Render_Marker(intkey, false);
                }
                //if(!tempb_isnew)
                //{
                //    foreach (var item in list_first_target_marker)
                //    {
                //        Debug.Log(item.ToString());
                //    }
                //}


                //bool isMakingButton = false;
                int buttonSize = 0;

                List<Vector3> list_edited_data = new List<Vector3>();
                dic_Edited_optiTrack_data.TryGetValue(stringKey, out list_edited_data);

                for (int j = 0; j < totalEntryCount; j++) // current entry = j
                {
                    //if (!tempb_isnew)
                    //{
                    //    Debug.Log("AAAAAAA");
                    //}


                    //if (list_first_target_marker[j] != Vector3.zero) // true data
                    if (list_edited_data[j] != Vector3.zero) // true data
                    {
                        buttonSize++;

                        //if (!tempb_isnew)
                        //{
                        //    Debug.Log("BBBBBBBB");
                        //}
                    }
                    else
                    {
                        if (buttonSize > 0)
                        {
                            GameObject m_button = Instantiate(each_MarkerInfoButton_pref);
                            m_button.GetComponent<RectTransform>().SetParent(list_EachMarkerInfoPanel[intkey].transform);
                            m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSize);
                            m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));

                            float aaaaaaaaaa = (-(panelWidth / 2) + panelMargin) + (j - (buttonSize / 2));
                            m_button.GetComponent<RectTransform>().localPosition = new Vector3(aaaaaaaaaa, 0.0f, 0.0f);
                            m_button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            m_button.GetComponent<MarkerInfoPanelButton>().Set_Marker_Info(intkey);
                            m_button.tag = "MarkerDataButton";
                            list_marker_InfoPanel_Button.Add(m_button);

                            buttonSize = 0;

                            //if (!tempb_isnew)
                            //{
                            //    Debug.Log("CCCCCCCCCCC");
                            //}
                        }
                        else
                        {

                        }
                    }


                }

                if (buttonSize > 0)
                {
                    GameObject m_button = Instantiate(each_MarkerInfoButton_pref);
                    m_button.GetComponent<RectTransform>().SetParent(list_EachMarkerInfoPanel[intkey].transform);
                    m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSize);
                    m_button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelMargin * 3));
                    float aaaaaaaaaa = (-(panelWidth / 2) + panelMargin) + (totalEntryCount - (buttonSize / 2));
                    m_button.GetComponent<RectTransform>().localPosition = new Vector3(aaaaaaaaaa, 0.0f, 0.0f);
                    m_button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    m_button.GetComponent<MarkerInfoPanelButton>().Set_Marker_Info(intkey);
                    list_marker_InfoPanel_Button.Add(m_button);

                    buttonSize = 0;

                    //if (!tempb_isnew)
                    //{
                    //    Debug.Log("DDDDDDDDDD");
                    //}
                }


                section_Slider01.GetComponent<RectTransform>().SetParent(transform);
                section_Slider01.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);
                section_Slider02.GetComponent<RectTransform>().SetParent(transform);
                section_Slider02.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);

                time_Slider.GetComponent<RectTransform>().SetParent(transform);
                time_Slider.GetComponent<RectTransform>().SetParent(markerInfo_Display_Panel.transform);




            }
        }

        private void Save_Edited_Data()
        {
            Queue<string> EditedQueue = new Queue<string>();
            List<string> _NicknamedMarkerCount = new List<string>();

            foreach (var item in dic_Edited_NickName_Index.Keys)
            {
                _NicknamedMarkerCount.Add(item);
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < totalEntryCount; i++)
            {
                sb.Clear();

                for (int j = 0; j < _NicknamedMarkerCount.Count; j++)
                {
                    if (dic_Edited_optiTrack_data.ContainsKey(_NicknamedMarkerCount[j]))
                    {
                        List<Vector3> Vecdata = new List<Vector3>();
                        dic_Edited_optiTrack_data.TryGetValue(_NicknamedMarkerCount[j], out Vecdata);

                        sb.AppendFormat("{0:F6}", Vecdata[i].x).Append(',');
                        sb.AppendFormat("{0:F6}", Vecdata[i].y).Append(',');
                        sb.AppendFormat("{0:F6}", Vecdata[i].z).Append(',');
                    }
                }

                if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                EditedQueue.Enqueue("||" + sb.ToString());
            }
          



            GM_DataLoader.instance.WriteEditedData_Batch(ref EditedQueue, ref _NicknamedMarkerCount);
        }

    }
}