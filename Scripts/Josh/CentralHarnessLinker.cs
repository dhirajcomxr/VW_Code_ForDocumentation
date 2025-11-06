using mixpanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CentralHarnessLinker : MonoBehaviour
{
    [SerializeField] SearchableListWidget listWidget;
    [SerializeField] ScreenLinker linker;
    [SerializeField] CentralHarnesses harnesses;   
   // [SerializeField] CentralHarnesses Slavia_harnesses;
    [SerializeField] ModuleHarnessManager harnessManager;
    [SerializeField] Button SearchCFDButton;
    //[SerializeField] GameObject Wiring_Harness;
    //[SerializeField] AutoModuleSelectHelper autoModule;
    public List<string> cfdNamesList;
    public List<string> currCfdNameList;
    List<int> currToMainLinker;
    public string selectedFileName = "";
    string curVariant = "";
     [SerializeField] string curCarVariant="";
    [System.Serializable]
    public class CfdHolder
    {
        public string cfd;
        public string fileName;
        public bool active, ambition, style;
    }
    [System.Serializable]
    public class CentralHarnesses
    {
        public CfdHolder[] harnesses;
    }
    public void SetUiData(Button cfdButton,SearchableListWidget _listWidget)
    {
        SearchCFDButton = cfdButton;
        listWidget = _listWidget;
    }
   
    ScreenLinker GetLinker()
    {
        if (linker == null)
            linker = FindObjectOfType<ScreenLinker>();
        return linker;
    }
    // Start is called before the first frame update
    private void OnEnable()
    {
        CheckAndUpdateEngineHarness();
        Debug.Log("[HARNESS] initialising " + gameObject.name,gameObject);
        selectedFileName = "";
       if (curCarVariant != GetLinker().GetSelectedVariant().variant)
            initialiseCfdList();
        //{
        //    curCarVariant = autoModule.carVariantName;
        //    if (curCarVariant != null)
        //    {
        //        initialiseCfdList();
        //    }
        //}
        listWidget.onListSelect.AddListener(SelectFromCurrCFD);
        listWidget.onSearchTextEdit.AddListener(SearchForCfd);
        SearchCFDButton.onClick.AddListener(LoadCentralHarness);
        //Wiring_Harness.GetComponent<UIScreen>().onOpen.AddListener(harnessManager.LoadHarness);
        //Wiring_Harness.GetComponent<UIScreen>().onClose.AddListener(harnessManager.OnBack);

    }
    private void OnDisable()
    {
        listWidget.onListSelect.RemoveListener(SelectFromCurrCFD);
        listWidget.onSearchTextEdit.RemoveListener(SearchForCfd);
        SearchCFDButton.onClick.RemoveListener(LoadCentralHarness);
        //Wiring_Harness.GetComponent<UIScreen>().onOpen.RemoveListener(harnessManager.LoadHarness);
        //Wiring_Harness.GetComponent<UIScreen>().onClose.RemoveListener(harnessManager.OnBack);

    }
    private void Start()
    {
        //Wiring_Harness.GetComponent<UIScreen>().onOpen.AddListener(harnessManager.LoadHarness);
        //Wiring_Harness.GetComponent<UIScreen>().onClose.AddListener(harnessManager.OnBack);
        /*     listWidget.onListSelect.AddListener(SelectFromCurrCFD);
             listWidget.onSearchTextEdit.AddListener(SearchForCfd);
             SearchCFDButton.onClick.AddListener(LoadCentralHarness);*/
        //initialiseCfdList();    
    }
    public string cfdTempName;

    //Select CFD from the dropdown
    public void SelectFromCurrCFD(int listIndex)
    {  
        CfdHolder selectedCfd = harnesses.harnesses[currToMainLinker[listIndex]];
        GetLinker().GetScreenManager().Toast("Selected " + selectedCfd.cfd);
        cfdTempName = selectedCfd.cfd;
       
        selectedFileName = selectedCfd.fileName.Length > 0 ? selectedCfd.fileName : selectedCfd.cfd;
        PlayerPrefs.SetString("cfd",selectedFileName);
       
        if (GetLinker().GetSelectedVariant() != null)
        {
            //string variantName = linker.GetSelectedVariant().variant;
            string[] variantNames = System.Enum.GetNames(typeof(CentralHarnessCSVMapping.variant));

            string variantResult = "";
            for (int i = 0; i < variantNames.Length; i++)
            {
                if (curVariant.ToLower().Contains(variantNames[i]))
                    variantResult = variantNames[i];
            }
            PlayerPrefs.SetString("variant", variantResult);
        }
    }

    public void SendselectedHarnessName()
    {
        //serverResp.response_data.vehicle_details.ModelName
        Mixpanel.Track("CFD Loaded","cfd_name",cfdTempName);
    }

    public void LoadCentralHarness()
    {
        if (selectedFileName.Length > 0)
        {
            //Debug.Log("Loading Central Harness Scene.."+linker.GetSelectedVariant().name.ToString());
         /*   if (linker.GetSelectedVariant().name == "Slavia")
            {
                harnessManager.SelectHarness("Slavia Central Harness");
            }
            else
            {
                harnessManager.SelectHarness("Slavia Central Harness");
            }
            */
            harnessManager.SelectHarness("Central Harness");

            GetLinker().GetScreenManager().SelectScreen(ScreenLinker.ScreenID.WIRING_HARNESS);
            gameObject.SetActive(false);
        }
        else
        {
            GetLinker().GetScreenManager().Toast("please select a CFD to continue");
        }
          
        //   harnessManager.LoadHarness();
    }
    public void SearchForCfd(string term)
    {
        List<string> result = new List<string>();
        List<int> linkerResult = new List<int>();
        string searchTerm = term.ToLower();
        for (int i = 0; i < cfdNamesList.Count; i++)
        {
            if (cfdNamesList[i].ToLower().Contains(searchTerm))
            {
                result.Add(cfdNamesList[i]);
                linkerResult.Add(i);
            }
        }
        currCfdNameList = result;
        currToMainLinker = linkerResult;
        listWidget.LoadResults(currCfdNameList);
    }
    void UpdateListWidget()
    {
        Debug.Log("Loaded " + cfdNamesList.Count + " results");
        listWidget.LoadResults(currCfdNameList);
    }
   void initialiseCfdList()
    {
        curCarVariant = GetLinker().GetSelectedVariant().variant;
        curVariant = GetLinker().GetSelectedVariant().variant;
        curVariant = GetKushaqVariantForTaigun(curVariant);
        Debug.LogError(curVariant+"   Cur Varient   ");
        if (curVariant.Length < 1)
        {
            Debug.Log("<color=blue>Kushaq</color>");
            curVariant = GetLinker().GetSelectedVariant().variant;
        }
        else
            Debug.Log("<color=blue>Taigun</color>");
        bool isActive = false
        , isAmbition=false, isStyle=false;
        if (curVariant.ToLower().Contains("active"))
        {
            curVariant = "active";
            isActive = true;
        }
        if (curVariant.ToLower().Contains("ambition"))
        {
            isAmbition = true;
            curVariant = "ambition";
        }
        if (curVariant.ToLower().Contains("style"))
        {
            isStyle = true;
            curVariant = "style";
        }
        cfdNamesList = new List<string>();
        currToMainLinker = new List<int>();
        for (int i = 0; i < harnesses.harnesses.Length; i++)
        {
            if(isActive)
            if (harnesses.harnesses[i].active)
            {
                cfdNamesList.Add(harnesses.harnesses[i].cfd);
                currToMainLinker.Add(i);
            }
            if (isAmbition)
                if (harnesses.harnesses[i].ambition)
                {
                    cfdNamesList.Add(harnesses.harnesses[i].cfd);
                    currToMainLinker.Add(i);
                }
            if (isStyle)
                if (harnesses.harnesses[i].style)
                {
                    cfdNamesList.Add(harnesses.harnesses[i].cfd);
                    currToMainLinker.Add(i);
                }

        }
        currCfdNameList = cfdNamesList;
        UpdateListWidget();
        Debug.Log("HarnessTesting : Populatig...");     
    }
    string GetKushaqVariantForTaigun(string curVar)
    {
        string cmp = curVar.ToLower();
        if (cmp.Contains("comfort"))
            return "active";
        if (cmp.Contains("high"))
            return "ambition";
        if (cmp.Contains("gt") || cmp.Contains("top"))
            return "style";
        else
            return "";
    }

    public void CheckAndUpdateEngineHarness()
    {        
        Car3d car3D = gameObject.GetComponentInParent<Car3d>();
        foreach (var item in car3D.modules)
        {
            if (item.name.Contains("Engine 1L") && item.isActive)
            {
                foreach (var item1 in harnesses.harnesses)
                {
                    if (item1.cfd.Contains("1L94PinConnector"))
                    {
                        item1.active = true;
                        item1.ambition = true;
                        item1.style = true;
                    }
                    else if (item1.cfd.Contains("1.5L94PinConnector"))
                    {
                        item1.active = false;
                        item1.ambition = false;
                        item1.style = false;
                    }
                }

            }
            else if (item.name.Contains("Engine 1.5L") && item.isActive)
            {
                foreach (var item1 in harnesses.harnesses)
                {
                    if (item1.cfd.Contains("1L94PinConnector"))
                    {
                        item1.active = false;
                        item1.ambition = false;
                        item1.style = false;
                    }
                    else if (item1.cfd.Contains("1.5L94PinConnector"))
                    {
                        item1.active = true;
                        item1.ambition = true;
                        item1.style = true;
                    }
                }
            }
        }
    }
   

}
