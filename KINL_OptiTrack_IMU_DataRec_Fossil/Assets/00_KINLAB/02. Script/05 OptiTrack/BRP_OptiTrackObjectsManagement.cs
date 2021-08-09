using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;

namespace _3DCL
{
    public class BRP_OptiTrackObjectsManagement : MonoBehaviour
    {
        public static BRP_OptiTrackObjectsManagement instance = null;

        [SerializeField]
        private GameObject Avatar01;

        [SerializeField]
        private GameObject Avatar02;

        [SerializeField]
        private Transform Avatar01_Origin;

        [SerializeField]
        private Transform Avatar02_Origin;

        [SerializeField]
        private GameObject Box01;

        //[SerializeField]
        //private Transform Box01_Origin;

        [SerializeField]
        private GameObject vive_Tracker_Device06;

        [SerializeField]
        private GameObject environment_world;

        [SerializeField]
        private OptitrackStreamingClient Client;

        //---------------------------------------------------------------------

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                //DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            //Avatar01.SetActive(false);
            //Avatar02.SetActive(false);
            //Box01.SetActive(false);
        }

        private void Update()
        {
            #region Area : Manupulate the offsets between The VR World and The OptiTrack World 
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                transform.Translate(transform.forward * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                transform.Translate(-transform.forward * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                transform.Translate(-transform.right * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                transform.Translate(transform.right * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                transform.Translate(transform.up * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                transform.Translate(-transform.up * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Vector3 tempV = vive_Tracker_Device06.transform.position - Box01.transform.position;
                tempV.y -= 0.07f;
                transform.position += tempV;
            }

            if(Input.GetKeyDown(KeyCode.PageUp))
            {
                environment_world.transform.Translate(transform.up * 0.01f);
            }

            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                environment_world.transform.Translate(-transform.up * 0.01f);
            }
            #endregion

            #region Area : OptiTrack Objects on/off
            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (Avatar01.activeSelf)
                {
                    Avatar01.SetActive(false);
                }
                else
                {
                    Avatar01.SetActive(true);
                }

                if (Avatar02.activeSelf)
                {
                    Avatar02.SetActive(false);
                }
                else
                {
                    Avatar02.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                if (Box01.activeSelf)
                {
                    Box01.SetActive(false);
                }
                else
                {
                    Box01.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                //Avatar01_Origin.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                //Avatar02_Origin.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
            }


            #endregion


            #region Area : Manupulate the offsets (for only Box) between The VR World and The OptiTrack World 
            //if (Input.GetKeyDown(KeyCode.W))
            //{
            //    Box01_Origin.Translate(transform.forward * 0.01f);
            //}

            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    Box01_Origin.Translate(-transform.forward * 0.01f);
            //}

            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    Box01_Origin.Translate(-transform.right * 0.01f);
            //}

            //if (Input.GetKeyDown(KeyCode.D))
            //{
            //    Box01_Origin.Translate(transform.right * 0.01f);
            //}

            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    Box01_Origin.Translate(transform.up * 0.01f);
            //}

            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    Box01_Origin.Translate(-transform.up * 0.01f);
            //}
            #endregion


            ///
            //string tempstr = string.Empty;
            //StringBuilder sb = new StringBuilder();

            //foreach (var item in Client.m_latestLabledMarkerStates)
            //{
            //    //tempstr += " @ " + item.Position.x + "/" + item.Position.y + "/" + item.Position.z;
            //    sb.AppendFormat("{0:F4}", item.Position.x).Append(',');
            //    sb.AppendFormat("{0:F4}", item.Position.y).Append(',');
            //    sb.AppendFormat("{0:F4}", item.Position.z).Append(',');
            //}

            //Debug.Log("Count : " + Client.m_latestLabledMarkerStates.Count + "###" + tempstr.ToString());
        }








    }
}