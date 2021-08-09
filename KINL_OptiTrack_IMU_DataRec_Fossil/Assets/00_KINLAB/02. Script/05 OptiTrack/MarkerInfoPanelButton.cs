using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace _KINLAB
{
    public class MarkerInfoPanelButton : MonoBehaviour
    {
        [SerializeField]
        private int marker_index = 0;
        //------------------------

        private void Start()
        {
            transform.GetComponent<Button>().onClick.AddListener
                (
                () =>
                {
                    OnMarkerInfoPanelButtonDown();
                    //otherFunction(); 
                }
                );
        }

        public void Set_Marker_Info(int _index)
        {
            marker_index = _index;
        }

        public void OnSelect(BaseEventData eventData)
        {
            Debug.Log(this.gameObject.name + " was selected");
        }

        private void OnMarkerInfoPanelButtonDown()
        {
            Debug.Log("MarkerInfoPanelButton Index " + marker_index);

            GM_DataEditor.instance.OnSelectMarkerInfoPanelButton(marker_index);

        }
    }
}