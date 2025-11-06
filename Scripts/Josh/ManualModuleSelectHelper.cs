using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//[ExecuteInEditMode]
public class ManualModuleSelectHelper : MonoBehaviour
{
    
    [SerializeField] VarType selectedVariant;
    [SerializeField] Dropdown engine, gearbox, ac, variants, car;
    [SerializeField] Button resetManualSelectionButton;
    [SerializeField] Sprite notAvailable;
    [SerializeField] List<string> gearboxOptions;
    [SerializeField] List<VariantDetail> details;
    [SerializeField] public List<VarType> variantTypes,openVariantTypes;
    [SerializeField] List<VariantDetail> curDetails;
    [SerializeField] StartScreen3dView select3D;
    [SerializeField] ScreenLinker linker;
    [SerializeField] string variantList;
    int uiSelAc, uiSelEng, uiSelGearbox;
    string currCar = "";
  [SerializeField] public List<string> engines, gearboxes, acs;
    [System.Serializable]
    public class VarType
    {
        public string vehicleID,name, car, engine, gearbox, ac, variant,model_code;
    }
    [System.Serializable]
    public class VariantDetail {
        public string type;
        public Car car;
        public Engine engine;
        public Gearbox gearbox;
        public Ac ac;
        public Variant variant;
        public int rowId;
    }

[System.Serializable]
    public class VariantList
    {
      public  List<VarType> variantTypes;

    }

    public enum Car
    {
      Kushaq, Taigun ,Slavia
    }
    public enum Engine
    {
        L1, L1_5
    }
    public enum Gearbox
    {
        MQ200, AQ250, DQ200, MQ281
    }
    public enum Ac
    {
        Manual, Automatic
    }
    public enum Variant
    {
        ActiveMT, AmbitionMT, AmbitionAT, StyleAT, StyleMT,// Kushaq
        ComfortlineMT, HighlineMT, HighlineAT, ToplineMT, ToplineAT, GTMT, GTDSG//Taigun
    }
    enum VariantKushaq
    {
       ActiveMT, AmbitionMT, AmbitionAT, StyleAT, StyleMT
    }
    enum VariantTaigun
    {
        None, ComfortlineMT, HighlineMT, HighlineAT, ToplineMT, ToplineAT, GTMT, GTDSG
    }
   enum VariantSlavia
    {
        None, ActiveMT, AmbitionMT, AmbitionAT, StyleAT, StyleMT
    }

    //engine 1L ,1.5L
    // gearbox 4 versions
    // ac 2 versions
    // Start is called before the first frame update

    //void OnEnable()
    //{
    //    variantTypes = new List<VarType>();
    //    for (int i = 0; i < details.Count; i++)
    //    {
    //        VarType t = new VarType();
    //        VariantDetail detail = details[i];
    //        t.name = detail.type.ToString();
    //        t.engine = detail.engine.ToString();
    //        t.gearbox = detail.gearbox.ToString();
    //        t.variant = detail.variant.ToString();
    //        t.car = detail.car.ToString();
    //        t.ac = detail.ac.ToString();
    //        variantTypes.Add(t);
    //        details[i].rowId = i;
    //    }
    //}

    private void OnEnable()
    {
     
       // ResetOptions();
        //VariantList vl = new VariantList();
   
        //vl.variantTypes = variantTypes;
        //variantList = JsonUtility.ToJson(vl);
    }
    /*  public void OpenManualOption()
      {

      }*/
    
    public void ResetOptions()
    {
      /*  for (int i = 3; i < DropdownMenu.Length; i++)
        {
            DropdownMenu[i].SetActive(false);
        }*/
        
        currCar = "";
        //ClearManualDropdowns();
        curDetails = null;
        curDetails = details;
        openVariantTypes = variantTypes;
        //  car.onValueChanged.AddListener(SelectCarAndUpdate);
        //  engine.onValueChanged.AddListener(SelectEngineAndUpdate);
        //  gearbox.onValueChanged.AddListener(SelectGearboxAndUpdate);
        //   variants.onValueChanged.AddListener(SelectVariantAndUpdate);

        // ac.onValueChanged.AddListener(SelectHvacForVariant);
        //  AddOptionToDropdown(engine, "Engine1L");
        //engine.AddOptions(new List<string> { "Engine" });
        //    PopulateDropdowns();
        // SelectCarAndUpdate(0);
        InitialiseDropdownAndSet(variantTypes);
        SelectCarAndUpdate(car.value);
        SelectEngineAndUpdate(engine.value);
        if (resetManualSelectionButton)
            resetManualSelectionButton.gameObject.SetActive(false);
   //     SetDropdownMarkingsTo();
    }
    private void OnDisable()
    {
        car.onValueChanged.RemoveListener(SelectCar);
        engine.onValueChanged.RemoveListener(SelectEngine);
        gearbox.onValueChanged.RemoveListener(SelectGearbox);
        variants.onValueChanged.RemoveListener(SelectVariant);
        ac.onValueChanged.RemoveListener(SelectHVAC);
    }
    //List<VariantDetail> SelectHVAC(int sel, List<VariantDetail> srcDet)
    //{
    //    List<VariantDetail> curDet = new List<VariantDetail>();
    //    for (int i = 0; i < srcDet.Count; i++)
    //    {
    //        Debug.Log(srcDet[i].)
    //        if ((int)srcDet[i].ac == sel)
    //        {
    //            curDet.Add(srcDet[i]);
    //            Debug.Log("SEL: " + srcDet[i].ac);
    //        }
    //    }
    //    return curDet;
    //}
    List<VariantDetail> SelectEngine(int sel, List<VariantDetail> srcDet)
    {
        List<VariantDetail> curDet = new List<VariantDetail>();
        for (int i = 0; i < srcDet.Count; i++)
        {
            if ((int)srcDet[i].engine == sel)
                curDet.Add(srcDet[i]);
        }
        return curDet;
    }
    List<VariantDetail> SelectGearbox(int sel, List<VariantDetail> srcDet)
    {
        List<VariantDetail> curDet = new List<VariantDetail>();
        for (int i = 0; i < srcDet.Count; i++)
        {
            if ((int)srcDet[i].gearbox == sel)
                curDet.Add(srcDet[i]);
        }
        return curDet;
    }
    List<VariantDetail> SelectVariant(string selName, List<VariantDetail> srcDet)
    {
        List<VariantDetail> curDet = new List<VariantDetail>();
        for (int i = 0; i < srcDet.Count; i++)
        {
            if (srcDet[i].variant.ToString() == selName)
            {
                Debug.Log("Adding " + srcDet[i].variant);
                curDet.Add(srcDet[i]);
            }
        }
        return curDet;
    }
    List<VariantDetail> SelectCar(int sel, List<VariantDetail> srcDet)
    {
        List<VariantDetail> curDet = new List<VariantDetail>();
        for (int i = 0; i < srcDet.Count; i++)
        {
            if ((int)srcDet[i].car == sel)
                curDet.Add(srcDet[i]);
        }
        return curDet;
    }

    public void SelectHVAC(int sel)
    {
        Debug.Log("Selected: " + ac.options[sel].text);
        //   curDetails = SelectHVAC(sel, curDetails);
        foreach (var item in curDetails)
        {
            Debug.Log(item.ac);
        }
        uiSelAc = sel;    
        SelectModules();
    }
    public void SelectEngine(int sel)
    {
        if(engine.options[sel].image==null)
        curDetails = SelectEngine(sel, curDetails);
        else
        curDetails = SelectEngine(sel, details);
        uiSelEng = sel;
        PopulateDropdowns();
    }
    public void SelectCar(int sel)
    {
        Debug.Log(sel+"Select Car...............");
        if (car.options[sel].image == null)
        {
            curDetails = SelectCar(sel, curDetails);
        }
        else
        {
            curDetails = SelectCar(sel, details);
        }
            
        PopulateDropdowns();
    }
    public void SelectGearbox(int sel)
    {
        if (gearbox.options[sel].image == null)
            curDetails = SelectGearbox(sel, curDetails);
        else
            curDetails = SelectGearbox(sel, details);
        uiSelGearbox = sel;
        PopulateDropdowns();
    }
    public void SelectVariant(int sel)
    {

        if (variants.options[sel].image == null)
        {
         //   Debug.Log("In Selection "+variants.options[sel].text);
            curDetails = SelectVariant(variants.options[sel].text, curDetails);
        }
        else
        {
         //   Debug.Log("Not in Selection " + variants.options[sel].text);
            curDetails = SelectVariant(variants.options[sel].text, details);
        }
        PopulateDropdowns();
    }

    public void SetVariantTypes(List<VarType> vartypeNew)
    {
        if (vartypeNew.Count > 0)
        {
            variantTypes = vartypeNew;
            InitialiseDropdownsTo(variantTypes);
            engines = new List<string>();
            gearboxes = new List<string>();
            acs = new List<string>();
            foreach (var item in variantTypes)
            {
                if (!DoesListHave(engines,item.engine))
                    engines.Add(item.engine);
                string gb = item.gearbox;
                if (gb.Contains("DQ"))
                     gb = gb.Split(' ')[0];
                if (!DoesListHave(gearboxes,gb))
                    gearboxes.Add(gb);
                if (!DoesListHave(acs,item.ac))
                    acs.Add(item.ac);
            }
            SetModuleGroups();
        }
    }

    //public void SetDetailTypes(List<VariantDetail> vartypeNew)
    //{
    //    if (vartypeNew.Count > 0)
    //    {
    //        details = vartypeNew;
    //        InitialiseDropdownsTo(variantTypes);
    //        engines = new List<string>();
    //        gearboxes = new List<string>();
    //        acs = new List<string>();
    //        foreach (var item in variantTypes)
    //        {
    //            if (!DoesListHave(engines, item.engine))
    //                engines.Add(item.engine);
    //            string gb = item.gearbox;
    //            if (gb.Contains("DQ"))
    //                gb = gb.Split(' ')[0];
    //            if (!DoesListHave(gearboxes, gb))
    //                gearboxes.Add(gb);
    //            if (!DoesListHave(acs, item.ac))
    //                acs.Add(item.ac);
    //        }
    //        SetModuleGroups();
    //    }
    //}
    //manual selection UI
    /* public GameObject[] DropdownMenu;

     public void EnableDropDownMenu(int Menu)
     {
         if (Menu == 3)
         {
             for (int i = 3; i < DropdownMenu.Length; i++)
             {
                 DropdownMenu[i].SetActive(true);
             }
         }
         DropdownMenu[Menu].SetActive(true);
     }*/
    public void InitialiseDropdownsTo(List<VarType> variantDets)
    {
        Debug.Log("InitialiseDropdownsTo.........");
        bool loadSuccess = false;

        Debug.Log(variantDets[0].ToString());
        
        if (variantDets != null)
            if (variantDets.Count > 0)
            {
                openVariantTypes = variantDets;
                loadSuccess = true;
                for (int i = 0; i < variantDets.Count; i++)
                {
                    int rowId = i;
                    VarType curvariant = variantDets[i];
                    AddToDropdownIfNotFound(engine, curvariant.engine.ToString());
                    string curGearbox = curvariant.gearbox;
                    if (curGearbox.Contains("DQ"))
                        curGearbox = curGearbox.Split(' ')[0];
                    AddToDropdownIfNotFound(gearbox, curvariant.gearbox.ToString());

                   //AddToDropdownIfNotFound(car, curvariant.car.ToString());

                    AddToDropdownIfNotFound(variants, curvariant.variant.ToString());

                }
                Debug.Log("Initialised Dropdowns for Manual Selection");
                AddOptionToDropdown(ac, Ac.Manual.ToString());
                AddOptionToDropdown(ac, Ac.Automatic.ToString());
            }
        if (!loadSuccess)
            Debug.LogError("Some Issue with Incoming Details");
    }

    public void SelectEngineAndUpdate(int id)
    {
        //SelectCarAndUpdate(car.value);
        Debug.Log("    "+id);

        string Engine;

        if (id == 0)
        {
            Engine = "1.0";
        }
        else
        {
            Engine = "1.5";
        }

        /* List<string> m_DropOptions = new List<string>();

         foreach (Dropdown.OptionData option in variants.options)
         {
             if (option.text.Contains(Engine))
             {
                 m_DropOptions.Add(option.ToString());
                 Debug.Log("Engine Name.........."+option);
             }

         }
 */
        List<Dropdown.OptionData> NewVar = new List<Dropdown.OptionData>();
        foreach (Dropdown.OptionData option in variants.options)
        {
            Debug.Log(option.text);
            string S1 = option.text;
           // string S2 = "1";
            string S3 = Engine;          
            if (S1.Contains(S3))
            {
                Debug.Log("Engine Name.........." + option.text);
                NewVar.Add(option);
            }           
        }
        variants.ClearOptions();
        variants.AddOptions(NewVar);

        //  variants.options.Find(o => string.Equals(o.text,Engine));

/*        Debug.Log("engine drop:" + engine.options.Count + "    id     " + id);

        var newOptions = SelectFromTypesWhere(GetNameOf(engine, id));
        Debug.Log("SelectEngineAndUpdate:" + newOptions);

        InitialiseDropdownAndSet(newOptions);*/

    }

    public void SelectGearboxAndUpdate(int id)
    {
        var newOptions = SelectFromTypesWhere("",GetNameOf(gearbox, id));
        if (newOptions.Count > 0)
            InitialiseDropdownAndSet(newOptions);
        else
            Debug.Log("NO Options Found!");
    }
    public void SelectCarAndUpdate(int id)
    {
        Debug.Log("Select Car And Update  "+id);
        var newOptions = SelectFromTypesWhere("","", GetNameOf(car, id));
        InitialiseDropdownAndSet(newOptions);
        var VarOption = SelectFromTypesWhere(GetNameOf(engine,engine.value));
       // SelectEngineAndUpdate(engine.value);
    }

    public void SelectVariantAndUpdate(int id)
    {
        var newOptions = SelectFromTypesWhere("", "","", GetNameOf(variants, id));
        InitialiseDropdownAndSet(newOptions);     
    }

    public void SelectVariantViaEngine()
    {
        int EngineValue = engine.value;

/*      var newOptions = SelectFromTypesWhere(GetNameOf(engine, engine.value), "", GetNameOf(car, car.value));
        InitialiseDropdownAndSet(newOptions);*/
       
        SelectCarAndUpdate(car.value);
        SelectEngineAndUpdate(EngineValue);
    }
    public void SelectHvacForVariant(int id)
    {
        selectedVariant.ac = ac.options[id].text;
    }
    
    string GetNameOf(Dropdown drop,int sel)
    {
        Debug.Log("Drop     "+drop+"    sel    "+sel);
        return drop.options[sel].text;
    }
     void InitialiseDropdownAndSet(List<VarType> vartys)
    {
        ClearManualDropdowns();
        InitialiseDropdownsTo(vartys);

        if (currCar.Length > 0)
        {

            for (int i = 0; i < vartys.Count; i++)
            {
                if (vartys[i].car == currCar)
                {
                    SetDropdownsTo(i);
                }
            }
        }
        else
        {
            Debug.Log("//SetDropdownsTo(0)");
           // SelectCarAndUpdate(0);
            //  SetDropdownsTo(0);
        }

        // select3D.SetModuleGroups(GetOptionsFor(engine), GetOptionsFor(gearbox), GetOptionsFor(ac));
        SetModuleGroups();
        if (resetManualSelectionButton)
            resetManualSelectionButton.gameObject.SetActive(true);
    }
    public void SetModuleGroups()
    {
        //select3D.SetModuleGroups(engines.ToArray(), gearboxes.ToArray(), acs.);
        select3D.SetModuleGroups(engines.ToArray(), gearboxes.ToArray(), acs.ToArray());
    }
    string[] GetOptionsFor(Dropdown drop)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < drop.options.Count; i++)
        {
            result.Add(drop.options[i].text);
        }
       return result.ToArray();
    }
    public List<VarType> SelectFromTypesWhere(string theEngine="",string theGearbox="",string theCar="",string theVar="")
    {       
        List<VarType> result = new List<VarType>();
      
        for (int i = 0; i < variantTypes.Count; i++)
        {
            VarType varIter = variantTypes[i];
            bool addThisVar = false;
            if (theEngine.Length > 0)
                if (varIter.engine == theEngine)
                    addThisVar = true;
            if (theGearbox.Length > 0)
                if (varIter.gearbox == theGearbox)
                    addThisVar = true;
            if (theCar.Length > 0)
            {
                currCar = theCar;
                if (varIter.car == theCar)
                    addThisVar = true;
            }
            if (theVar.Length > 0)
                if (varIter.variant == theVar)
                    addThisVar = true;

            if (addThisVar)
                result.Add(varIter);
        }
        return result;
    }
   
    void SetDropdownsTo(int variantTypeId)
    {
        Debug.Log("varient Type......................."+variantTypeId);
       selectedVariant = openVariantTypes[variantTypeId];
      /*  int engineId = GetDropdownIndexFor(engine,selectedVariant.engine);
        if (engineId >= 0)
        {
            engine.SetValueWithoutNotify(engineId);
            Debug.Log("engineId"+ engineId);
        }*/
          
        int gearboxId = GetDropdownIndexFor(gearbox, selectedVariant.gearbox);
        if (gearboxId >= 0)
        {
            gearbox.SetValueWithoutNotify(gearboxId);
            Debug.Log("gearboxId" + gearboxId);
        }
          
        int variantId = GetDropdownIndexFor(variants, selectedVariant.variant);
        if (variantId >= 0)
        {
            variants.SetValueWithoutNotify(variantId);
            Debug.Log("variantId" + variantId);
        }
           
        int carId = GetDropdownIndexFor(car, selectedVariant.car);
        if (carId >= 0)
        {
            car.SetValueWithoutNotify(carId);
            Debug.Log("variantId" + carId);
        }
          
        linker.GetScreenManager().DebugToScreen("Initialised to " + selectedVariant.car + ": " + selectedVariant.variant);

    }
    int GetDropdownIndexFor(Dropdown dropdown, string searchTerm)
    {
       var options = dropdown.options;
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].text == searchTerm)
                return i;
        }
        return -1;
    }
    void AddToDropdownIfNotFound(Dropdown drop, string opt)
    {
        bool exists = false;
        for (int i = 0; i < drop.options.Count; i++)
        {
            if (!exists)
                exists = drop.options[i].text.Contains(opt);
        }
        if (!exists)
        {
        //    Dropdown.OptionData option = new Dropdown.OptionData(opt, notAvailable);
            Dropdown.OptionData option = new Dropdown.OptionData(opt);
            drop.AddOptions(new List<Dropdown.OptionData>() { option });
         //   drop.RefreshShownValue();
        }
        if (drop.options.Count < 1)
            Debug.LogError("Could Not Add Dropdown options to " + drop.gameObject.name, drop.gameObject);
    }

    void ClearManualDropdowns()
    {
        Debug.Log("Clear Manual DropDown..............");
        //  if (engine.options.Count > 1)
        //engine.ClearOptions();
        //  if (gearbox.options.Count > 1)
            gearbox.ClearOptions();
       // if (variants.options.Count > 1)
            variants.ClearOptions();
     /*   if (car.options.Count > 1)
            car.ClearOptions();*/
    }
    void SetDropdownMarkings(Dropdown d,Sprite sp =null)
    {
        for (int i = 0; i < d.options.Count; i++)
        {
            d.options[i].image = sp;
        }
        d.SetValueWithoutNotify(0);
    }
    void SetDropdownMarkingsTo(Sprite sp=null)
    {
        Dropdown[] drops = new Dropdown[] { engine, gearbox, car, variants };
        foreach (var item in drops)
        {
            SetDropdownMarkings(item,sp);
        }
    }
    void InitialiseDropdowns()
    {
        Debug.Log("InitialiseDropdowns .....................");
        //     Debug.Log("Re-Populating "+curDetails.Count);
        for (int i = 0; i < curDetails.Count; i++)
        {
            int rowId = curDetails[i].rowId;
            //  Debug.Log(curDetails[i].gearbox.ToString());
            //if(engine.options.Count>1)
           AddOptionToDropdown(engine, variantTypes[rowId].engine.ToString());
        //    AddToEngines(variantTypes[rowId].engine);
            //if(gearbox.options.Count>1)
            AddOptionToDropdown(gearbox, variantTypes[rowId].gearbox.ToString());
         ///   AddToGearboxes(variantTypes[rowId].gearbox);
            //if(car.options.Count>1)
         //   AddOptionToDropdown(car, variantTypes[rowId].car.ToString());
            //if(variants.options.Count>1)
            AddOptionToDropdown(variants, variantTypes[rowId].variant.ToString());

        }
      
        AddOptionToDropdown(ac, Ac.Manual.ToString());
        AddOptionToDropdown(ac, Ac.Automatic.ToString());
    }
    void PopulateDropdowns()
    {
        SetDropdownMarkingsTo(notAvailable);
        InitialiseDropdowns();
        if (resetManualSelectionButton)
            resetManualSelectionButton.gameObject.SetActive(true);  
   //     SelectModules();
    }
    bool DoesListHave(List<string> ipList, string name)
    {
        bool found = false;
        for (int i = 0; i < ipList.Count; i++)
        {
            if (ipList[i] == name)
            {
                found = true;
                return found;
            }
        }
        return found;
    }
    //void AddToEngines(string name)
    //{
    //    bool hasName = DoesListHave(engines, name);
    //    if (!hasName)
    //        engines.Add(name);
    //}
    //void AddToGearboxes(string name)
    //{
    //    bool hasName = DoesListHave(gearboxes, name);
    //    if (!hasName)
    //        gearboxes.Add(name);
    //}
    void AddOptionToDropdown(Dropdown drop, string opt)
    {
        bool exists = false, available=false;
        int sel = -1;
        for (int i = 0; i < drop.options.Count; i++)
        {
            if (!exists)
            {
                exists = drop.options[i].text.Contains(opt);
                if (exists)
                {
                    drop.options[i].image = null;
                
                    drop.SetValueWithoutNotify(i);
                }
            }
        
        }
        if (!exists) {
            Dropdown.OptionData option = new Dropdown.OptionData(opt, notAvailable);
            drop.AddOptions(new List<Dropdown.OptionData>() { option});
        }
       // if(sel>=0)
       // drop.SetValueWithoutNotify(sel);

        //foreach (var item in drop.options)
        //{
        //    if (!exists)
        //        exists = (item.text.Contains(opt));
        //    if (exists)
        //        item.image = null;
        //    else
        //        item.image = notAvailable;

        //        //     Debug.Log("Added " + opt, drop.gameObject);

        //}
        //if (!exists)
        //{
        //    //     Debug.Log("Added " + opt, drop.gameObject);
        //    drop.AddOptions(new List<string>() { opt });
        //}
        if (drop.options.Count < 1)
            Debug.Log("Dropdown options!: " + drop.gameObject.name, drop.gameObject);

        Debug.Log("Add Option in DropDown.........." + drop.options);
    }
    int selectedEngine, selectedGearbox, selectedHvac,selectedCar;
    public void SaveSelection() => SelectModulesV2();
    bool useStringBase = true;
    void SelectModules()
    {

        select3D.SelectCarWithName(car.options[car.value].text.ToString());
        if (useStringBase)
        {

            select3D.SelectModuleFromGroups(engine.options[engine.value].text,
                gearbox.options[gearbox.value].text,ac.options[ac.value].text);
        }
        else
        {
            selectedEngine = select3D.GetModuleIdFor(engine.options[engine.value].text.ToString() + "");
            selectedGearbox = select3D.GetModuleIdFor(gearbox.options[gearbox.value].text.ToString());
            if (uiSelAc < 0)
                uiSelAc = 0;
            selectedHvac = select3D.GetModuleIdFor(ac.options[uiSelAc].text.ToString());

            Debug.Log("SEL:" + selectedHvac);

            if (selectedEngine < 0)
                Debug.Log(engine.options[0].text.ToString());
            if (selectedGearbox < 0)
                Debug.Log(gearbox.options[0].text.ToString());
            if (selectedHvac < 0)
                Debug.Log(ac.options[0].text.ToString());

            //     Debug.Log("Engine " + engine.options[selectedEngine].text.ToString());
            select3D.SelectActiveModule(selectedEngine, 0, 1);
            select3D.SelectActiveModule(selectedGearbox, 4, 7);
            select3D.SelectActiveModule(selectedHvac, 2, 3);
            if (selectedHvac >= 0 && selectedHvac < ac.options.Count)
                Debug.Log("AC " + ac.options[selectedHvac].text.ToString());
            //   Debug.Log("Gearbox " + gearbox.options[selectedGearbox].text.ToString());
        }
        select3D.InitialiseDropdown();
    }
    void SelectModulesV2()
    {
        //select3D.SelectCarWithName(selectedVariant.car);
        //    select3D.SelectModuleFromGroups(selectedVariant.engine,
        //        selectedVariant.gearbox, selectedVariant.ac);
        linker.GetApiManager().SetModelCode(selectedVariant.model_code);
        linker.GetScreenManager().SetLoadingState(true,()=>
        {
            Debug.Log("Cancelled Loading");
        //    linker.GetScreenManager().SelectScreenIfNotSelected(ScreenLinker.ScreenID.VIN_INPUT);
        });

        linker.SetSelectedVariant(selectedVariant);
        Debug.Log("Get Vehical API ......");
        linker.GetApiManager().GetVehicleMasters(OnVehicleMasterWithModules);
        linker.GetApiManager().GetVehicleMastersForTorque(OnVehicleMasterWithModulesForTorque);
        //     Filter();

    }

    void OnVehicleMasterWithModulesForTorque(string arg0)
    {

        //linker.GetScreenManager().SetLoadingState(false);
       ManualSelector.mastersForTorque = AppApiManager.GetServerResponseVehicleMasters(arg0).response_data;
        //linker.UpdateVehicleMasters(masters);
    }

    void InitialiseCarSelection()
    {
        select3D.SelectCarWithName(selectedVariant.car);
        if (selectedVariant.gearbox.Contains("DQ"))
            selectedVariant.gearbox = selectedVariant.gearbox.Split(' ')[0];
        select3D.SelectModuleFromGroups(selectedVariant.engine,
            selectedVariant.gearbox, selectedVariant.ac);
        select3D.InitialiseDropdown();
    }
    void OnVehicleMasterWithModules(string arg0)
    {
        Debug.Log("Get Vehical APi Response..............");
        linker.GetScreenManager().SelectScreen(ScreenLinker.ScreenID.PROCESS_SELECTION);
        InitialiseCarSelection();
        linker.GetScreenManager().SetLoadingState(false);
     
        Debug.Log("<color=blue>[VEH-MSTRS+MODS]</color> " + arg0, gameObject);
        AppApiManager.VehicleMasters masters = AppApiManager.GetServerResponseVehicleMasters(arg0).response_data;
        linker.UpdateVehicleMasters(masters);
    }
   public void Filter()
    {
        Debug.Log(engine.value);
        // ClearManualDropdowns();
        // SelectCarAndUpdate(0);
        //SelectCarAndUpdate(0);
    /*    Debug.Log("Filter.........");
        foreach (Dropdown.OptionData VarOP in variants.options)
        {
            Debug.Log("Dropdown............" + VarOP.text);
        }*/


    }
    public void SetModel(List<string> modelList)
    {
        car.ClearOptions();
        car.AddOptions(new List<string>() { "Please Select" });
        car.AddOptions(modelList);
        
     //   Debug.LogError("Select Car  " + car);
       // Debug.Log(car.AddOptions(modelList) + "SET Model");
    }

    void Update()
    {

    }
}
