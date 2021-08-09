using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KINLAB
{
    public class TempDF : MonoBehaviour
    {
        public static TempDF instance = null;

        [SerializeField]
        GameObject temp01;

        [SerializeField]
        GameObject temp02;

        private void Awake()
        {
            instance = this;
        }

        public void Func01()
        {
            temp01.SetActive(false);
            temp02.SetActive(true);


        }
    }
}