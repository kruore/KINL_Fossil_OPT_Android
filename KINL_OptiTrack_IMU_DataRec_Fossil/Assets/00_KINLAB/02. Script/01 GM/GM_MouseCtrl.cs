using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KINLAB
{
    public class GM_MouseCtrl : MonoBehaviour
    {
        private float MouseZoomSpeed = 15.0f;
        private float ZoomMinBound = 20f;
        private float ZoomMaxBound = 90f;

        private bool isRightMouse_Hold = false;
        private bool isMouseWheel_Hold = false;

        private float MouseRotSpeed = 6.0f;
        private float MouseMoveSpeed = 0.5f;

        private Camera main_Cam;

        //[SerializeField]
        //private GameObject floor;

        //-------------------------------------
        private void Start()
        {
            main_Cam = GetComponent<Camera>();
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            Zoom(scroll, MouseZoomSpeed);

            if (Input.GetMouseButton(1))
            {
                isRightMouse_Hold = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                isRightMouse_Hold = false;
            }

            if (Input.GetMouseButton(2))
            {
                isMouseWheel_Hold = true;
            }

            if (Input.GetMouseButtonUp(2))
            {
                isMouseWheel_Hold = false;
            }


            if (isRightMouse_Hold)
            {
                ////float moveX = Input.GetAxis("Mouse X");
                ////Debug.Log(moveX.ToString("F3"));

                //Vector3 tempV = transform.localEulerAngles;

                //tempV.x -= MouseRotSpeed * Input.GetAxis("Mouse Y");
                //tempV.y += MouseRotSpeed * Input.GetAxis("Mouse X");

                //transform.localEulerAngles = tempV;



                Vector3 tempV = transform.parent.eulerAngles;

                tempV.x -= MouseRotSpeed * Input.GetAxis("Mouse Y");
                tempV.y += MouseRotSpeed * Input.GetAxis("Mouse X");

                transform.parent.eulerAngles = tempV;
            }

            if (isMouseWheel_Hold)
            {
                //Debug.Log(Input.GetAxis("Mouse Y").ToString("F3")); //위아래 핏치

                Vector3 TemVY = MouseMoveSpeed * Input.GetAxis("Mouse Y") * -transform.up;
                Vector3 TemVX = MouseMoveSpeed * Input.GetAxis("Mouse X") * -transform.right;

                Vector3 tempV = transform.position + TemVY + TemVX;

                transform.position = tempV;
            }

        }

        void Zoom(float deltaMagnitudeDiff, float speed)
        {
            //main_Cam.fieldOfView += deltaMagnitudeDiff * speed;
            //// set min and max value of Clamp function upon your requirement
            //main_Cam.fieldOfView = Mathf.Clamp(main_Cam.fieldOfView, ZoomMinBound, ZoomMaxBound);

            transform.Translate(transform.forward * deltaMagnitudeDiff, Space.World);
        }




    }
}