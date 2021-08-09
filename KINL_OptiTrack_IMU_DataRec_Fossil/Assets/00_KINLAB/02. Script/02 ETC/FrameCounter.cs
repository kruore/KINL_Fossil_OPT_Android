using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace _KINLAB
{
    public class FrameCounter : MonoBehaviour
    {

        private bool isClearSecond = false;
        private int frameCount = 0;
        private float seconds = 0.0f;

        public Text frameText;

        private bool tempb = false;

        float deltaTime = 0.0f;

        // Update is called once per frame
        void Update()
        {
            //if(tem)

            //while (!isClearSecond)
            //{
            //    frameCount++;
            //    frameText.text = "FPS : " +  frameCount.ToString();

            //    seconds += Time.deltaTime;

            //    if (seconds >= 1.0f)
            //    {
            //        frameCount = 0;
            //        seconds = 0.0f;
            //        isClearSecond = true;
            //    }
            //}

            //isClearSecond = false;

            //Debug.Log("UPDATE: Frame count = " + Time.frameCount + "\nRendered frame count = " + Time.renderedFrameCount);

            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;

            frameText.text = "FPS Rate : " + ((int)fps).ToString();
        }
    }
}