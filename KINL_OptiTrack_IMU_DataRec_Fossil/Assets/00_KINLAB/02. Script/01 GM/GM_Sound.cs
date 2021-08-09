using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KINLAB
{
    namespace _KINLAB_IMU
    {
        public class GM_Sound : MonoBehaviour
        {
            public static GM_Sound instance = null;

            private float soundVolume = 1.0f;

            private bool isSoundEffectMute = false;

            [SerializeField]
            private List<AudioClip> SEF_List = new List<AudioClip>();

            //-------------------------

            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;
                }
            }

            public void SoundEffect_by_Index(Vector3 _pos, int _index)
            {
                if (isSoundEffectMute)
                    return;

                GameObject soundObject = new GameObject();
                soundObject.transform.position = _pos;

                AudioSource audioSource = soundObject.AddComponent<AudioSource>();
                audioSource.clip = SEF_List[_index];
                audioSource.minDistance = 5.0f;
                audioSource.maxDistance = 10.0f;
                audioSource.volume = soundVolume;

                audioSource.Play();

                //Debug.Log("Play Sound");

                Destroy(soundObject, SEF_List[_index].length);
            }
        }
    }
}