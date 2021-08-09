using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace _KINLAB
{
    public class MarkerNicknameTableInterface : MonoBehaviour
    {
        public enum Nickname_Table_Interface_Type { NONE, first_unlabeledMaker_Set_button, second_unlabeledMaker_Set_button, NicknameCasting_button };

        [SerializeField]
        private Nickname_Table_Interface_Type m_button_type;

        private bool isclick = false;

        private ColorBlock original_button_color;
        private ColorBlock selected_button_color;

        //------------------------------------------------

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(delegate { OnClickButton(); });

            original_button_color = selected_button_color = transform.GetComponent<Button>().colors;
            Color newColor = Color.green;
            selected_button_color.selectedColor = newColor;
            selected_button_color.normalColor = newColor;

        }

        private void OnClickButton()
        {
            if(GM_DataEditor.instance != null)
            {
                if(m_button_type == Nickname_Table_Interface_Type.first_unlabeledMaker_Set_button || m_button_type == Nickname_Table_Interface_Type.second_unlabeledMaker_Set_button)
                {
                    if (isclick)
                    {
                        GM_DataEditor.instance.OnClick_MarkerNickSetButton(m_button_type, false);
                        GetComponent<Button>().colors = original_button_color;
                        isclick = false;
                    }
                    else
                    {
                        GM_DataEditor.instance.OnClick_MarkerNickSetButton(m_button_type, true);
                        GetComponent<Button>().colors = selected_button_color;
                        isclick = true;
                    }
                }
                else if(m_button_type == Nickname_Table_Interface_Type.NicknameCasting_button)
                {
                    GM_DataEditor.instance.OnClick_MarkerNickSetButton(Nickname_Table_Interface_Type.NONE, false);
                    GM_DataEditor.instance.OnClick_MarkerNickCast();
                }
                
            }
            
        }

        public void ResetButton()
        {
            isclick = false;
            GetComponent<Button>().colors = original_button_color;
        }

        
    }
}