using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class LabelLine : MonoBehaviour
{
   public LineRenderer line;

    public Transform point;
    public GameObject label;
    private void Reset()
    {
        line = GetComponent<LineRenderer>();
        if (!line)
            line = gameObject.AddComponent<LineRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!line)
            line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        if (point != null && label != null)
            line.SetPositions(new Vector3[] { point.position, label.transform.position });
    }

    // Update is called once per frame
    void Update()
    {
        if(point!=null && label!=null)
            line.SetPositions(new Vector3[] { point.position, label.transform.position });
    }
}
