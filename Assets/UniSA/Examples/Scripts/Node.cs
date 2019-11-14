using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public string text;

    public TextMesh textMesh;

    public HEVS.ClusterBool found = new HEVS.ClusterBool(false);
    public bool actualFound = false;

    public void Awake()
    {
        if (string.IsNullOrEmpty(text))
        {
            float random = Random.value * 8;
            if (random < 1f) text = "A";
            else if (random < 2f) text = "B";
            else if (random < 3f) text = "C";
            else if (random < 4f) text = "D";
            else if (random < 5f) text = "E";
            else if (random < 6f) text = "F";
            else if (random < 7f) text = "G";
            else text = "H";
        }
    }

    public void Start()
    {
        if (HEVS.Cluster.isMaster)
        {
            textMesh = new GameObject().AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.transform.SetParent(transform, false);
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.transform.LookAt(transform.parent.position);
            textMesh.transform.Rotate(Vector3.up, 180f);
            textMesh.fontSize = 48;
            textMesh.transform.localScale /= 4;
        }
    }

    public void Update()
    {
        if (HEVS.Cluster.isMaster)
        {
            textMesh.gameObject.SetActive(found.GetValue());
            textMesh.transform.LookAt(transform.parent.position);
            textMesh.transform.Rotate(Vector3.up, 180f);
        }
    }
}
