using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestReferenceFinder : MonoBehaviour
{
    [SerializeField] TextAsset file;
    [SerializeField] bool DoIT = false;
    [SerializeField] StepsEximProcessor exim;

    [SerializeField] List<Step> testList;
    // Start is called before the first frame update
    private void Reset()
    {
        exim = FindObjectOfType<StepsEximProcessor>();
    }
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (DoIT)
        {
            ObjectReferenceMapper inst = ObjectReferenceMapper.Instance;
            DoIT = false;
            testList = exim.GetStepsFromFile(file);
            //for (int i = 0; i < testList.Count; i++)
            //{
            //    Step s = testList[i];
            //    //   List<GameObject> gos = new List<GameObject>();
            //    s.objsToEnable = ObjectReferenceMapper.Instance.GetGameObjectsFromIds(s.data.objectsToEnableIds);
            //    s.objsToHighlight = ObjectReferenceMapper.Instance.GetGameObjectsFromIds(s.data.objToHighlightIds);
            //    s.objsToDisable = ObjectReferenceMapper.Instance.GetGameObjectsFromIds(s.data.objectsToDisableIds);
            //    if (s.data.cameraPosId != 0)
            //        s.overrideCameraPosition = inst.GetGameObjectFromId(s.data.cameraPosId).transform;
            //    if (s.data.lookAtPosId != 0)
            //        s.lookAtPoint = inst.GetGameObjectFromId(s.data.lookAtPosId).transform;
            //    //foreach( int id in s.data.objectsToEnableIds)
            //    //{
            //    //    gos.Add(ObjectReferenceMapper.Instance.GetGameObjectFromId(id));
            //    //}
            //    //s.objsToEnable = gos.ToArray();
            //    testList[i] = s;
            //}
        }
    }
}
