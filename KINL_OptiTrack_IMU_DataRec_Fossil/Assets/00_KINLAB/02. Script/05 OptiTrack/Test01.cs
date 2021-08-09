using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test01 : MonoBehaviour
{
    [SerializeField]
    private GameObject deviceObj;

    [SerializeField]
    private GameObject deviceChildObj;

    private void FixedUpdate()
    {
        Vector3 tempV = deviceObj.transform.position;
        transform.position = tempV;

        Vector3 tempVV = deviceChildObj.transform.rotation.eulerAngles;
        tempVV.x = 0.0f;

        transform.rotation = Quaternion.Euler(tempVV);
    }
}
