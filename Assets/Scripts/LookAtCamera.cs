using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform cameraTrans = null;

    private void Start()
    {
        cameraTrans = Camera.main.transform;
    }

    void Update()
    {
        transform.LookAt( cameraTrans );
    }
}
