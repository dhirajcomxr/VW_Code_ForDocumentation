using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ObjDirReferenceFinder : MonoBehaviour
{
    [SerializeField] string dirRef;
    [SerializeField] bool findIt,refIt;
    [SerializeField] StoreStepGo ip, op;
    [System.Serializable]
    public class StoreStepGo
    {
        public GameObject f1;
        public GameObject f2;
        public GameObject f3;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (findIt)
        {
            findIt = false;

            op = JsonUtility.FromJson<StoreStepGo>(dirRef);
        }
        if (refIt)
        {
            refIt = false;
            dirRef = JsonUtility.ToJson(ip);
        }
    }
}
