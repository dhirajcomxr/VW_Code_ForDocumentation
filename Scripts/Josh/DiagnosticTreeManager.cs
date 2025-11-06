using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiagnosticTreeManager : MonoBehaviour
{
    int DT_END_SCREEN = 12;//Close Ticket Screen
    [SerializeField] Dropdown dtSelector;
   [SerializeField] bool useStepPlayer = false;
    [SerializeField] DTPlayer player;
    [SerializeField] DeviceCameraControl cameraControl;
    [SerializeField] List<TextAsset> treeFiles;
    
    [SerializeField] List<Sprite> imgs;
    public bool useRemoteLoading = false;
    List<string> availableDts;
    #region Variables
    private UIScreenManager screenManager;
    [SerializeField] VWModuleManager moduleManager;
    [SerializeField] PartListSearchable partList;
   

    private AssetLoader loader;
    [SerializeField] string imageTest;
    int curDTIndex = 0;
    [SerializeField] public Sprite testSprite;
    [SerializeField] string[] moduleFileNames; // file names of module file.CSV in Resources Folder

    public PartHighlighter highlighter; // Current Part Highlighter to send part names to
    [SerializeField] public TextPanel display; // assign display panels 
    [Header("Debug")]
    [SerializeField]
    List<string> headerKeyNames, rowValues;
    [SerializeField] bool load = false, loadLast = false;
    // [SerializeField]
    List<string> activityNumbers;
    [SerializeField]
    List<DiagnosticStep> steps;
    public bool isOn2 = false;
    int totalSteps = 0;
  [SerializeField]  DiagnosticStep curStep,dtMethodStep;
    string complaintName;
    private List<Dictionary<string, object>> data;

    public int curr = 0;
    // Display Panel Blueprint
    [System.Serializable]
   public struct TextPanel
    {
        public TMPro.TextMeshProUGUI question, instruction, part_number, activity, scan_data, stepId, caution_notes, specs, uploadInfo, complaintTitle;
        public GameObject options_bar, scan_dialog, next_button, switchBtnForImg2, switchBtnForImg3;
        public Image stepFill, image;//,img1,img2,img3,img4,img5;
        public UIScreen screen;
        public Slider progress;
      //public Text stageLabel;
        public ImageLabelCollection[] images;
        public Dropdown buttonList;
        public GameObject uiTableModule, rangeModule, calculatorModule, tableModuleOk;
        public InputField rangeModuleIp,CalcInputA,calcInputB;
        public InputField[] tableModuleSideA,tableModuleSideB;
        public GameObject tableYes, tableNo;
        public GameObject viewRnrBtn;
        
    }
    // CSV Header Names
    [System.Serializable]
    class Headers
    {
        public string part_number = "PART", id = "#", activity_id = "ACTIVITY", instruction = "MESSAGE", activity = "ACTIVITIES", specs = "SPEC",
            question = "QUESTION", display_message = "NOTE", diagnostic_block = "DIAGNOSTIC", scan = "PICTURE", caution_notes = "NOTE",
         yes = "YES", no = "NO", na = "N/A", image = "IMAGE", imgs = "", img1 = "SECTION 1", img2 = "SECTION 2", img3 = "SECTION 3", img4 = "SECTION 4", img5 = "SECTION 5",
            button1 = "BTN A", button2 = "BUTTON 2", button3 = "BUTTON 3", button4 = "BUTTON 4", button5 = "BUTTON 5", button6 = "BUTTON 6",
            button7 = "BUTTON 7", button8 = "BUTTON 8", button9 = "BUTTON 9", button10 = "BUTTON 10", buttonNameList = "TEXT FOR BUTTON",method="Method",
            comment = "REMARK", end = "END";

    }
    public GameObject ImgScan;
    public GameObject VideoScan;
    public GameObject AudioScan;

    [HideInInspector]
    [SerializeField]
    Headers HEADER_REGEX, header;

    // Step Blueprint
    //[System.Serializable]
    //public class DiagnosticStep
    //{
    //    public string instruction, activity_id, diagnostic_block_number, part_number, on_yes, on_no, question, activity, scan, caution_notes, reference, image, specs, stage,method;
    //    public string[] imgs = new string[5], buttons = new string[10], buttonNames = new string[10];
    //}
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        display.instruction.margin = new Vector4(1, 1, 0, 0);
        SearchableDTListShow = false;
        //LoadData(0); // Load first file from module names 
        //ShowStep(0);// default to show first step from the module
        screenManager = FindObjectOfType<UIScreenManager>();
        loader = FindObjectOfType<AssetLoader>();
        if (load)
            if (loadLast)
                LoadAndStart(moduleFileNames.Length - 1);
    }
    public void LoadDTsToDropdown()
    {
        availableDts = new List<string>();
        screenManager.SetLoadingState(true);
        Debug.Log("Loading DTs..");
        StartCoroutine(LoadDts());
        //for (int i = 0; i < treeFiles.Count; i++)
        //{
        //    DiagnosticTree oTree = JsonUtility.FromJson<DiagnosticTree>(treeFiles[i].text);
        //    availableDts.Add(oTree.complaintName);
        //}
        //dtSelector.ClearOptions();
        //dtSelector.AddOptions(availableDts);
        //Debug.Log("Loaded " + availableDts.Count + "DTs successfully!");
    }

 
    IEnumerator LoadDts()
    {
        for (int i = 0; i < treeFiles.Count; i++)
        {
            DiagnosticTree oTree = JsonUtility.FromJson<DiagnosticTree>(treeFiles[i].text);
            Debug.Log("<Color=blue>DT Tree Files</color>"+oTree);

            availableDts.Add(oTree.complaintName);
            yield return null;
        }
        dtSelector.ClearOptions();
        dtSelector.AddOptions(availableDts);
        Debug.Log("Loaded " + availableDts.Count + "DTs successfully!");
        screenManager.SetLoadingState(false);
    }
    private void OnEnable()
    {
        if (totalSteps > curr)
            ShowStep(curr);
        else
            if (load)
            LoadAndStart(0);

     
    }
    public AssetLoader GetAssetLoader() => loader;
    // Update is called once per frame
    private bool SearchableDTListShow;
    public GameObject ComplaintDropDownSearch;

    public void ScanOptionSelect(int Type)
    {
        switch (Type)
        {
            case 0:
                Debug.Log("Scan Option Type " + (int)player.GetCurrentStep().scanOption);
                ImgScan.SetActive(true);
                VideoScan.SetActive(false);
                AudioScan.SetActive(false);
                break;
            case 1:
                Debug.Log("Scan Option Type " + (int)player.GetCurrentStep().scanOption);
                ImgScan.SetActive(false);
                VideoScan.SetActive(true);
                AudioScan.SetActive(false);
                break;
            case 2:
                Debug.Log("Scan Option Type " + (int)player.GetCurrentStep().scanOption);
                ImgScan.SetActive(false);
                VideoScan.SetActive(false);
                AudioScan.SetActive(true);
                break;
            case 3:
                Debug.Log("Scan Option Type " + (int)player.GetCurrentStep().scanOption);
                ImgScan.SetActive(false);
                VideoScan.SetActive(false);
                AudioScan.SetActive(false);
                break;

            default:
                ImgScan.SetActive(false);
                VideoScan.SetActive(false);
                AudioScan.SetActive(false);
                break;
        }
    }
    void Update()
    {
     // Debug.Log("Scan Option Type"+((int)player.GetCurrentStep().scanOption));
        if (imageTest.Length > 0)
        {
            testSprite = GetImage(imageTest);
            imageTest = "";
        }
    }
    
    #region UI 
    bool hasScan = false; // to denote that current step has a following upload/scan document step 
    public void OnYes()
    {
        LogOnAnswer("Yes");

        SetNextStep(curStep.on_yes); // Button function to call when Clicked on Yes
    }

    public void OnNo()
    {
        LogOnAnswer("No");

        SetNextStep(curStep.on_no); // Button function to call when Clicked on Yes
    }

    public void Next()
    {
        LogOnAnswer("Next");

        SetNextStep(curStep.on_yes);// Call this function from the Next button
    }

    // Call Upload scan on the upload dialog box
    public void SelectOption(int optNum)
    {
        Debug.Log("Selected: " + optNum);
        if (optNum < curStep.buttonNames.Length)
            LogOnAnswer(curStep.buttonNames[optNum]);

        SetNextStep(curStep.buttons[optNum]);
      
    }
    List<string> GetImageNames(List<DiagnosticStep> list)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < list.Count; i++)
        {
            foreach (string img in list[i].imgs)
            {
                if(img.Length>0)
                result.Add(img);
            }
        }
        return result;
    }
    public List<string> GetImagesFor(int dt_id)
    {
         List<string> imageList = new List<string>();
        LoadData(dt_id);
       imageList= GetImageNames(steps);
        return imageList;
    }
    public List<string> GetAllImages()
    {
        int total = 0,totalSteps=0;
        List<string> allNames = new List<string>();
        List<DiagnosticStep> imgSteps = new List<DiagnosticStep>();
        for (int i = 0; i < moduleFileNames.Length; i++)
        {
            List<DiagnosticStep> st = new List<DiagnosticStep>();
            List<string> imageNames = new List<string>();
            Debug.Log("Loading for " + i + " named " + moduleFileNames[i]);
            st = LoadDataList(moduleFileNames[i]);
            totalSteps += st.Count;
            imgSteps.Concat(st);
            imageNames = GetImageNames(st);
            Debug.Log("Found "+imageNames.Count+" images for "+i);
            allNames.AddRange(imageNames);
            //allNames.Concat<string>(script.GetImagesFor(i));
        }
      
        List<string> dwnldImgs = new List<string>();
        for (int i = 0; i < allNames.Count; i++)
        {
            string iName = allNames[i];
            bool found = false;
            for (int j = 0; j < dwnldImgs.Count; j++)
            {
                if (dwnldImgs[j] == iName)
                    found = true;
            }
            if (!found)
                dwnldImgs.Add(iName);
        }

        Debug.Log("TOTAL Steps : "+totalSteps+" :: Total Images: "+allNames.Count);
     //   allNames = GetImageNames(imgSteps);
        return dwnldImgs;
    }
    public string DtLink = "";
    public void Download_DT_Images()
    {
        Debug.Log("Download  Images : ");
        if (useRemoteLoading)
        {
            //    loader.LoadSprites("dt_images");
            List<string> dwnldImgs = new List<string>();
            if (useStepPlayer)
              dwnldImgs=  GetImageNames(player.GetSteps());
            else
             dwnldImgs=   GetImageNames(steps);
            List<string> imgNames = new List<string>();
            for (int i = 0; i < dwnldImgs.Count; i++)
            {
                string iName = dwnldImgs[i];
                bool found = false;
                for (int j = 0; j < imgNames.Count; j++)
                {
                    if (imgNames[j] == iName)
                        found = true;
                }
                if(!found)
                    imgNames.Add(iName);
            }
            Debug.Log("Total image Count:" + imgNames.Count);
            if (imgNames.Count > 0)
            {
                screenManager.SetLoadingState(true);
                loader.DownloadImages(imgNames, DtLink,".jpg",FinishedDownloadingDTImages);                             
             // loader.finishedLoading += FinishedDownload;
            }else if (imgNames.Count==0)
            {
                LoadDTWithoutImage();
                Debug.Log("No Image.........................");
            }
        }
        else
            Debug.Log("Remote Loading Images for DT is:" + useRemoteLoading);
           
    }
    void FinishedDownloadingDTImages(List<Sprite> spriteList)
    {
        Debug.Log("<color=blue>Finished Downloading Images</color>"+spriteList);
        imgs = spriteList;
        screenManager.SelectScreen(ScreenLinker.ScreenID.DT);
        Begin();
        screenManager.SetLoadingState(false);
    }
    void LoadDTWithoutImage()
    {
        screenManager.SelectScreen(ScreenLinker.ScreenID.DT);
        Begin();
        screenManager.SetLoadingState(false);
    }
    void FinishedDownload()
    {
        loader.finishedLoading -= FinishedDownload;
        screenManager.SetLoadingState(false);
    }
    public void OpenCameraForDT()
    {
        cameraControl.ClickForDT();
        cameraControl.onImageCapture += UploadScan;
        
    }
    public void OpenCameraVideoForDT()
    {
/*        cameraControl.ClickForDT();
        
        cameraControl.onImageCapture += UploadScan;*/
    }
    public void OpenCameraAudioForDT()
    {
      /*  cameraControl.ClickForDT();
        cameraControl.onImageCapture += UploadScan;*/
    }

    public void UploadScan()
    {
        Debug.Log("Upload Scan");

        cameraControl.onImageCapture -= UploadScan;
        // TODO: implement upload 
        if (cameraControl.GetImage() != null)
        {
            Debug.Log("Image Recieved, will play ["+curr+"]");
            display.scan_dialog.SetActive(false);
            ScanOptionSelect(3);
            if (useStepPlayer)
                player.ShowCurrStep();
            else
            {
                hasScan = false;
                ShowStep(curr);
            }
        }
        else
        {
            Debug.Log("Please Click a picture to continue");
            screenManager.Toast("Please Click a picture to continue");
        }
        // call script with upload functionality
        //  Next();
    }
    public DeviceCameraControl DeviceCamera;

    // Show Step using activity Number
    public void SetNextStep(string nextId)
    {
        if(activityNumbers == null)
        {
            Debug.Log("activity number is null.................");
        }
        DeviceCamera.DTStepNum = System.Convert.ToInt32(nextId);
        Debug.Log("Set Next Step   " + nextId);
        //Debug.Log("  Activity Number   "+activityNumbers[0]);
        int nid = -1;
        if (nextId != "")
            for (int i = 0; i < activityNumbers.Count; i++)
            {
                if (activityNumbers[i] == nextId)
                {
                    curr = i;
                    nid = i;
                }
                //   ShowStep(i);
            }
        else
            ++curr;
        if (nid < 0)
            ++curr;
        if (hasScan)
        {
            display.scan_dialog.SetActive(true);
            ScanOptionSelect((int)player.GetCurrentStep().scanOption);
        }
        else
            ShowStep(curr);
        Debug.Log("Next: " + nextId);
    }
    // show step x of current loaded module
    public void Begin()
    {
        if (useStepPlayer)
            player.ShowStep(0);
        else
        ShowStep(0);
        AppLogger.Log(AppLogger.EventType.DIAG);
    }

    public void ShowStep(int counter)
    {
        if (counter >= (totalSteps - 1))
        {
            screenManager.SelectScreen(DT_END_SCREEN);
        }
            if (counter < totalSteps)
        {
            curStep = steps[counter];
            float progress = (counter + 1) * 1f / totalSteps;
           
            if (display.stepFill)
                display.stepFill.fillAmount = progress;
            if (display.progress != null)
                display.progress.SetValueWithoutNotify(progress);
          /*  if (display.stageLabel != null) //StageLabel Perfomming logic
                if (curStep.stage.Length > 0)
                    display.stageLabel.text = curStep.stage;
                else
                    display.stageLabel.gameObject.SetActive(false);*/
            display.stepId.text = "" + (counter + 1);//+ ":" + totalSteps;
            display.question.text = curStep.question;

            display.instruction.text = curStep.instruction;
        
            display.activity.text = curStep.activity;
            display.scan_data.text = curStep.scan;
            display.caution_notes.transform.parent.gameObject.SetActive(curStep.caution_notes != "");
            display.caution_notes.text = curStep.caution_notes;
            display.uploadInfo.transform.parent.gameObject.SetActive(curStep.scan != "");
            display.uploadInfo.text = curStep.scan.Replace(System.Environment.NewLine, "");
            display.specs.transform.parent.gameObject.SetActive(curStep.specs != "");
            display.specs.text = curStep.specs;

            hasScan = curStep.scan != "" ? true : false;
           
                CleanPrevStep();
                SetInputMethod(curStep);
           

            for (int i = 0; i < display.images.Length; i++)
            {
                if (display.images[i] != null)
                    SetImage(display.images[i], curStep.imgs[i]);
            }

            //  SetImage(display.images[0], curStep.imgs[0]);
            //  SetParentVisiblity(new Image[] { display.images[0] });
            // display.images[0].transform.parent.gameObject.SetActive(!(display.images[0].sprite == null));
            //  SetParentVisiblity(new Image[] { display.images[3], display.images[4] });
            //  display.images[4].transform.parent.gameObject.SetActive((display.images[3].sprite || display.images[4].sprite));
            //  switchImg23();
            if (curStep.part_number != "")
            {
                display.part_number.text = curStep.part_number;
                string[] parts = curStep.part_number.Split(';', ':');
                highlighter.Highlight(parts);
            }
            else
                highlighter.RemoveHighLight();
        }
        else
            Debug.Log("Play Next Module");
    }
    void CleanPrevStep()
    {
        Disable(new GameObject[] {
            display.options_bar,display.next_button,
           display.tableModuleOk, display.tableNo,display.tableYes,display.uiTableModule,
            display.rangeModule,display.calculatorModule,display.viewRnrBtn

        });
        //display.options_bar.SetActive(false);
        //display.next_button.SetActive(false);
        //if (display.rangeModule)
        //    display.rangeModule.SetActive(false);
        //if (display.calculatorModule)
        //    display.calculatorModule.SetActive(false);
        //if (display.uiTableModule)
        //    display.uiTableModule.SetActive(false);
        //if (display.tableModuleOk)
        //    display.tableModuleOk.SetActive(false);
        void Disable(GameObject[] g)
        {
            for (int i = 0; i < g.Length; i++)
            {
                if (g[i])
                    g[i].SetActive(false);
            }
        }
    }
    public void OnRangeModuleInput()
    {
        if (display.rangeModuleIp != null)
        {
            string s = display.rangeModuleIp.text;
            float f;
            if (float.TryParse(s,out f))
            {
                if (f < 11.7f)
                {
                    Debug.Log("Lesser than 11.7");
                    OnYes();
                }
                else
                {
                    Debug.Log("Not Lesser than 11.7");
                    OnNo();
                }
            }
            else
                Debug.LogError("Parse Error for value!, no float value found.");
        }
    }

    public void OnCalculatorModuleInput()
    {
        float a=0, b=0;
        if (HasVal(display.CalcInputA,out a) && HasVal(display.calcInputB,out b))
        {
            if (b > (0.1f * a))
                OnYes();
            else
                OnNo();
        }
        bool HasVal(InputField ip, out float opF)
        {
            bool result = false;
            opF = -1;
            if (ip != null)
            {
                result = float.TryParse(ip.text, out opF);          
            }
         
            return result;
        }
      
    }
    public void OnViewRnR()
    {
        if (dtMethodStep.buttons.Length > 0)
        {
            Debug.Log("DT:" + curStep.buttonNames[0]);
            string[] result = curStep.buttonNames[0].Split(new string[] { "\r\n", "\n" },StringSplitOptions.None);
            if (result.Length > 1)
            {
                int m = int.Parse(result[0].ToString());
                string pName = result[1];
                if (m > -1)
                {
                    Debug.Log("MODULE:" + m);
                    if (moduleManager)
                        moduleManager.SelectModule(m);
                    if (screenManager)
                        screenManager.SelectScreen(19);

                    if (partList)
                    {
                        partList.SearchFor(result[1]);
                        partList.OnListSelected(0);
                    }
                }
            }
        }
        else
            Debug.Log("BUTTON LENGHT");
        Debug.Log("DT:" + curStep.buttons[1].ToString());
     
    }
    public void OnViewRnr(DiagnosticStep curStep)
    {
        Debug.Log("DT:" + curStep.buttons[1].ToString());
    }
    public void OnTableModuleInput()
    {
        string sideAresult = FusesWithHigherCur(display.tableModuleSideA, "SB");
        string sideBresult = FusesWithHigherCur(display.tableModuleSideB, "SC");
        if (sideAresult.Length > 0 || sideBresult.Length > 0)
        {
            string res = "";
            if (sideAresult.Length > 0 && sideBresult.Length > 0)
                res = sideAresult + ", " + sideBresult;
            else
            {
                if (sideAresult.Length > 0)
                    res = sideAresult;
                else
                    res = sideBresult;
            }
            display.instruction.text = "Remove fuse " + res + " and observe stand up values, it should drop.";
            display.question.text = "Components connected to the fuses " + res + " are consuming more than 40mA current";
            display.tableYes.SetActive(true);
        }
        else
            display.tableNo.SetActive(true);
        string FusesWithHigherCur(InputField[] ips, string prefix)
        {
            string totalFusesOc = "";
            for (int i = 0; i < ips.Length; i++)
            {
                InputField ip = ips[i];
                if (ip != null)
                {
                    string s = ip.text;
                    float f=0;
                    if(s.Length>0)
                    if (float.TryParse(s, out f))
                    {
                        if (f > 0.04f)
                        {
                            if (totalFusesOc.Length > 0)
                                totalFusesOc += ", ";
                            totalFusesOc += prefix + (i+1);
                            Debug.Log(prefix+i+" has Greater than 40mA: "+f);
                        }
                        else
                        {
                            Debug.Log(prefix + i + " Lesser than 40mA " +f);
                        }
                    }
                    else
                        Debug.LogError("Parse Error for value!, no float value found.");
                }
            }

            return totalFusesOc;
        }
      
    }
    private void SetInputMethod(DiagnosticStep ipStep)
    {   

        if (ipStep.method.Length > 0)
            SetMethod(ipStep.method);
        else
        {
            SetOptions(ipStep.question == "" || ipStep.on_no == "");// || curStep.question.ToUpper() == "NA" || curStep.question.ToUpper() == "N/A");// if has question
            SetMultipleOptions(ipStep);
        }
                void SetMethod(string mthdName)
            {

            dtMethodStep = ipStep;
            Debug.Log("Method: " + mthdName);
            switch (mthdName)
            {
                case "Options":
                    SetMultipleOptions(ipStep);
                    break;
                case "Range":
                    Debug.Log("Range !");
                    display.rangeModule.SetActive(true);
                    break;
                case "Calculator":
                    Debug.Log("Calculator !");
                    display.calculatorModule.SetActive(true);             
                    break;
                case "UI Table":
                    Debug.Log("Table !");
                    display.uiTableModule.SetActive(true);
                    display.tableModuleOk.SetActive(true);
                    break;
                case "RNR":
                    Debug.Log("RnR !");
                    display.viewRnrBtn.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }
    public void LogOnAnswer(string answer)
    {
        if (useStepPlayer)
            AppLogger.LogDtStep(player.GetCurrentStep().instruction, player.GetCurrentStep().question, answer);
        else
        AppLogger.LogDtStep(curStep.instruction, curStep.question, answer);
    }
    void SetParentVisiblity(Image[] children)
    {
        bool result = true;
        foreach (var item in children)
        {
            if (item.sprite == null)
                result = false;
        }

        children[0].transform.parent.gameObject.SetActive(result);

    }
    // Set Yes/No button or Next Button :context aware
    public void switchImg23()
    {
        isOn2 = !isOn2;
        //     ToggleImagesAB(display.images[1], display.images[2], isOn2);
    }

    void ToggleImagesAB(Image a, Image b, bool aState)
    {
        if (b.sprite && a.sprite)
        {
            b.gameObject.SetActive(aState);
            a.gameObject.SetActive(!aState);
        }
    }
    void SetImage(ImageLabelCollection imgDisplay, string imgList)
    {
        Debug.Log("hjvgdvjbvjbvhv");
        imgList = imgList.Replace(":;:", ",");
        string[] st = imgList.Split(',');
        
        List<Sprite> sp = new List<Sprite>();
        for (int i = 0; i < st.Length; i++)
        {

           Debug.Log("ST:"+st[i]+"L:"+st.Length+" i:"+i,imgDisplay.gameObject);
            if (st[i].Length > 0)
                sp.Add(GetImage(st[i]));
        }
        //if (sp.Count < 1)
        //    imgDisplay.Load(null);
        //else
        //***************************************
       // imgDisplay.transform.parent.gameObject.SetActive(true);
        //if (sp != null)
        //{
        //    if (sp.Count > 0)
                imgDisplay.Load(sp.ToArray());
        //    else
        //        imgDisplay.transform.parent.gameObject.SetActive(false);
        //}
        //else
        //    imgDisplay.transform.parent.gameObject.SetActive(false);

    }
    Sprite GetImageFromList(string imgName)
    {
        Sprite result = null;
        for (int i = 0; i < imgs.Count; i++)
        {
            if (imgs[i].name == imgName)
            {
                result = imgs[i];
                return result;
            }
        }
        return result;
    }
    public Sprite GetDTSprite(string imgName)
        => GetImage(imgName);
   public Sprite GetImage(string imageName)
    {
        if (useRemoteLoading)
        {
            if (imgs.Count > 0)
                return GetImageFromList(imageName);
            else
            return loader.GetSprite(imageName);
        }
        else
        {
            char dirSeperator = System.IO.Path.DirectorySeparatorChar;
            Sprite s = null;
            if (imageName.Length > 0)
                s = Resources.Load<Sprite>("IMAGES" + dirSeperator + "DT IMAGES" + dirSeperator + imageName);
            if (s == null)
                Debug.Log("No Sprite set for " + imageName);
            else
                testSprite = s;

            return s;
        }
    }
    void SetImage(Image img, string imageName)
    {
        if (img)
        {
            char dirSeperator = System.IO.Path.DirectorySeparatorChar;
            Sprite s = null;
            if (imageName.Length > 0)
                s = Resources.Load<Sprite>("Images" + dirSeperator + "DT IMAGES" + dirSeperator + imageName);
            if (s == null)
                Debug.Log("No Sprite set for " + imageName);
            img.sprite = s;
            img.gameObject.SetActive(s != null);
            if (img.sprite != null)
            {
                Debug.Log("Found " + imageName);
                img.preserveAspect = true;
            }
            else
                Debug.Log(imageName + " not found");
        }
    }
    void SetOptions(bool hasQ)// q is emtpy
    {
        display.next_button.SetActive(hasQ);
        display.options_bar.SetActive(!hasQ); // Yes or No
      
    }
    void SetMultipleOptions(DiagnosticStep step)
    {
        bool hasOptions = step.buttonNames.Length > 0;

        display.buttonList.gameObject.SetActive(hasOptions);
        if (hasOptions)
        {
            Debug.Log("Found " + step.buttonNames.Length + " options");
            display.buttonList.ClearOptions();
            List<string> optList = new List<string>(step.buttonNames);
            optList.Insert(0,"Choose an option.");
         
            display.buttonList.AddOptions(optList);
         //   display.buttonList.Load(step.buttonNames);
        }
        else
            Debug.Log("No Options found in this activity");
    }
    #endregion

    #region Load Data
    public List<DiagnosticStep> GetSteps() => steps;
    public void SetSteps(List<DiagnosticStep> steplist) => steps = steplist;

    // Load Data from module names using index
    public SearchableListWidget SearchResult;
    public StartScreen3dView startScreen3DView;
    public string DTSearchName;
    int SearchIndex;
    public void CheckDT(int index)
    {
      
        SearchIndex = index;
        //Debug.Log("<color=blue>Selected Tree File List</color>" + SearchResult.resultList[index]);
        if (SearchResult.resultList[index].Contains("[DEV]"))
        {       
            screenManager.Toast("Diagnostics for complaint: " + SearchResult.resultList[index].Replace("[DEV]", "") + " coming soon");
        }
        else
        {
            screenManager.Toast("Click Start to begin");
        }
        /*  if (treeFiles[index].name.Contains("[DEV]"))
              screenManager.Toast("Diagnostics for complaint: " + treeFiles[index].name.Replace("[DEV]", "") + " coming soon");
          else
              screenManager.Toast("Click Start to begin");*/
    }

/*    public void DTSearchList()
    {
        // serverData.diagnostic_trees = GetResponse(data).response_data.diagnostic_trees;
        SearchResult.searchBox.text = startScreen3DView.modName.ToString();
        Debug.Log("<color=blue> Selected Tree File List</color>" + SearchResult.searchBox.text);
        List<string> Result = new List<string>();
        foreach (TextAsset TreeFilelist in treeFiles)
        {
            //Debug.Log("<color=blue>Search Tree File List</color>"+SearchResult.searchBox);
            if (TreeFilelist.name.Contains(startScreen3DView.modName))
            {
                // SearchResult.searchBox = ;
                Result.Add(TreeFilelist.name);
                SearchResult.LoadResults(Result);
            }
        }
    }*/

    public void LoadDtFile(TextAsset DTFile,string DTName)
    {
        player.Load(DTFile);

        player.useRemoteLoading = useRemoteLoading;
        AppLogger.LogEventDesc(AppLogger.EventType.DIAG, "Loaded " + DTName);
        dynamicModuleName = DTName;
        Debug.Log("My diag " + dynamicModuleName);
    }

    public void LoadAndBeginDT()
    {
     /*   Debug.Log("<color=blue>Loading </color>");
        foreach (TextAsset SelectedDT in treeFiles)
        {
            if (SelectedDT.name == SearchResult.resultList[SearchIndex])
            {
                if (SelectedDT.name.Contains("[DEV]"))
                {
                    screenManager.Toast("Diagnostics for complaint: " + SelectedDT.name.Replace("[DEV]", "") + " coming soon");
                }
                else
                {
                    Debug.Log("<color=blue>Loading DT </color>" + SelectedDT.text);
                    player.Load(SelectedDT);
                    Download_DT_Images();
                    screenManager.SelectScreen(ScreenLinker.ScreenID.DT);
                    Begin();
                }

            }
        }*/
        // if (SearchResult.resultList[SearchIndex].Contains("[DEV]"))
        int index = dtSelector.value;//DropDown Value 

        if (treeFiles[index].name.Contains("[DEV]")) 
        {
            screenManager.Toast("Diagnostics for complaint: " + treeFiles[index].name.Replace("[DEV]", "") + " coming soon");
        }
        else
        {
           // LoadData(index);
            // player.Load(treeFiles[index]);
          //  Download_DT_Images();
           // screenManager.SelectScreen(ScreenLinker.ScreenID.DT);
           // Begin();
        }
    }
    public string dynamicModuleName;
    public void LoadData(int index)
    {
        if (useStepPlayer)
        {
            //if (treeFiles[index].name.Contains("[DEV]"))
            //    screenManager.Toast("Diagnostics for complaint: " + treeFiles[index].name.Replace("[DEV]", "") + " coming soon");
            player.Load(treeFiles[index]);      
            player.useRemoteLoading = useRemoteLoading;
            AppLogger.LogEventDesc(AppLogger.EventType.DIAG, "Loaded " + treeFiles[index].name);
        }
        else
        {
            curDTIndex = index;
            Debug.Log("OPTION " + index);
            LoadData(moduleFileNames[index]);
            AppLogger.LogEventDesc(AppLogger.EventType.DIAG, "Loaded " + moduleFileNames[index]);
           
        }
      
    }
    
    public void OnBackFromDT()
    {
        if (!player.dtcompleted)
        {
            Debug.Log("Back pressed from dt module " + dynamicModuleName);
            AppLogger.LogEventDesc(AppLogger.EventType.DIAG, "End " + dynamicModuleName);
        }
        else
        {
            Debug.Log("Back from complete screen");
           
        }
       
    }
    public string GetCurrentActivityNumber()
    {
        return activityNumbers[curr];
    }
    public int GetCurrentDtIndex() => curDTIndex;
    public void LoadAndStartFrom(int dtNumber, string activityNumber)
    {
        LoadData(dtNumber);
        LoadStep(activityNumber);
    }
    public void LoadStep(string aun)
    {
        bool foundAun = false;
        for (int i = 0; i < activityNumbers.Count; i++)
        {
            if (activityNumbers[i] == aun)
            {
                foundAun = true;
                curr = i;

            }
        }
        if (foundAun)
            ShowStep(curr);
        else
        {
            curr = 0;
            ShowStep(curr);
        }

    }
    public void LoadAndStart(int index)
    {
        curr = 0;
        LoadData(index);
        if(!useStepPlayer)
        ShowStep(curr);
    }
    private void LoadData(string file)
    {
        steps = LoadDataList(file);
    }
    // Load data from fileName
    private List<DiagnosticStep> LoadDataList(string file)
    {
        data = null;
        List<DiagnosticStep> steps = new List<DiagnosticStep>();
        // data = CSVReader.Read("DT05N");
        string fData = file.ToUpper();
        if (!fData.Contains("OLD"))// if doesn't Contain OLD
        {
            Debug.Log("Using V2 for " + fData);
            data = CSVSimple.ReadV2("DT/" + file);
             steps = InitialiseDataV2List();
        }
        else
        {
            data = CSVSimple.Read(file);
            InitialiseData();
        }
        
        totalSteps = data.Count;
        Debug.Log(file + " has " + totalSteps + " records");
        if (display.screen != null)
            display.screen.SetScreenName(complaintName);
        return steps;
    }
    // auto initialised from LoadData
    private void InitialiseData()
    {
        activityNumbers = new List<string>();
        rowValues = new List<string>();
        steps = new List<DiagnosticStep>();
        //if(data.Count>=1)
        // = data[0].Keys.ToList();

        for (int i = 0; i < data.Count; i++)
        {
            var vals = data[i].Values.ToArray();
            string valSt = "D:" + i + ":";

            foreach (var item in vals)
            {
                valSt += item.ToString();
            }
            rowValues.Add(valSt);
            Debug.Log(valSt);

            headerKeyNames = data[i].Keys.ToList();
            UpdateHeaders();

            activityNumbers.Add(data[i][header.activity_id].ToString());

            DiagnosticStep step = new DiagnosticStep();
            step.activity_id = data[i][header.activity_id].ToString();

            step.diagnostic_block_number = data[i][header.diagnostic_block].ToString();
            step.question = data[i][header.question].ToString();
            step.on_yes = data[i][header.yes].ToString();
            step.on_no = data[i][header.no].ToString();
            step.part_number = data[i][header.part_number].ToString();
            step.activity = data[i][header.activity].ToString();
            step.instruction = data[i][header.instruction].ToString();
            string picScanData = data[i][header.scan].ToString();
            if (picScanData != null)
                step.scan = data[i][header.scan].ToString();


            steps.Add(step);

        }
    }
    void UpdateHeaders()
    {
        for (int i = 0; i < headerKeyNames.Count; i++)
        {
            header.activity_id = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.activity_id) ? headerKeyNames[i] : header.activity_id;
            header.activity = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.activity) ? headerKeyNames[i] : header.activity;
            header.scan = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.scan) ? headerKeyNames[i] : header.scan;
            header.question = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.question) ? headerKeyNames[i] : header.question;
            header.part_number = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.part_number) ? headerKeyNames[i] : header.part_number;
            header.diagnostic_block = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.diagnostic_block) ? headerKeyNames[i] : header.diagnostic_block;
            header.display_message = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.display_message) ? headerKeyNames[i] : header.display_message;
            header.instruction = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.instruction) ? headerKeyNames[i] : header.instruction;
            header.yes = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.yes) ? headerKeyNames[i] : header.yes;
            header.no = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.no) ? headerKeyNames[i] : header.no;
            header.image = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.image) ? headerKeyNames[i] : header.image;
            header.caution_notes = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.caution_notes) ? headerKeyNames[i] : header.caution_notes;
            header.specs = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.specs) ? headerKeyNames[i] : header.specs;

            header.img1 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.img1) ? headerKeyNames[i] : header.img1;
            header.img2 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.img2) ? headerKeyNames[i] : header.img2;
            header.img3 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.img3) ? headerKeyNames[i] : header.img3;
            header.img4 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.img4) ? headerKeyNames[i] : header.img4;
            header.img5 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.img5) ? headerKeyNames[i] : header.img5;

            header.buttonNameList = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.buttonNameList) ? headerKeyNames[i] : header.buttonNameList;

            header.button1 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button1) ? headerKeyNames[i] : header.button1;
            header.button2 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button2) ? headerKeyNames[i] : header.button2;
            header.button3 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button3) ? headerKeyNames[i] : header.button3;
            header.button4 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button4) ? headerKeyNames[i] : header.button4;
            header.button5 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button5) ? headerKeyNames[i] : header.button5;

            header.button6 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button6) ? headerKeyNames[i] : header.button6;
            header.button7 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button7) ? headerKeyNames[i] : header.button7;
            header.button8 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button8) ? headerKeyNames[i] : header.button8;
            header.button9 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button9) ? headerKeyNames[i] : header.button9;
            header.button10 = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.button10) ? headerKeyNames[i] : header.button10;

            header.method = headerKeyNames[i].ToUpper().Contains(HEADER_REGEX.method) ? headerKeyNames[i] : header.method;
        }

    }
    string CheckHeader(string ip, string regex)
    {
        if (ip.ToUpper().Contains(regex))
            return ip;
        else
            return regex;

    }
    private void InitialiseDataV2()
    {
        steps = InitialiseDataV2List();
    }
    // auto initialised from LoadData
    private List<DiagnosticStep> InitialiseDataV2List()
    {
        activityNumbers = new List<string>();
       List<DiagnosticStep> steps = new List<DiagnosticStep>();
        if (data.Count >= 2)
        {

            headerKeyNames = data[1].Keys.ToList();
            if (data[0].ContainsKey("COMPLAINT"))
            {
                complaintName = data[0]["COMPLAINT"].ToString();
                if (complaintName.Length > 0)
                    complaintName = complaintName.Replace(",", "");
            }
            Debug.Log("COMPLAINT:" + data[0]["COMPLAINT"].ToString());
            display.complaintTitle.text = data[0]["COMPLAINT"].ToString().Replace(",", "");
            UpdateHeaders();
        }
        for (int i = 2; i < data.Count; i++)
        {
            //     Debug.Log(i);
            //      headerKeyNames = data[i].Keys.ToList();

            var aunObj = data[i][header.activity_id];
            string aun = "";
            if (aunObj != null)
                aun = aunObj.ToString();
            else
                aun = "ISSUE FOUND";

            activityNumbers.Add(data[i][header.activity_id].ToString());
            DiagnosticStep step = new DiagnosticStep();
            step.activity_id = data[i][header.activity_id].ToString();
            step.diagnostic_block_number = data[i][header.diagnostic_block].ToString();
            step.question = data[i][header.question].ToString();
            step.on_yes = data[i][header.yes].ToString();
            step.on_no = data[i][header.no].ToString();
            step.part_number = data[i][header.part_number].ToString();
            step.activity = data[i][header.activity].ToString();
            step.instruction = data[i][header.instruction].ToString();
            step.image = data[i].ContainsKey(header.image) ? data[i][header.image].ToString() : "";
            step.caution_notes = data[i][header.caution_notes].ToString();
            step.specs = data[i].ContainsKey(header.specs) ? data[i][header.specs].ToString() : "";
            string picScanData = data[i][header.scan].ToString();
            if (picScanData != null)
                step.scan = data[i][header.scan].ToString();
            step.stage = data[i].ContainsKey("Stage") ? data[i]["Stage"].ToString() : "";
            step.imgs[0] = data[i].ContainsKey(header.img1) ? data[i][header.img1].ToString() : "";
            step.imgs[1] = data[i].ContainsKey(header.img2) ? data[i][header.img2].ToString() : "";
            step.imgs[2] = data[i].ContainsKey(header.img3) ? data[i][header.img3].ToString() : "";
            step.imgs[3] = data[i].ContainsKey(header.img4) ? data[i][header.img4].ToString() : "";
            step.imgs[4] = data[i].ContainsKey(header.img5) ? data[i][header.img5].ToString() : "";

            string allBtnNames = data[i].ContainsKey(header.buttonNameList) ? data[i][header.buttonNameList].ToString() : "";
            if (allBtnNames.Length <= 0)
            {
                step.buttonNames = new string[0];
                step.buttons = null;
            }
            else
            {
                step.buttonNames = allBtnNames.Split(';');
                CheckStepImportData(data[i],i);

                step.buttons[0] = data[i].ContainsKey(header.button1) ? data[i][header.button1].ToString() : "";
                step.buttons[1] = data[i].ContainsKey(header.button2) ? data[i][header.button2].ToString() : "";
                step.buttons[2] = data[i].ContainsKey(header.button3) ? data[i][header.button3].ToString() : "";
                step.buttons[3] = data[i].ContainsKey(header.button4) ? data[i][header.button4].ToString() : "";
                step.buttons[4] = data[i].ContainsKey(header.button5) ? data[i][header.button5].ToString() : "";

                step.buttons[5] = data[i].ContainsKey(header.button6) ? data[i][header.button6].ToString() : "";
                step.buttons[6] = data[i].ContainsKey(header.button7) ? data[i][header.button7].ToString() : "";
                step.buttons[7] = data[i].ContainsKey(header.button8) ? data[i][header.button8].ToString() : "";
                step.buttons[8] = data[i].ContainsKey(header.button9) ? data[i][header.button9].ToString() : "";
                step.buttons[9] = data[i].ContainsKey(header.button10) ? data[i][header.button10].ToString() : "";
            }
            if (data[i].ContainsKey(header.method))
            {
             //   Debug.Log("Has Method in " + i);
                step.method = data[i].ContainsKey(header.method) ? data[i][header.method].ToString() : "";
                if (step.method.Length > 0)
                    Debug.Log("<color=blue>step: " + i + " Method Found:</color> " + step.method);
            }
            else
                step.method = "";
            steps.Add(step);
        }
        return steps;
    }
    void CheckStepImportData(Dictionary<string, object> impData,int stepNum)
    {
        if (impData.ContainsKey(header.button1))
            Debug.Log("Has Key for Button1" + impData[header.button1].ToString()+" in step "+stepNum);
        if (impData.ContainsKey(header.button2))
            Debug.Log("Has Key for Button2" + impData[header.button2].ToString() + " in step " + stepNum);

    }
}
    #endregion


#if UNITY_EDITOR
    [CustomEditor(typeof(DiagnosticTreeManager))]
public class DiagnosticTreeManagerEditor : Editor
{
    int loadDTNumber = -1;
    string testSpriteName = "";
    public override void OnInspectorGUI()
    {
        DiagnosticTreeManager script = (DiagnosticTreeManager)target;
        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox("Input a DT name in Resources/DT Folder", MessageType.None);
        EditorGUILayout.BeginHorizontal();
        loadDTNumber = EditorGUILayout.IntField(loadDTNumber);
        if (GUILayout.Button("Load File", GUILayout.MinHeight(25)))
            script.LoadData(loadDTNumber);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("Input a Sprite Name in Resources/Images/DT Images Folder",MessageType.None);
        EditorGUILayout.BeginHorizontal();
        testSpriteName = EditorGUILayout.TextField(testSpriteName);
     
        if (GUILayout.Button("Get Sprite"))
          script.testSprite=  script.GetDTSprite(testSpriteName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        if (GUILayout.Button("Get All Images"))
        {
            List<string> allNames = new List<string>();
          allNames=  script.GetAllImages();
            Debug.Log("Total Sprites: " + allNames.Count);
            Debug.Log("<Color=Blue>Editor Download Link </color>");
         script.GetAssetLoader().DownloadImages(allNames, "",".jpg");
         
        }
        base.OnInspectorGUI();

    }
}
#endif