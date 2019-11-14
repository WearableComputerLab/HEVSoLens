using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HEVS;
using HEVS.UniSA;
using System.Linq;

public class DrawOutlines : MonoBehaviour
{
    public List<string> ids = new List<string>();
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        if (Cluster.isMaster)
        {
            gameObject.SetActive(false);
            return;
        }

        foreach (string id in ids)
        {
            LineRenderer outline = new GameObject().AddComponent<LineRenderer>();
            outline.transform.SetParent(transform);
            outline.material = material;
            outline.receiveShadows = false;
            outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            DisplayConfig disp = PlatformConfig.current.displays.Find(i => i.id == id);
            WindowOffAxisDisplay offAxis = disp.displayRig as WindowOffAxisDisplay;
            outline.startWidth = outline.endWidth = 0.1f;
            outline.numCornerVertices = 4;
            outline.positionCount = 5;
            outline.SetPositions(new Vector3[] { disp.transformOffset.TransformPoint(offAxis.display.ll), disp.transformOffset.TransformPoint(offAxis.display.ul),
                disp.transformOffset.TransformPoint(offAxis.display.ur), disp.transformOffset.TransformPoint(offAxis.display.lr), disp.transformOffset.TransformPoint(offAxis.display.ll) });
        }
    }
}
