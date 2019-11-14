using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HEVS;

public class FaceHoloLens : MonoBehaviour
{
    public Transform holoLens;

    private void Start()
    {
        if (Cluster.isMaster)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void Update()
    {
        transform.LookAt(holoLens);
    }
}
