using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class VWSectionModule : MonoBehaviour
{
    [SerializeField] string moduleName = "";
    [SerializeField]
    public TextAsset assemblyFile, dismantlingFile,
        servicablePartsFile, partSequenceFile;
    [SerializeField]
    public Steps stepMain;
    public ExplodedView explodedView;
    public GameObject main;
    public string GetModuleName() => moduleName;
    public void ExplodedView() => RnRMode(false);
    public void RnRView() => RnRMode(true);
    void RnRMode(bool isRnR)
    {
        Debug.Log("Called for RnR: " + isRnR);
        if (stepMain)
    //        if (!stepMain.gameObject.activeSelf && isRnR)
                stepMain.gameObject.SetActive(isRnR);
        if (explodedView)
    //        if (!explodedView.gameObject.activeSelf && !isRnR)
                explodedView.gameObject.SetActive(!isRnR);

        Debug.Log("<color=blue>" + "STEP:" + stepMain.gameObject.activeSelf + "EXP:" + explodedView.gameObject.activeSelf + " </color>");
    }
}
