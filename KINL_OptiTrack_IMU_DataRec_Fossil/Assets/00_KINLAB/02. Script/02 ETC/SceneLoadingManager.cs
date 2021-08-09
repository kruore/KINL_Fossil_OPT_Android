using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
namespace _KINLAB
{
    public class SceneLoadingManager : MonoBehaviour
    {
        public string nextSceneName = string.Empty;

        public float delayTime = 0.0f;

        private void Start()
        {
            StartCoroutine(DelayLoadScene());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        private IEnumerator DelayLoadScene()
        {
            if (delayTime != 0.0f)
            {
                if (nextSceneName != string.Empty)
                {
                    yield return new WaitForSeconds(delayTime);
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    yield return new WaitForSeconds(delayTime);
                    Application.Quit();
                }
            }
            else
            {
                yield break;
            }
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}