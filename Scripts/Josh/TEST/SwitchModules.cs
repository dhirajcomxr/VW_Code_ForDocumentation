using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SwitchModules : MonoBehaviour
{
    public Text dispText;
  public  VWModuleManager moduleManager;
    public VWReferencesManager referencesManager;
    string textString = "Module: ";
    public StepManagerAssignments stepAssignments;
    public int module = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Clean()
    {
        System.GC.Collect();
    }
    public void CycleModule()
    {
        module++;
        if (module >= moduleManager.GetTotalPrefabs())
            module = 0;
        moduleManager.SelectModule(module);
       
        VWSectionModule mod = moduleManager.GetLastLoadedModule();

        if (mod)
        {
            if (dispText)
                dispText.text = textString + mod.GetModuleName();
            if(referencesManager)
            {
                referencesManager.UpdateReferencesTo(mod);
            }
            if (stepAssignments)
            {
                stepAssignments.stepMain = mod.stepMain;
               
            }
            mod.RnRView();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
