using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace _KINLAB
{
    public class OptiTrackMarkerInterface : MonoBehaviour
    {
        [SerializeField]
        private Text marker_name_index;

        private int marker_index = 0;

        ///------------------------

        public void Set_Marker_Info(int _index, string _markerName)
        {
            marker_index = _index;

            if(_markerName == string.Empty)
            {
                marker_name_index.text = "Unlabeled Marker " + _index;
            }
            else
            {
                marker_name_index.text = _markerName;
            }

        }

        public void Toggle_Render_MarkerName(bool _enable)
        {
            marker_name_index.enabled = _enable;
        }

        private void OnMouseDown()
        {
            Debug.Log("This Marker is Unlabed Marker " + marker_index);
        }
    }
}