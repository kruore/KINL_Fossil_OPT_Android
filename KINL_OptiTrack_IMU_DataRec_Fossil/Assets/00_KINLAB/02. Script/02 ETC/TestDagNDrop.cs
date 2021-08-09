using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KINLAB
{
    public class TestDagNDrop : MonoBehaviour
    {

        private Vector3 mOffset;
        private float mZCoor;

        private void OnMouseDown()
        {
            mZCoor = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

            mOffset = gameObject.transform.position - GetMouseWorldPos();
        }
        private void OnMouseDrag()
        {
            transform.position = GetMouseWorldPos() + mOffset;
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;

            mousePoint.z = mZCoor;

            return Camera.main.ScreenToWorldPoint(mousePoint);
        }


    }
}