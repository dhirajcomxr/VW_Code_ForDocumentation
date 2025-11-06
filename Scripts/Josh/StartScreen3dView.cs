using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mixpanel;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class StartScreen3dView : MonoBehaviour
{
    #region Public Variables
    [Header("UI")]
    [SerializeField] bool serverUpdate = true;
    [SerializeField] GameObject carGo;
    [SerializeField] Car3d[] allCars;
    [SerializeField] Car3d currentCar;
    [SerializeField] Material mat;
    [SerializeField] GameObject sureDialog, selectSubModuleDialog;
    [SerializeField] ImageLabelList submoduleList;
    [SerializeField] GameObject[] enableOnSelection;
    [SerializeField] GameObject[] disableOnSelection;
    [SerializeField] Text display_PartName, display_DialogText, display_ViewButtonText;
    [SerializeField] Button[] sectionButtons;
    [SerializeField] Dropdown dropdown;
    [Space(20)]
    [SerializeField] ModuleGroups moduleGroups;
    [SerializeField] List<Car3d.CarModules3D> modules, curModules;
    [SerializeField] GameObject[] sections;
    [SerializeField] int curSection, lastModule;
    [Header("Settings")]
    [SerializeField] UIScreenManager screen;
    [SerializeField] VWModuleManager vwm;
    [SerializeField] ModuleHarnessManager harnessManager;
    [SerializeField] AssetLoader assetLoader;
    [SerializeField] ExteriorCam cam;
    [SerializeField] Transform startPoint3d, camPivot;
    [SerializeField] float threshold = 1f;
    [SerializeField] PartHighlighter highlighter;
    [SerializeField] ScreenLinker linker;
    [Header("Fade Settings")]
    [SerializeField] Vector2 fadeValMinMax;
    [SerializeField] float fadeSpeed;
    bool fade = false;
    [SerializeField] float fadeStat = 0, nextFadeVal;
    [SerializeField] int selectedModule = -1, lastOpenedModule;
    [SerializeField] int[] selectedModuleSubModules;
    string[] selectedModuleNames;
    [SerializeField] Renderer[] fadeObjs;
    [SerializeField] float dist = -1;
    public SearchableDTList DtList;
    [SerializeField] HarnessLinkData harnessLinkData;
    public GameObject networkWarnning, sceneSpecific;

    #endregion
    int function = -1;
    string curCarName = "";
    int FUNCTION_RNR = 1, FUNCTION_DT = 2, FUNCTION_WH = 3;

    string functionName = "";
    public string modName = "";
    public Dropdown hvacDropdown;
    public string selectHvacName = "";
    [System.Serializable]
    public struct HarnessLinkData
    {
        public SearchableListWidget listWidget;
        public Button searchCFDButton;
    }
  
  [SerializeField]  bool allow3DSelection = false;
 
    [System.Serializable]
    public struct ModuleGroups
    {
        public string[] engines;
        public string[] transmissions;
        public string[] hvac;
    }
    #region Car Selection
        public void SelectCarWithName(string carName)
    {
        bool result = CarExist(carName);
        if (result)
            SelectCar(GetCarWithName(carName));

    }
    public bool HasCurrentCar()
    {
        bool result = false;
        if (currentCar != null)
            result = true;
        return result;
    }
    public void SelectCar(Car3d car)
    {
        bool loadNew = true;
       
        if (currentCar)
            if (currentCar.carName == car.carName)
                loadNew = false;
        if(loadNew)
            {
                for (int i = 0; i < allCars.Length; i++)
                {
                    if (allCars[i] != car || car == null)
                    {
                        allCars[i].gameObject.SetActive(false);
                    }
                }
                InitialiseCar(car);
                assetLoader.LoadAllSceneAddresses();
            }

    }
    void InitialiseCar(Car3d car)
    {
        if (car != null)
        {
            if (currentCar != null)
                Destroy(currentCar.gameObject);
            currentCar = Instantiate(car, transform);
            //     currentCar = car;
            currentCar.gameObject.SetActive(true);
            carGo = currentCar.car;
            modules = currentCar.modules;
            sections = currentCar.sections;
            assetLoader.andCatLinkDev = currentCar.catalogAddressDEV;
            assetLoader.andCatLinkStg = currentCar.catalogAddressSTAG;
            assetLoader.andCatLinkPro = currentCar.catalogAddressPROD;
            assetLoader.nameAddresses = currentCar.nameAddresses;
          
            curCarName = currentCar.carName;
            harnessManager = currentCar.harness.GetComponent<ModuleHarnessManager>();
            harnessManager.startScreen3D = this;
            harnessManager.centralHarnessLinker.SetUiData(harnessLinkData.searchCFDButton, harnessLinkData.listWidget);
                    carGo.SetActive(true);
            currentCar.harness.SetActive(false);
            assetLoader.InitConfigs();
            SetCameraFocus(currentCar.transform.position);
            Show3DViewDefaultSelection();
        }
        else
            Debug.LogError("[CAR] No Car to Initialise", gameObject);
    }
    public void SetHarnessState(bool state)
    {
        if (currentCar)
        {
            currentCar.harness.SetActive(state);
            currentCar.harness.transform.Find("Central Harness").gameObject.SetActive(state);
        }
        else
            Debug.LogError("[EXCEPTION] No Car Selected");
    }
    public void LoadHarness()
    {        
        if (harnessManager)
        {
            if (assetLoader.bundleLoader != null)
            {
                assetLoader.bundleLoader.Clear();
            }
            harnessManager.LoadHarness();
        }
    }
    public void ExitHarness()
    {
        if (harnessManager)
        {
            harnessManager.OnBack();
        }
    }
    public bool CarExist(string carName)
    {
        bool result=false;
        if (GetCarId(carName) >= 0)
            result = true;
        return result;
    }
    public int GetCarId(string carName)
    {
        int result = -1;
        if (carName.Length > 0)
        {
            for (int i = 0; i < allCars.Length; i++)
            {
                if (allCars[i].carName.ToLower().Contains(carName.ToLower()))
                    result = i;
            }
        }
        return result;
    }
    public Car3d GetCarWithName(string carName)
    {
        Car3d result = null;
        int carId = GetCarId(carName);
        if (carId >= 0)
            return allCars[carId];
        
        return result;
    }
    public void CopyToCar()
    {
        currentCar.modules = modules;
        currentCar.sections = sections;
        
    }
    public void CopyNameAddressesToCar()
    {
        currentCar.nameAddresses = assetLoader.nameAddresses;
        Debug.Log("Copy Completed for "+currentCar.carName);
    }
    #endregion
 

    public string GetCurrentCarName()
    {
        if (currentCar != null)
            return currentCar.carName;
        else
            return "";
    }
    public void Allow3DSelection()
    {
        allow3DSelection = true;
        Debug.Log("Allow 3d Selection: " + allow3DSelection);
    }

    public void Allow3DSelectionIn(float secondTime)
    {
        Invoke("Allow3DSelection", secondTime);
    }
    public void Disable3DSelection()
    {
        allow3DSelection = false;
        Debug.Log("Allow 3d Selection: " + allow3DSelection);
    }

    public int GetModuleIdFor(string moduleName)
    {
        int result = -1;
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i].name.Contains(moduleName))
                result = i;
        }
        if (result < 0)
            Debug.Log("Did not find :" + moduleName);
        return result;
    }
    public void InitialiseDropdown()
    {
        selectSubModuleDialog.SetActive(false);
        CheckAvailableModules();
        PopulateDropdown();
        EnableAvailableModules();
    }
    void EnableAvailableModules()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            foreach (GameObject item in modules[i].objects)
            {
                item.SetActive(modules[i].displayModule);
            }
        }
    }
   
    void PopulateDropdown()
    {
         curModules = new List<Car3d.CarModules3D>();
        List<string> dropdownOptions = new List<string>();
        dropdownOptions.Add("Select to Load");
            for (int i = 0; i < modules.Count; i++)
            {
            if (modules[i].isActive)
            {
                curModules.Add(modules[i]);
                dropdownOptions.Add(modules[i].name);
            }
            }
        dropdown.ClearOptions();
        dropdown.AddOptions(dropdownOptions);
    }
    
    public bool IsDropdownLoaded()
    {
        bool result = false;
        if (dropdown != null)
            if (dropdown.options.Count > 0)
                result = true;
        return result;
    }
    bool selectedViaDropdown = false;
    string curModule = "";

    //Select any available module through drop down
    public void SelectViaDropdown(int sel)
    {
       
        Debug.Log("Select Via DropDown....ID"+sel+"    module count "+ dropdown.options[sel].text);
        selectedViaDropdown = true;
             string  dropdownSelMod = dropdown.options[sel].text;
            for (int i = 0; i < curModules.Count; i++)
            {
                if (dropdownSelMod == curModules[i].name)
                {
                    curModule = curModules[i].name;
                    selectedModule = i;
                    Debug.Log("Selected: " + curModules[i].name.ToString() + " via dropdown............"+selectedModule);
                }
                else if(dropdownSelMod == "Select to Load")
                {
                    Reset3DView();
                }
            }

        if (!allow3DSelection)
        {
                SelectModule(curModules[selectedModule].moduleId);
         }
    }

    public void SetHvacValue()
    {
        selectHvacName = hvacDropdown.captionText.text;
        Debug.Log("SELECT HVAC Name....." + selectHvacName);
    }
    public void SelectModuleFromGroups(string engine,string transmission,string hvac)
    {
        Debug.Log("SelectModuleFromGroups");
        SelectActiveModule(engine, moduleGroups.engines);
        SelectActiveModule(transmission, moduleGroups.transmissions);

        Debug.Log("[DEBUG HVAC]" + hvac);
        SelectActiveModule(hvac, moduleGroups.hvac);
    }   
   
    public void SelectActiveModule(int sel, int start, int end)
    {
        Debug.Log("<color=red>[REMOVE]</color>Selected Active Module: " + modules[sel].name);
        for (int i = start; i <= end; i++)
        {
            modules[i].isActive = (i == sel);
            modules[i].displayModule = (i == sel);
       
        }
    
    }

    //After fetching all the data of a vehicle, set it on the application(UI, dropdown etc)
    public void SetModuleGroups(string[] engines,string[] gearboxes,string[] hvacs)
    {
        Debug.Log("[DEBUG HVAC]");
        string allHvacs = "";
        foreach (var item in hvacs)
        {
            allHvacs = allHvacs + ","+item;
        }
        Debug.Log("[DEBUG HVAC]" + allHvacs);
        moduleGroups.engines = engines;
        moduleGroups.transmissions = gearboxes;
        for (int i = 0; i < moduleGroups.transmissions.Length; i++)
        {
            if (moduleGroups.transmissions[i].Contains("DQ"))
                moduleGroups.transmissions[i] = moduleGroups.transmissions[i].Split(' ')[0];
        }
        moduleGroups.hvac = hvacs;
    }

    //Enable or Disable a dummy model of a car which is available  
    public void SelectActiveModule(string selected, string[] disableMods)
    {
        string msg = "Selected " + selected+", Disabled: ";
        Debug.Log("Selected Active Module: " +selected );

        for (int i = 0; i < disableMods.Length; i++)
        {
            Debug.Log(" Disabled:...."+disableMods[i].ToString());
           int disableThis = GetModuleIdFor(disableMods[i]);
            msg +=", "+ disableMods[i];
            if (disableThis >= 0)
            {
                modules[disableThis].isActive = false;
                modules[disableThis].displayModule = false;
            }
            else
                Debug.Log("CANNOT DISABLE " + disableMods[i]);
        }
        int sel = GetModuleIdFor(selected);
        if(sel>=0)
        {
            Debug.Log("<color=blue>[3D Select]</color>Selected: " + selected);
            modules[sel].isActive = true;
            modules[sel].displayModule = true;
        }
        Debug.Log(msg);
    }
    private void Reset()
    {
        cam = FindObjectOfType<ExteriorCam>();
        startPoint3d = transform;
        highlighter = FindObjectOfType<PartHighlighter>();
    }
  
    private void OnEnable()
    {
        if (currentCar)
            SelectCar(currentCar);
        else
            Debug.Log("[CAR]Starting with no current Car");
     
    }
    public void Reset3DView()
    {
        if (cam != null)
        {
           SelectModule(-1);

            if (camPivot != null)
            {
                camPivot.position = startPoint3d.position;
                cam.target = camPivot;
            }
            InitialiseDropdown();
            SetStateOnReset();
        }
    }

    #region Module Debug Functions

    //Module present in a car
    public void CheckAvailableModules()
    {
        List<string> missing = new List<string>();
        for (int i = 0; i < modules.Count; i++)
        {
            int[] id = assetLoader.GetIdsForModuleName(modules[i].name);
            if (id.Length <= 0)
            {
                modules[i].isActive = false;
                missing.Add(modules[i].name);
            }
        }
        foreach (var item in missing)
        {
            Debug.Log("<color=red>Module Not Available:</color>" + item);
        }
    }
    public void CheckModuleNames()
    {

        List<string> modNames = new List<string>(),
            missingMods = new List<string>();

        for (int i = 0; i < modules.Count; i++)
        {
            modNames.Add(modules[i].name);
        }

        missingMods = assetLoader.GetModuleNameMatches(modNames);
     
        if (missingMods.Count > 0)
            foreach (var item in missingMods)
            {
                Debug.Log("<color=red> Missing</color>" + item);
            }
        else
            Debug.Log("Found All Modules");
    }
    public void CheckModuleIds()
    {
        List<string> modNames = new List<string>(),
           missingMods = new List<string>();
        int got = 0, missing = 0;
        for (int i = 0; i < modules.Count; i++)
        {
          int[] result=   assetLoader.GetIdsForModuleName(modules[i].name);
            if (result.Length <= 0)
            {
                missing++;
                missingMods.Add(modules[i].name);
            }
            else
            {
                got++;
                Debug.Log("<color=gray>Found Id for " + modules[i].name + "</color> : " + result);
            }
        }
   

        if (missingMods.Count > 0)
            foreach (var item in missingMods)
            {
                Debug.Log("<color=red> Missing</color>" + item);
            }
        else
            Debug.Log("Found All Module Ids: "+got);
        Debug.Log("Found " + got + " modules, missing id for " + missing + " modules");
    }
    public void CheckMasterModuleIds()
    {
        if(linker==null)
         linker = FindObjectOfType<ScreenLinker>();
        for (int i = 0; i < modules.Count; i++)
        {
            
            if (!linker.HasModuleIdFor(modules[i].name))
                Debug.LogError("Did not find ID for " + modules[i].name);
        }
    }
    public void ResetModuleIds()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].moduleId = i;
        }
        Debug.Log("Done!");
    }
    #endregion

    void Update()
    {
        if (allow3DSelection)
            if(!ExteriorCam.MouseOverUI())
          CheckPointer();

        if (fade)
        { 
            if (fadeStat <= 1f)
            {
                      fadeStat += Time.deltaTime * fadeSpeed;
                      FadeMat(fadeStat, nextFadeVal);
            }
            else
                fade = false;
        }
    }

    Vector3 startPos, endPos,pointerPos;
   [SerializeField] bool pointing = false;

    void CheckPointer()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            pointing = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            // only process the tap if it is shorter than threshold
            endPos = Input.mousePosition;
            float dist = Vector3.Distance(startPos, endPos);
            if (pointing)
            if (Vector3.Distance(startPos, endPos) < threshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(startPos);
                RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        if (hit.collider.gameObject)
                        {
                            OnSelectObject(hit.collider.gameObject);
                        }
                    }

            }
            pointing = false;
        }
    }
    void OnSelectObject(GameObject g)
    {
       
        if (g != null)
        {
            SelectModule(GetModule(g));
        }
       
    }
 
    //Hide the car
    public void HideCar()
    {
        Debug.Log("Hide Car");
        if (currentCar != null)
        {
            currentCar.gameObject.SetActive(false);
            Disable3DSelection();
        }
    }
    public void ShowCar()
    {
        Debug.Log("Show car");
        if (currentCar != null)
        {
            currentCar.gameObject.SetActive(true);
            Allow3DSelection();
        }
    }
    public void OnSureDialogYes()
    {
        //Debug.Log("SURE DIALOG: YES");
        OpenFunction(function);
    }

    //Select different function 
     void OpenFunction(int f)
    {
        int curSection;
        string curCarName;
        switch (function)
        {
            case 1:
                OpenRnRForModule(selectedModule);                
                curSection = modules[selectedModule].section;
                Mixpanel.Track(sectionButtons[curSection].name + "_Selected" );
                Mixpanel.Track("Part_Selected","module_name",modules[selectedModule].name);
                break;
            case 2:
                OpenDT();
                break;
            case 3:
                OpenWireHarness();
                break;
            default:
                break;
        }
        selectedModule = -1;
    }
    public void SelectFunctionAndShowDialog(int f)
    {
        SelectFunction(f);
        if (sureDialog)
            if (modName.Length > 0)
            {
                if (CheckFunctionAvailable(f))
                {
                    sureDialog.SetActive(true);
                    HideCar();
                }
                else
                    screen.Toast(functionName + " not available for " + modules[selectedModule].name);
            }
            else
                Debug.Log("No Module Selected!!");
    }
    public bool CheckFunctionAvailable(int f)
    {
        bool result = false;
        switch (f)
        {
            case 1:
                result = true;
                break;
            case 2:
                result = true;
                break;
            case 3:
                if (harnessManager.HasHarnessFor(modules[selectedModule].name))
                    result = true;
                break;
            default:
                break;
        }
        return result;
    }
    public void SelectFunction(int f)
    {
        string prefix = "<color=blue>";
        string suffix = "</color>";
        function = f;
        string optionName="";
        switch (f)
        {
            case 1:
                optionName = "RnR";
                break;
            case 2:
                optionName = "DT";
                break;
            case 3:
                optionName = "Wire Harness";
                break;
            default:
                break;
        }
       
        functionName = prefix + optionName + suffix;
        SetDisplayText(modName);
        if (display_DialogText)
            display_DialogText.text = "Open " + functionName + " for " + modName;
      
    }

    public void InitSections()
    {
        Debug.Log("Init Sections");
        //    SelectModule(-1);
        SetHarnessState(false);
        dist = 2f;
        SetCameraFocus(startPoint3d.position);
        Invoke("Show3DViewDefaultSelection", 0.5f);
    }
    void Show3DViewDefaultSelection()
    {
        Reset3DView();
        int[] sectionsOn = { 1, 2 };
        for (int i = 0; i < sections.Length; i++)
        {
            sections[i].SetActive(i > 0);
            if (sectionButtons[i])
                if (i > 0)
                {
                    sectionButtons[i].image.color = sectionButtons[i].colors.highlightedColor;
                    sectionButtons[i].GetComponentInChildren<Text>().color = Color.white;
                }
                else
                {
                    sectionButtons[i].image.color = sectionButtons[i].colors.normalColor;
                    sectionButtons[i].GetComponentInChildren<Text>().color = Color.black;
                }
        }
    }
    public void SelectSection(int sectionNumber)
    {         
        EnableSection(sectionNumber);
        FocusModule(-1);
    }
    void EnableSection(int sectionNumber)
    {
        curSection = sectionNumber;
       
        for (int i = 0; i < sections.Length; i++)
        {
            sections[i].SetActive(sectionNumber == i);
            if (sectionButtons[i])
                if (sectionNumber == i)
                {
                    sectionButtons[i].image.color = sectionButtons[i].colors.highlightedColor;
                    sectionButtons[i].GetComponentInChildren<Text>().color = Color.white;
                }
                else
                {
                    sectionButtons[i].image.color = sectionButtons[i].colors.normalColor;
                    sectionButtons[i].GetComponentInChildren<Text>().color = Color.black;
                }
        }

        for (int i = 0; i < sectionButtons.Length; i++)
        {
            if (sectionButtons[i])
                if (sectionNumber == i)
                {
                    sectionButtons[i].image.color = sectionButtons[i].colors.highlightedColor;
                    sectionButtons[i].GetComponentInChildren<Text>().color = Color.white;
                }
                else
                {
                    sectionButtons[i].image.color = sectionButtons[i].colors.normalColor;
                    sectionButtons[i].GetComponentInChildren<Text>().color = Color.black;
                }
        }
    }

    //Select and open module which has more than 1 module in a screen
    public void OpenRnRForSubModule(int s)
    {
        vwm.lastModuleId = s;
        //vwm.isBundleLoaderClosed = true;
        //Debug.Log("CRC : Open Sub Module");
        if (serverUpdate)
        {
            linker.GetModuleLoader().DownloadModule(selectedModuleNames[s], () =>
            {
                Debug.Log("COMPLETED DOWNLOAD");
            });
        }
        else
        if (selectedModuleSubModules.Length > 0)
        {
            int selectedSubmodule = selectedModuleSubModules[s];
            vwm.SelectModule(selectedSubmodule);
            linker.SelectSvrModule("", assetLoader.GetAddressForModule(selectedSubmodule));
            screen.DebugToScreen("Loading " + assetLoader.GetNameForModule(selectedSubmodule));
        }
    }

    //Open the RnR module
    public void OpenRnRForModule(int m)
    {
        Debug.Log("Opening RnR for Module");
        //Debug.Log("CRC : Open Single Module");
        //vwm.isBundleLoaderClosed = false;
        if (m >= 0)
        {
            selectedModuleSubModules = assetLoader.GetIdsForModuleName(modules[m].name); //
            //Debug.Log("Got Module:" + selectedModuleSubModules.Length);

            foreach (var item in selectedModuleSubModules)
            {
                Debug.Log($"Available Module: <b>{assetLoader.GetNameForModule(item)}</b>");
            }

            if (selectedModuleSubModules.Length > 0)
            {
                if (selectedModuleSubModules.Length == 1)
                {
                    vwm.SelectModule(selectedModuleSubModules[0]);
                    linker.SelectSvrModule("", linker.GetAssetLoader().GetAddressForModule(selectedModuleSubModules[0])); //Get an address eg: Asset/Prefab/"modulename".prefab
                    //vwm.lastModuleId = selectedModuleSubModules[0];
                }
                else
                {
                    List<string> options = new List<string>();
                    for (int i = 0; i < selectedModuleSubModules.Length; i++)
                    {
                        options.Add(assetLoader.GetNameForModule(selectedModuleSubModules[i]));
                    }
                    submoduleList.Load(options.ToArray());
                    selectSubModuleDialog.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("Cannot Find Modules Matching this id: " + m);
                vwm.SelectModule(m);
                //vwm.lastModuleId = m;
            }
        }
    }
    void OpenWireHarness()
    {
        if (selectedModule >= 0)
            if (harnessManager.HasHarnessFor(modules[selectedModule].name))
            {
                Debug.LogError(modules[selectedModule].name);
                harnessManager.SelectHarness(modules[selectedModule].name);
                if (screen)
                    screen.SelectScreen(31);
            }
       

    }
    void OpenDT()
    {
        if (screen)
            if (linker.IsCurrentTicketOpen())
            {
                screen.SelectScreen(26);
                DtList.GetSearchDT(modName);
                Mixpanel.Track("DT_List_Fetched");
            }
            else
            {
                screen.GetDialog().Show("Select Vehicle", "Select a Vehicle to Access Diagnostics",
                    new List<UIScreenManager.NamedActions>(){ new UIScreenManager.NamedActions("Cancel",()=>Reset3DView()),
                    new UIScreenManager.NamedActions("Select Vehicle",()=>screen.SelectScreen(ScreenLinker.ScreenID.VIN_INPUT))});
            }
    }
    
    public void SelectAndLoadModule(int m)
    {
        if(m>=0 && m<modules.Count)
        {
            if (screen.GetCurrentScreenIndex() != 15)
                screen.SelectScreen(15);
            SelectModule(m);
        }
    }
    bool updatedDropdown = false;

    //Select a module via dropdown and by clicking on 3D model of the car
    public void SelectModule(int m)
    {

        Debug.Log("Selct Module........."+m);
            selectedModule = m;
  
            if (m >= 0)
            {
                if (dropdown)
                    if (dropdown.value != m)
                    {
                        Debug.Log("VALUE IS :" + m);
                        updatedDropdown = true;
                }
                    else
                        Debug.Log("Updating Via Dropdown: " + modName);

            Debug.Log("     modules     "+ modules[m]);

          

            Car3d.CarModules3D curModule = modules[m];
                modName = curModule.name;
                if (display_PartName)
                display_PartName.text = modName;
                highlighter.RemoveHighLight();
                highlighter.Highlight(curModule.objects);
                dist = GetDistance(curModule.objects);
                SetCameraFocus(Avg(curModule.objects));
                EnableSection(modules[m].section);
                SetStateOnSelection();
                FocusModule(m);

            for (int i=0;i<dropdown.options.Count;i++)
            {
                if (dropdown.options[i].text == curModule.name)
                {
                    dropdown.value = i;
                    dropdown.RefreshShownValue();

                }
            }
        }
            else
            {
                dist = 2;
                highlighter.RemoveHighLight();
                SetDisplayText("");
                SetCameraFocus(startPoint3d.position);
                FadeCar(fadeValMinMax.y);
                FocusModule(-1);
            }
        Debug.Log("3D SEL:"+allow3DSelection+" MOD: " + m+" name:"+modName);

        ShowCar();
    }

    public void SelectModuleV2(int m)
    {
        Car3d.CarModules3D curModule = modules[m];
        modName = curModule.name;
        if (display_PartName)
            display_PartName.text = modName;
        highlighter.RemoveHighLight();
        highlighter.Highlight(curModule.objects);
        dist = GetDistance(curModule.objects);
        SetCameraFocus(Avg(curModule.objects));
        EnableSection(modules[m].section);
        SetStateOnSelection();
        FocusModule(m);
        ShowCar();
    }
    void SetCameraFocus(Vector3 pos)
    {
        float zoomDist = 1f;
        if (dist > 0.1f)
            zoomDist = dist;
        if (camPivot != null)
        {
            camPivot.position = pos;// + Vector3.up * 0.1f;
            if (cam != null)
            {
                cam.SetDistance(zoomDist);
                cam.NewPosRot(cam.transform.rotation *( Vector3.forward*-zoomDist)+camPivot.position, cam.transform.rotation);
            }
        }
    }
    int GetModule(GameObject g)
    {
        int result = -1;
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i].objects.Length>0)
            {
                GameObject[] obj = modules[i].objects;
                for (int o = 0; o < obj.Length; o++)
                {
                    if(obj[o]!=null)
                    if(obj[o]==g)
                        result = i;
                }               
            //    SetAlpha(modules[i].module, 1);
            }
            //          else
            //    SetAlpha(modules[i].module, 0.25f);
        }
        if (result < 0)
            Debug.Log("No Module!");
        else
              if (!modules[result].isActive)
        {
            Debug.Log("<color=yellow>Inactive Module Selected:</color>" + modules[result].name);
            screen.Toast(modules[result].name + " coming soon");
            return -1;
        }
         //   Debug.Log(result);
        return result;  
    }
 
    //On clicking any module in app
    void FocusModule(int m)
    {
        int section = -1;
        if (m >= 0)
            section = modules[m].section;
        else
        {
            if (curSection >= 0)
                section = curSection;
            else
                InitSections();
        }
        if (m >= 0)
        {
            Debug.Log("<color=red>Select Module: </color>" + modules[m].name);
        }
       
      
        for (int i = 0; i < modules.Count; i++)
            {
            GameObject[] objs= modules[i].objects;
            bool activeState = modules[i].section == section && modules[i].displayModule;
            float alpha = i == m ? 1f:0.25f;
            if (m < 0)
            {
                alpha = 1f;
            }
            if (i == m)
            {
                Debug.Log("Selected................. " + modules[i].name);
            }          
                
                SetObjectFade(objs, alpha, activeState);
        }    
    }

    void SetDisplayText(string s)
    {
        if (s.Length > 0)
        {
            if (display_PartName)
                display_PartName.text = s;
            SetStateOnSelection();
        }
        else
        {
            SetStateOnReset();
        }

    }

    #region Helper Functions
    void SetObjectFade(GameObject[] obs,float alpha,bool active)
    {
        for (int ob = 0; ob < obs.Length; ob++)
        {
            obs[ob].SetActive(active);
            Renderer[] rend = obs[ob].GetComponentsInChildren<Renderer>();
            for (int re = 0; re < rend.Length; re++)
            {
                Material[] mats = rend[re].materials;
                for (int ma = 0; ma < mats.Length; ma++)
                {
                    Color c = mats[ma].color;
                    c.a = alpha;
                    mats[ma].color = c;
                }
                rend[re].materials = mats;
            }
        }
    }
    private void FadeMat(float stat, float val)
    {
        Color c =mat.color;
        c.a = Mathf.Lerp(c.a, val, stat);
        mat.color = c;
    }
    float GetDistance(GameObject[] g)
    {
        Bounds b = new Bounds();
        float result = -1;

        for (int i = 0; i < g.Length; i++)
        {
            Renderer r = g[i].GetComponent<Renderer>();
            if (r != null)
                b.Encapsulate(r.bounds);
        }
        result = Mathf.Max(Mathf.Max(b.size.x, b.size.y), b.size.z);
        return result + 0.5f * result;
    }
    int GetIdForDropdown(int sel)
    {
        int result = -1;
        for (int i = 0; i < curModules.Count; i++)
        {
            if (curModules[i].moduleId == sel)
                result = i;
        }
        return result;
    }
    Vector3 Avg(GameObject[] objs)
    {
        Vector3 result = Vector3.zero;
        if (objs.Length > 0)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                result += objs[i].transform.position;
            }
            result = result / objs.Length;
        }
        return result;
    }
    private void FadeCar(float newFadeVal)
    {
        fadeObjs = carGo.GetComponentsInChildren<Renderer>();
        fade = true;
        fadeStat = 0;
        nextFadeVal = newFadeVal;
    }
    void SetStateOnSelection()
    {
        SetStateFor(enableOnSelection, true);
        SetStateFor(disableOnSelection, false);
    }
    void SetStateOnReset()
    {
        SetStateFor(enableOnSelection, false);
        SetStateFor(disableOnSelection, true);
    }
    void SetStateFor(GameObject[] objs, bool state)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] != null)
                objs[i].SetActive(state);
        }
    }
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(StartScreen3dView))]
[CanEditMultipleObjects]
public class StartScreen3dViewEditor : Editor
{

    int loadMod = -1;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StartScreen3dView script =(StartScreen3dView) target;
        if (GUILayout.Button("Check Modules"))
            script.CheckModuleNames();
        if (GUILayout.Button("Check Module Ids"))
            script.CheckModuleIds();
        if (GUILayout.Button("Check Server For Module Ids"))
            script.CheckMasterModuleIds();
        if (GUILayout.Button("Reset 3DModule Ids"))
        {
            script.ResetModuleIds();
        }
        if (GUILayout.Button("Copy to Module"))
            script.CopyNameAddressesToCar();
        EditorGUILayout.BeginHorizontal();
        loadMod = EditorGUILayout.IntField(loadMod);
        if (GUILayout.Button("Load Module>>"))
        {
            script.SelectAndLoadModule(loadMod);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
