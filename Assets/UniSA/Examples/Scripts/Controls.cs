using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    private Node[] _nodes;

    public string navigate;
    public string navigateX;
    public string navigateY;

    public Material original;
    public Material found;

    public Transform hololens;

    private void Start()
    {
        _nodes = GetComponentsInChildren<Node>();
    }

    void Update()
    {
        Input();

        FindNodes();

        StyleNodes();
    }

    private void Input()
    {
        if (HEVS.Cluster.isMaster)
        {
            float scale = 1 / Vector3.Distance(transform.position, hololens.position) * 80f;

            if (HEVS.Input.GetButton(navigate))
            {
                transform.RotateAround(transform.position, hololens.up, -HEVS.Input.GetAxis(navigateX) * scale);
                transform.RotateAround(transform.position, hololens.right, HEVS.Input.GetAxis(navigateY) * scale);
            }
        }
    }

    private void FindNodes()
    {
        foreach (Node node in _nodes)
            node.actualFound = false;

        foreach (Camera camera in Camera.allCameras)
        {
            foreach (Node node in _nodes)
            {
                if (node.actualFound) continue;

                Vector3 pos = camera.WorldToViewportPoint(node.transform.position);

                if (pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f)
                    node.actualFound = true;
            }
        }
    }

    private void StyleNodes()
    {
        foreach (Node node in _nodes)
        {
            if (HEVS.Cluster.isMaster)
                node.found.SetValue(node.actualFound);

            var renderer = node.GetComponent<MeshRenderer>();

            if (node.found.GetValue())
            {
                if (renderer.sharedMaterial != found)
                    renderer.sharedMaterial = found;
            }
            else
            {
                if (renderer.sharedMaterial != original)
                    renderer.sharedMaterial = original;
            }
        }
    }
}
