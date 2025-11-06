using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{

    public List<Material> mats;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("Randomize Materials")]
    void RandomMaterial()
    {
        foreach (MeshRenderer m in transform.GetComponentsInChildren<MeshRenderer>()) {
            m.material = mats[Random.Range(0, mats.Count)];
        }
    }
}
