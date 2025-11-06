using mixpanel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Variant {
    public string generalName;
    public string model;
    public string engine;
    public string variant;
    public string transmission;
}


public class ManualSelector : MonoBehaviour {

    public List<ManualModuleSelectHelper.VarType> openVarientList;
    [SerializeField] ManualModuleSelectHelper.VarType SelctedVarient;
    public List<Variant> variants;
    public Dropdown model, engine, variant, transmission,Hvac;

        public StartScreen3dView select3D;
    public ManualModuleSelectHelper manualModuleSelectHelper;
    [SerializeField] ScreenLinker linker;
    [SerializeField] TechnicianLogin technicianLogin;
    //public GameObject Slavia_Harness;
    //public GameObject Kushaq_Harness;
    //public GameObject Taigun_Harness;
    //public GameObject Virtus_Harness;

    public string manualCarName;

    public static AppApiManager.VehicleMasters mastersForTorque;

    private void OnEnable()
    {
        linker.GetManualSelector().SetModel(new List<string>( technicianLogin.masters.models));
        //manualModuleSelectHelper.SetModel(new List<string>(masters.models));
    }
    // Start is called before the first frame update
    void Start() {

        openVarientList = manualModuleSelectHelper.openVariantTypes;
        InitializeVariants();
    }

    // Update is called once per frame
    void Update() {

   
    }

    public void ResetAll() {

    }

 /*   [ContextMenu("Populate General Name")]
    public void PopulateGeneralName() {
        foreach (Variant v in variant) {
            v.generalName = v.model + "-" + v.engine + "-" + v.variant + "-" + v.transmission;
        }
    }*/

    private void InitializeVariants() {
        List<string> models = new List<string>();
        models.Add("Select Car");
        foreach (ManualModuleSelectHelper.VarType v in openVarientList) {
            models.Add(v.car);
        }
        models = models.Distinct().ToList<string>();
        //model.ClearOptions();
        //model.AddOptions(models);

        List<string> emptyVal = new List<string>();
        emptyVal.Add("Please Select");

        engine.ClearOptions();
        engine.AddOptions(emptyVal);
        variant.ClearOptions();
        variant.AddOptions(emptyVal);
        transmission.ClearOptions();
        transmission.AddOptions(emptyVal);
    }

    public void SetModel(List<string> modelList )
    {
        model.ClearOptions();
        model.AddOptions(modelList);
    }

    //Select a car in unity
    public void OnModelChange() {
        string _model = model.options[model.value].text;
        manualCarName = _model;
        Debug.Log($"Selected dropdown : {_model}");
        ManualOpenTicketCreator.instance.SetCarModelText(_model);

        List<string> engines = new List<string>();
        engines.Add("Please Select");
        foreach (ManualModuleSelectHelper.VarType v in openVarientList)
        {
            if (v.car == _model)
                engines.Add(v.engine);
        }
        //manual list options are given
     /*   engines.Add("Engine 1L");
        engines.Add("Engine 1.5L");*/
        engines = engines.Distinct().ToList<string>();
        engine.ClearOptions();
        engine.AddOptions(engines);

        List<string> emptyVal = new List<string>();
        emptyVal.Add("Please Select");
        variant.ClearOptions();
        variant.AddOptions(emptyVal);
        transmission.ClearOptions();
        transmission.AddOptions(emptyVal);
    }


    public void OnEngineChange() {
        string _model = model.options[model.value].text;
        string _engine = engine.options[engine.value].text;
        Debug.Log(_model + " - " + _engine);
        Debug.Log($"Selected dropdown : {_engine}");
        ManualOpenTicketCreator.instance.SetEngineTypeText(_engine);

        List<string> _variant = new List<string>();
        _variant.Add("Please Select");
        foreach (ManualModuleSelectHelper.VarType v in openVarientList) {
            if (v.engine == _engine && v.car == _model) {
                _variant.Add(v.variant);
            }
        }
        _variant = _variant.Distinct().ToList<string>();
        variant.ClearOptions();
        variant.AddOptions(_variant);

        List<string> emptyVal = new List<string>();
        emptyVal.Add("Please Select");
        transmission.ClearOptions();
        transmission.AddOptions(emptyVal);

    }
    public GameObject ModelYear;
    public GameObject hvacGameobject;
    public void OnVariantChange() {
        string _model = model.options[model.value].text;
        string _engine = engine.options[engine.value].text;
        string _variant = variant.options[variant.value].text;
        Debug.Log(_model + " - " + _engine + " - " + _variant);
        Debug.Log($"Selected dropdown : {_variant}");
        ManualOpenTicketCreator.instance.SetVeriantText(_variant);
        if (_variant == "Please Select")
        {
            ModelYear.SetActive(false);
            hvacGameobject.SetActive(false);
        }
        else
        {
            ModelYear.SetActive(true);
            hvacGameobject.SetActive(true);
        }
        List<string> trans = new List<string>();
        List<string> ac = new List<string>();
        //trans.Add("-");
        foreach (ManualModuleSelectHelper.VarType v in openVarientList) {
            if (v.engine == _engine && v.car == _model && v.variant == _variant) {
                trans.Add(v.gearbox);
                ac.Add(v.ac);
                SelctedVarient = v;
            }
        }
        trans = trans.Distinct().ToList<string>();
        transmission.ClearOptions();
        transmission.AddOptions(trans);

       // ac = ac.Distinct().ToList<string>();
       // Hvac.ClearOptions();
       // Hvac.AddOptions(ac);

        ManualModuleSelectHelper.VarType variantType = new ManualModuleSelectHelper.VarType();
        variantType.variant = _variant;
        linker.SetSelectedVariant(variantType);
    }

    //Open a car in Home screen
    public void OpenCar()
    {
        string _model = model.options[model.value].text;
        string _engine = engine.options[engine.value].text;
        string _variant = variant.options[variant.value].text;
        string _trans = transmission.options[transmission.value].text;
        string _Hvac =Hvac.options[Hvac.value].text;
        InitialiseCarSelection();
        linker.GetApiManager().SetModelCode(SelctedVarient.model_code);
        linker.GetScreenManager().SetLoadingState(true, () =>
        {
            Debug.Log("Cancelled Loading");
            //    linker.GetScreenManager().SelectScreenIfNotSelected(ScreenLinker.ScreenID.VIN_INPUT);
        });
        linker.SetSelectedVariant(SelctedVarient);
        /*   select3D.SelectCarWithName(_model);
           if (_variant.Contains("DQ"))
               _trans = _trans.Split(' ')[0];
           select3D.SelectModuleFromGroups(_engine,
               _trans, _Hvac);

           select3D.InitialiseDropdown();*/

        linker.GetApiManager().GetVehicleMasters(OnVehicleMasterWithModules);
        linker.GetApiManager().GetVehicleMastersForTorque(OnVehicleMasterWithModulesForTorque);

        linker.GetScreenManager().SelectScreen(ScreenLinker.ScreenID.PROCESS_SELECTION);
        //openharness(_model);
        Mixpanel.Track("Manual_Vehicle_Configuration_Complete");
    }

    void InitialiseCarSelection()
    {
       // Debug.LogError("Selected varient   Engine "+ SelctedVarient.engine+"   gear box  "+ SelctedVarient.gearbox+"   HVAc  "+Hvac.options[Hvac.value].text);
        select3D.SelectCarWithName(SelctedVarient.car);
        if (SelctedVarient.gearbox.Contains("DQ"))
            SelctedVarient.gearbox = SelctedVarient.gearbox.Split(' ')[0];
        select3D.SelectModuleFromGroups(SelctedVarient.engine,
            SelctedVarient.gearbox,Hvac.options[Hvac.value].text);
        select3D.InitialiseDropdown();
    }
    void OnVehicleMasterWithModules(string arg0)
    {
      
        linker.GetScreenManager().SetLoadingState(false);
        AppApiManager.VehicleMasters masters = AppApiManager.GetServerResponseVehicleMasters(arg0).response_data;
        linker.screenManager.loadingAnimated.SetActive(true);
        linker.UpdateVehicleMasters(masters);
    }

    void OnVehicleMasterWithModulesForTorque(string arg0)
    {

        //linker.GetScreenManager().SetLoadingState(false);
        mastersForTorque = AppApiManager.GetServerResponseVehicleMasters(arg0).response_data;
        //linker.UpdateVehicleMasters(masters);
    }


    public void  Rest()
    {
        InitializeVariants();
    }
}
